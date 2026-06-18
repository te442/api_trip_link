using API_trip_link.Data.Repositories;
using API_trip_link.Models;

namespace API_trip_link.Services
{
    public class LookupService
    {
        private readonly ILookupRepository _repository;

        public LookupService(ILookupRepository repository)
        {
            _repository = repository;
        }
        //פעולות המחזירות את כל האילוצים
        public Task<List<CategoryDto>> GetCategoriesAsync() => _repository.GetCategoriesAsync();
        public Task<List<DifficultyLevelDto>> GetLevelsAsync() => _repository.GetLevelsAsync();
        public Task<List<TravelerTypeDto>> GetTravelerTypesAsync() => _repository.GetTravelerTypesAsync();
        public Task<List<FeatureTypeDto>> GetFeaturesAsync() => _repository.GetFeaturesAsync();
        public Task<List<string>> GetRegionsAsync() => _repository.GetRegionsAsync();
    }
}
