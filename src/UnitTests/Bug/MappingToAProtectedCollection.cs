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
		private Destination _destination;

		public class Source
		{
			public int[] Things { get; set; }
			public int[] Stuff { get; set; }
		}

		public class Destination
		{
			private readonly List<int> _things = new List<int>();
			private readonly List<int> _stuff = new List<int>();

			public IEnumerable<int> Things
			{
				get { return _things.Select(x => x); }
				set{ }
			}

			public IEnumerable<int> Stuff
			{
				get { return _stuff.Select(x => x); }
				set { }
			} 

			public void AddThing(int thing)
			{
				_things.Add(thing);
			}

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
			public object Map(ResolutionContext context, IMappingEngineRunner mapper)
			{
				Type genericType = typeof(EnumerableMapper<>);

				var elementType = TypeHelper.GetElementType(context.DestinationType);

				var enumerableMapper = genericType.MakeGenericType(elementType);

				var objectMapper = (IObjectMapper)Activator.CreateInstance(enumerableMapper);

				var nullDestinationValueSoTheReadOnlyCollectionMapperWorks = context.CreateMemberContext(context.TypeMap, context.SourceValue, null, context.SourceType, context.PropertyMap);

				return objectMapper.Map(nullDestinationValueSoTheReadOnlyCollectionMapperWorks, mapper);
			}

			public bool IsMatch(ResolutionContext context)
			{
				if (!context.SourceType.IsEnumerableType() || !context.DestinationType.IsEnumerableType())
					return false;

				MethodInfo addMethod = GetAddMethod(context);
				return addMethod != null;
			}

			private static MethodInfo GetAddMethod(ResolutionContext context)
			{
				if (context.PropertyMap == null || context.PropertyMap.DestinationProperty == null || context.PropertyMap.DestinationProperty.MemberInfo.DeclaringType == null)
					return null;

				string addMethodName = GetAddMethodName(context);
				Type declaringType = context.PropertyMap.DestinationProperty.MemberInfo.DeclaringType;
				return declaringType.GetMethod(addMethodName);
			}

			private static string GetAddMethodName(ResolutionContext context)
			{
				string memberName = context.MemberName;
				string suffix = memberName.EndsWith("s") ? memberName.Substring(0, memberName.Length - 1) : memberName;

				return string.Format("Add{0}", suffix);
			}

			#region NestedType: EnumerableMapper
			private class EnumerableMapper<TElement> : EnumerableMapperBase<IList<TElement>>
			{
				private Action<TElement> _addElement;

				public override bool IsMatch(ResolutionContext context)
				{
					throw new NotImplementedException();
				}

				protected override void SetElementValue(IList<TElement> elements, object mappedValue, int index)
				{
					_addElement((TElement)mappedValue);
				}

				protected override IList<TElement> GetEnumerableFor(object destination)
				{
					return null;
				}

				protected override IList<TElement> CreateDestinationObjectBase(Type destElementType, int sourceLength)
				{
					throw new NotImplementedException();
				}

				protected override object CreateDestinationObject(ResolutionContext context, Type destinationElementType, int count, IMappingEngineRunner mapper)
				{
					throw new NotImplementedException();
				}

				protected override object GetOrCreateDestinationObject(ResolutionContext context, IMappingEngineRunner mapper, Type destElementType, int sourceLength)
				{
					MethodInfo addMethod = GetAddMethod(context);
					object parent = context.Parent.Parent.DestinationValue; // relies on ResolutionContext.DestinationValue being updated as values are resolved
					_addElement = element => addMethod.Invoke(parent, new object[] { element });

					return null;
				}
			}
			#endregion
		}

		protected override void Establish_context()
		{
            Mapper.Initialize(cfg =>
            {
                MapperRegistry.Mappers.Insert(0, new ProtectedCollectionMapper());
                cfg.CreateMap<Source, Destination>();
            });
		}

		protected override void Because_of()
		{
			var source = new Source
			{
				Things = new[] { 1, 2, 3, 4 },
				Stuff = new[] { 5, 6 },
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