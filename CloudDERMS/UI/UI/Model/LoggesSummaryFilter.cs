using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.Model
{
    public class LoggesSummaryFilter
    {
        public LoggesSummaryFilter()
        {
            FilterByTime = false;
            SelectedFilterLogLevel = "";
            SelectedFilterComponent = "";
            Message = "Message";
            From = DateTime.Today.AddDays(-1);
            To = DateTime.Today.AddDays(1);
        }

        public bool FilterByTime
        {
            get;
            set;
        }
        public string Message
        {
            get;
            set;
        }
        public string SelectedFilterLogLevel
        {
            get;
            set;
        }
        public string SelectedFilterComponent
        {
            get;
            set;
        }
        public DateTime From
        {
            get;
            set;
        }
        public DateTime To
        {
            get;
            set;
        }
    }
}
