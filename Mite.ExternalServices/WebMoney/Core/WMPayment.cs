using NLog;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Serialization;
using WebMoney.Cryptography;

namespace Mite.ExternalServices.WebMoney.Core
{
    public abstract class WMPayment
    {
        private const string KeeperKey = "<RSAKeyValue><Modulus>jfVCumxZmMS82zKzeIJQOiG6zmd/miDHIBdCqJq12tL1uw5yfX6BSvoSPNzoab/1ccb5HsyPTvinDz5d722LJ4Ay</Modulus><D>rUU9aGOR7eiXiAeZDbk/b2naxl3enWZtYCeIa7wB6qU0w+Ig8oz8si/kT7GMAzrwX/TM9BTAwPvaiN5n3rkZTPkO</D></RSAKeyValue>";

        protected const string WmrPurse = "R326302628760";
        protected const string WmzPurse = "Z331493990411";
        protected const string MasterWmId = "311992204649";

        protected readonly HttpClient Client;
        protected readonly ILogger Logger;
        protected readonly Signer Signer;

        public WMPayment(HttpClient client, ILogger logger)
        {
            Client = client;
            Logger = logger;

            Signer = new Signer();
            Signer.Initialize(KeeperKey);
        }
        protected async Task<TResponse> PostAsync<TResponse>(WMRequest request)
            where TResponse : class
        {
            var serializer = new XmlSerializer(request.GetType());
            request.Language = "ru-RU";
            request.WmId = MasterWmId;
            request.Sign = Signer.Sign(request.SignMessage);

            using (var memoryStream = new MemoryStream())
            {
                serializer.Serialize(memoryStream, request);
                var reader = new StreamReader(memoryStream);
                memoryStream.Position = 0;
                //var testStr = await reader.ReadToEndAsync();
                var content = new StreamContent(memoryStream);
                content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
                var response = await Client.PostAsync(request.RequestUri, content);

                var respSerializer = new XmlSerializer(typeof(TResponse));
                var contentStr = await response.Content.ReadAsStringAsync();
                Logger.Info($"WmContent: {contentStr}");
                return respSerializer.Deserialize(await response.Content.ReadAsStreamAsync()) as TResponse;
            }
        }
    }
}
