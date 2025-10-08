using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace TickerManageProgram
{
    internal class FormFetcher
    {
        readonly static string userInfo = "Kwon Yongsoo (kys0521016@gmail.com)";
        readonly string ticker;

        public FormFetcher(string ticker)
        {
            this.ticker = ticker;
        }

        public async Task<JsonNode> FetchFilings()
        {
            string cik = TickerManageProgram.cikProvider.GetCIK(ticker);
            string submissionsUrl = $"https://data.sec.gov/submissions/CIK{cik}.json";

            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", userInfo);

            try
            {
                // STEP 1: 최근 제출 JSON 가져오기
                var response = await client.GetStreamAsync(submissionsUrl);
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

        public async Task<XDocument> GetXMLForm(JsonNode filings, int index)
        {
            if (filings == null)
            { return null; }
            string cik = TickerManageProgram.cikProvider.GetCIK(ticker);

            var forms = filings["form"];
            var accessionNumbers = filings["accessionNumber"];
            var primaryDocs = filings["primaryDocument"];

            List<XDocument> xmlForms = new();
            var formsArr = forms.AsArray();
            var accessionArr = accessionNumbers.AsArray();
            var primaryDocArr = primaryDocs.AsArray();

            string accession = accessionArr[index].GetValue<string>().Replace("-", "");
            string primaryDoc = primaryDocArr[index].GetValue<string>();

            // xslF345X05/ 같은 폴더 부분 제거 → 진짜 XML 파일명만 남김
            if (primaryDoc.Contains("/"))
            {
                primaryDoc = primaryDoc.Substring(primaryDoc.LastIndexOf('/') + 1);
            }

            // 원본 XML URL 만들기
            string xmlUrl = $"https://www.sec.gov/Archives/edgar/data/{cik}/{accession}/{primaryDoc}";

            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", userInfo);
            string xmlString = await client.GetStringAsync(xmlUrl);

            // XML 파싱
            var xdoc = XDocument.Parse(xmlString);
            return xdoc;
        }
    }
}
