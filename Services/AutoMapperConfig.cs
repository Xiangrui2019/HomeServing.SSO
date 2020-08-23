using AutoMapper;
using HomeServing.SSO.Modules.ClientManage;
using IdentityServer4.Models;

namespace HomeServing.SSO.Services
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<ClientAddViewModel, Client>();
            CreateMap<Client, ClientAddViewModel>();
        }
    }
}