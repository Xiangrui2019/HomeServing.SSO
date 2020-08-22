using System.Collections.Generic;
using HomeServing.SSO.Models;

namespace HomeServing.SSO.Modules.Role
{
    public class UserRoleViewModel
    {
        public List<ApplicationUser> Users { get; set; }

        public string UserId { get; set; }

        public string RoleId { get; set; }
    }
}