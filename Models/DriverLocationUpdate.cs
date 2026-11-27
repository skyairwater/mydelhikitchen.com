using System;

namespace EcommerceStore.Models
{
    public class DriverLocationUpdate
    {
        public string DriverId { get; set; } = "Delivery Driver";
        
        /// <summary>
        /// Delivery category: "Food" or "Grocery"
        /// </summary>
        public string DeliveryType { get; set; } = "Food";

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Speed { get; set; } // m/s
        public double? Bearing { get; set; } // degrees (0-360)
        public double? Accuracy { get; set; } // meters
        public int? BatteryLevel { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsSharing { get; set; } = true;
    }
}
