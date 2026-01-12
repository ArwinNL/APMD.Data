using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APMD.Data
{
    public class DataManager
    {
        private static readonly Serilog.ILogger Log = Serilog.Log.ForContext<DataManager>();

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
        public Model Model { get; set;}
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