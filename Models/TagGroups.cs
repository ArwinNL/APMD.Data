namespace APMD.Data
{
    public class TagGroups
    {
        public int PK_TAGGROUP_ID { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public int? FK_PHOTO_ID { get; set; }
    }
}