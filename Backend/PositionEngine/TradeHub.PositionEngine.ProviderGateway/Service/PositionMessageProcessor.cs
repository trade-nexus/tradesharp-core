using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;

namespace TradeHub.PositionEngine.ProviderGateway.Service
{
    public class PositionMessageProcessor
    {
        private Type _type = typeof (PositionMessageProcessor);

        /// <summary>
        /// Keeps track of all the provider instances and their positions
        /// Key =  Provider Name
        /// Value = Positions List
        /// </summary>
        private Dictionary<string, List<Position>> _providersMap;

        private Dictionary<string, List<string>> _providerPositionsRequest; 
        //private Dictionary<string, int> _openPositions;
        ////private Dictionary<string, int> _closePositions;
        //private Dictionary<string, List<Position>>  _filledPositions; 
        //private List<Position> _positions; 

        
        public PositionMessageProcessor()
        {
            _providersMap=new Dictionary<string, List<Position>>();
            //_openPositions=new Dictionary<string, int>();
            //_closePositions=new Dictionary<string, int>();
            //_filledPositions=new Dictionary<string, List<Position>>();
        }

        public void ProviderRequestReceived(string provider,string appID)
        {
            if (_providerPositionsRequest.ContainsKey(appID))
            {
                List<string> list = _providerPositionsRequest[appID];
                if (list.Contains(provider))
                {
                    Logger.Info(string.Format("This provider {0} is already registered against appID={1}", provider, appID), _type.FullName, "_mqServer_ProviderRequestReceived");
                }
                else
                {
                    list.Add(provider);
                }
            }
            else
            {
                List<string> list = new List<string>();
                list.Add(provider);
                _providerPositionsRequest.Add(appID, list);
            }
        }

        /// <summary>
        /// Handles Positions Message Requests from Applications
        /// </summary>
        public void OnPositionMessageRecieved(Position position)
        {

           if (Logger.IsInfoEnabled)
            {
                Logger.Info(
                    "Position Message received from: " + position.ToString(),
            _type.FullName, "OnPositionMessageRecieved");
            }
            try
            {
                //adding to position to specific provider
                if (_providersMap.ContainsKey(position.Provider))
                {
                    List<Position> positions = _providersMap[position.Provider];
                    //var result = from temp in positions where temp.Security.Symbol == position.Security.Symbol select temp;
                    bool check = true;
                    for (int i = 0; i < positions.Count; i++)
                    {
                        if (positions[i].Security.Symbol == position.Security.Symbol)
                        {
                            positions[i] = position;
                            check = false;
                            break;
                        }
                    }
                    if (check)
                        positions.Add(position);
                    _providersMap[position.Provider] = positions;
                }
                else
                {
                    List<Position> positions = new List<Position>();
                    positions.Add(position);
                    _providersMap.Add(position.Provider, positions);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnPositionMessageRecieved");
            }
        }

        private void UpdateStats(Position position)
        {
            //if (_openPositions.ContainsKey(position.Provider)&&position.isOpen)
            //{
            //    int count = _openPositions[position.Provider];
            //    count++;
            //    _openPositions[position.Provider] = count;
            //}
            //else
            //{
            //    _openPositions.Add(position.Provider,1);
                
            //}
            //if (_closePositions.ContainsKey(position.Provider) && !position.isOpen)
            //{
            //    int count = _closePositions[position.Provider];
            //    count++;
            //    _closePositions[position.Provider] = count;
            //    //if (_filledPositions.ContainsKey(position.Provider))
            //    //{
            //    //    List<Position> positions = _filledPositions[position.Provider];
            //    //    positions.Add(position);
            //    //    _filledPositions[position.Provider] = positions;
            //    //}
            //}
            //else
            //{
            //    _closePositions.Add(position.Provider, 1);

            //}
        }
    }
}
