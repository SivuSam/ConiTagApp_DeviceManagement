using DeviceManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IAssetService
{
    Task<Asset> CreateAsync(Asset asset);
    Task<Asset> UpdateAsync(Asset asset);
    Task<bool> DeleteAsync(int assetId);
    Task<Asset> GetByIdAsync(int assetId);
    Task<IEnumerable<Asset>> GetAllAsync();
}
