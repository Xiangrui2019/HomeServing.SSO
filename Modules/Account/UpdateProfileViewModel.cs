using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using HomeServing.SSO.Models;

namespace HomeServing.SSO.Modules.Account
{
    public class UpdateProfileViewModel
    {
        public string UserName { get; set; }
        [Required]
        public string NickName { get; set; }
        [Required]
        public string Bio { get; set; }
        [Required]
        public Gender Gender { get; set; }
    }
}
