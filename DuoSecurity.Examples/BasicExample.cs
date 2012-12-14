using System.Collections.Generic;
using DuoVerificationService;
using HttpWebAdapters;

namespace DuoSecurity.Examples
{
    public class BasicExample
    {
        const string IntegrationKey = "ASASASASASASASA";
        const string SecretKey = "asddfdgfsdhghgdfjhjh";
        const string Host = "api-xxxxxx.duosecurity.com";
        const string Path = "/rest/v1/auth";

        public void RunAuthorization()
        {
            var duoRestApiService = new RestApiService(IntegrationKey, SecretKey, Host);

            var adapter = new HttpWebRequestFactory();
            duoRestApiService.QueryDuoApi(adapter, "https", Path, HttpWebRequestMethod.POST, GetQueryValues("test@test.com"));
        }

        private static IEnumerable<KeyValuePair<string, string>> GetQueryValues(string user)
        {
            var ps = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("auto", "phone1"),
                    new KeyValuePair<string, string>("factor", "auto"),
                    new KeyValuePair<string, string>("user", user)
                };
            return ps;
        }

    }
}
