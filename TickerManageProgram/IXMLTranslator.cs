using System.Xml.Linq;

namespace TickerManageProgram
{
    internal interface IXMLTranslator
    {
        public string Summary(XDocument xdoc);
    }
}
