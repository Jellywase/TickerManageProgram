using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickerManageProgram.TradeWatching
{
    internal class TradeDataMiner
    {
        string ticker { get; }
        TradeDataMemory memory = new();

        public TradeDataMiner(string ticker)
        {
            this.ticker = ticker;
        }

        public Task<StochasticRSI> GetStochasticRSI(DateOnly when)
        {
            throw new NotImplementedException();
        }
    }
}
