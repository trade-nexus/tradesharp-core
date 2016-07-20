using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.Repositories
{
    public interface IEntityKey<TKey>
    {
        TKey Id { get; }
    }
}
