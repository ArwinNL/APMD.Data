using System.ComponentModel.DataAnnotations.Schema;

namespace APMD.Data
{
    public class Websites
    {
        public int PK_WEBSITE_ID { get; set; }
        public required string Name { get; set; }
        public string? Url { get; set; }
        public int? FK_PHOTO_ID { get; set; }

        [ForeignKey("FK_PHOTO_ID")]
        public Photo? WebsitePhoto { get; set; }

        public int TheNudeId { get; set; }
    }
}