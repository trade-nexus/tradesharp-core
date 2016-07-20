using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor;
using TraceSourceLogger;
using TradeHub.Common.Core.Repositories;
using TradeHub.Common.Core.Utility;

namespace TradeHub.Common.Persistence
{
    /// <summary>
    /// Receive data from disruptor and persist
    /// </summary>
    public class Journaler:IEventHandler<byte[]>
    {
        private IPersistRepository<object> _persistRepository;
        private Type _type = typeof (Journaler);
        public Journaler(IPersistRepository<object> persistRepository)
        {
            _persistRepository = persistRepository;
        }

        #region Disruptor Implementation
        public void OnNext(byte[] data, long sequence, bool endOfBatch)
        {
            try
            {
                object obj = StreamConversion.ByteArrayToObject(data);
                _persistRepository.AddUpdate(obj);
            }
            catch (Exception exception)
            {
                Logger.Error(exception,_type.FullName,"OnNext");
            }
        }
        #endregion
    }
}
