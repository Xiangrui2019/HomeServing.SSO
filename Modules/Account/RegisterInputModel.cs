using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HomeServing.SSO.Modules.Account
{
    public class RegisterInputModel
    {
        [Description("用户名")]
        [Required] public string Username { get; set; }
        [Description("密码")]
        [Required] public string Password { get; set; }
        public string ReturnUrl { get; set; }
    }
}
