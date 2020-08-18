using System;
using Microsoft.AspNetCore.Identity;

namespace HomeServing.SSO.Models
{
    public enum Gender
    {
        男 = 0,
        女 = 1,
        未知 = 2
    }

    public class ApplicationUser : IdentityUser
    {
        public string NikeName { get; set; }
        public string Avatar { get; set; }
        public string Bio { get; set; }
        public Gender Gender { get; set; } = Gender.未知;
    }
}