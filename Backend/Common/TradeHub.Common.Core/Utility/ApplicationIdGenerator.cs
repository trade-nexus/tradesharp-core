using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceSourceLogger;

namespace TradeHub.Common.Core.Utility
{
    /// <summary>
    /// Generates Unique IDs to be assigned to connecting Applications
    /// </summary>
    public static class ApplicationIdGenerator
    {
        private static Type _type = typeof (ApplicationIdGenerator);

        private const int Min = 0xA00;
        private const int Max = 0xFF9;
        private static int _value = Min - 1;
        
        // Contains the IDs which are currently in use
        private static List<string> _idsInUse = new List<string>();


        /// <summary>
        /// Provides New Valid ID
        /// </summary>
        /// <returns></returns>
        public static string NextId()
        {
            if (_value < Max)
            {
                _value++;
            }
            else
            {
                _value = Min;
            }
            return _value.ToString("X");
        }

        /// <summary>
        /// Adds the New Generated ID to the local IDs Map
        /// </summary>
        /// <param name="id">Unqiue ID</param>
        private static bool AddNewId(string id)
        {
            try
            {
                if (!_idsInUse.Contains(id))
                {
                    _idsInUse.Add(id);
                    return true;
                }
                return false;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "AddNewId");
                return false;
            }
        }

        /// <summary>
        /// Removes specified ID from the local IDs Map
        /// </summary>
        /// <param name="id">Unqiue ID</param>
        private static bool RemoveId(string id)
        {
            try
            {
                if (_idsInUse.Remove(id))
                {
                    return true;
                }
                return false;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RemoveId");
                return false;
            }
        }
    }
}
