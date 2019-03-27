using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoLocation.Models;
using Xamarin.Essentials;
using Xamarin.Forms;


namespace GeoLocation.Pages
{
    public partial class MainPage : ContentPage
    {
        private bool _keepPolling = false;
        private readonly double _defaultRadius = 30.0;
        private Dictionary<string, double> _radiusInfo = new Dictionary<string, double>();
        private double _longitudinalOffset = 0.0;
        private int _pollingIntervalInSeconds = 20;


        public MainPage()
        {
            InitializeComponent();

            BtnStopLocationPolling.IsEnabled = false;
            BtnStartLocationPolling.IsEnabled = StpBlue.IsEnabled = StpRed.IsEnabled = StpGreen.IsEnabled = SldOffset.IsEnabled = true;
            LblZone.BackgroundColor = Managers.GeoManager.ColorWhenNotPolling;

            StpBlue.Value = _defaultRadius;
            StpRed.Value = _defaultRadius;
            StpGreen.Value = _defaultRadius;

            LblBlueRadius.Text = StpBlue.Value.ToString(CultureInfo.CurrentCulture);
            LblRedRadius.Text = StpRed.Value.ToString(CultureInfo.CurrentCulture);
            LblGreenRadius.Text = StpGreen.Value.ToString(CultureInfo.CurrentCulture);
        }


        private async void PollLocation()
        {
            while (_keepPolling)
            {
                Managers.GeoManager.UpdateRadiusForPresentationZones(_radiusInfo);
                Managers.GeoManager.UpdateLongitudeOffsetForPresentationZones(_longitudinalOffset);

                string locationDescription = string.Empty;
                var location = await Managers.GeoManager.GetLocation();
                if (location != null)
                {
                    locationDescription = $"Latitude: {location.Latitude}, Longitude: {location.Longitude}";
                    Console.WriteLine(locationDescription);
                    var zoneEvent = Managers.GeoManager.GetClosestContainingZoneEvent(location);
                    if (_keepPolling)
                    {
                        LblZone.BackgroundColor = zoneEvent.ZoneColor;
                        if (zoneEvent.RelativeDistanceFromZone < 1.0)
                        {
                            locationDescription += "\nMeters: " + zoneEvent.DistanceFromZoneInMeters.ToString("N0") + ", Relative Distance: " + zoneEvent.RelativeDistanceFromZone.ToString("P0");
                        }
                    }
                    LblLocation.Text = locationDescription;
                }

                if (_keepPolling)
                {
                    await Task.Delay(TimeSpan.FromSeconds(_pollingIntervalInSeconds));
                }
            }
        }

        private void BtnStartLocationPolling_OnClicked(object sender, EventArgs e)
        {
            BtnStartLocationPolling.IsEnabled = StpBlue.IsEnabled = StpRed.IsEnabled = StpGreen.IsEnabled = SldOffset.IsEnabled = false;
            BtnStopLocationPolling.IsEnabled = true;
            LblLocation.Text = string.Empty;
            _keepPolling = true;
            PollLocation();
        }

        private void BtnStopLocationPolling_OnClicked(object sender, EventArgs e)
        {
            BtnStopLocationPolling.IsEnabled = false;
            _keepPolling = false;
            LblZone.BackgroundColor = Managers.GeoManager.ColorWhenNotPolling;
            BtnStartLocationPolling.IsEnabled = StpBlue.IsEnabled = StpRed.IsEnabled = StpGreen.IsEnabled = SldOffset.IsEnabled = true;
        }


        private void StpBlue_OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            _radiusInfo[Managers.GeoManager.ZoneNameBlue] = e.NewValue;
            LblBlueRadius.Text = e.NewValue.ToString(CultureInfo.CurrentCulture);
        }

        private void StpRed_OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            _radiusInfo[Managers.GeoManager.ZoneNameRed] = e.NewValue;
            LblRedRadius.Text = e.NewValue.ToString(CultureInfo.CurrentCulture);
        }

        private void StpGreen_OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            _radiusInfo[Managers.GeoManager.ZoneNameGreen] = e.NewValue;
            LblGreenRadius.Text = e.NewValue.ToString(CultureInfo.CurrentCulture);
        }

        private void Slider_OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            _longitudinalOffset = -1.0 * (0.00005 * e.NewValue);
        }

        private void SldPollInterval_OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            _pollingIntervalInSeconds = Convert.ToInt32((e.NewValue + 1.0) * 5);
        }
    }
}
