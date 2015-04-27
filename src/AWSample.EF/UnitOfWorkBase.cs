using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AWSample.EF.Database.Repositories;

namespace AWSample.EF
{
    internal abstract class UnitOfWorkBase : IDisposable, IDbContext
    {
        #region Variables
        private bool disposed;
        #endregion Variables

        #region Properties
        public virtual System.Data.Entity.DbContext Context
        {
            get { throw new NotImplementedException(); }
        }
        #endregion Properties

        #region Methods
        public virtual void Save()
        {
            try
            {
                this.Context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (DbEntityValidationResult eve in ex.EntityValidationErrors)
                {
                    sb.Append(string.Format(CultureInfo.CurrentCulture, Properties.Resources.entityValidationErrorFormat, eve.Entry.Entity.GetType().Name, eve.Entry.State));
                    sb.Append(Environment.NewLine);
                    foreach (DbValidationError ve in eve.ValidationErrors)
                    {
                        sb.Append(string.Format(CultureInfo.CurrentCulture, Properties.Resources.propertyValidationErrorFormat, ve.PropertyName, ve.ErrorMessage));
                        sb.Append(Environment.NewLine);
                    }
                }

                throw new InvalidOperationException(sb.ToString(), ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                    this.Context.Dispose();
            }

            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion Methods
    }
}
