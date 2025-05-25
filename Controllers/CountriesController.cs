using Microsoft.AspNetCore.Mvc;
using sendbol_videoshop.Server.Models;
using sendbol_videoshop.Server.Services;
using System.Net.Http;
using System.Text.Json;

namespace sendbol_videoshop.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CountriesController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CountriesService _countriesService;

        public CountriesController(IHttpClientFactory httpClientFactory, CountriesService countriesService)
        {
            _httpClientFactory = httpClientFactory;
            _countriesService = countriesService;
        }

        [HttpGet]        // filepath: [CountriesController.cs](http://_vscodecontentref_/0)
        [HttpGet]
        public async Task<IActionResult> GetCountries()
        {
            try
            {
                var countries = await _countriesService.GetOrFetchAllCountriesAsync(_httpClientFactory);
                return Ok(countries);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los países", error = ex.Message });
            }
        }
    }
}
