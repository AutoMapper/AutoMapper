using System;
using Breeze.Sharp;
          
namespace AutoMapperSamples.Breeze.Dto
{
    public class OrderDto : BaseEntity
    {
        public string FullName
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public Guid Id
        {
            get { return GetValue<Guid>(); }
            set { SetValue(value); }
        }
        public DateTime Ordered
        {
            get { return GetValue<DateTime>(); }
            set { SetValue(value); }
        }
        public double Price
        {
            get { return GetValue<double>(); }
            set { SetValue(value); }
        }
        public CustomerDto Customer
        {
            get { return GetValue<CustomerDto>(); }
            set { SetValue(value); }
        }
    }
}
