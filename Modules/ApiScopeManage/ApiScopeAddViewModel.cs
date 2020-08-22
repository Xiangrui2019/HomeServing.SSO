using System.ComponentModel.DataAnnotations;

namespace HomeServing.SSO.Modules.ApiScopeManage
{
    public class ApiScopeAddViewModel
    {
        [Required]
        public bool Enabled { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string DisplayName { get; set; }
        [Required]
        public string Description { get; set; }
    }
}