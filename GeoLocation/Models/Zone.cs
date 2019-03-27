using System;
using System.Collections.Generic;
using System.Text;

namespace GeoLocation.Models
{
    public class Zone
    {
        public string Name { get; set; }

        public System.Drawing.Color Color { get; set; }

        public Xamarin.Essentials.Location Location { get; set; }

        public Xamarin.Essentials.Location OffsetLocation { get; set; }

        public double Radius { get; set; }
    }
}
