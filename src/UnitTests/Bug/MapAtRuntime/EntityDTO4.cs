using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmmitedDTOModel3
{
    public class EntityDTO4 : BaseEntity
    {
        public Guid Entity3Id { get; set; }
        public EntityDTO3 Entity3 { get; set; }
    }
}
