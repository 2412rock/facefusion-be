
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace FacefusionBE.Controllers
{
    [Route("api/")]
    [ApiController]
    public class FaceSwapController : ControllerBase
    {
        private readonly string pythonApiUrl = "http://localhost:5000/swap"; // Python API endpoint

        [HttpPost("swap-face")]
        public async Task<IActionResult> SwapFace([FromForm] IFormFile sourceImage, [FromForm] IFormFile targetVideo)
        {
            if (sourceImage == null || targetVideo == null)
            {
                return BadRequest("Both source image and target video are required.");
            }

            try
            {
                // Save source image to memory
                var sourceImageStream = new MemoryStream();
                await sourceImage.CopyToAsync(sourceImageStream);
                sourceImageStream.Position = 0; // Important!

                // Save target video to memory
                var targetVideoStream = new MemoryStream();
                await targetVideo.CopyToAsync(targetVideoStream);
                targetVideoStream.Position = 0; // Important!

                // Analyze the video duration (pass MemoryStream to your method)
                double duration = await GetVideoDurationAsync(targetVideoStream);
                Console.WriteLine($"Video Duration: {duration} seconds");

                // Reset the position again before sending to Python
                targetVideoStream.Position = 0;

                // Now build the request to the Python API
                using var client = new HttpClient();
                using var content = new MultipartFormDataContent
        {
            { new StreamContent(sourceImageStream), "source_image", "source.jpg" },
            { new StreamContent(targetVideoStream), "target_video", "target.mp4" },
            { new StringContent("inswapper_128"), "model" }
        };

                var response = await client.PostAsync(pythonApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsByteArrayAsync();
                    return File(responseData, "video/mp4", "output.mp4");
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Face swap failed.");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        private async Task<double> GetVideoDurationAsync(Stream videoStream)
        {
            var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");

            try
            {
                // Write stream to temporary file
                using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await videoStream.CopyToAsync(fs);
                    await fs.FlushAsync();
                }

                // Analyze video
                var mediaInfo = await FFmpeg.GetMediaInfo(tempFilePath);
                return mediaInfo.Duration.TotalSeconds;
            }
            finally
            {
                // Always try to delete the temp file
                if (System.IO.File.Exists(tempFilePath))
                {
                    try
                    {
                        System.IO.File.Delete(tempFilePath);
                    }
                    catch
                    {
                        // Ignore any delete errors
                    }
                }
            }
        }
    }
}
