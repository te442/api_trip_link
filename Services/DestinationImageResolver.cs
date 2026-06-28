using API_trip_link.Models;

namespace API_trip_link.Services
{
    internal static class DestinationImageResolver
    //פעולה ששולחת תמונות יעד לפי קטגוריה
    {
        public static string Resolve(int desId, int? primaryCategoryId)
        {
            int cat = primaryCategoryId is >= 1 and <= 7 ? primaryCategoryId.Value : 0;
            if (cat == 0)
                cat = (desId % 7) + 1;

            return $"/images/categories/{cat}.svg";
        }

        public static void ApplyDisplayImages(IEnumerable<DestinationDto> destinations)
        {
            foreach (var dest in destinations)
                dest.ImageUrl = Resolve(dest.DesId, dest.PrimaryCategoryId);
        }
    }
}
