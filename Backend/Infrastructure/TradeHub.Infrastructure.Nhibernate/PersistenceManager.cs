/***************************************************************************** 
* Copyright 2016 Aurora Solutions 
* 
*    http://www.aurorasolutions.io 
* 
* Aurora Solutions is an innovative services and product company at 
* the forefront of the software industry, with processes and practices 
* involving Domain Driven Design(DDD), Agile methodologies to build 
* scalable, secure, reliable and high performance products.
* 
* TradeSharp is a C# based data feed and broker neutral Algorithmic 
* Trading Platform that lets trading firms or individuals automate 
* any rules based trading strategies in stocks, forex and ETFs. 
* TradeSharp allows users to connect to providers like Tradier Brokerage, 
* IQFeed, FXCM, Blackwood, Forexware, Integral, HotSpot, Currenex, 
* Interactive Brokers and more. 
* Key features: Place and Manage Orders, Risk Management, 
* Generate Customized Reports etc 
* 
* Licensed under the Apache License, Version 2.0 (the "License"); 
* you may not use this file except in compliance with the License. 
* You may obtain a copy of the License at 
* 
*    http://www.apache.org/licenses/LICENSE-2.0 
* 
* Unless required by applicable law or agreed to in writing, software 
* distributed under the License is distributed on an "AS IS" BASIS, 
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
* See the License for the specific language governing permissions and 
* limitations under the License. 
*****************************************************************************/


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;
using Spring.Stereotype;
using TradeHub.Common.Core.Repositories;
using TradeHub.Infrastructure.Nhibernate.NhibernateMappings;
using Spring.Transaction.Interceptor;

namespace TradeHub.Infrastructure.Nhibernate
{
    /// <summary>
    /// Persistence Manager
    /// </summary>
    /// <typeparam name="TKey">Key data type</typeparam>
    /// <typeparam name="T">Entity Data type</typeparam>
    [Repository]
    public class PersistenceManager<TEntity,TKey>:SessionFactory,IPersistRepository<TEntity>,IReadOnlyRepository<TEntity,TKey> where TEntity:class 
    { 
        //public static void AddorUpdate(T item)
        //{
            
        //    using (var session = GetSessionFactory().OpenSession())
        //    {
        //        using (var tx = session.BeginTransaction())
        //        {
        //            session.SaveOrUpdate(item);
        //            tx.Commit();
        //        }
        //        Console.WriteLine("Press <ENTER> to exit...");
        //        Console.ReadLine();
        //    }
        //}

        
        //public static IList<LimitOrder> ListAll()
        //{
        //    IList<LimitOrder> list = null;
        //    using (var session = GetSessionFactory().OpenSession())
        //    {
        //        using (var tx = session.BeginTransaction())
        //        {
        //            list = session.CreateCriteria<LimitOrder>()
        //                .List<LimitOrder>();
                    
                   
        //        }
                
        //    }
        //    return  list;
        //}

        //public static Q FindByID<Q>(object id) where Q : class
        //{
            
        //    using (var session = GetSessionFactory().OpenSession())
        //    {
        //        using (var tx = session.BeginTransaction())
        //        {
        //            var limitorders = session.CreateCriteria<Q>()
        //            .Add(Restrictions.Eq("OrderID", id))
        //            .List<Q>();
        //            return limitorders.FirstOrDefault();

        //        }

        //    }
           
        //}

       

        [Transaction]
        public void AddUpdate(TEntity entity)
        {

            using (var session = GetSessionFactory().OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    session.SaveOrUpdate(entity);
                    tx.Commit();
                }

            }

        }

        public void AddUpdate(IEnumerable<TEntity> collection)
        {
            using (var session = GetSessionFactory().OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    foreach (var entity in collection)
                    {
                        session.SaveOrUpdate(entity);
                        tx.Commit();
                    }
                    
                }

            }
        }


        [Transaction]
        public void Delete(TEntity entity)
        {
            //CurrentSession.Delete(entity);
            using (var session = GetSessionFactory().OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    session.Delete(entity);
                    tx.Commit();
                }

            }
        }


        [Transaction(ReadOnly = true)]
        public IList<TEntity> FilterBy(Expression<Func<TEntity, bool>> expression)
        {
            IList<TEntity> search;
            using (var session = GetSessionFactory().OpenSession())
            {
                search = session.Query<TEntity>().Where(expression).AsQueryable().ToList();
            }
            return search;
            //return CurrentSession.Query<T>().Where(expression).AsQueryable().ToList();
        }

        [Transaction(ReadOnly = true)]
        public TEntity FindBy(TKey id)
        {
            using (var session = GetSessionFactory().OpenSession())
            {
                return session.Get<TEntity>(id);

            }
           // return CurrentSession.Get<T>(id);
        }

        [Transaction(ReadOnly = true)]
        public IList<TEntity> ListAll()
        {
            IList<TEntity> list = null;
            using (var session = GetSessionFactory().OpenSession())
            {
                list = session.CreateCriteria<TEntity>()
                       .List<TEntity>();

            }
            return list;
            //return CurrentSession.CreateCriteria<T>().List<T>();
        }


       
    }
}
