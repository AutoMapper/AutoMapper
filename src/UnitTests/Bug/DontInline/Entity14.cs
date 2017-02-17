using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OmmitedDatabaseModel3
{
    public class Entity14 : BaseEntity
    {
        public Entity14()
        {
            this.Entities12 = new Entity12();
            this.Entities1 = new Entity1();
        }

        public Entity12 Entities12 { get; set; }
        public Entity1 Entities1 { get; set; }
    }
}
