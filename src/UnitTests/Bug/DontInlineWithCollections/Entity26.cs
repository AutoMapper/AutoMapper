using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmmitedDatabaseModel3WithCollections
{
    public class Entity26 : BaseEntity
    {
        public Entity26()
        {
            this.Entities20 = new List<Entity20>();
        }

        public ICollection<Entity20> Entities20 { get; set; }
    }
}
