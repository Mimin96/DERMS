using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DermsUI.Model
{
    public class Log
    {
        private string description;
        private DateTime datetime;
        private Enums.Content logContent;
        private Enums.Components belongsTo;

        public Log()
        {

        }

        public string Description { get { return description; } set { description = value; } }
        public DateTime Datetime { get { return datetime; } set { datetime = value; } }
        public Enums.Content LogContent { get { return logContent; } set { logContent = value; } }
        public Enums.Components BelongsTo { get { return belongsTo; } set { belongsTo = value; } }
    }
}
