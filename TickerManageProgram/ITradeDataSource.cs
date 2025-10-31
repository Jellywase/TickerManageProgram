using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickerManageProgram
{
    internal interface ITradeDataSource
    {
        public CumulatedTradeData GetCumulatedTradeData(string ticker, DateTime from, DateTime to);
        public DailyTradeData GetSingleTradeData(string ticker, DateTime when);
    }
}
