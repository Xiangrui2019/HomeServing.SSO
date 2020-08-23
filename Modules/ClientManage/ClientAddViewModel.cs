using System;
using System.Collections.Generic;
using System.Text;
using IdentityServer4.Models;
using Novell.Directory.Ldap.Utilclass;

namespace HomeServing.SSO.Modules.ClientManage
{
    public class ClientAddViewModel : Client
    {
        public string ClientSecretsString { get; set; }
        public string RedirectUrisString { get; set; }
        public string AllowedGrantTypesString { get; set; }
        public string PostLogoutRedirectUrisString { get; set; }
        public string AllowedScopesString { get; set; }
        public string AllowedCorsOriginsString { get; set; }

        public void InitModel()
        {
            StringToClientSecrets();
            StringToRedirectUris();
            StringToAllowedGrantTypes();
            StringToPostLogoutRedirectUris();
            StringToAllowedScopes();
            StringToAllowedCorsOrigins();

            AlwaysIncludeUserClaimsInIdToken = true;
            RefreshTokenUsage = TokenUsage.ReUse;
            AlwaysSendClientClaims = true;
        }

        public void StringToClientSecrets()
        {
            foreach (var secret in ToLists(ClientSecretsString))
            {
                ClientSecrets.Add(new Secret(secret.Sha256()));
            }
        }

        public void StringToRedirectUris()
        {
            foreach (var redirectUri in ToLists(RedirectUrisString))
            {
                RedirectUris.Add(redirectUri);
            }
        }

        public void StringToAllowedGrantTypes()
        {
            foreach (var grantType in ToLists(AllowedGrantTypesString))
            {
                AllowedGrantTypes.Add(grantType);
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
            => source.Split("/");
    }
}