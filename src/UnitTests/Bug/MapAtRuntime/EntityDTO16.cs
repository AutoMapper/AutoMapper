﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmmitedDTOModel3
{
    public class EntityDTO16 : BaseEntity
    {
        public Guid Entity20Id { get; set; }
        public EntityDTO20 Entity20 { get; set; }
        public Guid Entity12Id { get; set; }
        public EntityDTO12 Entity12 { get; set; }
    }
}
