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
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Linq;
using Spring.Stereotype;
using Spring.Transaction;
using Spring.Transaction.Interceptor;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.Repositories;
using TradeHub.Common.Core.Repositories.Parameters;

namespace TradeHub.Infrastructure.Nhibernate.Repositories
{
    /// <summary>
    /// Contains Trade Persistence related functionality
    /// </summary>
    [Repository]
    public class TradeRepository : HibernateDao, ITradeRepository
    {
        /// <summary>
        /// DB Table in which the Trade Information is persisted
        /// Only used during custom SQL Query fetch
        /// </summary>
        private const string Table = "trades";

        /// <summary>
        /// Default Constructor
        /// </summary>
        public TradeRepository()
        {
            
        }

        #region ADD/UPDATE/DELETE

        /// <summary>
        /// Add Update Trade Entity
        /// </summary>
        /// <param name="entity"></param>
        [Transaction(TransactionPropagation.Required, ReadOnly = false)]
        public void AddUpdate(Trade entity)
        {
            // Persist entity
            CurrentSession.SaveOrUpdate(entity);
        }

        /// <summary>
        /// Add Update Trade Collection
        /// </summary>
        /// <param name="collection"></param>
        [Transaction(TransactionPropagation.Required, ReadOnly = false)]
        public void AddUpdate(IEnumerable<Trade> collection)
        {
            // Traverse Collection
            foreach (var entity in collection)
            {
                // Persist each entity
                CurrentSession.SaveOrUpdate(entity);
            }
        }

        /// <summary>
        /// Delete Trade Entity
        /// </summary>
        /// <param name="entity"></param>
        [Transaction(TransactionPropagation.Required, ReadOnly = false)]
        public void Delete(Trade entity)
        {
            // Delete Entity
            CurrentSession.Delete(entity);
        }

        #endregion

        #region Filter/Fetch

        /// <summary>
        /// Returns Trades for the given Order Execution Provider
        /// </summary>
        /// <param name="executionProvider">Execution Provider to be used for search</param>
        /// <returns></returns>
        [Transaction(ReadOnly = true)]
        public IList<Trade> FilterByExecutionProvider(string executionProvider)
        {
            // Request Trades for the given Execution Provider
            return FilterBy(trade => trade.ExecutionProvider.Equals(executionProvider));
        }

        /// <summary>
        /// Returns Trades for the given Trade Side
        /// </summary>
        /// <param name="tradeSide">Trade Side to be used for search</param>
        /// <returns></returns>
        [Transaction(ReadOnly = true)]
        public IList<Trade> FilterByTradeSide(TradeSide tradeSide)
        {
            // Request Trades for the given Trade Side
            return FilterBy(trade => trade.TradeSide == tradeSide);
        }

        /// <summary>
        /// Returns Trades for the given Security
        /// </summary>
        /// <param name="security"></param>
        /// <returns></returns>
        [Transaction(ReadOnly = true)]
        public IList<Trade> FilterBySecurity(Security security)
        {
            // Request Trades for the given security
            return FilterBy(trade => trade.Security.Symbol.Equals(security.Symbol));
        }

        /// <summary>
        /// Filter by some search criteria
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        [Transaction(ReadOnly = true)]
        private IList<Trade> FilterBy(Expression<Func<Trade, bool>> expression)
        {
            IList<Trade> search = CurrentSession.Query<Trade>().Where(expression).AsQueryable().ToList();
            return search;
        }

        /// <summary>
        /// Filter Depending upon given paramters
        /// </summary>
        /// <param name="parameters">Parameters to filter out Trades</param>
        /// <returns></returns>
        [Transaction(ReadOnly = true)]
        public IList<Trade> Filter(Dictionary<TradeParameters, string> parameters)
        {
            // Create Valid WHERE Clause for incoming request
            string whereClause = CreateWhereClause(parameters);

            // Incoming Clause should not be empty
            if (!string.IsNullOrEmpty(whereClause))
            {
                // Create SQL Query
                string sqlQuery = "SELECT * FROM " + Table + whereClause;

                // Fetch Information from DB
                var result = CurrentSession.CreateSQLQuery(sqlQuery).AddEntity(typeof(Trade)).List<Trade>();

                // Return Information
                return result;
            }

            return null;
        }

        #endregion

        #region Find

        /// <summary>
        /// Get all Trades
        /// </summary>
        /// <returns></returns>
        [Transaction(ReadOnly = true)]
        public IList<Trade> ListAll()
        {
            IList<Trade> list = CurrentSession.CreateCriteria<Trade>().List<Trade>();
            return list;
        }

        /// <summary>
        /// Find Trade by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Transaction(ReadOnly = true)]
        public Trade FindBy(string id)
        {
            Trade trade = CurrentSession.Get<Trade>(id);
            if (trade != null)
                NHibernateUtil.Initialize(trade.ExecutionDetails);
            return trade;
        }

        #endregion


        /// <summary>
        /// Create Where Clause to be used in SQL query from the incoming parameters
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string CreateWhereClause(Dictionary<TradeParameters, string> parameters)
        {
            string whereClause = string.Empty;
            string joinStatement = string.Empty;

            bool initialValue = true;

            // Traverse Parameters
            foreach (KeyValuePair<TradeParameters, string> valuePair in parameters)
            {
                if (initialValue)
                {
                    initialValue = false;

                    whereClause += " WHERE ";

                    // Create Join Statement
                    joinStatement = CreateJoinSegment("tradedetails", "TradeId");
                }
                else
                {
                    whereClause += " AND ";
                }

                // Check if 'OR' condition needs to be placed
                if (valuePair.Value.Contains(','))
                {
                    // Split on ',' to separate values as individual
                    string[] splitValues = valuePair.Value.Split(',');

                    // Save length to be used in further processing
                    int count = splitValues.Length;

                    for (int i = 0; i < count; i++)
                    {
                        // Start 'OR' Comparison
                        if (i == 0)
                        {
                            whereClause += " ( ";
                        }

                        // Create Comparison
                        whereClause += valuePair.Key + " = '" + splitValues[i] + "'";

                        // Close 'OR' Comparison
                        if (i == (count - 1))
                        {
                            whereClause += " )";
                        }
                        // Place 'OR' Condition
                        else
                        {
                            whereClause += " OR ";
                        }
                    }
                }
                // Place 'AND' Condition
                else
                {
                    // Check for Start Date Time Value
                    if (valuePair.Key.Equals(TradeParameters.StartTime))
                    {
                        whereClause += TradeParameters.StartTime + " >= '" + valuePair.Value + "'";
                    }
                    // Check for End Date Time Value
                    else if (valuePair.Key.Equals(TradeParameters.CompletionTime))
                    {
                        whereClause += TradeParameters.CompletionTime + " <= '" + valuePair.Value + "'";
                    }
                    else
                    {
                        whereClause += valuePair.Key + " = '" + valuePair.Value + "'";
                    }
                }
            }

            return joinStatement + whereClause + " Group by TradeId";
        }

        /// <summary>
        /// Creates JOIN segment to be used in the SQL Query
        /// </summary>
        /// <param name="joinTable">Second Table in the JOIN statement</param>
        /// <param name="property">JOIN Property</param>
        /// <returns></returns>
        private string CreateJoinSegment(string joinTable, string property)
        {
            return " INNER JOIN " + joinTable + " ON " + Table + ".Id" + "=" + joinTable + "." + property;
        }
    }
}
