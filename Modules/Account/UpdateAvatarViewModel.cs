using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HomeServing.SSO.Modules.Account
{
    public class UpdateAvatarViewModel : UpdateAvatarInputModel
    {
        public string AvatarUrl { get; set; }
    }
}
