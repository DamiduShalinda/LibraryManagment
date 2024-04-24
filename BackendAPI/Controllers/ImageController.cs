using HeyRed.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {

        private readonly string CoverImagesFolder = "images/bookcovers";


        [HttpGet("{ImageName}")]
        public async Task<IActionResult> GetImage(string ImageName)
        {
            try
            {
                if (string.IsNullOrEmpty(ImageName))
                {
                    return BadRequest("Image name cannot be null or empty.");
                }
                var filePath = Path.Combine(CoverImagesFolder, ImageName);
                if (System.IO.File.Exists(filePath))
                {
                    var fileBytes = System.IO.File.ReadAllBytes(filePath);
                    var fileType = MimeTypesMap.GetMimeType(filePath);
                    Response.Headers.Append("Content-Disposition", "inline; filename=" + ImageName);
                    return File(fileBytes, fileType);
                }
                else
                {
                    return NotFound();
                }
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
