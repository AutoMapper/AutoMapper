using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.OData;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using AutoMapperSamples.EF;
using AutoMapperSamples.EF.Dtos;
using AutoMapperSamples.EF.Model;

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
                    .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FullName));
                cfg.CreateMap<Order, OrderDto>()
                    .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.Name));
                
                cfg.CreateMap<CustomerDto, Customer>().ReverseMap();
            });

            context = new TestContext(Effort.DbConnectionFactory.CreateTransient());
        }

        [EnableQuery]
        public IQueryable<OrderDto> Get()
        {
            return context.OrderSet.UseAsDataSource(Mapper.Engine)
                // add an optional exceptionhandler that will be invoked
                // in case an exception is raised upon query execution.
                // otherwise it would get lost on the WebApi side and all we would get would be
                // an unspecific "500 internal server error" response
                .OnError(OnException)
                // add an additional visitor to convert "Queryable.LongCount" calls to "Queryable.Count" calls
                // as EntityFramework does not support this method
                .VisitBeforeMapping(new EntityFrameworkCompatibilityVisitor())
                .For<OrderDto>()
                // modify the enumerated results before returning them to the client
                .OnEnumerated((enumerator) =>
                {
                    // we always pass in an IEnumerator<object> as there could a "select" be issued on the OData client-side
                    // which would cause the type of the IEnumerator interface to not be OrderDTO but some "System.Web.Http.OData.Query.Expressions.SelectExpandBinder+SelectSome"
                    var orderEnumerator = enumerator as IEnumerator<OrderDto>;
                    if (orderEnumerator != null)
                    {
                        // transfers the modified DTOs into a new list
                        // as the LazyEnumerator of EntityFramework does not support a call to "Reset"
                        var list = new List<OrderDto>();
                        while (orderEnumerator.MoveNext())
                        {
                            var dto = orderEnumerator.Current;
                            dto.FullName = "Intercepted: " + dto.FullName;
                            list.Add(dto);
                        }
                        return list.GetEnumerator();
                    }
                    return enumerator;
                })
                .OrderBy(o => o.Price);
        }

        protected override void Dispose(bool disposing)
        {
            context?.Dispose();
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// EntityFramework does not support "LongCount" so we need to rewrite the expression to a call to the "Count" method
    /// </summary>
    public class EntityFrameworkCompatibilityVisitor : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            // replace call to "LongCount" with "Count"            
            if (node.Method.IsGenericMethod && node.Method.DeclaringType == typeof(Queryable) && node.Method.Name == "LongCount")
            {
                var method = node.Method;
                method = node.Method.DeclaringType.GetMethods(BindingFlags.Public | BindingFlags.Static).First(m => m.Name == "Count");
                method = method.MakeGenericMethod(node.Method.GetGenericArguments());
                return Expression.Call(method, node.Arguments);
            }

            // ignore all "Select" calls as we do not support them now
            if (node.Method.DeclaringType == typeof(Queryable) && node.Method.Name == "Select")
            {
                return node.Arguments.First();
            }

            // ignore all "Expand" calls as we do NOT want to support them - do we? :)

            return base.VisitMethodCall(node);
        }
        
    }
}
