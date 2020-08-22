using HomeServing.SSO.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace HomeServing.SSO.Modules.User
{
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public UserController(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await  _userManager.Users.ToListAsync();

            return View(users);
        }

        [HttpGet]
        public IActionResult AddUser()
        {
            return View(new UserAddViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(UserAddViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                NikeName = $"nick_{model.UserName}",
                Bio = "这个人很懒, 什么都没有写.",
                Avatar = _configuration["DefaultAvatar"],
                Gender = Gender.未知,
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                return View(MakeUserEditViewModel(user));
            }
            else
            {
                ModelState.AddModelError(string.Empty, "无法找到这个用户");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(UserEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            user.UserName = model.UserName;
            user.NikeName = model.NickName;
            user.Bio = model.Bio;
            user.Gender = model.Gender;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "更新用户失败.");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                if (user.UserName != User.Identity.Name)
                {
                    var result = await _userManager.DeleteAsync(user);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }

                    ModelState.AddModelError(string.Empty, "删除用户失败");
                }

                ModelState.AddModelError(string.Empty, "您不能删除您自己.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "用户无法找到.");
            }

            return View("Index", await _userManager.Users.ToListAsync());
        }

        public UserEditViewModel MakeUserEditViewModel(ApplicationUser user)
        {
            return new UserEditViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                NickName = user.NikeName,
                Bio = user.Bio,
                Gender = user.Gender,
            };
        }
    }
}
