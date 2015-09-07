using System;
using Breeze.Sharp;
          
namespace AutoMapperSamples.Breeze.Dto
{
    public class OrderDto : BaseEntity
    {
        public Guid Id
        {
            get { return GetValue<Guid>(); }
            set { SetValue(value); }
        }
        public string FullName
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public DateTime? OrderDate
        {
            get { return GetValue<DateTime?>(); }
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

        public virtual Guid? CustomerID
        {
            get { return GetValue<Guid>(); }
            set { SetValue(value); }
        }
        public virtual DateTime? RequiredDate
        {
            get { return GetValue<DateTime?>(); }
            set { SetValue(value); }
        }
        public virtual DateTime? ShippedDate
        {
            get { return GetValue<DateTime?>(); }
            set { SetValue(value); }
        }
        public virtual string ShipName
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public virtual string ShipAddress
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public virtual string ShipCity
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public virtual string ShipRegion
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public virtual string ShipPostalCode
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public virtual string ShipCountry
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public virtual int RowVersion
        {
            get { return GetValue<int>(); }
            set { SetValue(value); }
        }
    }
}
