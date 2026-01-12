using Dapper;
using System.ComponentModel.DataAnnotations.Schema;
using Dapper.Contrib.Extensions;

namespace APMD.Data
{
    [Dapper.Contrib.Extensions.Table("Sets")]
    public class Set
    {
        private Websites? _website;
        private Photo _setPhoto;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PK_SET_ID { get; set; }

        [ForeignKeyAttribute("FK_WEBSITE_ID")]
        public int FK_WEBSITE_ID { get; set; }
        public string Title { get; set; }
        public long? FK_PHOTO_ID { get; set; }
        public bool Archived { get; set; }
        public bool Tagged { get; set; }
        public DateTime? PublishedAt { get; set; }
        public bool AllPhotosStored { get; set; }


        [ForeignKey("FK_PHOTO_ID")]
        public Photo? SetPhoto{ 
            get
            {
                if (_setPhoto == null && Photos != null)
                {
                    _setPhoto = Photos.FirstOrDefault(p => p.PK_PHOTO_ID == FK_PHOTO_ID);
                }
                if (_setPhoto != null &&  Archived)
                    _setPhoto.Archived = true;
                return _setPhoto;
            }
            set => _setPhoto = value; }

        [ForeignKey("FK_SET_ID")]
        public List<Model> Models { get; set; } = new List<Model>();

        [NotMapped]
        public List<Photo> Photos { get; set; } = new List<Photo>();

        [NotMapped]
        public List<Tag> Tags { get; set; } = new List<Tag>();


        [NotMapped]
        public Websites? Website 
        { 
            get
            {
                return _website;
            }
            set
            {
                _website = value;
                if (value != null)
                {
                    FK_WEBSITE_ID = value.PK_WEBSITE_ID;
                }
            } 
        }

        public Set()
        {
            Title = string.Empty;
            Archived = false;
            Tagged = false;
        }
    }
}