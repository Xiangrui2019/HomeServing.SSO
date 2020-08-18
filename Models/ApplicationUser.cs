using System;
using Microsoft.AspNetCore.Identity;

namespace HomeServing.SSO.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string NikeName { get; set; }
        public string Avatar { get; set; }
        public string Bio { get; set; }
    }
}