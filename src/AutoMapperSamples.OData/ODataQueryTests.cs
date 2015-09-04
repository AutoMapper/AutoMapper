using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapperSamples.EF;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AutoMapperSamples.OData
{
    /// <summary>
    /// Proves that MappedQuery can be exposed as IQueryable in an WebApi 2 OData Controller
    /// uses self-hosted controller: http://www.asp.net/web-api/overview/hosting-aspnet-web-api/use-owin-to-self-host-web-api
    /// </summary>
    public class ODataQueryTests
    {
        private IDisposable _webApp;
        private string _baseAddress;
        private List<Exception> _exceptions;

        [SetUp]
        public void SetUp()
        {
            _baseAddress = "http://localhost:9000/";
            _exceptions = new List<Exception>();
            OrdersController.OnException = (x) => _exceptions.Add(x);

            // Start OWIN host 
            _webApp = WebApp.Start<Startup>(url: _baseAddress);
        }

        [TearDown]
        public void TearDown()
        {
            _webApp.Dispose();
        }

        [Test]
        public void CanGetAllOrders()
        {
            // Arrange
            HttpClient client = new HttpClient();

            // Act
            var response = client.GetAsync(_baseAddress + "api/Orders").Result;
            
            // Assert
            Console.WriteLine(response);
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            if (_exceptions.Count != 0)
                Assert.Fail(_exceptions.First().Message + "\n" + _exceptions.First().StackTrace);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var dtos = JsonConvert.DeserializeObject<OrderDto[]>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(3, dtos.Length);
        }
        [Test]
        public void CanSkipAndTake()
        {
            // Arrange
            HttpClient client = new HttpClient();

            // Act
            var response = client.GetAsync(_baseAddress + "api/Orders?$skip=1&$top=1").Result;

            // Assert
            Console.WriteLine(response);
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            if (_exceptions.Count != 0)
                Assert.Fail(_exceptions.First().Message + "\n" + _exceptions.First().StackTrace);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var dtos = JsonConvert.DeserializeObject<OrderDto[]>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(1, dtos.Length, "medium priced dto should be there");
        }

        [Test]
        public void CanOrderSkipAndTake()
        {
            // Arrange
            HttpClient client = new HttpClient();

            // Act
            var response = client.GetAsync(_baseAddress + "api/Orders?$skip=1&$top=1&$orderby=Price").Result;

            // Assert
            Console.WriteLine(response);
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            if (_exceptions.Count != 0)
                Assert.Fail(_exceptions.First().Message + "\n" + _exceptions.First().StackTrace);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var dtos = JsonConvert.DeserializeObject<OrderDto[]>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(1, dtos.Length, "medium priced dto should be there");
            Assert.AreEqual(85D, dtos.Single().Price);
        }

        [Test]
        public void CanFilter_FullNameEndsWith()
        {
            // Arrange
            HttpClient client = new HttpClient();

            // Act
            var response = client.GetAsync(_baseAddress + "api/Orders?$filter=endswith(FullName,'Bestellung')").Result;
            
            // Assert
            Console.WriteLine(response);
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            if(_exceptions.Count != 0)
                Assert.Fail(_exceptions.First().Message+"\n"+_exceptions.First().StackTrace);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var dtos = JsonConvert.DeserializeObject<OrderDto[]>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(2, dtos.Length, "should be filtered to two dtos");
        }
        [Test]
        public void CanFilter_NotFullNameEndsWith()
        {
            // Arrange
            HttpClient client = new HttpClient();

            // Act
            var response = client.GetAsync(_baseAddress + "api/Orders?$filter=not endswith(FullName,'Bestellung')").Result;

            // Assert
            Console.WriteLine(response);
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            if (_exceptions.Count != 0)
                Assert.Fail(_exceptions.First().Message + "\n" + _exceptions.First().StackTrace);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var dtos = JsonConvert.DeserializeObject<OrderDto[]>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(1, dtos.Length, "should be filtered to one dto");
        }

        [Test]
        public void CanFilter_FullNameEquals()
        {
            // Arrange
            HttpClient client = new HttpClient();

            // Act
            var response = client.GetAsync(_baseAddress + "api/Orders?$filter=FullName eq 'Zalando Bestellung'").Result;

            // Assert
            Console.WriteLine(response);
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            if (_exceptions.Count != 0)
                Assert.Fail(_exceptions.First().Message + "\n" + _exceptions.First().StackTrace);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var dtos = JsonConvert.DeserializeObject<OrderDto[]>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(1, dtos.Length, "should be filtered to one dtos");
        }

        [Test]
        public void CanFilter_PriceGt()
        {
            // Arrange
            HttpClient client = new HttpClient();

            // Act
            var response = client.GetAsync(_baseAddress + "api/Orders?$filter=Price gt 75.0").Result;

            // Assert
            Console.WriteLine(response);
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            if (_exceptions.Count != 0)
                Assert.Fail(_exceptions.First().Message + "\n" + _exceptions.First().StackTrace);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var dtos = JsonConvert.DeserializeObject<OrderDto[]>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(2, dtos.Length, "should be filtered to two dtos");
        }

        [Test]
        public void CanFilter_NotFullNameEquals()
        {
            // Arrange
            HttpClient client = new HttpClient();

            // Act
            var response = client.GetAsync(_baseAddress + "api/Orders?$filter=FullName ne 'Zalando Bestellung'").Result;

            // Assert
            Console.WriteLine(response);
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            if (_exceptions.Count != 0)
                Assert.Fail(_exceptions.First().Message + "\n" + _exceptions.First().StackTrace);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var dtos = JsonConvert.DeserializeObject<OrderDto[]>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(2, dtos.Length, "should be filtered to two dtos");
        }
    }
}
