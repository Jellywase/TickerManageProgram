using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickerManageProgram.TradeWatching
{
    internal class TradeDataMemory
    {
        public SortedDictionary<DateOnly, DetailedPrice> detailedPriceHistory = new();
    }
}
