using System;
using System.Linq;
using System.Web.Http;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using AutoMapperSamples.Breeze.Dto;
using AutoMapperSamples.EF;
using AutoMapperSamples.EF.Dtos;
using Breeze.ContextProvider;
using Breeze.ContextProvider.EF6;
using Breeze.WebApi2;
using Newtonsoft.Json.Linq;
using CustomerDto = AutoMapperSamples.EF.Dtos.CustomerDto;
using OrderDto = AutoMapperSamples.EF.Dtos.OrderDto;

namespace AutoMapperSamples.Breeze
{
    [BreezeController]
    public class OrdersController : ApiController
    {
        EFContextProvider<MetadataOnlyDtoContext> _contextProvider = new EFContextProvider<MetadataOnlyDtoContext>();
        private TestDbContext _context;
        internal static Action<Exception> OnException { get; set; }

        public OrdersController()
        {
            Mapper.Initialize(MappingConfiguration.Configure);
            _context = new TestDbContext(Effort.DbConnectionFactory.CreateTransient());
        }

        // ~/breeze/orders/Metadata 
        [HttpGet]
        public string Metadata()
        {
            // unfortunately BreezeJs itself detects client-side DTOs of BreezeSharp and adds a "cSpaceOSpaceMapping" property to the metadata.
            // this however confuses the BreezeJS client in such a way that it cannot find the client DTO types anymore.
            // therefore we simply remove this property
            var metadata = _contextProvider.Metadata();
            var jobject = JObject.Parse(metadata);
            var schemaObj = (JObject)jobject["schema"];
            schemaObj.Remove("cSpaceOSpaceMapping");
            metadata = jobject.ToString();
            return metadata;
        }

        // ~/breeze/orders/Orders
        // ~/breeze/orders/Orders?$filter=Price eq 85.0&$orderby=FullName 
        [HttpGet]
        public IQueryable<EF.Dtos.OrderDto> Orders()
        {
            return _context.OrderSet.Include("Customer").UseAsDataSource(Mapper.Engine)
                // add an optional exceptionhandler that will be invoked
                // in case an exception is raised upon query execution.
                // otherwise it would get lost on the WebApi side and all we would get would be
                // an unspecific "500 internal server error" response
                .OnError(OnException)
                // trace out original expression
                .TraceSourceExpressionTo(Console.Out)
                // trace out mapped expression
                .TraceDestinationExpressionTo(Console.Out)
                .For<EF.Dtos.OrderDto>()
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
                    var customersOrders = _context.CustomerSet
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

        // ~/breeze/todos/SaveChanges
        [HttpPost]
        public SaveResult SaveChanges(JObject saveBundle)
        {
            return _contextProvider.SaveChanges(saveBundle);
        }

        protected override void Dispose(bool disposing)
        {
            _context?.Dispose();
            base.Dispose(disposing);
        }
    }
}
