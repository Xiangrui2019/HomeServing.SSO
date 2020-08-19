using Aliyun.OSS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HomeServing.SSO.Modules.Files
{
    public class FileController : Controller
    {
        private readonly OssClient _ossClient;
        private readonly FileExtensionContentTypeProvider _extensionContentTypeProdvider;
        private readonly IConfiguration _configuration;

        public FileController(
            OssClient ossClient,
            FileExtensionContentTypeProvider extensionContentTypeProdvider,
            IConfiguration configuration)
        {
            _ossClient = ossClient;
            _extensionContentTypeProdvider = extensionContentTypeProdvider;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetAvatarFile([FromQuery] string regexName)
        {
            var splited = regexName.Split("%%");
            var objectName = splited[0];
            var endFix = $".{splited[1].ToLower()}";

            var obj = _ossClient.GetObject(
                _configuration["AliyunOSS:BucketName"],
                objectName);
            var memoryStream = new MemoryStream();


            using (var requestStream = obj.Content)
            {
                var buf = new byte[1024];
                var len = 0;

                while ((len = requestStream.Read(buf, 0, 1024)) != 0)
                {
                    memoryStream.Write(buf, 0, len);
                }
            }

            return File(memoryStream.ToArray(),
                _extensionContentTypeProdvider.Mappings[endFix]);
        }
    }
}
