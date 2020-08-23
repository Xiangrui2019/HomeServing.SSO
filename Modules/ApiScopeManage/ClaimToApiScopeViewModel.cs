using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IdentityServer4.EntityFramework.Entities;

namespace HomeServing.SSO.Modules.ApiScopeManage
{
    public class ClaimToApiScopeViewModel
    {
        public int ApiScopeId { get; set; }

        [Required]
        public string ClaimType { get; set; }

        public List<ApiScopeClaim> ApiScopeClaims { get; set; }
    }
}