using Microsoft.AspNetCore.Mvc;

namespace EcommerceStore.Controllers
{
    public class DeliveryTrackingController : Controller
    {
        [Route("delivery-tracking")]
        [Route("delivery-tracking/live")]
        [Route("driver-live")]
        public IActionResult Live([FromQuery] string? type)
        {
            ViewData["DeliveryType"] = string.IsNullOrWhiteSpace(type) ? "All" : type;
            return View("~/Views/DeliveryTracking/Live.cshtml");
        }

        [Route("delivery-tracking/food")]
        [Route("food-delivery/tracking")]
        public IActionResult FoodLive()
        {
            ViewData["DeliveryType"] = "Food";
            return View("~/Views/DeliveryTracking/Live.cshtml");
        }

        [Route("delivery-tracking/groceries")]
        [Route("grocery-delivery/tracking")]
        public IActionResult GroceryLive()
        {
            ViewData["DeliveryType"] = "Grocery";
            return View("~/Views/DeliveryTracking/Live.cshtml");
        }
    }
}
