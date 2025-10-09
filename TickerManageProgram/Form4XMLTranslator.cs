using System.Text;
using System.Xml.Linq;

namespace TickerManageProgram
{
    internal class Form4XMLTranslator : IXMLTranslator
    {
        public string Summary(XDocument xdoc)
        {
            StringBuilder sb = new();

            // 보고일
            var ownershipDocs = xdoc.Descendants("ownershipDocument");
            foreach (var ownershipDoc in ownershipDocs)
            {
                string reportingDate = ownershipDoc?.Element("periodOfReport")?.Value ?? "N/A";
                sb.Append($"보고일: {reportingDate}");
                sb.AppendLine();
            }


            // 보고자(내부자) 정보
            var owners = xdoc.Descendants("reportingOwner");
            foreach (var owner in owners)
            {
                string name = owner.Element("reportingOwnerId")?.Element("rptOwnerName")?.Value ?? "N/A";
                string title = owner.Element("reportingOwnerRelationship")?.Element("officerTitle")?.Value ?? "N/A";
                sb.Append($"보고자: {name} ({title})");
                sb.AppendLine();
            }

            // 거래 내역
            var transactions = xdoc.Descendants("nonDerivativeTransaction");
            int cnt = transactions.Count();
            int i = 0;
            foreach (var tx in transactions)
            {
                string code = tx.Element("transactionCoding")?.Element("transactionCode")?.Value ?? "N/A";
                string date = tx.Element("transactionDate")?.Element("value")?.Value ?? "N/A";
                string shares = tx.Element("transactionAmounts")?.Element("transactionShares")?.Element("value")?.Value ?? "N/A";
                string price = tx.Element("transactionAmounts")?.Element("transactionPricePerShare")?.Element("value")?.Value ?? "N/A";

                sb.Append($"- 거래일: {date}, 코드: {code}, 수량: {shares}, 단가: {price}");
                i++;
                if (i < cnt)
                { sb.AppendLine(); }
            }
            return sb.ToString();
        }
    }
}
