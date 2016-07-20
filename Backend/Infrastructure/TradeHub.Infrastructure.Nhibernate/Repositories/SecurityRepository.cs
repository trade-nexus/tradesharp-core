using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.Repositories;

namespace TradeHub.Infrastructure.Nhibernate.Repositories
{
    public class SecurityRepository:ISecurityRepository
    {
        public Common.Core.DomainModels.Security FindyBySymbol(object symbol)
        {
            throw new NotImplementedException();
        }

        public void Add(Common.Core.DomainModels.Security item)
        {
            
        }

        public void Delete(Common.Core.DomainModels.Security item)
        {
            throw new NotImplementedException();
        }

        public void Update(Common.Core.DomainModels.Security item)
        {
            throw new NotImplementedException();
        }

        public IList<Common.Core.DomainModels.Security> FindAll()
        {
            throw new NotImplementedException();
        }

        public Common.Core.DomainModels.Security FindByID(object id)
        {
            throw new NotImplementedException();
        }
    }
}
