using DeviceManagement.Data;
using DeviceManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManagement.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class MobileApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public MobileApiController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpPost("assets")]
        public async Task<IActionResult> Create([FromForm] AssetDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string? imageName = null;
            if (model.Picture != null && model.Picture.Length > 0)
            {
                
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                imageName = Guid.NewGuid() + Path.GetExtension(model.Picture.FileName);
                var filePath = Path.Combine(uploadsFolder, imageName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Picture.CopyToAsync(stream);
                }
            }

            var asset = new Asset
            {
                AssetTag = model.AssetTag,
                AssetName = model.AssetName,
                Room = model.Room,
                Condition = model.Condition,
                Location = model.Location,
                Notes = model.Notes,
                Verified = "N",
                
                PicturePath = imageName != null ? "/images/" + imageName : null,
                SubmittedDate = DateTime.Now
            };

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Asset saved successfully" });
        }
    }
}
