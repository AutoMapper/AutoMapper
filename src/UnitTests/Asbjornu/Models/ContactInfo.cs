using System;

namespace AutoMapper.UnitTests.Asbjornu.Models
{
   public class ContactInfo
   {
      public ContactInfo()
      {
      }


      public ContactInfo(string email, string firstName, string lastName)
      {
         if (String.IsNullOrEmpty(email))
            throw new ArgumentNullException("email");

         if (String.IsNullOrEmpty(firstName))
            throw new ArgumentNullException("firstName");

         if (String.IsNullOrEmpty(lastName))
            throw new ArgumentNullException("lastName");

         Email = email;
         FirstName = firstName;
         LastName = lastName;
      }


      public ContactInfo(string email, string firstName, string lastName, string phone)
         : this(email, firstName, lastName)
      {
         if (String.IsNullOrEmpty(phone))
            throw new ArgumentNullException("phone");

         this.Phone = phone;
      }


      public ContactInfo(string email)
      {
         if (String.IsNullOrEmpty(email))
            throw new ArgumentNullException("email");

         Email = email;
      }


      public string Email { get; set; }

      public string FirstName { get; set; }

      public string LastName { get; set; }

      public string Phone { get; set; }
   }
}