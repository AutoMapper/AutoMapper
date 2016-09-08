using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using AutoMapperSamples.EF;
using AutoMapperSamples.EF.Dtos;
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
        private bool _wasEnabled;

        [SetUp]
        public virtual void SetUp()
        {
            _wasEnabled = EF.TestDbContext.DynamicProxiesEnabled;
            EF.TestDbContext.DynamicProxiesEnabled = EnableDynamicProxies;

            _baseAddress = "http://localhost:9000/";
            _exceptions = new List<Exception>();
            OrdersController.OnException = (x) => _exceptions.Add(x);

            // Start OWIN host 
            _webApp = WebApp.Start<Startup>(url: _baseAddress);
        }
        
        [TearDown]
        public virtual void TearDown()
        {
            _webApp.Dispose();
            EF.TestDbContext.DynamicProxiesEnabled = _wasEnabled;
        }

        protected virtual bool EnableDynamicProxies { get { return false; } }

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
            foreach (var dto in dtos)
            {
                Assert.IsNotNull(dto.Customer);
                Assert.IsNotEmpty(dto.Customer.Orders);
            }
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
            foreach (var dto in dtos)
            {
                Assert.IsNotNull(dto.Customer);
                Assert.IsNotEmpty(dto.Customer.Orders);
            }
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
            foreach (var dto in dtos)
            {
                Assert.IsNotNull(dto.Customer);
                Assert.IsNotEmpty(dto.Customer.Orders);
            }
        }

        [Test]
        public void CanSkipAndTake_AndGetTotalCount()
        {
            // Arrange
            HttpClient client = new HttpClient();

            // Act
            var response = client.GetAsync(_baseAddress + "api/Orders?$inlinecount=allpages&$skip=1&$top=1&$orderby=Price").Result;

            // Assert
            Console.WriteLine(response);
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            if (_exceptions.Count != 0)
                Assert.Fail(_exceptions.First().Message + "\n" + _exceptions.First().StackTrace);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var dtos = JsonConvert.DeserializeObject<OrderDto[]>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(1, dtos.Length, "medium priced dto should be there");
            Assert.AreEqual(85D, dtos.Single().Price);
            foreach (var dto in dtos)
            {
                Assert.IsNotNull(dto.Customer);
                Assert.IsNotEmpty(dto.Customer.Orders);
            }
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

            if (_exceptions.Count != 0)
                Assert.Fail(_exceptions.First().Message + "\n" + _exceptions.First().StackTrace);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var dtos = JsonConvert.DeserializeObject<OrderDto[]>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(2, dtos.Length, "should be filtered to two dtos");
            foreach (var dto in dtos)
            {
                Assert.IsNotNull(dto.Customer);
                Assert.IsNotEmpty(dto.Customer.Orders);
            }
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
            foreach (var dto in dtos)
            {
                Assert.IsNotNull(dto.Customer);
                Assert.IsNotEmpty(dto.Customer.Orders);
            }
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
            foreach (var dto in dtos)
            {
                Assert.IsNotNull(dto.Customer);
                Assert.IsNotEmpty(dto.Customer.Orders);
            }
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
            foreach (var dto in dtos)
            {
                Assert.IsNotNull(dto.Customer);
                Assert.IsNotEmpty(dto.Customer.Orders);
            }
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
            foreach (var dto in dtos)
            {
                Assert.IsNotNull(dto.Customer);
                Assert.IsNotEmpty(dto.Customer.Orders);
            }
        }

        [Test]
        public void CanFilter_ReturnsDtosThatWereModified_ByEnumeratorInterceptor()
        {
            // Arrange
            HttpClient client = new HttpClient();

            // Act
            var response = client.GetAsync(_baseAddress + "api/Orders?$filter=endswith(FullName,'Bestellung')").Result;

            // Assert
            Console.WriteLine(response);
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            if (_exceptions.Count != 0)
                Assert.Fail(_exceptions.First().Message + "\n" + _exceptions.First().StackTrace);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var dtos = JsonConvert.DeserializeObject<OrderDto[]>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(2, dtos.Length, "should be filtered to two dtos");
            foreach (var dto in dtos)
            {
                Assert.IsTrue(dto.FullName.StartsWith("Intercepted:"), "dto {0} was not intercepted", dto.FullName);
                Assert.IsNotNull(dto.Customer);
                Assert.IsNotEmpty(dto.Customer.Orders);
            }
        }

        [Test]
        public void CanFilter_OrderDateGt()
        {
            // Arrange
            HttpClient client = new HttpClient();

            // Act
            var response = client.GetAsync(_baseAddress + "api/Orders?$filter=OrderDate gt DateTime'2015-02-01T00:00:00'").Result;
            
            // Assert
            Console.WriteLine(response);
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            if (_exceptions.Count != 0)
                Assert.Fail(_exceptions.First().Message + "\n" + _exceptions.First().StackTrace);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var dtos = JsonConvert.DeserializeObject<OrderDto[]>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(2, dtos.Length, "should be filtered to two dtos");
            foreach (var dto in dtos)
            {
                Assert.IsNotNull(dto.Customer);
                Assert.IsNotEmpty(dto.Customer.Orders);
            }
        }

        [Test]
        [Ignore("Not supported yet")]
        public void SupportsProjection()
        {
            // Arrange
            HttpClient client = new HttpClient();

            // Act
            var response = client.GetAsync(_baseAddress + "api/Orders?$filter=endswith(FullName,'Bestellung')&$select=Price,FullName").Result;

            // Assert
            Console.WriteLine(response);
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            if (_exceptions.Count != 0)
                Assert.Fail(_exceptions.First().Message + "\n" + _exceptions.First().StackTrace);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var dtos = JsonConvert.DeserializeObject<OrderDto[]>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(2, dtos.Length, "should be filtered to two dtos");
            foreach (var dto in dtos)
            {
                Assert.IsTrue(dto.FullName.StartsWith("Intercepted:"), "dto {0} was not intercepted", dto.FullName);
            }
        }

        [Test]
        [Ignore("Not supported yet")]
        public void SupportsExpand()
        {
            // Arrange
            HttpClient client = new HttpClient();

            // Act
            var response = client.GetAsync(_baseAddress + "api/Orders?$filter=endswith(FullName,'Bestellung')&$expand=Customer").Result;

            // Assert
            Console.WriteLine(response);
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            if (_exceptions.Count != 0)
                Assert.Fail(_exceptions.First().Message + "\n" + _exceptions.First().StackTrace);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var dtos = JsonConvert.DeserializeObject<OrderDto[]>(response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(2, dtos.Length, "should be filtered to two dtos");
            foreach (var dto in dtos)
            {
                Assert.IsTrue(dto.FullName.StartsWith("Intercepted:"), "dto {0} was not intercepted", dto.FullName);
            }
        }
    }
}
