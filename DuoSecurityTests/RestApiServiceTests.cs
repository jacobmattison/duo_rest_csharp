using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using DuoVerificationService;
using HttpWebAdapters;
using Moq;
using NUnit.Framework;

namespace DuoSecurityTests
{
    [TestFixture]
    public class RestApiServiceTests
    {
        const string IntegrationKey = "1234";
        const string SecretKey = "abcd";
        const string Host = "api-xxxxxxxx.duosecurity.com";


        [Test]
        public void AuthorizationKeyShouldBeCorrectlyCalculated()
        {
            // Arrange
            // Act
            var sut = new RestApiService(IntegrationKey, SecretKey, Host);
            var result = sut.GetAuthorizationKey(HttpWebRequestMethod.POST, "/rest/v1/auth", "auto=phone1&factor=auto&ipaddr=141.213.231.43&user=bob");

            // Assert
            Assert.That(result, Is.EqualTo("MTIzNDpmNDYzM2YzZWNhOGQ2YmFmNzk5Mzc5ZDgzMGM3YmU2ODA1Y2FkMWQ2"));
        }

        [Test]
        public void CreateSignedDuoWebRequestShouldReturnAWebRequestWhoseHeaderHasHadAuthorizationAddedTo()
        {
            // Arrange
            var queryItems = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("auto", "phone1"),
                    new KeyValuePair<string, string>("factor", "auto"),
                    new KeyValuePair<string, string>("ipaddr", "141.213.231.43"),
                    new KeyValuePair<string, string>("user", "bob")
                };

            var mockWebRequest = new Mock<IHttpWebRequest>(MockBehavior.Strict);
            mockWebRequest.SetupSet(o => o.Method = HttpWebRequestMethod.POST).Verifiable("Method not correctly set");
            mockWebRequest.SetupSet(o => o.Accept = "application/json").Verifiable("Json not set as acceptable");
            mockWebRequest.SetupSet(o => o.Timeout = 180000).Verifiable("Timeout is not set");
            mockWebRequest.SetupSet(o => o.ContentType = "application/x-www-form-urlencoded").Verifiable("Form content type is not set");
            mockWebRequest.Setup(o => o.Headers.Add("Authorization", "Basic MTIzNDpmNDYzM2YzZWNhOGQ2YmFmNzk5Mzc5ZDgzMGM3YmU2ODA1Y2FkMWQ2")).Verifiable("Authorization header not added");
            mockWebRequest.Setup(o => o.GetRequestStream()).Returns(new MemoryStream()).Verifiable("Request stream is not returned");

            var mockWebRequestFactory = new Mock<IHttpWebRequestFactory>(MockBehavior.Strict);
            mockWebRequestFactory.Setup(o => o.Create(new Uri("https://api-xxxxxxxx.duosecurity.com/rest/v1/auth"))).Returns(mockWebRequest.Object).Verifiable("Uri was incorrect");

            // Act
            var sut = new RestApiService(IntegrationKey, SecretKey, Host);
            sut.CreateSignedDuoWebRequest(mockWebRequestFactory.Object, "https", "/rest/v1/auth", HttpWebRequestMethod.POST, queryItems);

            // Assert
            mockWebRequestFactory.Verify();
            mockWebRequest.Verify();
        }

        [Test]
        public void QueryDuoApiShouldReturnObjectFromJsonStream()
        {
            // Arrange
            var mockWebResponse = new Mock<IHttpWebResponse>(MockBehavior.Strict);
            mockWebResponse.Setup(o => o.GetResponseStream()).Returns(new MemoryStream()).Verifiable();

            var mockWebRequest = new Mock<IHttpWebRequest>(MockBehavior.Strict);
            mockWebRequest.Setup(o => o.GetResponse()).Returns(mockWebResponse.Object);


            // Act
            var sut = new RestApiService(IntegrationKey, SecretKey, Host);
            var result = sut.QueryDuoApi(mockWebRequest.Object);

            // Assert
            mockWebRequest.Verify();
        }

        [Test]
        public void QueryDuoApiShouldResturnNullIfGetResponseReturnsNull()
        {
            // Arrange
            var mockWebRequest = new Mock<IHttpWebRequest>();
            mockWebRequest.Setup(o => o.GetResponse()).Returns((IHttpWebResponse)null);


            // Act
            var sut = new RestApiService(IntegrationKey, SecretKey, Host);
            var result = sut.QueryDuoApi(mockWebRequest.Object);

            // Assert
            Assert.That(result,Is.Null);
  
        }

        [Test]
        public void QueryDuoApiShouldResturnNullIfGetResponseStreamReturnsNull()
        {
            // Arrange
            var mockWebResponse = new Mock<IHttpWebResponse>();
            mockWebResponse.Setup(o => o.GetResponseStream()).Returns((Stream)null);

            var mockWebRequest = new Mock<IHttpWebRequest>();
            mockWebRequest.Setup(o => o.GetResponse()).Returns(mockWebResponse.Object);

            // Act
            var sut = new RestApiService(IntegrationKey, SecretKey, Host);
            var result = sut.QueryDuoApi(mockWebRequest.Object);

            // Assert
            Assert.That(result, Is.Null);

        }

        [Test]
        public void QueryDuoApiShouldReturnNullIfThereIsAnException()
        {
            // Arrange
            var mockWebRequest = new Mock<IHttpWebRequest>();
            mockWebRequest.Setup(o => o.GetResponse()).Throws(new WebException());


            // Act
            var sut = new RestApiService(IntegrationKey, SecretKey, Host);
            var result = sut.QueryDuoApi(mockWebRequest.Object);

            // Assert
            Assert.That(result, Is.Null);
  
        }
    }
}
