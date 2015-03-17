using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSample.Domain.Person
{
    public class BusinessEntityContactModel : BaseModel
    {
        public int BusinessEntityID { get; set; }

        public int PersonID { get; set; }

        public int ContactTypeID { get; set; }

        public Guid rowguid { get; set; }

        public DateTime ModifiedDate { get; set; }

        public PersonModel Person { get; set; }
    }
}
