using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudCommon
{
    public class MessageWriting : IMessageWriting
    {
        public event EventHandler<string> MessageRcv;

        protected void MessageReceivedEvent(string e)
        {
            this.MessageRcv?.Invoke(this, e);
        }
    }
}
