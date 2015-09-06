using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMapperSamples.EF.Dtos
{
    public class CustomerDto
    {

        public CustomerDto()
        {
            Orders = new Guid[0];
        }

        public string Name { get; set; }
        
        public Guid Id { get; set; }

        public Guid[] Orders { get; set; }
    }
}
