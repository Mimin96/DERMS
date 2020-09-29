using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudCommon
{
    public interface IMessageWriting
    {
        event EventHandler<string> MessageRcv;
    }
}
