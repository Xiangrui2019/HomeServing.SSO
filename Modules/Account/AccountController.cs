﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Aliyun.OSS;
using HomeServing.SSO.Models;
using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;

namespace HomeServing.SSO.Modules.Account
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;
        private readonly IConfiguration _configuration;
        private readonly OssClient _ossClient;
        private readonly FileExtensionContentTypeProvider _extensionContentTypeProdvider;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            IConfiguration configuration,
            OssClient ossClient,
            FileExtensionContentTypeProvider extensionContentTypeProdvider)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
            _configuration = configuration;
            _ossClient = ossClient;
            _extensionContentTypeProdvider = extensionContentTypeProdvider;
        }

        /// <summary>
        /// Entry point into the login workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // build a model so we know what to show on the login page
            var vm = await BuildLoginViewModelAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return RedirectToAction("Challenge", "External", new {scheme = vm.ExternalLoginScheme, returnUrl});
            }

            return View(vm);
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {
            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            // the user clicked the "cancel" button
            if (button != "login")
            {
                if (context != null)
                {
                    // if the user cancels, send a result back into IdentityServer as if they 
                    // denied the consent (even if this client does not require consent).
                    // this will send back an access denied OIDC error response to the client.
                    await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    if (context.IsNativeClient())
                    {
                        // The client is native, so this change in how to
                        // return the response is for better UX for the end user.
                        return this.LoadingPage("Redirect", model.ReturnUrl);
                    }

                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    // since we don't have a valid context, then we just go back to the home page
                    return Redirect("~/");
                }
            }

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password,
                    model.RememberLogin, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(model.Username);
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName,
                        clientId: context?.Client.ClientId));

                    if (context != null)
                    {
                        if (context.IsNativeClient())
                        {
                            // The client is native, so this change in how to
                            // return the response is for better UX for the end user.
                            return this.LoadingPage("Redirect", model.ReturnUrl);
                        }

                        // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                        return Redirect(model.ReturnUrl);
                    }

                    // request for a local page
                    if (Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else if (string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return Redirect("~/");
                    }
                    else
                    {
                        // user might have clicked on a malicious link - should be logged
                        throw new Exception("不合法的return URI");
                    }
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "不合法的凭证",
                    clientId: context?.Client.ClientId));
                ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
            }

            // something went wrong, show form with error
            var vm = await BuildLoginViewModelAsync(model);
            return View(vm);
        }

        [HttpGet]
        public IActionResult Register(string redirectUrl)
        {
            var vm = new RegisterViewModel
            {
                ReturnUrl = redirectUrl,
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel vm, string button)
        {
            if (Url.IsLocalUrl(vm.ReturnUrl))
            {
                vm.ReturnUrl = vm.ReturnUrl;
            }
            else if (string.IsNullOrEmpty(vm.ReturnUrl))
            {
                vm.ReturnUrl = "~/";
            }
            else
            {
                throw new Exception("不合法的return URI");
            }

            if (button != "register")
            {
                return Redirect(vm.ReturnUrl);
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = vm.Username,
                    NikeName = $"nike_{vm.Username}",
                    Bio = "这个人很懒, 什么都没有写.",
                    Avatar = _configuration["DefaultAvatar"],
                    Gender = Gender.未知,
                };

                var result = await _userManager.CreateAsync(user, vm.Password);

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                if (result.Succeeded)
                {
                    return Redirect(vm.ReturnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "注册用户出现未知问题!");
                }
            }

            return View(vm);
        }

        private UpdateProfileViewModel BuildUpdateProfileViewModel(ApplicationUser user)
        {
            return new UpdateProfileViewModel
            {
                UserName = user.UserName,
                NickName = user.NikeName,
                Bio = user.Bio,
                Gender = user.Gender,
            };
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProfile()
        {
            if (User?.Identity.IsAuthenticated == false)
            {
                return Redirect("~/Account/Login");
            }

            var user = await _userManager.GetUserAsync(User);
            var vm = BuildUpdateProfileViewModel(user);

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel vm)
        {
            if (User?.Identity.IsAuthenticated == false)
            {
                return Redirect("~/Account/Login");
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(vm.UserName);
                user.NikeName = vm.NickName;
                user.Bio = vm.Bio;
                user.Gender = vm.Gender;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return Redirect("/Account/UpdateProfile");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "更新用户信息失败.");
                }
            }

            return View(vm);
        }


        [HttpGet]
        public IActionResult UpdatePassword()
        {
            if (User?.Identity.IsAuthenticated == false)
            {
                return Redirect("~/Account/Login");
            }

            return View(new UpdatePasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordViewModel vm)
        {
            if (User?.Identity.IsAuthenticated == false)
            {
                return Redirect("~/Account/Login");
            }

            if (vm.NewPassword == vm.ConfirmNewPassword)
            {
                var user = await GetCurrentUser();

                var result = await _userManager.ChangePasswordAsync(user, vm.OldPassword, vm.NewPassword);

                if (result.Succeeded)
                {
                    return Redirect("~/Account/UpdatePassword");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "更新用户密码失败.");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "您的新密码和确认密码不匹配.");
            }

            return View(vm);
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> UpdateAvatar()
        {
            if (User?.Identity.IsAuthenticated == false)
            {
                return Redirect("~/Account/Login");
            }

            var user = await GetCurrentUser();

            return View(new UpdateAvatarViewModel
            {
                AvatarUrl = user.Avatar,
            });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateAvatar(UpdateAvatarViewModel vm)
        {
            var stream = vm.Avatar.OpenReadStream();
            var user = await GetCurrentUser();
            var Endfix = vm.Avatar.FileName.Split(".").Reverse().First().ToString();
            var genObjectName = $"{user.UserName}_{Guid.NewGuid().ToString()}.{Endfix}";
            
            _ossClient.PutObject(
                _configuration["AliyunOSS:BucketName"],
                genObjectName,
                stream);

            user.Avatar = $"{_configuration["SSOServiceUrl"]}/Account/GetAvatarFile?regexName={genObjectName}%%{Endfix}";

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Redirect("/Account/UpdateAvatar");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "更新用户头像失败.");
            }

            return View(vm);
        }

        [HttpGet]
        public IActionResult GetAvatarFile([FromQuery] string regexName)
        {
            var splited = regexName.Split("%%");
            var objectName = splited[0];
            var endFix = $".{splited[1].ToLower()}";

            var obj = _ossClient.GetObject(
                _configuration["AliyunOSS:BucketName"],
                objectName);
            var memoryStream = new MemoryStream();
            

            using (var requestStream = obj.Content)
            {
                var buf = new byte[1024];
                var len = 0;

                while ((len = requestStream.Read(buf, 0, 1024)) != 0)
                {
                    memoryStream.Write(buf, 0, len);
                }
            }

            return File(memoryStream.ToArray(),
                _extensionContentTypeProdvider.Mappings[endFix]);
        }

        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // build a model so the logout page knows what to display
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await _signInManager.SignOutAsync();

                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new {logoutId = vm.LogoutId});

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties {RedirectUri = url}, vm.ExternalAuthenticationScheme);
            }

            return View("LoggedOut", vm);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }


        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/
        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == IdentityServer4.IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                var vm = new LoginViewModel
                {
                    EnableLocalLogin = local,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                };

                if (!local)
                {
                    vm.ExternalProviders = new[] {new ExternalProvider {AuthenticationScheme = context.IdP}};
                }

                return vm;
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null)
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName ?? x.Name,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.Client.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider =>
                            client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
            {
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray()
            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel {LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt};

            if (User?.Identity.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }
        
        private Task<ApplicationUser> GetCurrentUser() => _userManager.GetUserAsync(User);
    }
}