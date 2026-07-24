using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.RemoteUpload.Api;

[ApiController]
[Authorize(Policy = Policies.RequiresElevation)]
[Route("mediaupload")]
public class UploadController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static ConcurrentDictionary<string, (CancellationTokenSource cts, string filePath, string fileName, long fileSize)> _uploadTasks = new ConcurrentDictionary<string, (CancellationTokenSource, string, string, long)>();
    PluginConfiguration? config;
    private string uploaddir;

    public UploadController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            config = Plugin.Instance.Configuration;
            uploaddir = config.uploaddir;

            if (!Directory.Exists(uploaddir))
            {
                Directory.CreateDirectory(uploaddir);
            }
        }

    [HttpPost("upload")]
    public async Task<IActionResult> OnPostUploadAsync([FromForm] IFormFile file, [FromForm] int chunkIndex, [FromForm] int totalChunks)
    {
        try
        {
            string sanitizedFileName = Path.GetFileName(file.FileName);

            if (file.Length > 0) {
                string tempFilePath = Path.Combine(uploaddir, $"{sanitizedFileName}.part");

                using (var stream = new FileStream(tempFilePath, chunkIndex == 0 ? FileMode.Create : FileMode.Append))
                {
                    await file.CopyToAsync(stream);
                }

                if (chunkIndex + 1 == totalChunks)
                {
                    // All chunks uploaded, rename the temporary file to the original filename
                    var finalFilePath = Path.Combine(uploaddir, sanitizedFileName);
                    if (System.IO.File.Exists(finalFilePath))
                    {
                        System.IO.File.Delete(finalFilePath);
                    }
                    System.IO.File.Move(tempFilePath, finalFilePath);
                }
            }

            return Ok(new { name = sanitizedFileName, chunk = chunkIndex });
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

        if (!IsDirectoryWritable(uploaddir)) {
            return BadRequest(new { message = "No permission to write in directory" });
        }

        string cancellationKey = Guid.NewGuid().ToString();
        var cts = new CancellationTokenSource();

        string? filename = null;
        string? destinationPath = null;
        long filesize = 0;

        string ex = "";

        var task = Task.Run(async () => 
        {
            try
            {
                using (var client = _httpClientFactory.CreateClient())
                {
                    using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cts.Token))
                    {
                        response.EnsureSuccessStatusCode(); 

                        filename = GetFileName(response, url);
                        filesize = GetFileSize(response);

                        destinationPath = Path.Combine(uploaddir, filename);

                        _uploadTasks.TryAdd(cancellationKey, (cts, destinationPath, filename, filesize)); // Add this task to uploadTasks

                        using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                        using (FileStream fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await contentStream.CopyToAsync(fileStream, cts.Token);
                        }
                        
                    }
                }
            }
            catch (Exception e)
            {
                ex = e.Message;
            }
            finally {
                _uploadTasks.TryRemove(cancellationKey, out _); // remove the task from uploadTasks when download is finished
            }
        }, cts.Token);

        await Task.Delay(500); // Wait 500ms until download starts

        if (!_uploadTasks.ContainsKey(cancellationKey)) // If download hasn't started, wait 3 seconds more
        {
            await Task.Delay(3000);
            if (!_uploadTasks.ContainsKey(cancellationKey)) // If download has started, there should be a cancellation key
            {
                return BadRequest(new { message = ex });
            }
        }

        return Ok(new { message = "Download started" });
    }

    [HttpPost("upload_bulk_url")]
    public async Task<IActionResult> URLOnPostBulkUploadAsync([FromForm] List<string> urls) {
        for (int i = 0; i<urls.Count; i++) {
            if (string.IsNullOrEmpty(urls[i]))
            {
                return BadRequest(new { message = "URL is required" });
            }
        }

        if (!IsDirectoryWritable(uploaddir)) {
            return BadRequest(new { message = "No permission to write in directory" });
        }

        string cancellationKey = "";
        var cts = new CancellationTokenSource();

        string? filename = null;
        string? destinationPath = null;
        string ex = "";
        long filesize = 0;

        var task = Task.Run(async () => 
        {
            foreach (var url in urls) {
                cancellationKey = Guid.NewGuid().ToString();
                try
                {
                    using (var client = _httpClientFactory.CreateClient())
                    {
                        using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cts.Token))
                        {
                            response.EnsureSuccessStatusCode(); 

                            filename = GetFileName(response, url);
                            filesize = GetFileSize(response);

                            destinationPath = Path.Combine(uploaddir, filename);

                            _uploadTasks.TryAdd(cancellationKey, (cts, destinationPath, filename, filesize)); // Add this task to uploadTasks

                            using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                            using (FileStream fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                await contentStream.CopyToAsync(fileStream, cts.Token);
                            }
                            
                        }
                    }
                }
                catch (Exception e)
                { 
                    ex=e.Message;
                }
                finally {
                    _uploadTasks.TryRemove(cancellationKey, out _); // remove the task from uploadTasks when download is finished
                }
            }
        }, cts.Token);

        await Task.Delay(500); // Wait 500ms until download starts

        if (!_uploadTasks.ContainsKey(cancellationKey)) // If download hasn't started, wait 3 seconds more
        {
            await Task.Delay(3000);
            if (!_uploadTasks.ContainsKey(cancellationKey)) // If download has started, there should be a cancellation key
            {
                return BadRequest(new { message = ex });
            }
        }

        return Ok(new { message = "Download started" });
    }

    [HttpPost]
    [Route("download")]
    public async Task<IActionResult> DownloadFile([FromForm] string path)
    {
        if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
        {
            return BadRequest(new { message = "File not found!" });
        }

        var memory = new MemoryStream();
        using (var stream = new FileStream(path, FileMode.Open))
        {
            await stream.CopyToAsync(memory);
        }
        memory.Position = 0;

        var contentType = "application/octet-stream";
        var fileName = Path.GetFileName(path);

        return File(memory, contentType, fileName);
    }

    [HttpPost]
    [Route("directory")]
    public IActionResult ListFolderContent([FromForm] string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                return BadRequest(new { message = "Folder not found!" });
            }

            List<string> files = new List<string>();

            foreach (string folder in Directory.GetDirectories(path)) {
                files.Add(Path.GetFileName(folder)+"/");
            }
            foreach (string file in Directory.GetFiles(path)) {
                files.Add(Path.GetFileName(file));
            }

            return Ok(files);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex });
        }
    }

    // Cancel a URL download
    [HttpPost("upload_cancel")]
    public async Task<IActionResult> CancelUpload([FromForm] string cancellationKey)
    {
        try {
            if (_uploadTasks.TryRemove(cancellationKey, out var taskInfo))
            {
                // This cancels the task, the file will also be deleted as in task.ContinueWith
                taskInfo.cts.Cancel();

                await Task.Delay(1000); // Wait one second to make sure, that the task has finished
                // Delete the file
                if (System.IO.File.Exists(taskInfo.filePath)) {
                    System.IO.File.Delete(taskInfo.filePath);
                }

                return Ok(new { message = "Upload cancelled", filename = taskInfo.fileName});
            }
            else
            {
                return BadRequest(new { message = "Task doesn't exist" });
            }
        }
        catch (Exception ex) {
            return BadRequest(new { message = ex.Message });
        }
        
    }

    // Gets all running tasks
    [HttpGet("get_tasks")]
    public IActionResult GetUploadTasks()
    {
        var tasks = _uploadTasks.Select(task => new
        { 
            Key = task.Key,
            FileName = task.Value.fileName, 
            FileSize = task.Value.fileSize, 
            FileSizeNow = new FileInfo(task.Value.filePath).Length
        });

        return Ok(tasks);
    }

    private string GetFileName(HttpResponseMessage response, string url) {
        string? filename = null;
        var contentDisposition = response.Content.Headers.ContentDisposition;

        if (contentDisposition != null)
        {
            filename = !string.IsNullOrEmpty(contentDisposition.FileNameStar)
                ? contentDisposition.FileNameStar.Trim('"')
                : contentDisposition.FileName?.Trim('"');
        }

        if (string.IsNullOrEmpty(filename))
        {
            var uri = new Uri(url);
            filename = Uri.UnescapeDataString(Path.GetFileName(uri.AbsolutePath));
        }

        filename = Path.GetFileName(filename);

        return string.IsNullOrEmpty(filename)
            ? "filewithoutname.mp4"
            : filename;
    }

    private long GetFileSize(HttpResponseMessage response) {
        try {
            if (response.Content.Headers.TryGetValues("Content-Length", out var values))
            {
                var contentLength = values.FirstOrDefault();
                if (long.TryParse(contentLength, out var fileSize))
                {
                    // We have a fileSize in bytes
                    return fileSize;
                }
                else
                {
                    // fileSize is not a long, we return 0
                    return 0;
                }
            }
            else
            {
                // No filesize in headers
                return 0;
            }
        }
        catch (Exception) {
            // Some error, we return 0
            return 0;
        }
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
