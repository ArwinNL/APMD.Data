namespace APMD.Data
{
    public class Filter
    {
        public List<Tag> Tags { get; set; } = new List<Tag>();
        public List<Model> Models { get; set; } = new List<Model>();
        public List<Websites> Websites { get; set; } = new List<Websites>();
        public List<int> Ranks { get; set; } = new List<int>();

        public bool AndModel;

        public bool AndTag;

        public bool AndWebsite;

        public bool OneSetPerModel;

        public Filter()
        {
            AndModel = false;
            AndTag = false;
            AndWebsite = false;
        }
        public int[] RankIds()
        {
            return Ranks.Select(r => r).Distinct().ToArray();
        }
        public int[] TagIds()
        {
            return Tags.Select(t => t.PK_TAG_ID).Distinct().ToArray();
        }
        public long[] ModelIds()
        {
            return Models.Select(m => m.PK_MODEL_ID).Distinct().ToArray();
        }
        public int[] WebsiteIds()
        {
            return Websites.Select(w => w.PK_WEBSITE_ID).Distinct().ToArray();
        }


    }
}
