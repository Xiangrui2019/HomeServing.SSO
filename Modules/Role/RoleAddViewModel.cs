using System.ComponentModel.DataAnnotations;

namespace HomeServing.SSO.Modules.Role
{
    public class RoleAddViewModel
    {
        [Required]
        public string RoleName { get; set; }
    }
}