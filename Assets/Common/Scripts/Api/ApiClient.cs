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
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiSettings.ApiToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<object> RequestDataAsync(Type dataType, HttpMethod method, string requestUri,
            Dictionary<string, string> bodyContent = null)
        {
            using (var request = new HttpRequestMessage(method, requestUri))
            {
                if (bodyContent != null && method.Equals(HttpMethod.Post))
                    request.Content = new StringContent(JsonConvert.SerializeObject(bodyContent), Encoding.UTF8,
                        "application/json");

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
                    request.Content = new StringContent(JsonConvert.SerializeObject(bodyContent), Encoding.UTF8,
                        "application/json");

                using (var response = await _httpClient.SendAsync(request, _cancellationTokenSource.Token))
                {
                    var stream = await response.Content.ReadAsStreamAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return JsonUtils.DeserializeJsonFromStream<T>(stream);
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

        // private void OnApplicationPause(bool pauseStatus)
        // {
        //     if (pauseStatus)
        //     {
        //         _cancellationTokenSource.Cancel();
        //     }
        // }
    }
}