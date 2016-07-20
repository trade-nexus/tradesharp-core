using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.Repositories
{
    /// <summary>
    /// Interface for persistence
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IPersistRepository<TEntity>
    {
        void AddUpdate(TEntity entity);
        void AddUpdate(IEnumerable<TEntity> collection);
        void Delete(TEntity entity);
    }
}
