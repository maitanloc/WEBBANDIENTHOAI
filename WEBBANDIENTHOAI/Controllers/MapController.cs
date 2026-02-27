using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace WEBBANDIENTHOAI.Controllers
{
    public class MapController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MapController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Autocomplete(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new object[] { });
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(q)}&format=json&limit=5&countrycodes=vn");
            request.Headers.Add("User-Agent", "WEBBANDIENTHOAI/1.0");

            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }

            return StatusCode((int)response.StatusCode, "Error connecting to Geocoding service");
        }

        [HttpGet]
        public async Task<IActionResult> Reverse(decimal lat, decimal lon)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://nominatim.openstreetmap.org/reverse?lat={lat}&lon={lon}&format=json");
            request.Headers.Add("User-Agent", "WEBBANDIENTHOAI/1.0");

            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }

            return StatusCode((int)response.StatusCode, "Error connecting to Geocoding service");
        }
    }
}
