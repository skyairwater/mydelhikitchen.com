using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EcommerceStore.Hubs;
using EcommerceStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace EcommerceStore.Controllers
{
    [ApiController]
    [Route("api/tracking")]
    public class TrackingApiController : ControllerBase
    {
        private readonly IHubContext<DeliveryTrackingHub> _hubContext;

        public TrackingApiController(IHubContext<DeliveryTrackingHub> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <summary>
        /// Endpoint called by the Android delivery driver app in real time.
        /// Accepts JSON or raw text payloads.
        /// </summary>
        [HttpPost("location")]
        [Consumes("application/json", "text/plain")]
        public async Task<IActionResult> UpdateLocation()
        {
            DriverLocationUpdate? update;

            try
            {
                using var reader = new StreamReader(Request.Body, Encoding.UTF8);
                var bodyText = await reader.ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(bodyText))
                {
                    return BadRequest(new { success = false, message = "Invalid or empty location payload." });
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                update = JsonSerializer.Deserialize<DriverLocationUpdate>(bodyText, options);
            }
            catch (JsonException)
            {
                return BadRequest(new { success = false, message = "Invalid or empty location payload." });
            }

            if (update == null)
            {
                return BadRequest(new { success = false, message = "Invalid or empty location payload." });
            }

            if (string.IsNullOrWhiteSpace(update.DriverId))
            {
                update.DriverId = "Driver 1";
            }

            if (string.IsNullOrWhiteSpace(update.DeliveryType))
            {
                update.DeliveryType = "Food";
            }

            if (update.Timestamp == default)
            {
                update.Timestamp = DateTime.UtcNow;
            }

            // Save in-memory cache
            DeliveryTrackingHub.SetLatestLocation(update);

            // Broadcast in real-time to all connected web tracking pages via SignalR
            await _hubContext.Clients.All.SendAsync("ReceiveLocation", update);

            return Ok(new
            {
                success = true,
                message = "Location updated successfully.",
                receivedAt = DateTime.UtcNow,
                data = update
            });
        }

        /// <summary>
        /// Clears any cached test/demo locations from memory.
        /// </summary>
        [HttpPost("clear")]
        public async Task<IActionResult> ClearLocation()
        {
            DeliveryTrackingHub.ClearLocations();
            await _hubContext.Clients.All.SendAsync("ReceiveLocation", new DriverLocationUpdate { DriverId = "ALL", IsSharing = false });
            return Ok(new { success = true, message = "Location cache cleared." });
        }

        /// <summary>
        /// Returns all currently active drivers, optionally filtered by deliveryType ("Food" or "Grocery").
        /// </summary>
        [HttpGet("active")]
        public IActionResult GetActiveDrivers([FromQuery] string? deliveryType)
        {
            var drivers = DeliveryTrackingHub.GetAllActiveDrivers(deliveryType);
            return Ok(new { success = true, count = drivers.Count(), data = drivers });
        }

        /// <summary>
        /// Fetch the latest known location. Useful when the web page initially loads.
        /// </summary>
        [HttpGet("latest")]
        public IActionResult GetLatestLocation([FromQuery] string? driverId)
        {
            var latest = DeliveryTrackingHub.GetLatestLocation(driverId ?? "Delivery Driver");
            if (latest == null)
            {
                return Ok(new { success = false, message = "No location updates received yet." });
            }

            return Ok(new { success = true, data = latest });
        }
    }
}
