using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickerManageProgram
{
    internal static class HttpClientProvider
    {
        static SemaphoreSlim instanceSS = new SemaphoreSlim(1, 1);
        static HttpClient instance;

        public static async Task<HttpClient> GetInstance()
        {
            await instanceSS.WaitAsync();
            try
            {
                if (instance == null)
                { 
                    instance = new HttpClient();
                    // set default header...
                }
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
    }
}
