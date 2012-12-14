using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using DuoSecurity.Examples;
using DuoVerificationService;
using HttpWebAdapters;
using Moq;
using NUnit.Framework;

namespace DuoSecurityTests
{
    [TestFixture]
    public class RestApiServiceTests
    {
        const string IntegrationKey = "DI8H9RXW9RFXGRGG4E64";
        const string SecretKey = "HlRW6cH7dcYe5SDUpKpQ9Q0pXKPoLlZNVCAo0qEF";
        const string Host = "api-f8aa1baa.duosecurity.com";
        private Mock<IHttpWebRequest> _mockWebRequest;
        private Mock<IHttpWebRequestFactory> _mockWebRequestFactory;
        private List<KeyValuePair<string, string>> _queryItems;

        [SetUp]
        public void Setup()
        {
            _queryItems = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("auto", "phone1"),
                    new KeyValuePair<string, string>("factor", "auto"),
                    new KeyValuePair<string, string>("ipaddr", "141.213.231.43"),
                    new KeyValuePair<string, string>("user", "bob")
                };

            _mockWebRequest = new Mock<IHttpWebRequest>(MockBehavior.Strict);
            _mockWebRequest.SetupSet(o => o.Method = HttpWebRequestMethod.POST).Verifiable("Method not correctly set");
            _mockWebRequest.SetupSet(o => o.Accept = "application/json").Verifiable("Json not set as acceptable");
            _mockWebRequest.SetupSet(o => o.Timeout = 180000).Verifiable("Timeout is not set");
            _mockWebRequest.SetupSet(o => o.ContentType = "application/x-www-form-urlencoded").Verifiable("Form content type is not set");
            _mockWebRequest.Setup(o => o.Headers.Add("Authorization", "Basic MTIzNDpmNDYzM2YzZWNhOGQ2YmFmNzk5Mzc5ZDgzMGM3YmU2ODA1Y2FkMWQ2")).Verifiable("Authorization header not added");
            _mockWebRequest.Setup(o => o.GetRequestStream()).Returns(new MemoryStream()).Verifiable("Request stream is not returned");

            _mockWebRequestFactory = new Mock<IHttpWebRequestFactory>(MockBehavior.Strict);
            _mockWebRequestFactory.Setup(o => o.Create(new Uri("https://api-xxxxxxxx.duosecurity.com/rest/v1/auth"))).Returns(_mockWebRequest.Object).Verifiable("Uri was incorrect");

        }

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
            var mockWebResponse = new Mock<IHttpWebResponse>(MockBehavior.Strict);
            mockWebResponse.Setup(o => o.GetResponseStream()).Returns(new MemoryStream());
            _mockWebRequest.Setup(o => o.GetResponse()).Returns(mockWebResponse.Object);

            // Act
            var sut = new RestApiService(IntegrationKey, SecretKey, Host);
            sut.QueryDuoApi(_mockWebRequestFactory.Object, "https", "/rest/v1/auth", HttpWebRequestMethod.POST, _queryItems);

            // Assert
            _mockWebRequestFactory.Verify();
            _mockWebRequest.Verify();
        }

        [Test]
        public void QueryDuoApiShouldReturnObjectFromJsonStream()
        {
            // Arrange
            var mockWebResponse = new Mock<IHttpWebResponse>(MockBehavior.Strict);
            mockWebResponse.Setup(o => o.GetResponseStream()).Returns(new MemoryStream()).Verifiable();
            _mockWebRequest.Setup(o => o.GetResponse()).Returns(mockWebResponse.Object);

            // Act
            var sut = new RestApiService(IntegrationKey, SecretKey, Host);
            sut.QueryDuoApi(_mockWebRequestFactory.Object, "https", "/rest/v1/auth", HttpWebRequestMethod.POST, _queryItems);

            // Assert
            _mockWebRequest.Verify();
        }

        [Test]
        public void QueryDuoApiShouldResturnNullIfGetResponseReturnsNull()
        {
            // Arrange
            _mockWebRequest.Setup(o => o.GetResponse()).Returns((IHttpWebResponse)null);

            // Act
            var sut = new RestApiService(IntegrationKey, SecretKey, Host);
            var result = sut.QueryDuoApi(_mockWebRequestFactory.Object, "https", "/rest/v1/auth", HttpWebRequestMethod.POST, _queryItems);

            // Assert
            Assert.That(result, Is.Null);

        }

        [Test]
        public void QueryDuoApiShouldResturnNullIfGetResponseStreamReturnsNull()
        {
            // Arrange
            var mockWebResponse = new Mock<IHttpWebResponse>();
            mockWebResponse.Setup(o => o.GetResponseStream()).Returns((Stream)null);
            _mockWebRequest.Setup(o => o.GetResponse()).Returns(mockWebResponse.Object);

            // Act
            var sut = new RestApiService(IntegrationKey, SecretKey, Host);
            var result = sut.QueryDuoApi(_mockWebRequestFactory.Object, "https", "/rest/v1/auth", HttpWebRequestMethod.POST, _queryItems);

            // Assert
            Assert.That(result, Is.Null);

        }

        [Test]
        public void QueryDuoApiShouldReturnNullIfThereIsAnException()
        {
            // Arrange
            _mockWebRequest.Setup(o => o.GetResponse()).Throws(new WebException());

            // Act
            var sut = new RestApiService(IntegrationKey, SecretKey, Host);
            var result = sut.QueryDuoApi(_mockWebRequestFactory.Object, "https", "/rest/v1/auth", HttpWebRequestMethod.POST, _queryItems);

            // Assert
            Assert.That(result, Is.Null);

        }

        [Test]
        public void test()
        {
            var sut = new BasicExample();
            sut.RunAuthorization();

            //var sut = new RestApiService(IntegrationKey, SecretKey, Host);
            //var result = sut.GetAuthorizationKey(HttpWebRequestMethod.POST, "/verify/v1/call", "message=Your%20PIN%20is%20%3Cpin%3E&phone=%2B15555555555");

            //// Assert
            //Assert.That(result, Is.EqualTo("MTIzNDo2NjRlOTE1MDgyYTI0YmZhMjBiNzQ0YzY3NThmNGZmZGU3MzhmMjBi"));
        }

        [Test]
        public void AuthorizationKeyShouldBeCorrectlyCalculated2()
        {
            // Arrange
            // Act
            var sut = new RestApiService("DIA1AQJCU97DCLD11AZE", "TZOiTvqx3xb8VuBBaF7ewtYSsqnfUfTq8V6W3EsT", "api-f8aa1baa.duosecurity.com");
            var result = sut.GetAuthorizationKey(HttpWebRequestMethod.POST, "/verify/v1/call", "message=the%20pin%20is%20%3Cpin%3E&phone=%2B447952556282");

            // Assert
            Assert.That(result, Is.EqualTo("RElBMUFRSkNVOTdEQ0xEMTFBWkU6ZTgxMjQwMGY2NjhhNTc3NDQ0MmQ0NGUyMGYxZTA4YmUzZWQzZWZhMA=="));
        }

    }
}
