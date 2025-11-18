using Serilog;
using System.Collections.ObjectModel;

namespace APMD.Data
{
    public class DataTagManager
    {
        private DataManager _dataManager;
        private readonly string _connectionString;
        private TagRepository _tagsRepository;
        private TagGroupsRepository _tagGroupsRepository;
        //private List<TagGroups> _tagGroups;
        public DataTagManager(DataManager dataManager)
        {
            _dataManager = dataManager;
            _connectionString = dataManager.ConnectionString;
            _tagsRepository = new TagRepository(_connectionString);
            _tagGroupsRepository = new TagGroupsRepository(_connectionString);
        }
        public ObservableCollection<TagGroups> AllGroups()
        {
            var _tagGroups = _tagGroupsRepository.GetAllFull();
            return _tagGroups;
        }

        public TagGroups? GetGroupFullById(int id)
        {
            var group = _tagGroupsRepository.GetFullById(id);
            if (group == null)
            {
                Log.Error($"TagGroup record with key ${id} hasn't been found");
            }
            return group;
        }

        public void UpdateTagGroups(TagGroups updateGroup)
        {
            _tagGroupsRepository.Update(updateGroup);
        }

        public void AllTagsForSet(Data.Set set)
        {
            set.Tags = _tagsRepository.GetAllForSet(set).ToList();
        }

        public List<Tag> AllTagsForModel(Data.Model model, bool unique = false)
        {
            var tags = _tagsRepository.GetAllForModel(model, unique);
            var tagGroups = _tagGroupsRepository.GetAll().ToList();
            foreach (var tag in tags)
            {
                tag.TagGroup = tagGroups.FirstOrDefault(g => g.PK_TAGGROUP_ID == tag.FK_TAGGROUP_ID);
            }
            return tags.ToList();
        }

        public void GetForSets(List<Set> sets)
        {
            if (sets == null) return;
            sets.ForEach( s => { AllTagsForSet(s); } );
        }

        public List<TagGroups> AllTagGroups()
        {
            return _tagGroupsRepository.GetAll().ToList();
        }

        public void DeleteTagGroups(TagGroups tagGroup)
        {
            _tagGroupsRepository.Delete(tagGroup.PK_TAGGROUP_ID);
        }

        public void InsertTagGroups(TagGroups tagGroup)
        {
            _tagGroupsRepository.Insert(tagGroup);
        }

        public void DeleteTag(Tag tag)
        {
            _tagsRepository.Delete(tag.PK_TAG_ID);
        }

        public int InsertTag(Tag tag)
        {
            return _tagsRepository.InsertTag(tag);
        }

        public async void UpdateTag(Tag tag)
        {
            await _tagsRepository.UpdateTagAsync(tag);
        }

        public Tag GetById(int pK_TAG_ID)
        {
            var result = _tagsRepository.GetById(pK_TAG_ID);
            if (result == null)
                throw new KeyNotFoundException($"Tag with ID {pK_TAG_ID} not found.");
            return result;
        }

        internal void DeleteTagsForSet(long pK_SET_ID)
        {
            var tags = _tagsRepository.GetAllForSet(pK_SET_ID).ToList();
            int count = 0;
            tags.ForEach(t => { count += _tagsRepository.Delete(t.PK_TAG_ID); });

            if (count == tags.Count) return; 

            var msgError = $"Not all tags have been deleted for set {pK_SET_ID}";
            Log.Error(msgError);
            throw new Exception(msgError);
        }

        public IEnumerable<Tag> AllTagsForGroup(TagGroups tagGroup)
        {
            return _tagsRepository.GetAllForGroup(tagGroup.PK_TAGGROUP_ID);
        }
    }


}