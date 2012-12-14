# Overview
C# service code for calling the Duo Rest API

# Setup

## Configuration
Pass integration key, secret key and host into the RestApiService constructor.
QueryDuoApi runs the get or post and passes back the results


## Third-Party Libraries
- HttpWebAdapters project from https://github.com/mausch/SolrNet
- Moq
- Nunit

# Example
<<<<<<< HEAD
An example call:

const string IntegrationKey2 = "XXXXXXXXXXXXX";  
const string SecretKey2 = "asasasadsdfgsfdgsfdsdgf";  
const string Host2 = "api-sdasdasd.duosecurity.com";
const string Path2 = "/rest/v1/auth";  

public void Call()  
{  
	var duoRestApiService = new RestApiService(IntegrationKey2, SecretKey2, Host2);  
	var adapter = new HttpWebRequestFactory();  
	var result = duoRestApiService.QueryDuoApi(adapter, "https", Path2, HttpWebRequestMethod.POST, GetQueryValues("thedude@gmail.com"));  
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
  
=======
Examples are now stored in the example project - DuoSecurity.Examples
>>>>>>> Moved examples to a project
