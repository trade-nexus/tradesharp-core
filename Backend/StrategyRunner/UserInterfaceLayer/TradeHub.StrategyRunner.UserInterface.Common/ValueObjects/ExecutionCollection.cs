using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.StrategyRunner.UserInterface.Common.Interface;

namespace TradeHub.StrategyRunner.UserInterface.Common.ValueObjects
{
    /// <summary>
    /// Contains order executions
    /// </summary>
    public class ExecutionCollection: IItemsProvider<Execution>
    {
        private IList<Execution> _executionList;

        public ExecutionCollection()
        {
            _executionList = new List<Execution>();
        }

        #region Implementation of IItemsProvider<Execution>

        /// <summary>
        /// Fetches the total number of items available.
        /// </summary>
        /// <returns></returns>
        public int FetchCount()
        {
            return _executionList.Count;
        }

        /// <summary>
        /// Fetches a range of items.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="count">The number of items to fetch.</param>
        /// <returns></returns>
        public IList<Execution> FetchRange(int startIndex, int count)
        {
            IList<Execution> list = new List<Execution>();
            for (int i = startIndex; i < startIndex + count && i < _executionList.Count; i++)
            {
                list.Add(_executionList[i]);
            }

            return list;
        }

        /// <summary>
        /// Adds new Item to the collection
        /// </summary>
        /// <param name="item"></param>
        public void AddItem(Execution item)
        {
            _executionList.Add(item);
        }

        #endregion
    }
}
