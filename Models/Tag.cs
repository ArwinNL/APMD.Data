using System.ComponentModel.DataAnnotations.Schema;

namespace APMD.Data
{
    public class Tag
    {
        public required int PK_TAG_ID { get; set; }
        public required string Name { get; set; }
        public int? FK_TAGGROUP_ID { get; set; }
        public int? FK_PHOTO_ID { get; set; }

        [ForeignKey("FK_TAGGROUP_ID")]
        public TagGroups? TagGroup { get; set; }

        [NotMapped]
        public Photo? TagPhoto { get; set; }
    }
}