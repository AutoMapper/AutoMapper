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
            //Orders = new List<OrderDto>();
        }

        public string Name { get; set; }
        
        public Guid Id { get; set; }

        //public ICollection<OrderDto> Orders { get; set; }
    }
}
