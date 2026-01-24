using Dapper;
using MySqlConnector;

namespace APMD.Data
{
    public class DataManager
    {
        private const string sqlFilterSets = @"SELECT DISTINCT s.* FROM Sets s";
        private const string sqlFilterSetCount = @"SELECT DISTINCT Count(s.PK_SET_ID) FROM Sets s";
        private const string sqlJoinSetTags = @" LEFT JOIN SetTags st ON st.FK_SET_ID = s.PK_SET_ID";
        private const string sqlJoinSetModels = @" LEFT JOIN SetModels sm ON sm.FK_SET_ID = s.PK_SET_ID";
        private const string sqlJoinModels = @" LEFT JOIN Models m ON sm.FK_MODEL_ID = m.PK_MODEL_ID";

        private static readonly Serilog.ILogger Log = Serilog.Log.ForContext<DataManager>();

        private static readonly Dictionary<String, String> Filters = new()
        {
            { "tag", "st.FK_TAG_ID IN @TagIds" },
            { "website",  "s.FK_WEBSITE_ID IN @WebsiteIds" },
            { "model", "m.PK_MODEL_ID IN @ModelIds"    },
            { "rank", "m.Rank IN @RankIds"    }
        };

        private readonly string _connectionString;
        private readonly DataTagManager _tag;
        private readonly DataPhotoManager _photo;
        private readonly DataModelManager _model;
        private readonly DataSetManager _set;
        private readonly DataServerShareManager _serverShare;
        private readonly DataWebsiteManager _website;
        private DataImportManager _import;

        public ClipboardData Clipboard { get; set; }

        internal string ConnectionString => _connectionString;
        public DataTagManager Tag => _tag;
        public DataPhotoManager Photo => _photo;
        public DataModelManager Model => _model;
        public DataSetManager Set => _set;
        public DataServerShareManager ServerShare => _serverShare;
        public DataWebsiteManager Website => _website;

        public DataImportManager Import => _import;

        public delegate void ModelChangeHandler(object sender, EventArgsModel e);
        public event ModelChangeHandler ModelChange;

        public delegate void SetChangeHandler(object sender, EventArgsSet e);
        public event SetChangeHandler SetChange;

        public DataManager(string connectionString)
        {
            Clipboard = new ClipboardData();

            _connectionString = connectionString;

            _serverShare = new DataServerShareManager(this);

            _tag = new DataTagManager(this);
            _photo = new DataPhotoManager(this);
            _model = new DataModelManager(this);
            _set = new DataSetManager(this);
            _website = new DataWebsiteManager(this);
            _import = new DataImportManager(this);
            ModelChange += (sender, e) => Log.Information($"Model change event triggered: {e.Action} for model {e.Model.Name}");
            SetChange += (sender, e) => Log.Information($"Set change event triggered: {e.Action} for set {e.Set.Title}");
        }

        internal void DoModelChange(object sender, EventArgsModel e)
        {
            ModelChange?.Invoke(sender, e);
        }
        internal void DoSetChange(object sender, EventArgsSet e)
        {
            SetChange?.Invoke(sender, e);
        }

        internal void BeginTransaction()
        {
        }

        internal void CommitTransaction()
        {
        }

        internal void RollbackTransaction()
        {
        }


        public (string sqlSelect, string sqlWhere, object paramsQuery) FilterParameters(Filter currentFilter, string sqlSelect)
        {
            var sqlWhere = " WHERE ";
            var values = new DynamicParameters();

            if (currentFilter.Tags.Count > 0)
            {
                sqlSelect += sqlJoinSetTags;
                sqlWhere += Filters["tag"];
                values.AddDynamicParams(new { TagIds = currentFilter.TagIds() });
            }
            if (currentFilter.Websites.Count > 0)
            {
                //sqlSelect += Filters["website"];
                sqlWhere += currentFilter.Tags.Count > 0 ? (currentFilter.AndWebsite ? " AND " : " OR ") : "";
                sqlWhere += Filters["website"];
                values.AddDynamicParams(new { WebsiteIds = currentFilter.WebsiteIds() });
            }
            if (currentFilter.Models.Count > 0)
            {
                sqlSelect += sqlJoinSetModels;
                sqlWhere += (currentFilter.Tags.Count > 0 || currentFilter.Websites.Count > 0) ? (currentFilter.AndModel ? " AND " : " OR ") : "";
                sqlWhere += Filters["model"];
                values.AddDynamicParams(new { ModelIds = currentFilter.ModelIds() });
            }
            if (currentFilter.Ranks.Count > 0)
            {
                if (currentFilter.Models.Count < 1)
                    sqlSelect += sqlJoinSetModels;
                sqlSelect += sqlJoinModels;
                sqlWhere += (currentFilter.Tags.Count > 0 || currentFilter.Websites.Count > 0 || currentFilter.Models.Count > 0) ? " AND " : "";
                sqlWhere += Filters["rank"];
                values.AddDynamicParams(new { RankIds = currentFilter.RankIds() });
            }
            return (sqlSelect, sqlWhere, values);
        }
        public List<Set> FilterSets(Filter currentFilter)
        {
            var sqlCount = sqlFilterSets;
            var result = FilterParameters(currentFilter, sqlCount);

            var _db = new MySqlConnection(_connectionString);
            _db.Open();
            var resultQuery = _db.Query<Set>(result.sqlSelect + result.sqlWhere, result.paramsQuery);
            return resultQuery.ToList();

        }

        public string FilterSetsCount(Filter currentFilter)
        {
            var sqlCount = sqlFilterSetCount;
            var result = FilterParameters(currentFilter, sqlCount);

            var _db = new MySqlConnection(_connectionString);
            _db.Open();

            var resultQuery = _db.ExecuteScalar(result.sqlSelect + result.sqlWhere, result.paramsQuery);
            return resultQuery?.ToString() ?? "0";
        }

    }

    public enum DataAction
    {
        Insert,
        Update,
        Delete
    }

    public class EventArgsModel
    {
        public DataAction Action { get; set; }
        public Model Model { get; set; }
        public EventArgsModel(Model model, DataAction action)
        {
            Model = model;
            Action = action;
        }
    }

    public class EventArgsSet
    {
        public DataAction Action { get; set; }
        public Set Set { get; set; }
        public EventArgsSet(Set set, DataAction action)
        {
            Set = set;
            Action = action;
        }
    }

    public class EventArgsSets
    {
        public DataAction Action { get; set; }
        public List<Set> Sets { get; set; }
        public EventArgsSets(List<Set> sets)
        {
            Sets = sets;
        }
    }

    public sealed class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;

        public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
            => (Items, Page, PageSize, TotalCount) = (items, page, pageSize, totalCount);
    }

    public class ClipboardData
    {
        public Model? Model { get; set; }
        public Set? Set { get; set; }
    }
}