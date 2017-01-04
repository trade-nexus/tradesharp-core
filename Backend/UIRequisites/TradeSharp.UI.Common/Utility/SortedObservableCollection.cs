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


using System;
using System.Collections.ObjectModel;

namespace TradeSharp.UI.Common.Utility
{
    public class SortedObservableCollection<T> : ObservableCollection<T> where T : IComparable<T>
    {
        protected override void InsertItem(int index, T item)
        {
            if (this.Count == 0)
            {
                base.InsertItem(0, item);
                return;
            }

            index = Compare(item, 0, this.Count - 1);

            base.InsertItem(index, item);
        }

        private int Compare(T item, int lowIndex, int highIndex)
        {
            int compareIndex = (lowIndex + highIndex) / 2;

            if (compareIndex == 0)
            {
                return SearchIndexByIteration(lowIndex, highIndex, item);
            }

            int result = item.CompareTo(this[compareIndex]);

            if (result < 0)
            {   //item precedes indexed obj in the sort order

                if ((lowIndex + compareIndex) < 100 || compareIndex == (lowIndex + compareIndex) / 2)
                {
                    return SearchIndexByIteration(lowIndex, compareIndex, item);
                }

                return Compare(item, lowIndex, compareIndex);
            }

            if (result > 0)
            {   //item follows indexed obj in the sort order

                if ((compareIndex + highIndex) < 100 || compareIndex == (compareIndex + highIndex) / 2)
                {
                    return SearchIndexByIteration(compareIndex, highIndex, item);
                }

                return Compare(item, compareIndex, highIndex);
            }

            return compareIndex;
        }

        /// <summary>
        /// Iterates through sequence of the collection from low to high index
        /// and returns the index where to insert the new item
        /// </summary>
        private int SearchIndexByIteration(int lowIndex, int highIndex, T item)
        {
            for (int i = lowIndex; i <= highIndex; i++)
            {
                if (item.CompareTo(this[i]) < 0)
                {
                    return i;
                }
            }

            return this.Count;
        }

        /// <summary>
        /// Adds the item to collection by ignoring the index
        /// </summary>
        protected override void SetItem(int index, T item)
        {
            this.InsertItem(index, item);
        }

        private const string _InsertErrorMessage
           = "Inserting and moving an item using an explicit index are not support by sorted observable collection";

        /// <summary>
        /// Throws an error because inserting an item using an explicit index
        /// is not support by sorted observable collection
        /// </summary>
        [Obsolete(_InsertErrorMessage)]
        public new void Insert(int index, T item)
        {
            throw new NotSupportedException(_InsertErrorMessage);
        }

        /// <summary>
        /// Throws an error because moving an item using explicit indexes
        /// is not support by sorted observable collection
        /// </summary>
        [Obsolete(_InsertErrorMessage)]
        public new void Move(int oldIndex, int newIndex)
        {
            throw new NotSupportedException(_InsertErrorMessage);
        }
    }

}
