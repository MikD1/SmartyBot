using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

namespace SmartyBot
{
    public class Dictionary : IDisposable
    {
        public Dictionary()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(_serviceAddress);

            _random = new Random();
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public async Task<string> GetPartOfSpeech(string word)
        {
            try
            {
                JObject data = await SendRequest(word);
                return data["def"][0]["pos"].Value<string>();
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<string> GetSynonym(string word)
        {
            try
            {
                JObject data = await SendRequest(word);

                int trCount = data["def"][0]["tr"].ToArray().Length;
                int trIndex = _random.Next(0, trCount - 1);

                int synCount = data["def"][0]["tr"][trIndex]["syn"].ToArray().Length;
                int synIndex = _random.Next(0, synCount - 1);

                return data["def"][0]["tr"][trIndex]["syn"][synIndex]["text"].Value<string>();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<JObject> SendRequest(string word)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
               string.Format(_resourcesFormat, _key, word));

            HttpResponseMessage response = await _client.SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();
            return JObject.Parse(content);
        }

        private readonly HttpClient _client;
        private readonly Random _random;

        const string _serviceAddress = "https://dictionary.yandex.net";
        const string _resourcesFormat = "api/v1/dicservice.json/lookup?key={0}&lang=ru-ru&text={1}";
        const string _key = "yandex.speller.key";
    }
}
