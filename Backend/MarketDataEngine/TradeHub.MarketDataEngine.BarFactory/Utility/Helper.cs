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

namespace TradeHub.MarketDataEngine.BarFactory.Utility
{
    /// <summary>
    /// Contains helper functions for Bar Factory
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Integer precision
        /// </summary>
        public const int IPREC = 1000000;

        /// <summary>
        /// Inverse integer precision
        /// </summary>
        public const decimal IPRECV = .000001m;

        /// <summary>
        /// Current date time to be used in whole application
        /// </summary>
        public static DateTime CURRENT_DATE_TIME = DateTime.UtcNow;

        /// <summary>
        /// 
        /// </summary>
        public static string DATE_TIME_FORMAT = "yyyyMMdd HH:mm:ss.fff";

        /// <summary>
        /// Trims seconds.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime RoundMinute(this DateTime dateTime)
        {
            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                dateTime.Hour,
                dateTime.Minute,
                0);
        }

        /// <summary>
        /// Trims millseconds.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime RoundSecond(this DateTime dateTime)
        {
            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                dateTime.Hour,
                dateTime.Minute,
                dateTime.Second,
                0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="date">Date in format 20060711</param>
        /// <returns>Year in formt 2006</returns>
        public static int GetYear(int date)
        {
            string strDate = Convert.ToString(date);
            string strYear = strDate.Substring(0, 4);
            return Convert.ToInt32(strYear);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date">Date in format 20060711</param>
        /// <returns>Month in formt 07</returns>
        public static int GetMonth(int date)
        {
            string strDate = Convert.ToString(date);
            string strMonth = strDate.Substring(4, 2);
            return Convert.ToInt32(strMonth);
        }

        /// <summary>
        /// Get month name
        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
        public static string GetMonthName(int month)
        {
            switch (month)
            {
                case 1:
                    {
                        return "JAN";
                    }
                case 2:
                    {
                        return "FEB";
                    }
                case 3:
                    {
                        return "MAR";
                    }
                case 4:
                    {
                        return "APR";
                    }
                case 5:
                    {
                        return "MAY";
                    }
                case 6:
                    {
                        return "JUN";
                    }
                case 7:
                    {
                        return "JUL";
                    }
                case 8:
                    {
                        return "AUG";
                    }
                case 9:
                    {
                        return "SEP";
                    }
                case 10:
                    {
                        return "OCT";
                    }
                case 11:
                    {
                        return "NOV";
                    }
                case 12:
                    {
                        return "DEC";
                    }
                default:
                    {
                        return "UNKNOWN";
                    }
            }
        }

    }
}
