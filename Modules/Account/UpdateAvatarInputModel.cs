using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HomeServing.SSO.Modules.Account
{
    public class UpdateAvatarInputModel
    {
        [Required]
        [Display(Name = "头像")]
        [FileExtensions(Extensions = ".jpg,.png,.bmp,.tif,.gif,.jpeg,.exif,.svg,.raw,.ico",
                        ErrorMessage = "请上传图片")]
        public IFormFile Avatar { get; set; }
    }
}
