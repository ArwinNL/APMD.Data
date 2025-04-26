using Dapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APMD.Data
{
    [Table("Sets")]
    public class Sets
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
        public List<Models> Models { get; set; }

        public ICollection<Photo> Photos { get; set; }
    }
}