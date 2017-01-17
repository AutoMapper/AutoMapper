using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Crud.DbMappers;
using ContosoUniversity.Data;

namespace ContosoUniversity.Crud
{
    interface IUnitOfWork : IDisposable
    {
        DbContext Context { get; }
        Dictionary<Type, dynamic> RepositoryDictionary { get; }
        Dictionary<Type, dynamic> MapperDictionary { get; }
        Task<bool> SaveChangesAsync();
        GenericRepository<T> GetRepository<T>() where T : BaseData;
        DbMapperBase<T> GetMapper<T>() where T : BaseData;
    }
}
