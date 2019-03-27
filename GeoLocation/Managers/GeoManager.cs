using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Drawing;
using GeoLocation.Models;
using Xamarin.Essentials;
using Xamarin.Forms.Xaml;


namespace GeoLocation.Managers
{
    public static class GeoManager
    {
        private static readonly Xamarin.Essentials.GeolocationRequest GeoRequest;
        private static List<Zone> _zones = new List<Zone>();
        private static readonly Zone DefaultZone;
        private const double AverageRadiusOfTheEarthInMeters = 6370999;
        private static readonly double _outerPerimeterThreshold = 0.90;
        private static readonly double _middlePerimeterThreshold = 0.75;


        public static readonly System.Drawing.Color ColorWhenNotPolling = Color.Gray;
        public static readonly string ZoneNameBlue = "Blue";
        public static readonly string ZoneNameRed = "Red";
        public static readonly string ZoneNameGreen = "Green";


        static GeoManager()
        {
            GeoRequest = new Xamarin.Essentials.GeolocationRequest(Xamarin.Essentials.GeolocationAccuracy.Best);
            DefaultZone = new Zone() { Location = new Xamarin.Essentials.Location() { Latitude = 0.0, Longitude = -0.0 }, Radius = 0.0, Color = Color.White, Name = "White" };
            InitializeZones();
        }

        private static Zone BuildZone(string name, Color color, double radius, double latitude, double longitude)
        {
            var zone = new Zone()
            {
                Name = name,
                Color = color,
                Radius = radius,
                Location = new Location() { Latitude = latitude, Longitude = longitude },
                OffsetLocation = new Location() { Latitude = latitude, Longitude = longitude }
            };
            return zone;
        }

        private static void InitializeZones()
        {
            var blueZone = BuildZone(ZoneNameBlue, Color.Blue, 30.0, 36.0730846, -95.9233244);
            var redZone = BuildZone(ZoneNameRed, Color.Red, 30.0, 36.07300167, -95.92327945);
            var greenZone = BuildZone(ZoneNameGreen, Color.Green, 30.0, 36.0732572, -95.9230817);
            var darkBlueZone = BuildZone("DarkBlue", Color.DarkBlue, 30.0, 36.07411200, -95.92466600);
            var darkRedZone = BuildZone("DarkRed", Color.DarkRed, 30.0, 36.07403849, -95.92551845);
            var darkGreenZone = BuildZone("DarkGreen", Color.DarkGreen, 120.0, 36.032276, -95.889212);
            var purpleZone = BuildZone("Purple", Color.Purple, 120.0, 35.983520, -96.084035);
            var orangeZone = BuildZone("Orange", Color.Orange, 120.0, 36.032826, -95.802200);
            var yellowZone = BuildZone("Yellow", Color.Yellow, 120.0, 36.030441, -95.901468);

            _zones.Add(blueZone);
            _zones.Add(redZone);
            _zones.Add(greenZone);
            _zones.Add(darkBlueZone);
            _zones.Add(darkRedZone);
            _zones.Add(darkGreenZone);
            _zones.Add(purpleZone);
            _zones.Add(orangeZone);
            _zones.Add(yellowZone);
        }

        public static void UpdateRadiusForPresentationZones(Dictionary<string, double> newZoneInfo)
        {
            foreach (var element in newZoneInfo)
            {
                var matchingZones = _zones.FindAll(x => x.Name.Contains(element.Key));
                foreach (var matchingZone in matchingZones)
                    matchingZone.Radius = element.Value;
            }
        }

        public static void UpdateLongitudeOffsetForPresentationZones(double offset)
        {
            foreach (var zone in _zones)
            {
                zone.OffsetLocation.Latitude = zone.Location.Latitude;
                zone.OffsetLocation.Longitude = zone.Location.Longitude + offset;
            }
        }

        public static async Task<Xamarin.Essentials.Location> GetLocation()
        {
            try
            {
                var location = await Xamarin.Essentials.Geolocation.GetLocationAsync(GeoRequest);
                return location;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        private static Color GetZoneColorAccountingForDistance(Zone zone, double relativeDistance)
        {
            var color = zone.Color;

            if (relativeDistance > _outerPerimeterThreshold)
                color = LightenColor(zone.Color, _outerPerimeterThreshold);
            else if (relativeDistance > _middlePerimeterThreshold)
                color = LightenColor(zone.Color, _middlePerimeterThreshold);
            return color;
        }

        private static Color LightenColor(Color color, double percent)
        {
            return Color.FromArgb(
                color.A,
                (int)Math.Min(255, color.R + 255 * percent),
                (int)Math.Min(255, color.G + 255 * percent),
                (int)Math.Min(255, color.B + 255 * percent));
        }

        public static GeoLocation.Helpers.ZoneEvent GetClosestContainingZoneEvent(Xamarin.Essentials.Location location)
        {
            var closestZoneEvent = new GeoLocation.Helpers.ZoneEvent()
            {
                ZoneName = DefaultZone.Name,
                ZoneColor = DefaultZone.Color,
                DistanceFromZoneInMeters = double.MaxValue,
                RelativeDistanceFromZone = 1.0
            };

            foreach (var zone in _zones)
            {
                var distance = GetDistanceInMeters(location, zone);
                if (IsLocationInsideZone(distance, zone))
                {
                    var relativeDistance = GetRelativeDistanceFromZoneNexus(distance, zone);
                    if (relativeDistance < closestZoneEvent.RelativeDistanceFromZone)
                    {
                        closestZoneEvent.RelativeDistanceFromZone = relativeDistance;
                        closestZoneEvent.DistanceFromZoneInMeters = distance;
                        closestZoneEvent.ZoneName = zone.Name;
                        closestZoneEvent.ZoneColor = GetZoneColorAccountingForDistance(zone, relativeDistance);
                    }
                }
            }

            return closestZoneEvent;
        }

        private static double GetRelativeDistanceFromZoneNexus(double distanceFromZoneNexus, Models.Zone zone)
        {
            return distanceFromZoneNexus / zone.Radius;
        }

        private static bool IsLocationInsideZone(double distanceFromZoneNexus, Models.Zone zone)
        {
            return distanceFromZoneNexus < zone.Radius;
        }

        private static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        private static double GetDistanceInMeters(Xamarin.Essentials.Location location, Models.Zone zone)
        {
            double differenceInLatitude = DegreeToRadian(zone.OffsetLocation.Latitude - location.Latitude);
            double differenceInLongitude = DegreeToRadian(zone.OffsetLocation.Longitude - location.Longitude);

            double latitudeHaversine = Math.Cos(DegreeToRadian(zone.OffsetLocation.Latitude)) * Math.Cos(DegreeToRadian(location.Latitude));
            double longitudeHaversine = Math.Sin(differenceInLongitude / 2) * Math.Sin(differenceInLongitude / 2);

            double innerHaversine = latitudeHaversine * longitudeHaversine;
            double haversine = (Math.Sin((differenceInLatitude) / 2) * Math.Sin((differenceInLatitude) / 2)) + (innerHaversine);

            var distanceInMeters = AverageRadiusOfTheEarthInMeters * 2 * Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(1 - haversine));
            return distanceInMeters;
        }

    }
}
