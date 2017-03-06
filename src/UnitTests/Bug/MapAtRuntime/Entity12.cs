using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmmitedDatabaseModel3
{
    public class Entity12 : BaseEntity
    {
        public Entity12()
        {
            //this.Entities20 = new Entity20();
            //this.Entities14 = new Entity14();
            //this.Entities16 = new Entity16();
        }
        public Entity20 Entities20 { get; set; }
        public Entity16 Entities16 { get; set; }
        public Entity14 Entities14 { get; set; }
    }
}
