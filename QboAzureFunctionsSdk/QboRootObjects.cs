using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fbi.QuickBooksSolutionTemplate.FunctionEntryPoint
{
    public class TBColData
    {
        public string value { get; set; }
        public string id { get; set; }
    }

    public class TBColData2
    {
        public string value { get; set; }
    }

    public class TBSummary
    {
        public List<TBColData2> ColData { get; set; }
    }

    public class TBRootObject
    {
        public List<TBColData> ColData { get; set; }
        public TBSummary Summary { get; set; }
        public string type { get; set; }
        public string group { get; set; }
    }

    public class ColData
    {
        public string value { get; set; }
        public string id { get; set; }
    }

    public class ColData2
    {
        public string value { get; set; }
    }

    public class Summary
    {
        public List<ColData2> ColData { get; set; }
    }

    public class RootObject
    {
        public List<ColData> ColData { get; set; }
        public Summary Summary { get; set; }
        public string type { get; set; }
        public string group { get; set; }
    }


}
