using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APMD.Data
{
    using System.Collections.Generic;
    using System.ComponentModel;

    public class WebsiteAbbreviationSet
    {
        // A unique key for the website—could be the hostname, or its display name.
        public int WebsiteKey { get; set; }

        // Bindable list of abbreviations (e.g. "FAQ", "TOS", etc).
        // BindingList<T> is WinForms‐friendly for DataGridView/ListBox.
        public List<string> Abbreviations { get; set; }

        // Parameterless constructor needed for JSON deserialization:
        public WebsiteAbbreviationSet()
        {
            Abbreviations = new List<string>();
        }

        public WebsiteAbbreviationSet(int websiteKey, IEnumerable<string>? initialAbbrevs = null)
        {
            WebsiteKey = websiteKey;
            Abbreviations = initialAbbrevs != null
                ? new List<string>(new List<string>(initialAbbrevs))
                : new List<string>();
        }
    }
}
