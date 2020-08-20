using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeServing.SSO.Modules.Api
{
    public class ApiController : Controller
    {
        private readonly ConfigurationDbContext _configurationDbContext;

        public ApiController(ConfigurationDbContext configurationDbContext)
        {
            _configurationDbContext = configurationDbContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreateApiScope(ApiScope scope)
        {
            var result = await _configurationDbContext.AddAsync(scope);

            await _configurationDbContext.SaveChangesAsync();

            return Json(result.Entity);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateApiScope(ApiScope scope)
        {
            var apiScope = await _configurationDbContext
                .ApiScopes
                .Where(q => q.Id == scope.Id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            apiScope.Enabled = scope.Enabled;
            apiScope.Name = scope.Name;
            apiScope.DisplayName = scope.DisplayName;
            apiScope.Description = scope.Description;
            apiScope.Required = scope.Required;
            apiScope.Emphasize = scope.Emphasize;
            apiScope.ShowInDiscoveryDocument = scope.ShowInDiscoveryDocument;
            apiScope.UserClaims = scope.UserClaims;
            apiScope.Properties = scope.Properties;

            _configurationDbContext
                .ApiScopes
                .Update(apiScope);

            await _configurationDbContext.SaveChangesAsync();

            return Json(apiScope);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteApiScope(int Id)
        {
            var apiScope = await _configurationDbContext
                .ApiScopes
                .Where(q => q.Id == Id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            _configurationDbContext.Remove(apiScope);

            await _configurationDbContext.SaveChangesAsync();

            return Json("");
        }

        [HttpGet]
        public async Task<IActionResult> ListApiScopes()
        {
            var apiScopes = await _configurationDbContext
                .ApiScopes
                .AsNoTracking()
                .ToListAsync();

            return Json(apiScopes);
        }

        public async Task<IActionResult> CreateClient(IdentityServer4.Models.Client client)
        {
            var result = await _configurationDbContext.AddAsync(client.ToEntity());
            await _configurationDbContext.SaveChangesAsync();

            return Json(result);
        }

        public async Task<IActionResult> UpdateClient(int Id, IdentityServer4.Models.Client client)
        {
            var client_entity = client.ToEntity();
            client_entity.Id = Id;

            _configurationDbContext.Update(client_entity);
            await _configurationDbContext.SaveChangesAsync();

            return Json(client_entity);
        }

        public async Task<IActionResult> DeleteClient(int Id)
        {
            var client = await _configurationDbContext
                .Clients
                .Where(q => q.Id == Id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            _configurationDbContext.Remove(client);
            await _configurationDbContext.SaveChangesAsync();

            return Json("");
        }

        public async Task<IActionResult> ListClients()
        {
            var clients = await _configurationDbContext
                .Clients
                .AsNoTracking()
                .ToListAsync();

            return Json(clients);
        }

        public async Task<IActionResult> ShowClient(int Id)
        {
            var client = await _configurationDbContext
                .Clients
                .Where(q => q.Id == Id)
                .FirstOrDefaultAsync();

            return Json(client);
        }
    }
}
