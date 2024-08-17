using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
    private readonly IHttpClientFactory _httpClientFactory;

    public UploadController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

    [HttpPost("upload")]
    public async Task<IActionResult> OnPostUploadAsync([FromForm] IFormFile file, [FromForm] int chunkIndex, [FromForm] int totalChunks)
    {
        try
        {
            PluginConfiguration? config = Plugin.Instance.Configuration;
            string uploaddir = config.uploaddir;

            if (!Directory.Exists(uploaddir))
                {
                    return BadRequest(new { message = "Directory doesn't exist!" });
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
        catch (Exception ex) // Catch any other exceptions
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    [HttpPost("upload_url")]
    public async Task<IActionResult> URLOnPostUploadAsync([FromForm] string url) {
        if (string.IsNullOrEmpty(url))
        {
            return BadRequest(new { message = "URL is required" });
        }

        PluginConfiguration? config = Plugin.Instance.Configuration;
        string uploaddir = config.uploaddir;

        if (!Directory.Exists(uploaddir))
        {
            return BadRequest(new { message = "Directory doesn't exist" });
        }

        if (!IsDirectoryWritable(uploaddir)) {
            return BadRequest(new { message = "No permission to write in directory" });
        }

        Task.Run(async () => 
        {
            try
            {
                using (var client = _httpClientFactory.CreateClient())
                {
                    using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                        {
                            response.EnsureSuccessStatusCode(); 

                            string filename = GetFileName(response, url);

                            var destinationPath = Path.Combine(uploaddir, filename);

                            using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                            using (FileStream fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                await contentStream.CopyToAsync(fileStream);
                            }
                            
                        }
                }
            }
            catch (Exception ex)
            {
                
            }
        });

        return Ok(new { message = "Success" });
    }

    private string GetFileName(HttpResponseMessage response, string url) {
        string filename = null;

        if (response.Content.Headers.ContentDisposition != null) {
            var contentDisposition = response.Content.Headers.ContentDisposition;
            if (!string.IsNullOrEmpty(contentDisposition.FileName)) {
                filename = contentDisposition.FileName.Trim('\"');
            }
        }

        if (filename == null) {
            Uri uri = new Uri(url);
            filename = Path.GetFileName(uri.AbsolutePath);
        }

        return filename ?? "filewithoutname.dat";
    }

    private bool IsDirectoryWritable(string dirPath, bool throwIfFails = false)
    {
        try
        {
            using (FileStream fs = new FileStream(Path.Combine(dirPath, Path.GetRandomFileName()), FileMode.Create, FileAccess.Write, FileShare.None, 1, FileOptions.DeleteOnClose))
            { }
            return true;
        }
        catch
        {
            if (throwIfFails)
                throw;
            else
                return false;
        }
    }
}
