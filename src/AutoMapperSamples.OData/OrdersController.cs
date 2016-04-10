using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.OData;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using AutoMapperSamples.EF;
using AutoMapperSamples.EF.Dtos;

namespace AutoMapperSamples.OData
{
    /// <summary>
    /// Controller returning OrderDTOs from an Effort-based EF6 DbContext
    /// see: http://www.asp.net/web-api/overview/odata-support-in-aspnet-web-api/supporting-odata-query-options
    /// </summary>
    public class OrdersController : ApiController
    {
        private TestDbContext context = null;
        internal static Action<Exception> OnException { get; set; }

        public OrdersController()
        {
            Mapper.Initialize(MappingConfiguration.Configure);

            context = new TestDbContext(Effort.DbConnectionFactory.CreateTransient());
        }

        [EnableQuery]
        public IQueryable<OrderDto> Get()
        {
            return context.OrderSet.Include("Customer").UseAsDataSource(Mapper.Instance)
                // add an optional exceptionhandler that will be invoked
                // in case an exception is raised upon query execution.
                // otherwise it would get lost on the WebApi side and all we would get would be
                // an unspecific "500 internal server error" response
                .OnError(OnException)
                // add an additional visitor to convert "Queryable.LongCount" calls to "Queryable.Count" calls
                // as EntityFramework does not support this method
                .BeforeProjection(new EntityFrameworkCompatibilityVisitor())
                .For<OrderDto>()
                // modify the enumerated results before returning them to the client
                .OnEnumerated((enumerator) =>
                {
                    // we always pass in an IEnumerator<object> as there could a "select" be issued on the OData client-side
                    // which would cause the type of the IEnumerator interface to not be OrderDTO but some "System.Web.Http.OData.Query.Expressions.SelectExpandBinder+SelectSome"
                    foreach (var dto in enumerator.OfType<OrderDto>())
                    {
                        // modify one of the DTOs
                        dto.FullName = "Intercepted: " + dto.FullName;
                    }

                    // load additionl propeties from database
                    var customerIds = enumerator.OfType<OrderDto>().Select(o => o.Customer.Id);
                    // add IDs of orders
                    var customersOrders = context.CustomerSet
                                                    .Include("Orders")
                                                    .Where(c => customerIds.Contains(c.Id))
                                                    .Select(c => new { CustomerId = c.Id, OrderIds = c.Orders.Select(o => o.Id) })
                                                    .ToDictionary(c => c.CustomerId);
                    // apply the list of IDs to each OrderDto
                    foreach (var order in enumerator.OfType<OrderDto>())
                    {
                        if (customersOrders.ContainsKey(order.Customer.Id))
                            order.Customer.Orders = customersOrders[order.Customer.Id].OrderIds.ToArray();
                    }
                })
                .OrderBy(o => o.Price);
        }

        protected override void Dispose(bool disposing)
        {
            context?.Dispose();

            base.Dispose(disposing);
        }
    }
}
