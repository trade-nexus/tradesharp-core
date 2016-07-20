using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Fix.Constants
{
    public class SubscriptionType
    {
        // '0' 	Snapshot
        public const char Snapshot = '0';

        // '1' 	Snapshot + Updates (Subscribe)
        public const char SubscribeSnapshotUpdate = '1';

        // '2' 	Disable previous Snapshot + Update Request (Unsubscribe)
        public const char UnsubscribeSnapshotUpdate = '2';
    }
}
