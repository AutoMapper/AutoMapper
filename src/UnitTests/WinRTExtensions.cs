using System.Collections.Generic;
using System.Linq;

#if NETFX_CORE
namespace AutoMapper
{
    using System.Reflection;
    public static class Assembly
    {
        public static System.Reflection.Assembly GetExecutingAssembly()
        {
            return typeof (Assembly).GetTypeInfo().Assembly;
        }
    }
}
#endif

namespace System.Reflection
{
    internal static class WinRTExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }

//        public static bool IsValueType(this Type type)
//        {
//#if NETFX_CORE
//            return type.GetTypeInfo().IsValueType;
//#else
//            return type.IsValueType;
//#endif
//        }

//        public static bool IsGenericType(this Type type)
//        {
//#if NETFX_CORE
//            return type.GetTypeInfo().IsGenericType;
//#else
//            return type.IsGenericType;
//#endif
//        }

#if NETFX_CORE
        public static PropertyInfo GetProperty(this Type type, string name, params Type[] parametertypes)
        {
            return type.GetTypeInfo().GetDeclaredProperty(name);
        }
        //public static FieldInfo GetField(this Type type, string name)
        //{
        //    return type.GetTypeInfo().GetDeclaredField(name);
        //}
        //public static MethodInfo GetMethod(this Type type, string name, params Type[] parametertypes)
        //{
        //    if (!parametertypes.Any())
        //        type.GetTypeInfo().GetDeclaredMethod(name);

        //    return type.GetTypeInfo().GetDeclaredMethods(name).Single(mi => mi.GetParameters().Select(pi => pi.ParameterType).SequenceEqual(parametertypes));
        //}

        //public static bool IsInstanceOfType(this Type type, object o)
        //{
        //    return o != null && type.IsAssignableFrom(o.GetType());
        //}

        internal static bool ImplementInterface(this Type type, Type ifaceType)
        {
            while (type != null)
            {
                Type[] interfaces = type.GetTypeInfo().ImplementedInterfaces.ToArray(); //  .GetInterfaces();
                if (interfaces != null)
                {
                    for (int i = 0; i < interfaces.Length; i++)
                    {
                        if (interfaces[i] == ifaceType || (interfaces[i] != null && interfaces[i].ImplementInterface(ifaceType)))
                        {
                            return true;
                        }
                    }
                }
                type = type.GetTypeInfo().BaseType;
                // type = type.BaseType;
            }
            return false;
        }

        //public static bool IsAssignableFrom(this Type type, Type c)
        //{
        //    if (c == null)
        //    {
        //        return false;
        //    }
        //    if (type == c)
        //    {
        //        return true;
        //    }

        //    //RuntimeType runtimeType = type.UnderlyingSystemType as RuntimeType;
        //    //if (runtimeType != null)
        //    //{
        //    //    return runtimeType.IsAssignableFrom(c);
        //    //}

        //    //if (c.IsSubclassOf(type))
        //    if (c.GetTypeInfo().IsSubclassOf(c))
        //    {
        //        return true;
        //    }

        //    //if (type.IsInterface)
        //    if (type.GetTypeInfo().IsInterface)
        //    {
        //        return c.ImplementInterface(type);
        //    }

        //    if (type.IsGenericParameter)
        //    {
        //        Type[] genericParameterConstraints = type.GetTypeInfo().GetGenericParameterConstraints();
        //        for (int i = 0; i < genericParameterConstraints.Length; i++)
        //        {
        //            if (!genericParameterConstraints[i].IsAssignableFrom(c))
        //            {
        //                return false;
        //            }
        //        }
        //        return true;
        //    }
        //    return false;
        //}
#endif
    }
}
