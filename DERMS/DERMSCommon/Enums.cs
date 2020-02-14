using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon
{
    public class Enums
    {
        public enum LogLevel
        {
            Info = 0,
            Warning,
            Error,
            Fatal
        }

        public enum Component
        {
            CalculationEngine = 0,
            NMS,
            SCADA,
            TransactionCoordinator,
            UI
        }

        public enum Topics
        {
            Default = 1
            //TREBA VIDETI SA CAVICEM KO SE SVE PRETPLACUJE NA PUBSUB
        }
    }
}
