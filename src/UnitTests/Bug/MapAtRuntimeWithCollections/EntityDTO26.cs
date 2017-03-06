using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmmitedDTOModel3WithCollections
{
    public class EntityDTO26 : BaseEntity
    {
        public EntityDTO26()
        {
            this.Entities20 = new List<EntityDTO20>();
        }

        public ICollection<EntityDTO20> Entities20 { get; set; }
    }
}
