using System;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;

namespace AutoMapper.Mappers
{
	public class DataReaderMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			if (IsDataReader(context))
			{
				var dataReader = (IDataReader)context.SourceValue;
                var destinationElementType = TypeHelper.GetElementType(context.DestinationType);
			    var resolveUsingContext = context;
                if (context.TypeMap == null)
                {
                    var configurationProvider = ((MappingEngine)(Mapper.Engine)).ConfigurationProvider;
                    TypeMap typeMap = configurationProvider.FindTypeMapFor(context.SourceValue, context.SourceType, destinationElementType);
                    resolveUsingContext = new ResolutionContext(typeMap, context.SourceValue, context.SourceType, destinationElementType);
                }
				var buildFrom = CreateBuilder(destinationElementType, dataReader);

				var results = ObjectCreator.CreateList(destinationElementType);
				while (dataReader.Read())
				{
                    var result = buildFrom(dataReader);
                    MapPropertyValues(resolveUsingContext, mapper, result);
                    results.Add(result);
                }

				return results;
			}
			
			if (IsDataRecord(context))
			{
				var dataRecord = context.SourceValue as IDataRecord;
				var buildFrom = CreateBuilder(context.DestinationType, dataRecord);

				var result = buildFrom(dataRecord);
                MapPropertyValues(context, mapper, result);

				return result;
			}

			return null;
		}

		public bool IsMatch(ResolutionContext context)
		{
			return IsDataReader(context) || IsDataRecord(context);
		}

		private static bool IsDataReader(ResolutionContext context)
		{
			return typeof(IDataReader).IsAssignableFrom(context.SourceType) &&
				   context.DestinationType.IsEnumerableType();
		}

		private static bool IsDataRecord(ResolutionContext context)
		{
			return typeof(IDataRecord).IsAssignableFrom(context.SourceType);
		}

		private static Build CreateBuilder(Type destinationType, IDataRecord dataRecord)
		{
			var method = new DynamicMethod("DynamicCreate", destinationType, new[] { typeof(IDataRecord) }, destinationType, true);
			var generator = method.GetILGenerator();

			var result = generator.DeclareLocal(destinationType);
			generator.Emit(OpCodes.Newobj, destinationType.GetConstructor(Type.EmptyTypes));
			generator.Emit(OpCodes.Stloc, result);

			for (var i = 0; i < dataRecord.FieldCount; i++)
			{
				var propertyInfo = destinationType.GetProperty(dataRecord.GetName(i));
				var endIfLabel = generator.DefineLabel();

				if (propertyInfo != null && propertyInfo.GetSetMethod(true) != null)
				{
					generator.Emit(OpCodes.Ldarg_0);
					generator.Emit(OpCodes.Ldc_I4, i);
					generator.Emit(OpCodes.Callvirt, isDBNullMethod);
					generator.Emit(OpCodes.Brtrue, endIfLabel);

					generator.Emit(OpCodes.Ldloc, result);
					generator.Emit(OpCodes.Ldarg_0);
					generator.Emit(OpCodes.Ldc_I4, i);
					generator.Emit(OpCodes.Callvirt, getValueMethod);
					generator.Emit(OpCodes.Unbox_Any, dataRecord.GetFieldType(i));
					generator.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod(true));

					generator.MarkLabel(endIfLabel);
				}
			}

			generator.Emit(OpCodes.Ldloc, result);
			generator.Emit(OpCodes.Ret);

			return (Build)method.CreateDelegate(typeof(Build));
		}

		private static void MapPropertyValues(ResolutionContext context, IMappingEngineRunner mapper, object result)
		{
			foreach (var propertyMap in context.TypeMap.GetPropertyMaps())
			{
				MapPropertyValue(context, mapper, result, propertyMap);
			}
		}

		private static void MapPropertyValue(ResolutionContext context, IMappingEngineRunner mapper,
											 object mappedObject, PropertyMap propertyMap)
		{
			if (false == propertyMap.CanResolveValue())
				return;

			var result = propertyMap.ResolveValue(context.SourceValue);
			var newContext = context.CreateMemberContext(null, result.Value, null, result.Type, propertyMap);

			try
			{
				var propertyValueToAssign = mapper.Map(newContext);

				if (propertyMap.CanBeSet)
					propertyMap.DestinationProperty.SetValue(mappedObject, propertyValueToAssign);
			}
			catch (Exception ex)
			{
				throw new AutoMapperMappingException(newContext, ex);
			}
		}

		private delegate object Build(IDataRecord dataRecord);

		private static readonly MethodInfo getValueMethod =
			typeof(IDataRecord).GetMethod("get_Item", new[] { typeof(int) });
		private static readonly MethodInfo isDBNullMethod =
			typeof(IDataRecord).GetMethod("IsDBNull", new[] { typeof(int) });
	}
}
