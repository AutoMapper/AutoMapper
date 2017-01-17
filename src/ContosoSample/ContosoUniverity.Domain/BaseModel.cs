using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoUniversity.Domain
{
    abstract public class BaseModel
    {
        public EntityStateType EntityState { get; set; }
    }
}
