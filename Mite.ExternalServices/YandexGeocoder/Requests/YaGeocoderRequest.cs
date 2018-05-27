using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mite.ExternalServices.YandexGeocoder.Requests
{
    public class YaGeocoderRequest
    {
        private const string url = "https://geocode-maps.yandex.ru/1.x/";
        private readonly HttpClient httpClient;

        public YaGeocoderRequest(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        public async Task<(string countryCode, string cityName)> GetCityNameAsync(double lat, double lng)
        {
            var reqUrl = url + $"?format=json&geocode={lng.ToString("0.00", new CultureInfo("en-us"))},{lat.ToString("0.00", new CultureInfo("en-us"))}";
            var resp = await httpClient.GetAsync(reqUrl);
            var content = await resp.Content.ReadAsStringAsync();
            var jContent = JObject.Parse(content);

            if ((int)jContent["response"]["GeoObjectCollection"]["metaDataProperty"]["GeocoderResponseMetaData"]["results"] == 0)
                return ((string)null, (string)null);
            var addressComponent = jContent["response"]["GeoObjectCollection"]["featureMember"][0]["GeoObject"]["metaDataProperty"]["GeocoderMetaData"]["Address"];

            return ((string)addressComponent["country_code"], (string)addressComponent["Components"].Children().FirstOrDefault(x => (string)x["kind"] == "locality").SelectToken("name"));
        }
    }
}
