using API_trip_link.Models;

namespace API_trip_link.Data.Repositories
{
    public interface ILookupRepository
    {
        Task<List<CategoryDto>> GetCategoriesAsync();
        Task<List<DifficultyLevelDto>> GetLevelsAsync();
        Task<List<TravelerTypeDto>> GetTravelerTypesAsync();
        Task<List<FeatureTypeDto>> GetFeaturesAsync();
        Task<List<string>> GetRegionsAsync();
    }
}
