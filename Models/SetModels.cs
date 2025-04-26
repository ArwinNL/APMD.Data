using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APMD.Data
{
    [Table("SetModels")]
    public class SetModels
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PK_SET_MODELS_ID { get; set; }
        public int FK_SET_ID { get; set; }
        public int FK_MODEL_ID { get; set; }
    }
}
