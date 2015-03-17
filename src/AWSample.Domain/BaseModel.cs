using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSample.Domain
{
    public class BaseModel
    {
        [NotMapped]
        public EntityStateType EntityState { get; set; }
    }
}
