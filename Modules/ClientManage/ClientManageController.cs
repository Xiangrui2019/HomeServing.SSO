using AutoMapper;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using IdentityServer4.Models;
using Microsoft.EntityFrameworkCore;
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
            return View(new Client
            {
                Enabled = true,
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddClient(Client client)
        {
        }
    }
}