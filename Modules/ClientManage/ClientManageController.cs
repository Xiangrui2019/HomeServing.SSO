using System.Linq;
using AutoMapper;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace HomeServing.SSO.Modules.ClientManage
{
    [UserAuthorize(Role = "Root,Administrator")]
    public class ClientManageController : Controller
    {
        private readonly ConfigurationDbContext _configurationDbContext;

        public ClientManageController(
            ConfigurationDbContext configurationDbContext)
        {
            _configurationDbContext = configurationDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var clients = await _configurationDbContext
                .Clients
                .AsNoTracking()
                .ToListAsync();

            return View(clients);
        }

        [HttpGet]
        public IActionResult AddClient()
        {
            return View(new ClientAddViewModel
            {
                Enabled = true,
                UpdateAccessTokenClaimsOnRefresh = true,
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddClient(ClientAddViewModel client)
        {
            client.ToDatabase();
            var clientEntity = client.ToEntity();

            await _configurationDbContext.AddAsync(clientEntity);
            await _configurationDbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ShowClient(int id)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _configurationDbContext
                .Clients
                .Where(q => q.Id == id)
                .FirstOrDefaultAsync();

            if (client == null)
            {
                var clients = await _configurationDbContext
                    .Clients
                    .AsNoTracking()
                    .ToListAsync();

                ModelState.AddModelError(string.Empty, "无法找到对应的客户端.");

                return View("Index", clients);
            }

            _configurationDbContext.Remove(client);
            await _configurationDbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}