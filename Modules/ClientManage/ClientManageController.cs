using AutoMapper;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Mvc;

namespace HomeServing.SSO.Modules.ClientManage
{
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


    }
}