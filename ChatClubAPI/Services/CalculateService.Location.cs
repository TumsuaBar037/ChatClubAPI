using ChatClubAPI.Data;

namespace ChatClubAPI.Services
{
    public partial class CalculateService
    {
        public async Task<Location?> CheckLocation(double myLat, double myLon)
        {
            double radiusMeters = 10;

            var locationlst = await _dbService.GetLocation();

            foreach ( var item in locationlst )
            {
                if (double.TryParse(item.Latitude, out var lat) && double.TryParse(item.Longtitude, out var lon))
                {
                    double distance = GetDistanceInMeters(myLat, myLon, lat, lon);
                    if (distance <= radiusMeters)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        public double GetDistanceInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // Earth radius in meters
            double dLat = (lat2 - lat1) * Math.PI / 180.0;
            double dLon = (lon2 - lon1) * Math.PI / 180.0;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}
