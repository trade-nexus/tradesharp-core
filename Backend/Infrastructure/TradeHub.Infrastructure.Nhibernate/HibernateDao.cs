using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;

namespace TradeHub.Infrastructure.Nhibernate
{
    /// <summary>
    /// Session factory
    /// </summary>
    public abstract class HibernateDao
    {
        private ISessionFactory sessionFactory;

        /// <summary>
        /// Session factory for sub-classes.
        /// </summary>
        public ISessionFactory SessionFactory
        {
            protected get { return sessionFactory; }
            set{sessionFactory = value;}
        }

        /// <summary>
        /// Get's the current active session. Will retrieve session as managed by the 
        /// Open Session In View module if enabled.
        /// </summary>
        protected ISession CurrentSession
        {
            get
            {
                return sessionFactory.GetCurrentSession();

            }
        }


    }
}
