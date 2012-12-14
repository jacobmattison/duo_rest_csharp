using System.Collections.Generic;
using DuoVerificationService;
using HttpWebAdapters;

namespace DuoSecurity.Examples
{
    public class BasicExample
    {
<<<<<<< HEAD
        const string IntegrationKey = "DIA1AQJCU97DCLD11AZE";
        const string SecretKey = "TZOiTvqx3xb8VuBBaF7ewtYSsqnfUfTq8V6W3EsT";
        const string Host = "api-f8aa1baa.duosecurity.com";
        const string Path = "/verify/v1/sms";

        public void RunAuthorization()
        {
            var duoRestApiService = new VerifyApiService(IntegrationKey, SecretKey, Host);

            var adapter = new HttpWebRequestFactory();
            var obj = duoRestApiService.QueryDuoApi(adapter, "https", Path, HttpWebRequestMethod.POST,
                                                    GetQueryValues("test@test.com"));
=======
        const string IntegrationKey = "ASASASASASASASA";
        const string SecretKey = "asddfdgfsdhghgdfjhjh";
        const string Host = "api-xxxxxx.duosecurity.com";
        const string Path = "/rest/v1/auth";

        public void RunAuthorization()
        {
            var duoRestApiService = new RestApiService(IntegrationKey, SecretKey, Host);

            var adapter = new HttpWebRequestFactory();
            duoRestApiService.QueryDuoApi(adapter, "https", Path, HttpWebRequestMethod.POST, GetQueryValues("test@test.com"));
>>>>>>> 873a34ea71e73013ed8e24f74fcabf1b51a81750
        }

        private static IEnumerable<KeyValuePair<string, string>> GetQueryValues(string user)
        {
            var ps = new List<KeyValuePair<string, string>>
                {
<<<<<<< HEAD
                     //new KeyValuePair<string, string>("txid", "the pin is <pin>")
                    new KeyValuePair<string, string>("message", "the pin is <pin>"),
                    new KeyValuePair<string, string>("phone", "+447952556282")
                    ////   new KeyValuePair<string, string>("auto", "phone1"),
                    //new KeyValuePair<string, string>("factor", "auto"),
                    //new KeyValuePair<string, string>("user","mpyoung@gmail.com")
=======
                    new KeyValuePair<string, string>("auto", "phone1"),
                    new KeyValuePair<string, string>("factor", "auto"),
                    new KeyValuePair<string, string>("user", user)
>>>>>>> 873a34ea71e73013ed8e24f74fcabf1b51a81750
                };
            return ps;
        }

    }
}
