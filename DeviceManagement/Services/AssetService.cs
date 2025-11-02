using DeviceManagement.Data;
using DeviceManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; // Add this using directive
using System.Security.Claims; // Add this using directive

public class AssetService : IAssetService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor; // Add this line

    public AssetService(IWebHostEnvironment webHostEnvironment, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _webHostEnvironment = webHostEnvironment;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public async Task<Asset> CreateAsync(Asset asset)
    {
        if (asset.Picture != null && asset.Picture.Length > 0)
        {
            asset.PicturePath = await SavePictureAsync(asset.Picture);
        }

        asset.SubmittedDate = DateTime.Now; // Set the date on creation

        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();

        return asset;
    }

    public async Task<Asset> UpdateAsync(Asset asset)
    {
        var existingAsset = await _context.Assets.FindAsync(asset.AssetId);
        if (existingAsset == null)
            return null;

        existingAsset.AssetTag = asset.AssetTag;
        existingAsset.AssetName = asset.AssetName;
        existingAsset.Room = asset.Room;
        existingAsset.Condition = asset.Condition;
        existingAsset.Location = asset.Location;
        existingAsset.Notes = asset.Notes;
        existingAsset.Verified = asset.Verified;
        existingAsset.SubmittedDate = DateTime.Now;

        if (asset.Picture != null && asset.Picture.Length > 0)
        {
            // Delete old picture
            if (!string.IsNullOrEmpty(existingAsset.PicturePath))
            {
                var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, existingAsset.PicturePath.TrimStart('/'));
                if (File.Exists(oldPath)) File.Delete(oldPath);
            }

            existingAsset.PicturePath = await SavePictureAsync(asset.Picture);
        }

        _context.Assets.Update(existingAsset);
        await _context.SaveChangesAsync();

        return existingAsset;
    }

    // (rest of the methods remain unchanged)
    public async Task<bool> DeleteAsync(int assetId)
    {
        var asset = await _context.Assets.FindAsync(assetId);
        if (asset == null)
            return false;

        if (!string.IsNullOrEmpty(asset.PicturePath))
        {
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, asset.PicturePath.TrimStart('/'));
            if (File.Exists(filePath)) File.Delete(filePath);
        }

        _context.Assets.Remove(asset);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<Asset> GetByIdAsync(int assetId)
    {
        return await _context.Assets.FindAsync(assetId);
    }

    public async Task<IEnumerable<Asset>> GetAllAsync()
    {
        var currentUserId = GetCurrentUserId();

        var userAssets = await _context.Assets
            .ToListAsync();

        return userAssets;
    }

    private async Task<string> SavePictureAsync(IFormFile picture)
    {
        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(picture.FileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await picture.CopyToAsync(fileStream);
        }

        return "/images/" + uniqueFileName;
    }
}
