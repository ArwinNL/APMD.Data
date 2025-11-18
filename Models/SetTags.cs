using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APMD.Data.Models
{
    public class SetTags
    {
        public int PK_SET_TAG_ID { get; set; }
        public long FK_SET_ID { get; set; }
        public int FK_TAG_ID { get; set; }
    }
}
