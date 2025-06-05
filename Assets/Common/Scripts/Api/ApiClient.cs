using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Scripts.Settings;
using Common.Scripts.UI;
using Common.Scripts.Utils;
using Newtonsoft.Json;
using UnityEngine;
using System.Net;

namespace Common.Scripts.API
{
    public class ApiException : Exception
    {
        public int StatusCode { get; set; }

        public string Content { get; set; }
    }

    public class ApiClient : Singleton<ApiClient>
    {
        [SerializeField] 
        private APISettings apiSettings;

        public APISettings APISettings => apiSettings;
        private HttpClient _httpClient;
        private CancellationTokenSource _cancellationTokenSource;

        private void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("x-api-unity-key", 
                "uc0jV1pE2z78EKxung2cB8palsn1VAd63I7R4sA3hCDMhb1rJRvl9JcX89ogPnHb2cAYQJsTcnZ9TH1G70HOaZDE6qn7yFFH");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<object> RequestDataAsync(Type dataType, HttpMethod method, string requestUri,
            Dictionary<string, string> bodyContent = null)
        {
            using (var request = new HttpRequestMessage(method, requestUri))
            {
                if (bodyContent != null && method.Equals(HttpMethod.Post))
                {
                    var wrappedBody = new Dictionary<string, object> { { "context", bodyContent } };
                    string json = JsonConvert.SerializeObject(wrappedBody);
                    Debug.Log($"[API POST BODY] {json}");
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                using (var response = await _httpClient.SendAsync(request, _cancellationTokenSource.Token))
                {
                    var stream = await response.Content.ReadAsStreamAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return JsonUtils.DeserializeJsonFromStream(dataType, stream);
                    }

                    var content = await JsonUtils.StreamToStringAsync(stream);
                    Debug.LogError($"{(int)response.StatusCode} {content}");
                    throw new ApiException
                    {
                        StatusCode = (int)response.StatusCode,
                        Content = content
                    };
                }
            }
        }

        public async Task<T> RequestDataAsync<T>(HttpMethod method, string requestUri,
            Dictionary<string, string> bodyContent = null)
        {
            using (var request = new HttpRequestMessage(method, requestUri))
            {
                if (bodyContent != null && method.Equals(HttpMethod.Post))
                {
                    var wrappedBody = new Dictionary<string, object> { { "context", bodyContent } };
                    string json = JsonConvert.SerializeObject(wrappedBody);
                    Debug.Log($"[API POST BODY] {json}"); // <-- Add this line
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                using (var response = await _httpClient.SendAsync(request, _cancellationTokenSource.Token))
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    var contentType = response.Content.Headers.ContentType?.MediaType;

                    if (response.IsSuccessStatusCode && contentType == "application/json")
                    {
                        return JsonUtils.DeserializeJsonFromStream<T>(stream);
                    }

                    var content = await JsonUtils.StreamToStringAsync(stream);
                    Debug.LogError($"[API ERROR] {(int)response.StatusCode} {response.ReasonPhrase}\n{content}");

                    throw new ApiException
                    {
                        StatusCode = (int)response.StatusCode,
                        Content = content
                    };
                }
            }
        }

        // private void OnApplicationPause(bool pauseStatus)
        // {
        //     if (pauseStatus)
        //     {
        //         _cancellationTokenSource.Cancel();
        //     }
        // }
    }
}