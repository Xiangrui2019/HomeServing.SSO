using System.ComponentModel.DataAnnotations;

namespace HomeServing.SSO.Modules.ApiScopeManage
{
    public class ClaimToApiScopeViewModel
    {
        public int ApiScopeId { get; set; }
        [Required]
        public string ClaimType { get; set; }
    }
}