using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("mediaupload")]
public class UploadController : ControllerBase
{
    [HttpPost("upload")]
    
    public async Task<IActionResult> OnPostUploadAsync(List<IFormFile> files)
    {
        long size = files.Sum(f => f.Length);

        foreach (var formFile in files)
        {
            if (formFile.Length > 0)
            {
                // Generate a unique file name to avoid overwriting
                var fileName = Path.GetFileName(formFile.FileName);
                var filePath = Path.Combine("/Users/grandguymc/Downloads", fileName);

                // Save the file to the specified directory
                using (var stream = System.IO.File.Create(filePath))
                {
                    await formFile.CopyToAsync(stream);
                }
            }
        }

        // Process uploaded files
        // Don't rely on or trust the FileName property without validation.

        return Ok(new { count = files.Count, size });
    }
}
