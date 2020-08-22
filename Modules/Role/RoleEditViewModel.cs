using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HomeServing.SSO.Modules.Role
{
    public class RoleEditViewModel
    {
        public string Id { get; set; }
        
        public List<string> Users { get; set; }

        [Required]
        public string RoleName { get; set; }
    }
}