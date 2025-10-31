using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickerManageProgram.TradeWatching
{
    internal interface ICumulated
    {
        public DateOnly from { get; }
        public DateOnly to { get; }
    }
    internal struct DetailedPrice
    {
        public float open => throw new NotImplementedException();
        public float close => throw new NotImplementedException();
        public float low => throw new NotImplementedException();
        public float high => throw new NotImplementedException();
    }
    internal struct MeanPrice : ICumulated
    {
        public DateOnly from => throw new NotImplementedException();
        public DateOnly to => throw new NotImplementedException();

        public double price => throw new NotImplementedException();
    }
    internal struct StochasticRSI : ICumulated
    {
        public DateOnly from => throw new NotImplementedException();

        public DateOnly to => throw new NotImplementedException();
        public float rsiK => throw new NotImplementedException();
        public float rsiD => throw new NotImplementedException();
    }
}
