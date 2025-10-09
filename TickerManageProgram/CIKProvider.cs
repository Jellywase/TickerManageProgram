using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickerManageProgram
{
    public static class CIKProvider
    {
        public static ICIKLibrary defaultCIKLibrary { get; private set; }
        public static async Task Initialize()
        {
            CIKLib cikLib = new();
            defaultCIKLibrary = cikLib;
            await cikLib.FetchTable();
        }
    }
}
