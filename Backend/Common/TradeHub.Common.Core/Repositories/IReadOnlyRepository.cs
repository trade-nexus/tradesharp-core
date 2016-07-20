using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.Repositories
{
    /// <summary>
    /// Interface for the Query Operations
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    public interface IReadOnlyRepository<TEntity,TKey>
    {
        IList<TEntity> ListAll();
        TEntity FindBy(TKey id);
    }
}
