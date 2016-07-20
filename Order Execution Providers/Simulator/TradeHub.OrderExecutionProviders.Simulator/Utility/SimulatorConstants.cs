namespace TradeHub.OrderExecutionProviders.Simulator.Utility
{
    /// <summary>
    /// Contains Constant values related to Simulated Orders
    /// </summary>
    public static class SimulatorConstants
    {
        /// <summary>
        /// Contains Message Types which simulator can process
        /// </summary>
        public static class MessageTypes
        {
            public const string New = "n";
            public const string Cancel = "c";
            public const string Execution = "e";
            public const string Rejection = "r";
            public const string Locate = "l";
            public const string Position = "p";
        }
    }
}
