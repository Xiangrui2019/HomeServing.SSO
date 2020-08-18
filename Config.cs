using IdentityModel;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeServing.SSO
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
                   new IdentityResource[]
                   {
                        new IdentityResources.OpenId(),
                        new IdentityResources.Profile(),
                        new IdentityResources.Address(),
                        new IdentityResources.Phone(),
                        new IdentityResource("avatar", "头像", new List<string> { JwtClaimTypes.Picture }),
                        new IdentityResource("profile", "个人简介", new List<string> { JwtClaimTypes.Profile }),
                        new IdentityResource("nick_name", "昵称", new List<string> { JwtClaimTypes.NickName })
                   };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("scope1"),
                new ApiScope("scope2"),
            };

        public static IEnumerable<Client> Clients()
        {
            return new Client[]
            {
            };
        }
    }
}
