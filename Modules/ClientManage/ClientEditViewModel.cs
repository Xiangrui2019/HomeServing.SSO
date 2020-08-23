using System;
using System.Collections.Generic;
using System.Text;
using IdentityServer4.Models;
using Novell.Directory.Ldap.Utilclass;

namespace HomeServing.SSO.Modules.ClientManage
{
    public class ClientEditViewModel : Client
    {
        public string RedirectUrisString { get; set; }
        public string AllowedGrantTypesString { get; set; }
        public string PostLogoutRedirectUrisString { get; set; }
        public string AllowedScopesString { get; set; }
        public string AllowedCorsOriginsString { get; set; }

        public void ToDatabase()
        {
            StringToAllowedGrantTypes();
            StringToPostLogoutRedirectUris();
            StringToRedirectUris();
            StringToAllowedScopes();
            StringToAllowedCorsOrigins();

            InitFields();
        }

        public void ToViewModel()
        {
            AllowedGrantTypesString = ToStringList(AllowedGrantTypes);
            RedirectUrisString = ToStringList(RedirectUris);
            PostLogoutRedirectUrisString = ToStringList(PostLogoutRedirectUris);
            AllowedScopesString = ToStringList(AllowedScopes);
            AllowedCorsOriginsString = ToStringList(AllowedCorsOrigins);

            InitFields();
        }

        public void InitFields()
        {
            AlwaysIncludeUserClaimsInIdToken = true;
            RefreshTokenUsage = TokenUsage.ReUse;
            AlwaysSendClientClaims = true;
        }

        public void StringToAllowedGrantTypes()
        {
            foreach (var grantType in ToLists(AllowedGrantTypesString))
            {
                AllowedGrantTypes.Add(grantType);
            }
        }

        public void StringToRedirectUris()
        {
            foreach (var redirectUri in ToLists(RedirectUrisString))
            {
                RedirectUris.Add(redirectUri);
            }
        }

        public void StringToPostLogoutRedirectUris()
        {
            foreach (var postLogout in ToLists(PostLogoutRedirectUrisString))
            {
                PostLogoutRedirectUris.Add(postLogout);
            }
        }

        public void StringToAllowedScopes()
        {
            foreach (var scope in ToLists(AllowedScopesString))
            {
                AllowedScopes.Add(scope);
            }
        }

        public void StringToAllowedCorsOrigins()
        {
            foreach (var origin in ToLists(AllowedScopesString))
            {
                AllowedCorsOrigins.Add(origin);
            }
        }

        public IEnumerable<string> ToLists(string source)
        {
            if (source == null)
            {
                return new List<string>();
            }

            return source.Split("/");
        }

        public string ToStringList(IEnumerable<string> source)
        {
            var jBuilder = new StringBuilder();

            var i = 0;
            foreach (var strings in source)
            {
                jBuilder.Append(strings);
                i += 1;
            }

            if (i != 0)
            {
                jBuilder.Remove(jBuilder.Length - 1, 1);
            }

            return jBuilder.ToString();
        }
    }
}