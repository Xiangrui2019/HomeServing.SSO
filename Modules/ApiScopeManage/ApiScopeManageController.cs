using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeServing.SSO.Modules.ApiScopeManage
{
    [UserAuthorize(Role = "Root,Administrator")]
    public class ApiScopeManageController : Controller
    {
        private readonly ConfigurationDbContext _configurationDbContext;

        public ApiScopeManageController(ConfigurationDbContext configurationDbContext)
        {
            _configurationDbContext = configurationDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var apiScopes = await _configurationDbContext
                .ApiScopes
                .AsNoTracking()
                .ToListAsync();

            return View(apiScopes);
        }

        [HttpGet]
        public IActionResult AddApiScope()
        {
            return View(new ApiScopeAddViewModel
            {
                Enabled = true,
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddApiScope(ApiScopeAddViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var apiScope = new ApiScope
            {
                Enabled = model.Enabled,
                Name = model.Name,
                DisplayName = model.DisplayName,
                Description = model.Description,
                ShowInDiscoveryDocument = true,
                UserClaims = new List<ApiScopeClaim>()
            };

            await _configurationDbContext
                .ApiScopes
                .AddAsync(apiScope);

            await _configurationDbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> EditApiScope(int id)
        {
            var apiScope = await _configurationDbContext
                .ApiScopes
                .Where(q => q.Id == id)
                .Include(q => q.UserClaims)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (apiScope == null)
            {
                ModelState.AddModelError(string.Empty, "ApiScope未找到.");

                var apiScopes = await _configurationDbContext
                    .ApiScopes
                    .AsNoTracking()
                    .ToListAsync();

                return View("Index", apiScopes);
            }

            var vm = new ApiScopeEditViewModel
            {
                Id = id,
                Enabled = apiScope.Enabled,
                Name = apiScope.Name,
                DisplayName = apiScope.DisplayName,
                Description = apiScope.Description,
                ApiScopeClaims = new List<string>()
            };

            if (!apiScope.UserClaims.IsNullOrEmpty())
            {
                foreach (var scope in apiScope.UserClaims)
                {
                    vm.ApiScopeClaims.Add(scope.Type);
                }
            }
            
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> EditApiScope(ApiScopeEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var apiScope = await _configurationDbContext
                .ApiScopes
                .Where(q => q.Id == model.Id)
                .FirstOrDefaultAsync();

            apiScope.Enabled = model.Enabled;
            apiScope.Name = model.Name;
            apiScope.Description = model.Description;
            apiScope.DisplayName = model.DisplayName;

            _configurationDbContext.Update(apiScope);
            await _configurationDbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult AddClaimToApiScope(int id)
        {
            return View(new ClaimToApiScopeViewModel
            {
                ApiScopeId = id,
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddClaimToApiScope(ClaimToApiScopeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var apiScope = await _configurationDbContext
                .ApiScopes
                .Where(q => q.Id == model.ApiScopeId)
                .FirstOrDefaultAsync();

            if (!apiScope.UserClaims.IsNullOrEmpty())
            {
                var i = 0;

                foreach (var claim in apiScope.UserClaims)
                {
                    if (claim.Type == model.ClaimType)
                    {
                        i += 1;
                    }
                }

                if (i > 0)
                {
                    ModelState.AddModelError(string.Empty, "您的ClaimType重复了, 请不要重复添加, 谢谢.");

                    return View(model);
                }
            }

            apiScope.UserClaims = new List<ApiScopeClaim>
            {
                new ApiScopeClaim
                {
                    Type = model.ClaimType,
                    Scope = apiScope,
                    ScopeId = apiScope.Id
                }
            };

            await _configurationDbContext.SaveChangesAsync();

            return RedirectToAction("EditApiScope", new { id = apiScope.Id });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteApiScope(int id)
        {
            var apiScope = await _configurationDbContext
                .ApiScopes
                .Where(q => q.Id == id)
                .FirstOrDefaultAsync();

            _configurationDbContext.Remove(apiScope);
            await _configurationDbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}