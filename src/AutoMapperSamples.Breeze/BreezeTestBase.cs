using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Breeze.Sharp;
using Microsoft.Owin.Hosting;
using NUnit.Framework;

namespace AutoMapperSamples.Breeze
{
    public abstract class BreezeTestBase
    {
        protected string _baseAddress;
        protected IDisposable _webApp;
        protected EntityManager _entityManager;
        protected List<Exception> _exceptions;

        [SetUp]
        public virtual void SetUp()
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
        public virtual void TearDown()
        {
            _webApp.Dispose();
        }
    }
}
