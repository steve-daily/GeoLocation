using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GeoLocation.Helpers
{
    public class ZoneEvent
    {
        public string ZoneName { get; set; }

        public Color ZoneColor { get; set; }

        public double Offset { get; set; }

        public double DistanceFromZoneInMeters { get; set; }

        public double RelativeDistanceFromZone { get; set; }
    }
}
