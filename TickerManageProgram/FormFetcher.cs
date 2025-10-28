using AngleSharp;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace TickerManageProgram
{
    internal class FormFetcher
    {
        readonly static string userInfo = "Kwon Yongsoo (kys0521016@gmail.com)";
        readonly string ticker;
        readonly HttpClient httpClient;

        public FormFetcher(string ticker)
        {
            this.ticker = ticker;
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", userInfo);
        }

        public async Task<JsonNode> FetchFilings()
        {
            string cik = CIKProvider.defaultCIKLibrary.GetCIK(ticker);
            string submissionsUrl = $"https://data.sec.gov/submissions/CIK{cik}.json";

            try
            {
                // STEP 1: 최근 제출 JSON 가져오기
                using var response = await httpClient.GetStreamAsync(submissionsUrl);
                using JsonDocument doc = JsonDocument.Parse(response);
                var filings = doc.RootElement.GetProperty("filings").GetProperty("recent");
                JsonNode filingsNode = JsonNode.Parse(filings.GetRawText());

                return filingsNode;
            }
            catch (Exception ex)
            {
                LogChannel.EnqueueLog(new Log(Log.LogType.system, "오류 발생: " + ex.Message));
            }
            return null;
        }

        FormMeta GetFormMeta(JsonNode filings, int index)
        {
            FormMeta result = new();
            if (filings == null)
            { return result; }

            var accessionNumbers = filings["accessionNumber"];
            var primaryDocs = filings["primaryDocument"];

            var accessionArr = accessionNumbers.AsArray();
            var primaryDocArr = primaryDocs.AsArray();


            result.cik = CIKProvider.defaultCIKLibrary.GetCIK(ticker);
            result.accession = accessionArr[index].GetValue<string>().Replace("-", "");
            result.primaryDoc = primaryDocArr[index].GetValue<string>();

            // xslF345X05/ 같은 폴더 부분 제거 → 진짜 파일명만 남김
            if (result.primaryDoc.Contains("/"))
            {
                result.primaryDoc = result.primaryDoc.Substring(result.primaryDoc.LastIndexOf('/') + 1);
            }
            return result;
        }
        
        public async Task<string> GetFormString(JsonNode filings, int index)
        {
            FormMeta meta = GetFormMeta(filings, index);
            return await GetFormStringInternal(meta);
        }
        async Task<string> GetFormStringInternal(FormMeta meta)
        {
            // 원본 URL 만들기
            string url = $"https://www.sec.gov/Archives/edgar/data/{meta.cik}/{meta.accession}/{meta.primaryDoc}";
            string formString = await httpClient.GetStringAsync(url);

            return formString;
        }
        public async Task<IEnumerable<string>> GetFormStringIncludeAttachment(JsonNode filings, int index)
        {
            List<string> result = new List<string>();
            FormMeta meta = GetFormMeta(filings, index);
            string formString = await GetFormStringInternal(meta);
            result.Add(formString);

            List<string> attachDocsList = new List<string>();
            const string prefix1 = "href=\"";
            const string prefix2 = "HREF=\"";

            IEnumerable<string> FindWithPrefix(string formString, string prefix, string exceptionDoc)
            {
                List<string> matches = new();
                int newPrefixIndex = 0;
                int newSearchIndex = 0;
                while ((newPrefixIndex = formString.IndexOf(prefix, newSearchIndex)) != -1)
                {
                    int valueIndex = newPrefixIndex + prefix.Length;
                    int endOfValueIndex = formString.IndexOf('"', valueIndex);
                    newSearchIndex = endOfValueIndex + 1;
                    string value = formString[valueIndex..endOfValueIndex];
                    if (!string.Equals(value, exceptionDoc))
                    { matches.Add(value); }
                }
                return matches;
            }

            var m1 = FindWithPrefix(formString, prefix1, meta.primaryDoc);
            attachDocsList.AddRange(m1);
            var m2 = FindWithPrefix(formString, prefix2, meta.primaryDoc);
            attachDocsList.AddRange(m2);

            foreach (var attachDoc in attachDocsList.Distinct())
            {
                FormMeta attachMeta = new FormMeta() { cik = meta.cik, accession = meta.accession, primaryDoc = attachDoc };
                // 추출한 첨부 form meta가 유효하지 않을 수 있음.
                try
                {
                    string attachString = await GetFormStringInternal(attachMeta);
                    result.Add(attachString);
                }
                catch
                {
                    Console.WriteLine($"{ticker}: 첨부 문서를 불러오는데 실패했습니다. CIK-{attachMeta.cik} , Accession-{attachMeta.accession} , PrimaryDoc-{attachMeta.primaryDoc}");
                }
            }
            return result;
        }
        public XDocument ParseToXML(string formString)
        {
            // XML 파싱
            var xdoc = XDocument.Parse(formString);
            return xdoc;
        }
        public async Task<string> ParseFromHTML(string formString)
        {
            using var context = BrowsingContext.New(Configuration.Default);
            using var doc = await context.OpenAsync(req => req.Content(formString));
            string html = doc.Body?.TextContent ?? "";

            html = System.Net.WebUtility.HtmlDecode(html);
            html = Regex.Replace(html, @"\s+", " ").Trim();
            return html;
        }
        struct FormMeta
        {
            public string cik;
            public string accession;
            public string primaryDoc;
        }
    }
}
