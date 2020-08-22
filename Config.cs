using IdentityModel;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace HomeServing.SSO
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
                   new IdentityResource[]
                   {
                        new IdentityResources.OpenId(),
                        new IdentityResources.Profile(),
                        new IdentityResource("avatar", "头像", new List<string> { JwtClaimTypes.Picture }),
                        new IdentityResource("nick_name", "昵称", new List<string> { JwtClaimTypes.NickName }),
                        new IdentityResource("bio", "个人简介", new List<string> { JwtClaimTypes.Profile }),
                        new IdentityResource("gender", "性别", new List<string> { JwtClaimTypes.Gender }),
                        new IdentityResource("user_name", "用户名", new List<string> { JwtClaimTypes.PreferredUserName })
                   };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
            };

        public static IEnumerable<Client> Clients()
        {
            return new Client[]
            {
            };
        }
    }
}
