using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper.Internal;
using AutoMapper.Mappers;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class MappingToAProtectedCollection : AutoMapperSpecBase
    {
        /// <summary>
        /// 
        /// </summary>
        private Destination _destination;

        /// <summary>
        /// 
        /// </summary>
        public class Source
        {
            /// <summary>
            /// 
            /// </summary>
            public int[] Things { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public int[] Stuff { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class Destination
        {
            private readonly List<int> _things = new List<int>();
            private readonly List<int> _stuff = new List<int>();

            /// <summary>
            /// 
            /// </summary>
            public IEnumerable<int> Things
            {
                get { return _things.Select(x => x); }
                set { }
            }

            /// <summary>
            /// 
            /// </summary>
            public IEnumerable<int> Stuff
            {
                get { return _stuff.Select(x => x); }
                set { }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="thing"></param>
            public void AddThing(int thing)
            {
                _things.Add(thing);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="stuff"></param>
            public void AddStuff(int stuff)
            {
                _stuff.Add(stuff);
            }
        }

        /// <summary>
        /// Maps to a collection that is not directly accessible and must be mutated via Add/Remove methods on the declaring type
        /// IEnumerable{T} Things -> Method: AddThing(T item)
        /// Property: IEnumerable{T} Children -> Method: AddChildren(T item)
        /// </summary>
        public class ProtectedCollectionMapper : IObjectMapper
        {
            private Type EnumerableMapperType { get; } = typeof (EnumerableMapper<>);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public object Map(ResolutionContext context)
            {
                var elementType = context.DestinationType.GetNullEnumerableElementType();

                var enumerableMapper = EnumerableMapperType.MakeGenericType(elementType);

                var objectMapper = (IObjectMapper) Activator.CreateInstance(enumerableMapper);

                var nullDestinationValueSoTheReadOnlyCollectionMapperWorks = context.CreateMemberContext(
                    context.TypeMap, context.SourceValue, null, context.SourceType, context.PropertyMap);

                return objectMapper.Map(nullDestinationValueSoTheReadOnlyCollectionMapperWorks);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public bool IsMatch(ResolutionContext context)
            {
                if (!context.SourceType.IsEnumerableType() || !context.DestinationType.IsEnumerableType())
                    return false;

                var addMethod = GetAddMethod(context);
                return addMethod != null;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            private static MethodInfo GetAddMethod(ResolutionContext context)
            {
                if (context.PropertyMap == null
                    || context.PropertyMap.DestinationProperty == null
                    || context.PropertyMap.DestinationProperty.MemberInfo.DeclaringType == null)
                    return null;

                var addMethodName = GetAddMethodName(context);
                var declaringType = context.PropertyMap.DestinationProperty.MemberInfo.DeclaringType;
                return declaringType.GetMethod(addMethodName);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            private static string GetAddMethodName(ResolutionContext context)
            {
                var memberName = context.MemberName;
                var suffix = memberName.EndsWith("s") ? memberName.Substring(0, memberName.Length - 1) : memberName;

                return $"Add{suffix}";
            }

            #region NestedType: EnumerableMapper

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="TElement"></typeparam>
            private class EnumerableMapper<TElement> : EnumerableMapperBase<IList<TElement>>
            {
                /// <summary>
                /// 
                /// </summary>
                private Action<TElement> _addElement;

                /// <summary>
                /// 
                /// </summary>
                /// <param name="context"></param>
                /// <returns></returns>
                public override bool IsMatch(ResolutionContext context)
                {
                    throw new NotImplementedException();
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="elements"></param>
                /// <param name="mappedValue"></param>
                /// <param name="index"></param>
                protected override void SetElementValue(IList<TElement> elements, object mappedValue, int index)
                {
                    _addElement((TElement) mappedValue);
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="destination"></param>
                /// <returns></returns>
                protected override IList<TElement> GetEnumerableFor(object destination)
                {
                    return null;
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="destElementType"></param>
                /// <param name="sourceLength"></param>
                /// <returns></returns>
                protected override IList<TElement> CreateDestinationObjectBase(Type destElementType, int sourceLength)
                {
                    throw new NotImplementedException();
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="context"></param>
                /// <param name="destinationElementType"></param>
                /// <param name="count"></param>
                /// <param name="mapper"></param>
                /// <returns></returns>
                protected override object CreateDestinationObject(ResolutionContext context, Type destinationElementType,
                    int count, IMappingEngineRunner mapper)
                {
                    throw new NotImplementedException();
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="context"></param>
                /// <param name="mapper"></param>
                /// <param name="destElementType"></param>
                /// <param name="sourceLength"></param>
                /// <returns></returns>
                protected override object GetOrCreateDestinationObject(ResolutionContext context, Type destElementType,
                    int sourceLength)
                {
                    var addMethod = GetAddMethod(context);
                    var parent = context.Parent.Parent.DestinationValue;
                    // relies on ResolutionContext.DestinationValue being updated as values are resolved
                    _addElement = element => addMethod.Invoke(parent, new object[] {element});

                    return null;
                }
            }

            #endregion
        }

        protected override void Establish_context()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.ObjectMappers.Insert(0, new ProtectedCollectionMapper());
                cfg.CreateMap<Source, Destination>();
            });
        }

        protected override void Because_of()
        {
            var source = new Source
            {
                Things = new[] {1, 2, 3, 4},
                Stuff = new[] {5, 6},
            };
            _destination = Mapper.Map<Source, Destination>(source);
        }

        [Fact]
        public void Should_map_the_list_of_source_items()
        {
            _destination.Things.ShouldNotBeNull();
            _destination.Things.ShouldBeOfLength(4);
            _destination.Things.ShouldContain(1);
            _destination.Things.ShouldContain(2);
            _destination.Things.ShouldContain(3);
            _destination.Things.ShouldContain(4);

            _destination.Stuff.ShouldNotBeNull();
            _destination.Stuff.ShouldBeOfLength(2);
            _destination.Stuff.ShouldContain(5);
            _destination.Stuff.ShouldContain(6);
        }
    }
}