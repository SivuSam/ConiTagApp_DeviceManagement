namespace DeviceManagement.Models
{
    public class AssetViewModel
    {
        public string AssetTag { get; set; }
        public string AssetName { get; set; }
        public string Room { get; set; }
        public string Condition { get; set; }
        public string Location { get; set; }
        public string Notes { get; set; }
        public string Verified { get; set; }
        public IFormFile? Picture { get; set; }
    }
}
