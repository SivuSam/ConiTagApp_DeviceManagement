using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceManagement.Models
{
    public class Asset
    {
        [Required]
        public int AssetId { get; set; }

        [Required]
        [MaxLength(50)]
        public string AssetTag { get; set; }

        [Required]
        [MaxLength(100)]
        public string AssetName { get; set; }

        [MaxLength(50)]
        public string Room { get; set; }

        [MaxLength(50)]
        public string Condition { get; set; }

        [MaxLength(100)]
        public string Location { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; }

        [MaxLength(1)]
        public string Verified { get; set; } // "Y" or "N"
         
        [Required]
        public DateTime SubmittedDate { get; set; }

        [NotMapped]
        public IFormFile Picture { get; set; }

        // Optional: store saved file path or name
        public string? PicturePath { get; set; }
    }

}
