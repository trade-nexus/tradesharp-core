namespace TradeHub.Common.Core.DomainModels
{
    /// <summary>
    /// For each opening transaction a position is created which also carries exit values
    /// and maintenance margin.
    /// </summary>
    public class Position
    {
        private long _quantity;
        private Security _security;
        private decimal _exitValue;
        private bool _isOpen;
        private string _provider;
        private decimal _avgBuyPrice;
        private decimal _avgSellPrice;
        private decimal _price;
        private PositionType _positionType;

        public Position()
        {
            
        }


        public decimal Price
        {
            get { return _price; }
            set { _price = value; }
        }

        public decimal AvgSellPrice
        {
            get { return _avgSellPrice; }
            set { _avgSellPrice = value; }
        }

        public decimal AvgBuyPrice
        {
            get { return _avgBuyPrice; }
            set { _avgBuyPrice = value; }
        }
        public long Quantity
        {
            get { return _quantity; }
            set { _quantity = value; }
        }
        
        public Security Security
        {
            get { return _security; }
            set { _security = value; }
        }

        public decimal ExitValue
        {
            get { return _exitValue; }
            set { _exitValue = value; }
        }

        public bool IsOpen
        {
            get { return _isOpen; }
            set { _isOpen=value; }
        }
        
        public string Provider
        {
            get { return _provider; }
            set { _provider = value; }
        }

        public PositionType Type
        {
            get { return _positionType; }
            set { _positionType = value; }
        }

        public override string ToString()
        {
            return "Provider: " + Provider + " | Symbol: " + Security.Symbol+
                   " | Position Type: " + Type + " | Quantity: " + Quantity +" | Price: " + _price+ " | AvgBuyPrice: " + _avgBuyPrice+
                   " | AvgSellPrice: " + _avgSellPrice + " | Open: " + IsOpen;
        }
    }



}

