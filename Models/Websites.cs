namespace APMD.Data
{
    public class Websites
    {
        public int PK_WEBSITE_ID { get; set; }
        public required string Name { get; set; }
        public string? Url { get; set; }
        public int? FK_PHOTO_ID { get; set; }
    }
}