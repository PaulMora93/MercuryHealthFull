using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Xamarin.Forms;

using HealthClinic.Shared;

namespace HealthClinic
{
    public abstract class BaseHttpClientService
    {
        #region Constant Fields
        static readonly Lazy<JsonSerializer> _serializerHolder = new Lazy<JsonSerializer>();
        static readonly Lazy<HttpClient> _clientHolder = new Lazy<HttpClient>(() => CreateHttpClient(HttpConstants.HttpTimeOut));
        #endregion

        #region Fields
        static int _networkIndicatorCount = 0;
        #endregion

        #region Properties
        protected static HttpClient Client => _clientHolder.Value;
        static JsonSerializer Serializer => _serializerHolder.Value;
        #endregion

        #region Methods
        protected static Task<HttpResponseMessage> GetResponseMessageFromAPI(string apiUrl) => GetResponseMessageFromAPI<object>(apiUrl);

        protected static async Task<HttpResponseMessage> GetResponseMessageFromAPI<TPayloadData>(string apiUrl, TPayloadData data = default)
        {
            var stringPayload = string.Empty;

            if (data?.Equals(default) == true)
                stringPayload = await Task.Run(() => JsonConvert.SerializeObject(data)).ConfigureAwait(false);

            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            try
            {
                UpdateActivityIndicatorStatus(true);

                return await Client.GetAsync(apiUrl);
            }
            catch (Exception e)
            {
                AppCenterService.LogException(e);
                return default;
            }
            finally
            {
                UpdateActivityIndicatorStatus(false);
            }
        }

        protected static Task<T> GetDataObjectFromAPI<T>(string apiUrl) => GetDataObjectFromAPI<T, object>(apiUrl);

        protected static async Task<TDataObject> GetDataObjectFromAPI<TDataObject, TPayloadData>(string apiUrl, TPayloadData data = default)
        {
            var stringPayload = string.Empty;

            if (data?.Equals(default) == true)
                stringPayload = await Task.Run(() => JsonConvert.SerializeObject(data)).ConfigureAwait(false);

            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            try
            {
                UpdateActivityIndicatorStatus(true);

                using (var stream = await Client.GetStreamAsync(apiUrl).ConfigureAwait(false))
                using (var reader = new StreamReader(stream))
                using (var json = new JsonTextReader(reader))
                {
                    if (json == null)
                        return default;

                    return await Task.Run(() => Serializer.Deserialize<TDataObject>(json)).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                AppCenterService.LogException(e);
                return default;
            }
            finally
            {
                UpdateActivityIndicatorStatus(false);
            }
        }

        protected static async Task<HttpResponseMessage> PostObjectToAPI<T>(string apiUrl, T data)
        {
            var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(data)).ConfigureAwait(false);

            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            try
            {
                UpdateActivityIndicatorStatus(true);

                return await Client.PostAsync(apiUrl, httpContent).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                AppCenterService.LogException(e);
                return null;
            }
            finally
            {
                UpdateActivityIndicatorStatus(false);
            }
        }

        protected static async Task<HttpResponseMessage> PatchObjectToAPI<T>(string apiUrl, T data)
        {
            var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(data)).ConfigureAwait(false);

            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage
            {
                Method = new HttpMethod("PATCH"),
                RequestUri = new Uri(apiUrl),
                Content = httpContent
            };

            try
            {
                UpdateActivityIndicatorStatus(true);

                return await Client.SendAsync(httpRequest).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                AppCenterService.LogException(e);
                return null;
            }
            finally
            {
                UpdateActivityIndicatorStatus(false);
            }
        }

        protected static async Task<HttpResponseMessage> DeleteObjectFromAPI(string apiUrl)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, new Uri(apiUrl));

            try
            {
                UpdateActivityIndicatorStatus(true);

                return await Client.SendAsync(httpRequest);
            }
            catch (Exception e)
            {
                AppCenterService.LogException(e);
                return null;
            }
            finally
            {
                UpdateActivityIndicatorStatus(false);
            }
        }

        static HttpClient CreateHttpClient(TimeSpan timeout) => new HttpClient { Timeout = timeout };

        static void UpdateActivityIndicatorStatus(bool isActivityInidicatorRunning)
        {
            if (isActivityInidicatorRunning)
            {
                Device.BeginInvokeOnMainThread(() => Application.Current.MainPage.IsBusy = true);
                _networkIndicatorCount++;
            }
            else if (--_networkIndicatorCount <= 0)
            {
                Device.BeginInvokeOnMainThread(() => Application.Current.MainPage.IsBusy = false);
                _networkIndicatorCount = 0;
            }
        }
        #endregion
    }
}
