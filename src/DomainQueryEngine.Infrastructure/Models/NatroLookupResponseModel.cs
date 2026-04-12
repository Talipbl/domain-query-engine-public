using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainQueryEngine.Natro
{
    public class NatroLookupResponseModel
    {
        public ServiceResponseModel ServiceResponse { get; set; }
        public class ServiceResponseModel
        {
            public Api Api { get; set; }
            public List<object> InBasket { get; set; }
        }
        public class Api
        {
            public Response Response { get; set; }
        }

        public class Audit
        {
            public string Start { get; set; }
            public string Finish { get; set; }
            public string Duration { get; set; }
        }

        public class Domain
        {
            public string Avail { get; set; }
            public string Xtype { get; set; }
            public string Xdata { get; set; }
            public string Ext { get; set; }
            public string OriginalPrice { get; set; }
            public string Price { get; set; }
            public string MasterDomain { get; set; }
            public string TldType { get; set; }
            public string TldTypeDescription { get; set; }
            public string MarketingPriceType { get; set; }
            public string MarketingPriceTypeDescription { get; set; }
            public string DisplayDomainName { get; set; }
            public string RequiredActionType { get; set; }
            public string RequiredActionTypeDetail { get; set; }
            public string RequiredActionTypeDescription { get; set; }
            public int Minyear { get; set; }
            public string Text { get; set; }
        }

        public class Resdata
        {
            public Domain Domain { get; set; }
        }

        public class Response
        {
            public Result Result { get; set; }
            public string Msg { get; set; }
            public Resdata Resdata { get; set; }
            public string Ver { get; set; }
            public string Dname { get; set; }
            public string Totalpagecount { get; set; }
            public string QueryId { get; set; }
            public Audit Audit { get; set; }
        }

        public class Result
        {
            public int Code { get; set; }
        }
    }
}
