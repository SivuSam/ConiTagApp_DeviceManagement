using DeviceManagement.Data;
using DeviceManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class AssetController : Controller
{
    private readonly IAssetService _assetService;
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public AssetController(IAssetService assetService, IWebHostEnvironment environment, ApplicationDbContext context)
    {
        _assetService = assetService;
        _environment = environment;
        _context = context;
    }

    // GET: /Asset
    public async Task<IActionResult> Index(string searchTerm, string conditionFilter)
    {
        var assets = await _assetService.GetAllAsync();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            assets = assets.Where(a =>
                (a.AssetTag != null && a.AssetTag.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (a.AssetName != null && a.AssetName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (a.Room != null && a.Room.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (a.Location != null && a.Location.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        if (!string.IsNullOrEmpty(conditionFilter))
        {
            assets = assets.Where(a =>
                a.Condition != null && a.Condition.Equals(conditionFilter, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        ViewData["CurrentSearch"] = searchTerm;
        ViewData["CurrentCondition"] = conditionFilter;

        return View(assets);
    }




    public async Task<IActionResult> Details(int id)
    {
        var asset = await _assetService.GetByIdAsync(id);
        if (asset == null)
            return NotFound();

        return View(asset);
    }





    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssetViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string? imageName = null;
            if (model.Picture != null && model.Picture.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
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
                Verified = model.Verified,
                PicturePath = imageName,
                SubmittedDate = DateTime.Now
            };

            _context.Assets.Add(asset);
            _context.SaveChanges();

            return RedirectToAction("Index", "Asset");


        }
        return View(model);
    }



    // GET: /Asset/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var asset = await _assetService.GetByIdAsync(id);
        if (asset == null)
            return NotFound();

        return View(asset);
    }

    // POST: /Asset/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Asset asset)
    {
        if (id != asset.AssetId)
            return BadRequest();


        var updatedAsset = await _assetService.UpdateAsync(asset);
        if (updatedAsset == null)
            return NotFound();

        return RedirectToAction(nameof(Index));
    }

    // GET: /Asset/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var asset = await _assetService.GetByIdAsync(id);
        if (asset == null)
            return NotFound();

        return View(asset);
    }

    // POST: /Asset/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        bool deleted = await _assetService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return RedirectToAction(nameof(Index));
    }
}