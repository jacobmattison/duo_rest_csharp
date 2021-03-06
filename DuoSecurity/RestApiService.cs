﻿using HttpWebAdapters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;


namespace DuoVerificationService
{
    public class RestApiService
    {
        private readonly string _integrationKey;
        private readonly string _secretKey;
        private readonly string _host;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="integrationKey"></param>
        /// <param name="secretKey"></param>
        /// <param name="host"></param>
        public RestApiService(string integrationKey, string secretKey, string host)
        {
            _integrationKey = integrationKey;
            _secretKey = secretKey;
            _host = host;
        }

        public Object QueryDuoApi(IHttpWebRequestFactory webRequestFactory, string protocol, string path, HttpWebRequestMethod method, IEnumerable<KeyValuePair<string, string>> queryValues)
        {
            var duoWebRequest = CreateSignedDuoWebRequest(webRequestFactory, protocol, path, method, queryValues);

            try
            {
                var duoResponse = duoWebRequest.GetResponse();

                if (duoResponse != null)
                {
                    var responseStream = duoResponse.GetResponseStream();
                    if (responseStream != null)
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            var js = new JavaScriptSerializer();
                            var objText = reader.ReadToEnd();
                            return js.DeserializeObject(objText);
                        }
                    }
                }
            }
            catch (WebException)
            {
                return null;
            }
            return null;
        }

        private IHttpWebRequest CreateSignedDuoWebRequest(IHttpWebRequestFactory webRequestFactory, string protocol, string path, HttpWebRequestMethod method, IEnumerable<KeyValuePair<string, string>> queryValues)
        {
            queryValues = queryValues.OrderBy(item => item.Key);
            var duoWebRequest = CreateDuoWebRequest(webRequestFactory, protocol, path, method);

            var safeQueryString = CreateSafeQueryString(queryValues);

            SignWebRequest(duoWebRequest, method, path, safeQueryString);

            if (method == HttpWebRequestMethod.POST) SetFormValuesForQuery(duoWebRequest, safeQueryString);

            return duoWebRequest;
        }

        public string GetAuthorizationKey(HttpWebRequestMethod method, DateTime date, string path, string queryString)
        {
            var canon = GetCanonRequest(date, method, _host, path, queryString);
            var signedCanon = SignHmac(_secretKey, canon);

            var authorization = _integrationKey + ":" + signedCanon;
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(authorization));
        }

        private void SignWebRequest(IHttpWebRequest webRequest, HttpWebRequestMethod method, string path, string queryString)
        {
            DateTime now = DateTime.Now;
            webRequest.Headers.Add("Authorization", string.Format("Basic {0}", GetAuthorizationKey(method, now, path, queryString)));
            webRequest.Date = now;
        }

        private IHttpWebRequest CreateDuoWebRequest(IHttpWebRequestFactory webRequestFactory, string protocol, string path, HttpWebRequestMethod method)
        {
            var duoWebRequest = webRequestFactory.Create(new Uri(string.Format(@"{0}://{1}{2}", protocol, _host, path)));
            duoWebRequest.Method = method;
            duoWebRequest.Accept = "application/json";
            duoWebRequest.Timeout = 3 * 60 * 1000;
            return duoWebRequest;
        }

        private static void SetFormValuesForQuery(IHttpWebRequest webRequest, string safeQueryString)
        {
            if (webRequest == null) throw new ArgumentNullException("webRequest");
            webRequest.ContentType = "application/x-www-form-urlencoded";
            var stream = webRequest.GetRequestStream();
            var bodyBytes = new ASCIIEncoding().GetBytes(safeQueryString);
            stream.Write(bodyBytes, 0, bodyBytes.Length);
            stream.Flush();
            stream.Close();
        }

        /// <summary>
        /// Create a duo friendly querystring
        /// </summary>
        /// <param name="queryStringItems"></param>
        /// <returns></returns>
        private static string CreateSafeQueryString(IEnumerable<KeyValuePair<string, string>> parameters)
        {
            return string.Join("&", parameters.Select(p => string.Format("{0}={1}", System.Web.HttpUtility.UrlEncode(p.Key), (System.Web.HttpUtility.UrlEncode(p.Value)))));
        }

        /// <summary>
        /// Sign the content using the key and SHA1
        /// </summary>
        /// <param name="key"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private static String SignHmac(String key, String content)
        {
            try
            {
                var encoding = Encoding.ASCII;
                var hmacSha1 = new HMACSHA1(encoding.GetBytes(key));
                return hmacSha1.ComputeHash(GetContentAsStream(content)).Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Return the content as a MemoryStream
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static MemoryStream GetContentAsStream(string content)
        {
            var contentAsBytes = Encoding.ASCII.GetBytes(content);
            var contentAsStream = new MemoryStream(contentAsBytes);
            return contentAsStream;
        }

        /// <summary>
        /// Build up the canon required for signing
        /// </summary>
        /// <param name="method"></param>
        /// <param name="host"></param>
        /// <param name="uri"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        private static String GetCanonRequest(DateTime date, HttpWebRequestMethod method, string host, string uri, string queryString)
        {
            var canon = new StringBuilder();

            canon.Append(string.Format("{0}\n", date.ToUniversalTime().ToString(new System.Globalization.DateTimeFormatInfo().RFC1123Pattern)));
            canon.Append(string.Format("{0}\n", method.ToString().ToUpper()));
            canon.Append(string.Format("{0}\n", host.ToLower()));
            canon.Append(string.Format("{0}\n", uri));
            canon.Append(queryString);

            return canon.ToString();
        }

    }
}
