using System.ComponentModel.DataAnnotations.Schema;



public class AssetDTO
{
    public string AssetTag { get; set; }
    public string AssetName { get; set; }
    public string Room { get; set; }
    public string Condition { get; set; }
    public string Location { get; set; }
    public string Notes { get; set; }
    public IFormFile? Picture { get; set; }
}