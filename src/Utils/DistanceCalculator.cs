using System;
using AasDemoapp.Database.Model;

namespace AasDemoapp.Utils
{
    /// <summary>
    /// Utility class for calculating distances between geographical coordinates.
    /// </summary>
    public static class DistanceCalculator
    {
        /// <summary>
        /// Earth's radius in kilometers.
        /// </summary>
        private const double EarthRadiusKm = 6371.0;

        /// <summary>
        /// Calculates the great-circle distance between two addresses using the Haversine formula.
        /// </summary>
        /// <param name="address1">First address with coordinates (Lat, Long)</param>
        /// <param name="address2">Second address with coordinates (Lat, Long)</param>
        /// <returns>Distance in kilometers, or null if either address lacks valid coordinates</returns>
        public static double? CalculateDistanceKm(Address? address1, Address? address2)
        {
            if (
                address1?.Lat == null
                || address1?.Long == null
                || address2?.Lat == null
                || address2?.Long == null
            )
            {
                return null;
            }

            return CalculateDistanceKm(
                address1.Lat.Value,
                address1.Long.Value,
                address2.Lat.Value,
                address2.Long.Value
            );
        }

        /// <summary>
        /// Calculates the great-circle distance between two points on Earth using the Haversine formula.
        /// </summary>
        /// <param name="lat1">Latitude of the first point in decimal degrees</param>
        /// <param name="lon1">Longitude of the first point in decimal degrees</param>
        /// <param name="lat2">Latitude of the second point in decimal degrees</param>
        /// <param name="lon2">Longitude of the second point in decimal degrees</param>
        /// <returns>Distance in kilometers</returns>
        public static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            // Convert degrees to radians
            var lat1Rad = DegreesToRadians(lat1);
            var lon1Rad = DegreesToRadians(lon1);
            var lat2Rad = DegreesToRadians(lat2);
            var lon2Rad = DegreesToRadians(lon2);

            // Calculate differences
            var deltaLat = lat2Rad - lat1Rad;
            var deltaLon = lon2Rad - lon1Rad;

            // Haversine formula
            var a =
                Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2)
                + Math.Cos(lat1Rad)
                    * Math.Cos(lat2Rad)
                    * Math.Sin(deltaLon / 2)
                    * Math.Sin(deltaLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            // Calculate distance
            return EarthRadiusKm * c;
        }

        /// <summary>
        /// Validates if an address has valid geographical coordinates.
        /// </summary>
        /// <param name="address">Address to validate</param>
        /// <returns>True if the address has valid latitude and longitude</returns>
        public static bool HasValidCoordinates(Address? address)
        {
            if (address?.Lat == null || address?.Long == null)
                return false;

            var lat = address.Lat.Value;
            var lon = address.Long.Value;

            return lat >= -90 && lat <= 90 && lon >= -180 && lon <= 180;
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="degrees">Angle in degrees</param>
        /// <returns>Angle in radians</returns>
        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}
