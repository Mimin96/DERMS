using CloudCommon.SCADA;
using DERMSCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace UI.Communication
{
    public class UIClientEvents : ClientBase<IEvetnsDatabase>, IEvetnsDatabase
    {
        public UIClientEvents()
        {

        }

        public UIClientEvents(string endpoint) : base(endpoint)
        {

        }

        public Task<List<Event>> GetEvents()
        {
            return Channel.GetEvents();
        }

        public Task SetEvent(Event eventt)
        {
            return Channel.SetEvent(eventt);
        }
    }
}
