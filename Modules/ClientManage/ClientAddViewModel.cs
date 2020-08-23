using System.Collections.Generic;
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

        public void ToEntity()
        {
            
        }

        public void StringToClientSecrets()
        {
            var raw = Base64ToString(ClientSecretsString);
            var strings = raw.Split("/");

            foreach (var secret in strings)
            {
                ClientSecrets.Add(new Secret(secret.Sha256()));
            }
        }

        public void StringToRedirectUris(string source)
        {
            var raw = Base64ToString(RedirectUrisString);
            var strings = raw.Split("/");

            foreach (var redirectUri in strings)
            {
                RedirectUris.Add(redirectUri);
            }
        }

        public void StringToAllowedGrantTypes()
        {
            var raw = Base64ToString(AllowedGrantTypesString);
            var strings = raw.Split("/");

            foreach (var grantType in strings)
            {
                AllowedGrantTypes.Add(grantType);
            }
        }

        public void StringToPostLogoutRedirectUris()
        {
            var raw = Base64ToString(PostLogoutRedirectUrisString);
            var strings = raw.Split("/");

            foreach (var postLogout in strings)
            {
                PostLogoutRedirectUris.Add(postLogout);
            }
        }

        public string Base64ToString(string source)
        {
            return "";
        }
    }
}