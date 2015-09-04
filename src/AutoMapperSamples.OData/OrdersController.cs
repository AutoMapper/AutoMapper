using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData;
using AutoMapper;
using AutoMapperSamples.EF;

namespace AutoMapperSamples.OData
{
    /// <summary>
    /// Controller returning OrderDTOs from an Effort-based EF6 DbContext
    /// see: http://www.asp.net/web-api/overview/odata-support-in-aspnet-web-api/supporting-odata-query-options
    /// </summary>
    public class OrdersController : ApiController
    {
        private TestContext context = null;
        internal static Action<Exception> OnException { get; set; }

        public OrdersController()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<OrderDto, Order>()
                    .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FullName))
                    .ReverseMap(); // reverse map added
            });

            context = new TestContext(Effort.DbConnectionFactory.CreateTransient());
        }

        [EnableQuery]
        public IQueryable<OrderDto> Get()
        {
            // return lazily mapped query
            return MappedQueryProvider<OrderDto, Order>.Map<OrderDto>(context.OrderSet.OrderBy(o => o.Price), Mapper.Engine, OnException);
        }

        protected override void Dispose(bool disposing)
        {
            if(context != null)
                context.Dispose();
            base.Dispose(disposing);
        }
    }
}
