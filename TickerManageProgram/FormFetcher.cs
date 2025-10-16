using AngleSharp;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
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
        public async Task<string> GetFormString(JsonNode filings, int index)
        {
            if (filings == null)
            { return null; }
            string cik = CIKProvider.defaultCIKLibrary.GetCIK(ticker);

            var accessionNumbers = filings["accessionNumber"];
            var primaryDocs = filings["primaryDocument"];

            var accessionArr = accessionNumbers.AsArray();
            var primaryDocArr = primaryDocs.AsArray();

            string accession = accessionArr[index].GetValue<string>().Replace("-", "");
            string primaryDoc = primaryDocArr[index].GetValue<string>();

            // xslF345X05/ 같은 폴더 부분 제거 → 진짜 파일명만 남김
            if (primaryDoc.Contains("/"))
            {
                primaryDoc = primaryDoc.Substring(primaryDoc.LastIndexOf('/') + 1);
            }

            // 원본 URL 만들기
            string url = $"https://www.sec.gov/Archives/edgar/data/{cik}/{accession}/{primaryDoc}";
            string formString = await httpClient.GetStringAsync(url);
            return formString;
        }
        public XDocument ParseToXML(string formString)
        {
            // XML 파싱
            var xdoc = XDocument.Parse(formString);
            return xdoc;
        }
        public async Task<string> ParseFromHTML(string formString)
        {
            var context = BrowsingContext.New(Configuration.Default);
            var doc = await context.OpenAsync(req => req.Content(formString));
            string html = doc.Body?.TextContent ?? "";

            html = System.Net.WebUtility.HtmlDecode(html);
            html = Regex.Replace(html, @"\s+", " ").Trim();
            return html;
        }
    }
}
