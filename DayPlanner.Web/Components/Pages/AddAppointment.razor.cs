using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DayPlanner.Abstractions.Models.DTO;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using Radzen;

namespace DayPlanner.Web.Components.Pages
{
    public sealed partial class AddAppointment : ComponentBase
    {
        [Inject]
        private DialogService DialogService { get; set; } = default!;
        [Inject]
        private IHttpClientFactory HttpClientFactory { get; set; } = default!;
        [Inject]
        private IConfiguration Configuration { get; set; } = default!;
        [Inject]
        private IStringLocalizer<AddAppointment> Localizer { get; set; } = default!;

        [Parameter]
        public DateTime Start { get; set; }
        [Parameter]
        public DateTime End { get; set; }

        private AppointmentRequest model = new();
        private HttpClient _httpClient = default!;
        private string _apiKey = string.Empty;
        private List<string?> _suggestions = new();

        protected override void OnInitialized()
        {
            _httpClient = HttpClientFactory.CreateClient("GeoApifyClient");
            _apiKey = Configuration["GeoApifyApi:HttpClient:ApiKey"]!;
        }

        protected override void OnParametersSet()
        {
            model.Start = Start;
            model.End = End;
        }

        private void OnSubmit(AppointmentRequest model)
        {
            DialogService.Close(model);
        }

        private async Task OnLocationChanged(ChangeEventArgs args)
        {
            string text = args.Value?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text) || text.Length < 4)
            {
                _suggestions.Clear();
                return;
            }
            string requestUrl = $"?text={Uri.EscapeDataString(text)}&lang={CultureInfo.CurrentUICulture.TwoLetterISOLanguageName}&limit=5&format=json&apiKey={_apiKey}";

            try
            {
                var response = await _httpClient.GetAsync(requestUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = JObject.Parse(await response.Content.ReadAsStringAsync());
                    _suggestions = json["results"]!
                        .Select(x => x["formatted"]?.ToString()) 
                        .Where(x => !string.IsNullOrEmpty(x))    
                        .ToList();
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching geolocation data: {ex.Message}");
            }
        }
    }
}
