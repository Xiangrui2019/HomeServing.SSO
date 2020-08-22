using HomeServing.SSO.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeServing.SSO.Modules.User
{
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
    }
}
