using Aliyun.OSS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServing.SSO.Data;
using HomeServing.SSO.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace HomeServing.SSO.Modules.Files
{
    public class FileController : Controller
    {
        private readonly OssClient _ossClient;
        private readonly FileExtensionContentTypeProvider _extensionContentTypeProdvider;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;

        public FileController(
            OssClient ossClient,
            FileExtensionContentTypeProvider extensionContentTypeProdvider,
            IConfiguration configuration,
            ApplicationDbContext dbContext)
        {
            _ossClient = ossClient;
            _extensionContentTypeProdvider = extensionContentTypeProdvider;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAvatarFile([FromQuery] string regexName)
        {
            var splited = regexName.Split("%%");
            var objectName = splited[0];
            var endFix = $".{splited[1].ToLower()}";

            var objDb = await _dbContext
                .ImageCaches
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.ImageKey == objectName);

            if (objDb != null)
            {
                byte[] objBytes = Convert.FromBase64String(objDb.BytesString);

                return File(objBytes, _extensionContentTypeProdvider.Mappings[endFix]);
            }

            var obj = _ossClient.GetObject(
                _configuration["AliyunOSS:BucketName"],
                objectName);
            var memoryStream = new MemoryStream();


            await using (var requestStream = obj.Content)
            {
                var buf = new byte[1024];
                var len = 0;

                while ((len = await requestStream.ReadAsync(buf, 0, 1024)) != 0)
                {
                    await memoryStream.WriteAsync(buf, 0, len);
                }
            }

            var resultBytes = memoryStream.ToArray();

            await _dbContext.AddAsync(new ImageCache
            {
                ImageKey = objectName,
                BytesString = Convert.ToBase64String(resultBytes),
            });

            await _dbContext.SaveChangesAsync();

            return File(resultBytes,
                _extensionContentTypeProdvider.Mappings[endFix]);
        }
    }
}
