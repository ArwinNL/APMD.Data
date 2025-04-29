using Dapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APMD.Data
{
    [Table("Sets")]
    public class Set
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PK_SET_ID { get; set; }

        [ForeignKeyAttribute("FK_WEBSITE_ID")]
        public int FK_WEBSITE_ID { get; set; }
        public string Title { get; set; }
        public string Set_identifier { get; set; }
        public int? FK_PHOTO_ID { get; set; }
        public bool Archived { get; set; }
        public bool Tagged { get; set; }

        [ForeignKey("FK_PHOTO_ID")]
        public Photo? SetPhoto{ get; set; }

        [ForeignKey("FK_SET_ID")]
        public List<Model> Models { get; set; }

        [NotMapped]
        public List<Photo> Photos { get; set; } = new List<Photo>();

        [NotMapped]
        public List<Tag> Tags { get; set; } = new List<Tag>();
    }
}