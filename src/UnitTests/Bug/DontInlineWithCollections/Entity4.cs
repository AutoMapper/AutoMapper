using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmmitedDatabaseModel3WithCollections
{
    public class Entity4 : BaseEntity
    {
        public Guid Entity3Id { get; set; }
        public Entity3 Entity3 { get; set; }
    }
}
