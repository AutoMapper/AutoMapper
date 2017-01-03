using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmmitedDTOModel3
{
    public class EntityDTO26 : BaseEntity
    {
        public EntityDTO26()
        {
            this.Entities20 = new EntityDTO20();
        }

        public EntityDTO20 Entities20 { get; set; }
    }
}
