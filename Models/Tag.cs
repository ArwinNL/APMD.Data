namespace APMD.Data
{
    public class Tag
    {
        public int PK_TAG_ID { get; set; }
        public string Name { get; set; }
        public int? FK_TAGGROUP_ID { get; set; }
        public int? FK_PHOTO_ID { get; set; }

        public Photo? Photo { get; set; }
    }
}