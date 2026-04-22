using Microsoft.AspNetCore.Mvc;

namespace MiniShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExternalController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public ExternalController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        // GET: api/external/inventory/{sku}
        [HttpGet("inventory/{sku}")]
        public async Task<IActionResult> GetInventory(string sku)
        {
            var response = await _httpClient.GetAsync($"http://localhost:3000/inventory/{sku}");
            var content = await response.Content.ReadAsStringAsync();
            return Ok(content);
        }

        // POST: api/external/payments
        [HttpPost("payments")]
        public async Task<IActionResult> ProcessPayment([FromBody] object paymentRequest)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(paymentRequest);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("http://localhost:3001/payments", content);
            var result = await response.Content.ReadAsStringAsync();
            return Ok(result);
        }
    }
}