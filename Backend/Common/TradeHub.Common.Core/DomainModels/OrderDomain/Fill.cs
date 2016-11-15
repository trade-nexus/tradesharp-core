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
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.Constants;

namespace TradeHub.Common.Core.DomainModels.OrderDomain
{
    /// <summary>
    /// Represents successfully executed order in the Exchange
    /// </summary>
    [Serializable]
    public class Fill : OrderEvent, ICloneable
    {
        #region Members

        private string _executionId;
        private int _executionSize;
        private decimal _executionPrice;
        private DateTime _executionDatetime;
        private string _executionSide;
        private ExecutionType _executionType;
        private int _leavesQuantity;
        private int _cummalativeQuantity;
        private string _currency;
        private decimal _averageExecutionPrice;
        private string _executionAccount;
        private string _executionExchange;

        #endregion
        
        #region Properties

        /// <summary>
        /// Execution id
        /// </summary>
        public string ExecutionId
        {
            get
            {
                return this._executionId;
            }
            set
            {
                this._executionId = value;
            }
        }

        /// <summary>
        /// Execution datetime
        /// </summary>
        public DateTime ExecutionDateTime
        {
            get
            {
                return this._executionDatetime;
            }
            set
            {
                this._executionDatetime = value;
            }
        }

        /// <summary>
        /// Execution size
        /// </summary>
        public int ExecutionSize
        {
            get
            {
                return this._executionSize;
            }
            set
            {
                this._executionSize = value;
            }
        }

        /// <summary>
        /// Execution price
        /// </summary>
        public decimal ExecutionPrice
        {
            get
            {
                return this._executionPrice;
            }
            set
            {
                this._executionPrice = value;
            }
        }

        /// <summary>
        /// Execution side
        /// </summary>
        public string ExecutionSide
        {
            get
            {
                return this._executionSide;
            }
            set
            {
                this._executionSide = value;
            }
        }

        /// <summary>
        /// Execution account
        /// </summary>
        public string ExecutionAccount
        {
            get
            {
                return this._executionAccount;
            }
            set
            {
                this._executionAccount = value;
            }
        }

        /// <summary>
        /// Execution exchange
        /// </summary>
        public string ExecutionExchange
        {
            get
            {
                return this._executionExchange;
            }
            set
            {
                this._executionExchange = value;
            }
        }

        /// <summary>
        /// Execution type.
        /// </summary>
        public ExecutionType ExecutionType
        {
            get
            {
                return this._executionType;
            }
            set
            {
                this._executionType = value;
            }
        }

        /// <summary>
        /// Leaves quantity
        /// </summary>
        public int LeavesQuantity
        {
            get
            {
                return this._leavesQuantity;
            }
            set
            {
                this._leavesQuantity = value;
            }
        }

        /// <summary>
        /// Commulative quantity
        /// </summary>
        public int CummalativeQuantity
        {
            get
            {
                return this._cummalativeQuantity;
            }
            set
            {
                this._cummalativeQuantity = value;
            }
        }

        /// <summary>
        /// Average execution price
        /// </summary>
        public decimal AverageExecutionPrice
        {
            get
            {
                return this._averageExecutionPrice;
            }
            set
            {
                this._averageExecutionPrice = value;
            }
        }

        /// <summary>
        /// Currency
        /// </summary>
        public string Currency
        {
            get
            {
                return this._currency;
            }
            set
            {
                this._currency = value;
            }
        }

        #endregion

        private Fill() : base()
        {

        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public Fill(Security security, string orderExecutionProvider, string orderId)
            : base(security, orderExecutionProvider)
        {
            OrderId = orderId;
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public Fill(Security security, string orderExecutionProvider, string orderId, DateTime dateTime)
            : base(security, orderExecutionProvider, dateTime)
        {
            OrderId = orderId;
        }

        /// <summary>
        /// ToString Overrider for Fill
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var str = new StringBuilder();

            str.Append("Fill :: ");
            str.Append(base.Security);
            str.Append(" | ");
            str.Append("ExecutionID : " + this._executionId);
            str.Append(" | ");
            str.Append("ExecutionSize : " + this._executionSize);
            str.Append(" | ");
            str.Append("ExecutionPrice : " + this._executionPrice);
            str.Append(" | ");
            str.Append("ExecutionDatetime : " + this._executionDatetime);
            str.Append(" | ");
            str.Append("ExecutionSide : " + this._executionSide);
            str.Append(" | ");
            str.Append("ExecutionType : " + this._executionType);
            str.Append(" | ");
            str.Append("LeavesQuantity : " + this._leavesQuantity);
            str.Append(" | ");
            str.Append("CummalativeQuantity : " + this._cummalativeQuantity);
            str.Append(" | ");
            str.Append("AverageExecutionPrice : " + this._averageExecutionPrice);
            str.Append(" | ");
            str.Append("Currency : " + this._currency);
            str.Append(" | ");
            str.Append("ExecutionAccount : " + this._executionAccount);
            str.Append(" | ");
            str.Append("ExecutionExchange : " + this._executionExchange);

            return str.ToString();
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
