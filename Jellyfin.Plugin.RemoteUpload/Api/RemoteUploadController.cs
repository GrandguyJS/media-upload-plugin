using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Api;
using MediaBrowser.Controller.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.RemoteUpload.Api;

[ApiController]
[Route("mediaupload")]
public class UploadController : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> OnPostUploadAsync([FromForm] IFormFile file, [FromForm] int chunkIndex, [FromForm] int totalChunks)
    {
        try {
            PluginConfiguration? config = Plugin.Instance.Configuration; 
            String uploaddir = config.uploaddir;

            if (!Directory.Exists(uploaddir))
                {
                    return BadRequest(new {message = "Directory doesn't exist!"});
                }

            if (file.Length > 0) {
                var tempFilePath = Path.Combine(uploaddir, $"{file.FileName}.part");

                using (var stream = new FileStream(tempFilePath, chunkIndex == 0 ? FileMode.Create : FileMode.Append))
                {
                    await file.CopyToAsync(stream);
                }

                if (chunkIndex + 1 == totalChunks)
                {
                    // All chunks uploaded, rename the temporary file to the original filename
                    var finalFilePath = Path.Combine(uploaddir, file.FileName);
                    if (System.IO.File.Exists(finalFilePath))
                    {
                        System.IO.File.Delete(finalFilePath);
                    }
                    System.IO.File.Move(tempFilePath, finalFilePath);
                }
            }

            return Ok(new { name = file.FileName, chunk = chunkIndex });
        }
        catch (Exception ex)
        {
            return BadRequest(new {message = "Something went wrong!"});
        }
    }
}
