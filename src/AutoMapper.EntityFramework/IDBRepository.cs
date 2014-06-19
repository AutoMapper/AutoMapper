using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.EquivilencyExpression;
using AutoMapper.Mappers;

namespace AutoMapper
{
    public class DBRepository<TDatabase> 
        where TDatabase:DbContext, new()
    {
        static DBRepository()
        {
            EquivilentExpressions.GenerateEquality.Add(new GenerateEntityFrameworkPrimaryKeyEquivilentExpressions<TDatabase>());
        }

        public void Save<T,TI>(object value) 
            where T : class
            where TI : class, T
        {
            using (var db = new TDatabase())
            {
                var equivExpr = Mapper.Map(value, value.GetNonDynamicProxyType(), typeof (Expression<Func<T, bool>>)) as Expression<Func<T,bool>>;
                if(equivExpr == null)
                    return;

                var set = db.Set<T>();
                var equivilent = set.FirstOrDefault(equivExpr);

                if (equivilent == null)
                {
                    equivilent = set.Create<TI>();
                    set.Add(equivilent);
                }
                Mapper.Map(value, equivilent, value.GetNonDynamicProxyType(), typeof(T));
                db.SaveChanges();
            }
        }

        public void Delete<T>(object value)
            where T : class
        {
            using (var db = new TDatabase())
            {
                var equivExpr = Mapper.Map(value, value.GetNonDynamicProxyType(), typeof(Expression<Func<T, bool>>)) as Expression<Func<T, bool>>;
                if (equivExpr == null)
                    return;
                var equivilent = db.Set<T>().FirstOrDefault(equivExpr);

                if (equivilent == null)
                    db.Set<T>().Remove(equivilent);
                db.SaveChanges();
            }
        }

        public TDTO GetSingle<T,TDTO>(Expression<Func<TDTO,bool>> equivExpr)
            where T : class
            where TDTO : class
        {
            using (var db = new TDatabase())
            {
                var equivExpr2 = Mapper.Map(equivExpr, typeof(Expression<Func<TDTO, bool>>), typeof(Expression<Func<T, bool>>)) as Expression<Func<T, bool>>;
                var equivilent = db.Set<T>().FirstOrDefault(equivExpr2);

                if (equivilent == null)
                    return null;
                return Mapper.Map<T,TDTO>(equivilent);
            }
        }

        public IEnumerable<TDTO> GetMany<T, TDTO>(Expression<Func<TDTO, bool>> equivExpr)
            where T : class
            where TDTO : class
        {
            using (var db = new TDatabase())
            {
                var equivExpr2 = Mapper.Map(equivExpr, typeof(Expression<Func<TDTO, bool>>), typeof(Expression<Func<T, bool>>)) as Expression<Func<T, bool>>;
                var equivilent = db.Set<T>().Where(equivExpr2);
                return equivilent.Select(e => Mapper.Map<T, TDTO>(e)).ToList();
            }
        }
    }

    public static class TypeExtensions
    {
        public static Type GetNonDynamicProxyType(this object item)
        {
            return item.GetType().GetNonDynamicProxyType();
        }

        public static Type GetNonDynamicProxyType(this Type item)
        {
            var type = item;
            if (IsDynamicProxy(type))
                type = type.BaseType;
            return type;
        }

        private static bool IsDynamicProxy(Type type)
        {
            return type.Namespace == "System.Data.Entity.DynamicProxies";
        }
    }
}