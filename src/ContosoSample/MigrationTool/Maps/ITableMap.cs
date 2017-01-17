using Microsoft.EntityFrameworkCore;

namespace MigrationTool.Maps
{
    public interface ITableMap
    {
        void Map(ModelBuilder modelBuilder);
    }
}
