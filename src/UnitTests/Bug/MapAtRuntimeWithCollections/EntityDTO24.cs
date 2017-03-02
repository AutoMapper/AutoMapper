using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmmitedDTOModel3WithCollections
{
    public class EntityDTO24 : BaseEntity
    {
        public Guid Entity3Id { get; set; }
        public EntityDTO3 Entity3 { get; set; }
        public Guid Entity22Id { get; set; }
        public EntityDTO22 Entity22 { get; set; }
    }
}
