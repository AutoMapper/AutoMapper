using System.Collections.Generic;

namespace ContosoUniversity.Crud.DbMappers
{
    internal interface IDbMapper<T>
    {
        void AddChanges(ICollection<T> entities);
        void AddGraphChanges(ICollection<T> entities);
    }
}
