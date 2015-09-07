using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapperSamples.Breeze.Dto;
using Breeze.Sharp;
using Microsoft.Owin.Hosting;
using NUnit.Framework;

namespace AutoMapperSamples.Breeze
{
    [TestFixture]
    public class BreezeQueryTests
    {
        private string _baseAddress;
        private IDisposable _webApp;
        private EntityManager _entityManager;
        private List<Exception> _exceptions;

        [SetUp]
        public void SetUp()
        {
            // wire up exceptionhandler for unittesting purpose
            _exceptions = new List<Exception>();
            OrdersController.OnException = (x) => _exceptions.Add(x);

            _baseAddress = "http://localhost:9000";

            // Start OWIN host 
            _webApp = WebApp.Start<Startup>(url: _baseAddress);
            
            _entityManager = new EntityManager($"{_baseAddress}/breeze/orders");

            // setting up BreezeSharp - http://breeze.github.io/doc-cs/get-feet-wet.html
            Configuration.Instance.ProbeAssemblies(typeof(AutoMapperSamples.Breeze.Dto.OrderDto).Assembly);
        }
        
        [TearDown]
        public void TearDown()
        {
            _webApp.Dispose();
        }

        [Test]
        public async Task CanFetchMetadata()
        {
            var ds = await _entityManager.FetchMetadata();
            Assert.IsNotNull(ds);
        }

        [Test]
        public async Task CanGetAllOrders()
        {
            // Arrange
            await InitializeWorkaroundForBreezeSharpBug();

            // Act
            var dtos = await new EntityQuery<OrderDto>("orders").Execute(_entityManager);

            // Assert
            Assert.AreEqual(3, dtos.Count(), "should be filtered to two dtos");
            foreach (var dto in dtos)
            {
                Assert.IsTrue(dto.FullName.StartsWith("Intercepted:"), "dto {0} was not intercepted", dto.FullName);
                Assert.IsNotNull(dto.Customer);
                Assert.IsNotEmpty(dto.Customer.Orders);
            }
        }

        [Test]
        public async Task CanSkipAndTake()
        {
            // Arrange
            await InitializeWorkaroundForBreezeSharpBug();

            // Act
            var dtos = await new EntityQuery<OrderDto>("orders")
                .Skip(1).Take(1)
                .Execute(_entityManager);

            // Assert

            if (_exceptions.Count != 0)
                Assert.Fail(_exceptions.First().Message + "\n" + _exceptions.First().StackTrace);
            
            Assert.AreEqual(1, dtos.Count(), "medium priced dto should be there");
            foreach (var dto in dtos)
            {
                Assert.IsNotNull(dto.Customer);
                Assert.IsNotEmpty(dto.Customer.Orders);
            }
        }

        [Test]
        public async Task CanOrderSkipAndTake()
        {
            // Arrange
            await InitializeWorkaroundForBreezeSharpBug();

            // Act
            var dtos = await new EntityQuery<OrderDto>("orders")
                .OrderBy(o => o.Price)
                .Skip(1).Take(1)
                .Execute(_entityManager);

            // Assert

            if (_exceptions.Count != 0)
                Assert.Fail(_exceptions.First().Message + "\n" + _exceptions.First().StackTrace);

            Assert.AreEqual(1, dtos.Count(), "medium priced dto should be there");
            Assert.AreEqual(85D, dtos.Single().Price);
            foreach (var dto in dtos)
            {
                Assert.IsNotNull(dto.Customer);
                Assert.IsNotEmpty(dto.Customer.Orders);
            }
        }
        
        [Test]
        public async Task CanFilter_FullNameEndsWith()
        {
            // Arrange
            await InitializeWorkaroundForBreezeSharpBug();

            // Act
            var dtos = await new EntityQuery<OrderDto>("orders")
                .Where(o => o.FullName.EndsWith("Bestellung"))
                .Execute(_entityManager);

            // Assert

            if (_exceptions.Count != 0)
                Assert.Fail(_exceptions.First().Message + "\n" + _exceptions.First().StackTrace);

            Assert.AreEqual(2, dtos.Count());
            foreach (var dto in dtos)
            {
                Assert.IsNotNull(dto.Customer);
                Assert.IsNotEmpty(dto.Customer.Orders);
            }
        }

        [Test]
        public async Task CanFilter_NotFullNameEndsWith()
        {
            // Arrange
            await InitializeWorkaroundForBreezeSharpBug();

            // Act
            var dtos = await new EntityQuery<OrderDto>("orders")
                .Where(o => !o.FullName.EndsWith("Bestellung"))
                .Execute(_entityManager);

            // Assert

            if (_exceptions.Count != 0)
                Assert.Fail(_exceptions.First().Message + "\n" + _exceptions.First().StackTrace);

            Assert.AreEqual(1, dtos.Count());
            foreach (var dto in dtos)
            {
                Assert.IsNotNull(dto.Customer);
                Assert.IsNotEmpty(dto.Customer.Orders);
            }
        }

        [Test]
        public async Task CanFilter_FullNameEquals()
        {
            // Arrange
            await InitializeWorkaroundForBreezeSharpBug();

            // Act
            var dtos = await new EntityQuery<OrderDto>("orders")
                .Where(o => o.FullName == "Zalando Bestellung")
                .Execute(_entityManager);

            // Assert

            if (_exceptions.Count != 0)
                Assert.Fail(_exceptions.First().Message + "\n" + _exceptions.First().StackTrace);

            Assert.AreEqual(1, dtos.Count());
            foreach (var dto in dtos)
            {
                Assert.IsNotNull(dto.Customer);
                Assert.IsNotEmpty(dto.Customer.Orders);
            }
        }

        [Test]
        public async Task CanFilter_NotFullNameEquals()
        {
            // Arrange
            await InitializeWorkaroundForBreezeSharpBug();

            // Act
            var dtos = await new EntityQuery<OrderDto>("orders")
                .Where(o => o.FullName != "Zalando Bestellung")
                .Execute(_entityManager);

            // Assert

            if (_exceptions.Count != 0)
                Assert.Fail(_exceptions.First().Message + "\n" + _exceptions.First().StackTrace);

            Assert.AreEqual(2, dtos.Count());
            foreach (var dto in dtos)
            {
                Assert.IsNotNull(dto.Customer);
                Assert.IsNotEmpty(dto.Customer.Orders);
            }
        }

        [Test]
        public async Task CanFilter_PriceGt()
        {
            // Arrange
            await InitializeWorkaroundForBreezeSharpBug();

            // Act
            var dtos = await new EntityQuery<OrderDto>("orders")
                .Where(o => o.Price > 75.0D)
                .Execute(_entityManager);

            // Assert

            if (_exceptions.Count != 0)
                Assert.Fail(_exceptions.First().Message + "\n" + _exceptions.First().StackTrace);

            Assert.AreEqual(2, dtos.Count());
            foreach (var dto in dtos)
            {
                Assert.IsNotNull(dto.Customer);
                Assert.IsNotEmpty(dto.Customer.Orders);
            }
        }
        
        [Test]
        public async Task CanFilter_ReturnsDtosThatWereModified_ByEnumeratorInterceptor()
        {
            // Arrange
            await InitializeWorkaroundForBreezeSharpBug();

            // Act
            var dtos = await new EntityQuery<OrderDto>("orders")
                .Where(o => o.Price > 75.0D)
                .Execute(_entityManager);

            // Assert

            if (_exceptions.Count != 0)
                Assert.Fail(_exceptions.First().Message + "\n" + _exceptions.First().StackTrace);

            Assert.AreEqual(2, dtos.Count(), "should be filtered to two dtos");
            foreach (var dto in dtos)
            {
                Assert.IsTrue(dto.FullName.StartsWith("Intercepted:"), "dto {0} was not intercepted", dto.FullName);
                Assert.IsNotNull(dto.Customer);
                Assert.IsNotEmpty(dto.Customer.Orders);
            }
        }
        
        private async Task InitializeWorkaroundForBreezeSharpBug()
        {
            // BUG-workaround for N A S T Y breezesharp bug that affects scenarios where DTOs and Cotroller are in referenced assemblies *grml*
            // first we fetch the stripped down metadata from the server ("cSpaceOSpaceMapping" removed in OrdersController)
            await _entityManager.FetchMetadata();
            // then we manually set the client->server and server->client namespace mappings as they are totally messed up
            var t = _entityManager.MetadataStore.NamingConvention.GetType();
            var fld = t.GetField("_clientServerNamespaceMap", BindingFlags.Instance | BindingFlags.NonPublic);
            fld.SetValue(_entityManager.MetadataStore.NamingConvention, new Dictionary<string, string>() { { "AutoMapperSamples.Breeze.Dto", "AutoMapperSamples.EF.Dtos" } });
            fld = t.GetField("_serverClientNamespaceMap", BindingFlags.Instance | BindingFlags.NonPublic);
            fld.SetValue(_entityManager.MetadataStore.NamingConvention, new Dictionary<string, string>() { { "AutoMapperSamples.EF.Dtos", "AutoMapperSamples.Breeze.Dto" } });
        }
    }
}
