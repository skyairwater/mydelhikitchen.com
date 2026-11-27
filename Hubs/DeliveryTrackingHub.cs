using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EcommerceStore.Models;
using Microsoft.AspNetCore.SignalR;

namespace EcommerceStore.Hubs
{
    public class DeliveryTrackingHub : Hub
    {
        // Thread-safe cache of the most recent location update per driver
        private static readonly ConcurrentDictionary<string, DriverLocationUpdate> _latestLocations = new(StringComparer.OrdinalIgnoreCase);

        public static DriverLocationUpdate? GetLatestLocation(string driverId = "Delivery Driver")
        {
            if (_latestLocations.TryGetValue(driverId, out var update))
            {
                return update;
            }

            // Return any latest active driver location if specific one not found
            var active = _latestLocations.Values.FirstOrDefault(u => u.IsSharing);
            return active ?? _latestLocations.Values.FirstOrDefault();
        }

        public static IEnumerable<DriverLocationUpdate> GetAllActiveDrivers(string? deliveryType = null)
        {
            var list = _latestLocations.Values.Where(u => u.IsSharing);
            if (!string.IsNullOrWhiteSpace(deliveryType) && !deliveryType.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                list = list.Where(u => string.Equals(u.DeliveryType, deliveryType, StringComparison.OrdinalIgnoreCase));
            }
            return list.ToList();
        }

        public static void SetLatestLocation(DriverLocationUpdate update)
        {
            var key = string.IsNullOrWhiteSpace(update.DriverId) ? "Delivery Driver" : update.DriverId.Trim();
            
            if (!update.IsSharing)
            {
                // Driver stopped sharing
                _latestLocations.TryRemove(key, out _);
            }
            else
            {
                _latestLocations[key] = update;
            }
        }

        public static void ClearLocations()
        {
            _latestLocations.Clear();
        }

        public async Task SendLocation(DriverLocationUpdate update)
        {
            SetLatestLocation(update);
            await Clients.All.SendAsync("ReceiveLocation", update);
        }

        public async Task GetCurrentLocation()
        {
            // Send all currently active drivers to the calling client
            var activeDrivers = GetAllActiveDrivers();
            foreach (var driver in activeDrivers)
            {
                await Clients.Caller.SendAsync("ReceiveLocation", driver);
            }
        }
    }
}
