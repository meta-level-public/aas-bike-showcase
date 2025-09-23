using System;
using AasDemoapp.Database.Model;
using AasDemoapp.Utils;
using Xunit;

namespace AasDemoapp.Tests.Utils
{
    public class DistanceCalculatorTests
    {
        [Fact]
        public void CalculateDistanceKm_WithValidCoordinates_ReturnsCorrectDistance()
        {
            // Arrange - Distanz zwischen Berlin und München
            var berlinLat = 52.52;
            var berlinLon = 13.405;
            var munichLat = 48.1351;
            var munichLon = 11.582;

            // Act
            var distance = DistanceCalculator.CalculateDistanceKm(
                berlinLat,
                berlinLon,
                munichLat,
                munichLon
            );

            // Assert - Erwartete Distanz ca. 504 km
            Assert.True(
                distance > 500 && distance < 510,
                $"Expected distance ~504km, but got {distance:F2}km"
            );
        }

        [Fact]
        public void CalculateDistanceKm_WithSameCoordinates_ReturnsZero()
        {
            // Arrange
            var lat = 52.52;
            var lon = 13.405;

            // Act
            var distance = DistanceCalculator.CalculateDistanceKm(lat, lon, lat, lon);

            // Assert
            Assert.Equal(0, distance, 3); // 3 Dezimalstellen Präzision
        }

        [Fact]
        public void CalculateDistanceKm_WithAddresses_ReturnsCorrectDistance()
        {
            // Arrange
            var address1 = new Address
            {
                Lat = 52.52, // Berlin
                Long = 13.405,
            };

            var address2 = new Address
            {
                Lat = 48.1351, // München
                Long = 11.582,
            };

            // Act
            var distance = DistanceCalculator.CalculateDistanceKm(address1, address2);

            // Assert
            Assert.NotNull(distance);
            Assert.True(
                distance > 500 && distance < 510,
                $"Expected distance ~504km, but got {distance:F2}km"
            );
        }

        [Fact]
        public void CalculateDistanceKm_WithNullAddress_ReturnsNull()
        {
            // Arrange
            var address1 = new Address { Lat = 52.52, Long = 13.405 };

            // Act & Assert
            Assert.Null(DistanceCalculator.CalculateDistanceKm(null, address1));
            Assert.Null(DistanceCalculator.CalculateDistanceKm(address1, null));
            Assert.Null(DistanceCalculator.CalculateDistanceKm(null, null));
        }

        [Fact]
        public void CalculateDistanceKm_WithMissingCoordinates_ReturnsNull()
        {
            // Arrange
            var addressWithoutLat = new Address { Long = 13.405 };

            var addressWithoutLon = new Address { Lat = 52.52 };

            var completeAddress = new Address { Lat = 52.52, Long = 13.405 };

            // Act & Assert
            Assert.Null(DistanceCalculator.CalculateDistanceKm(addressWithoutLat, completeAddress));
            Assert.Null(DistanceCalculator.CalculateDistanceKm(addressWithoutLon, completeAddress));
            Assert.Null(DistanceCalculator.CalculateDistanceKm(completeAddress, addressWithoutLat));
        }

        [Theory]
        [InlineData(52.52, 13.405, true)] // Berlin
        [InlineData(90, 180, true)] // Extremwerte
        [InlineData(-90, -180, true)] // Extremwerte
        [InlineData(0, 0, true)] // Nullpunkt
        [InlineData(91, 0, false)] // Ungültiger Breitengrad
        [InlineData(-91, 0, false)] // Ungültiger Breitengrad
        [InlineData(0, 181, false)] // Ungültiger Längengrad
        [InlineData(0, -181, false)] // Ungültiger Längengrad
        public void HasValidCoordinates_ValidatesCorrectly(
            double lat,
            double lon,
            bool expectedValid
        )
        {
            // Arrange
            var address = new Address { Lat = lat, Long = lon };

            // Act
            var isValid = DistanceCalculator.HasValidCoordinates(address);

            // Assert
            Assert.Equal(expectedValid, isValid);
        }

        [Fact]
        public void HasValidCoordinates_WithNullAddress_ReturnsFalse()
        {
            // Act & Assert
            Assert.False(DistanceCalculator.HasValidCoordinates(null));
        }

        [Fact]
        public void HasValidCoordinates_WithMissingCoordinates_ReturnsFalse()
        {
            // Arrange
            var addressWithoutLat = new Address { Long = 13.405 };
            var addressWithoutLon = new Address { Lat = 52.52 };
            var emptyAddress = new Address();

            // Act & Assert
            Assert.False(DistanceCalculator.HasValidCoordinates(addressWithoutLat));
            Assert.False(DistanceCalculator.HasValidCoordinates(addressWithoutLon));
            Assert.False(DistanceCalculator.HasValidCoordinates(emptyAddress));
        }

        [Fact]
        public void CalculateDistanceKm_ShortDistance_ReturnsAccurateResult()
        {
            // Arrange - Zwei Punkte in derselben Stadt (ca. 5km Entfernung)
            var point1Lat = 52.5200; // Berlin Mitte
            var point1Lon = 13.4050;
            var point2Lat = 52.4797; // Berlin Tempelhof
            var point2Lon = 13.4014;

            // Act
            var distance = DistanceCalculator.CalculateDistanceKm(
                point1Lat,
                point1Lon,
                point2Lat,
                point2Lon
            );

            // Assert - Erwartete Distanz ca. 4.5km
            Assert.True(
                distance > 4 && distance < 6,
                $"Expected distance ~4.5km, but got {distance:F2}km"
            );
        }
    }
}
