using System;
using Breeze.Sharp;

namespace AutoMapperSamples.Breeze.Dto
{
    public class CustomerDto : BaseEntity
    {

        public CustomerDto()
        {
            //Orders = new Guid[0];
        }

        public string Name
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }

        public Guid Id
        {
            get { return GetValue<Guid>(); }
            set { SetValue(value); }
        }

        public Guid[] Orders
        {
            get { return GetValue<Guid[]>(); }
            set { SetValue(value); }
        }
    }
}
