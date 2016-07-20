using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.StrategyRunner.UserInterface.Common.ValueObjects
{
    /// <summary>
    /// VO for publishing event to execute next iteration
    /// </summary>
    public class ExecuteNext
    {
        public bool Execute { get; set; }
    }
}