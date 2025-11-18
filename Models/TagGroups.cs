using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace APMD.Data
{
    public class TagGroups
    {
        public required int PK_TAGGROUP_ID { get; set; }
        public required string Name { get; set; }
        public int? FK_PHOTO_ID { get; set; }
        public int? FK_MAIN_TAGGROUP_ID { get; set; }

        public string? Color { get; set; }

        [NotMapped]
        public ObservableCollection<Tag> Tags { get; set; } = new();

        [NotMapped]
        public TagGroups? MainTagGroup { get; set; }

        [NotMapped]
        public Photo? Photo { get; set; }
    }
}