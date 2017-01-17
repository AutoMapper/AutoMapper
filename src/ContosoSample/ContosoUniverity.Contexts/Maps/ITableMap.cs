using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Contexts.Maps
{
    public interface ITableMap
    {
        void Map(ModelBuilder modelBuilder);
    }
}
