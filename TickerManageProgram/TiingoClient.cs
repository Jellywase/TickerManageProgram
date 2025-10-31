using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickerManageProgram
{
    internal class TiingoClient : ITradeDataSource
    {
        static TiingoClient instance;
        static SemaphoreSlim instanceSS = new SemaphoreSlim(1, 1);


        readonly string apiKey = "764e0b5e97a7ddc7210c9c814fd91752e39959c0";
        HttpClient httpClient;
        bool initialized;

        public static async Task<TiingoClient> GetInstance()
        {
            await instanceSS.WaitAsync();
            try
            {
                if (instance == null)
                { instance = new TiingoClient(); }
                if (!instance.initialized)
                { await instance.Initialize(); }
                return instance;
            }
            catch
            {
                throw;
            }
            finally
            {
                instanceSS.Release();
            }
        }

        async Task Initialize()
        {
            initialized = true;
        }
    }
}
