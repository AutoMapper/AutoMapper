using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSample.EF.POCO
{
    public class BasePOCO
    {
        [NotMapped]
        public EntityStateType EntityState { get; set; }
    }
}
