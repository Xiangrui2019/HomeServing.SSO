using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HomeServing.SSO.Modules.ApiScopeManage
{
    public class ApiScopeEditViewModel
    {
        public int Id { get; set; }

        [Required]
        public bool Enabled { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string DisplayName { get; set; }
        [Required]
        public string Description { get; set; }
        public List<string> ApiScopeClaims { get; set; }
    }
}