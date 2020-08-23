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
        private readonly IMapper _mapper;

        public ClientManageController(
            ConfigurationDbContext configurationDbContext,
            IMapper mapper)
        {
            _configurationDbContext = configurationDbContext;
            _mapper = mapper;
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
        public async Task<IActionResult> EditClient(int id)
        {
            var clientEntity = await _configurationDbContext
                .Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == id);

            var client = clientEntity.ToModel();
            var vm = _mapper.Map<Client, ClientEditViewModel>(client);

            vm.ToViewModel();

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> EditClient(ClientEditViewModel vm)
        {
            vm.ToDatabase();
            var clientS = await _configurationDbContext
                .Clients
                .FirstOrDefaultAsync(q => q.ClientId == vm.ClientId);

            var clientEntity = 

            _configurationDbContext.Update(clientEntity);
            await _configurationDbContext.SaveChangesAsync();

            return RedirectToAction("Index");
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