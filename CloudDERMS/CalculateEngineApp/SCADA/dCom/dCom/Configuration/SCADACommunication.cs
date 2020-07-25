using Common;
using DERMSCommon.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dCom.Configuration
{
    public class SCADACommunication
    {

        public static Dictionary<long, IdentifiedObject> digitalniStari = new Dictionary<long, IdentifiedObject>();
        public static Dictionary<long, IdentifiedObject> analogniStari = new Dictionary<long, IdentifiedObject>();
        public static Dictionary<List<long>, ushort> GidoviNaAdresu = new Dictionary<List<long>, ushort>();
        public static IFunctionExecutor commandExecutor;
        public static IConfiguration configuration;
    }
}
