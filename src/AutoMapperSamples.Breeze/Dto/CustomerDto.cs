using System;
using Breeze.Sharp;

namespace AutoMapperSamples.Breeze.Dto
{
    public class CustomerDto : BaseEntity
    {

        public CustomerDto()
        {
        }
        

        public Guid Id
        {
            get { return GetValue<Guid>(); }
            set { SetValue(value); }
        }

        public string Name
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }


        public virtual string CompanyName
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public virtual string ContactName
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public virtual string ContactTitle
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public virtual string Address
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public virtual string City
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public virtual string Region
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public virtual string PostalCode
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public virtual string Country
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public virtual string Phone
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public virtual string Fax
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public virtual int RowVersion
        {
            get { return GetValue<int>(); }
            set { SetValue(value); }
        }

        public Guid[] Orders
        {
            get { return GetValue<Guid[]>(); }
            set { SetValue(value); }
        }
    }
}
