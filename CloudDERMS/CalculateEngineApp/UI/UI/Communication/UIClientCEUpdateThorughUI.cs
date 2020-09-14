using CalculationEngineServiceCommon;
using DERMSCommon.DataModel.Core;
using DERMSCommon.WeatherForecast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace UI.Communication
{
    public class UIClientCEUpdateThorughUI : ClientBase<ICEUpdateThroughUI>, ICEUpdateThroughUI
    {
        public UIClientCEUpdateThorughUI()
        {

        }

        public UIClientCEUpdateThorughUI(string endpoint) : base(endpoint)
        {

        }
        public Task<List<long>> AllGeoRegions()
        {
            return Channel.AllGeoRegions();
        }

        public Task<List<long>> AllowOptimization(long gid)
        {
            return Channel.AllowOptimization(gid);
        }

        public Task<float> Balance(Dictionary<long, DerForecastDayAhead> prod, long GidUi, Dictionary<long, IdentifiedObject> networkModel, List<long> TurnedOffGenerators)
        {
            return Balance(prod, GidUi, networkModel, TurnedOffGenerators);
        }

        public Task<float> BalanceNetworkModel()
        {
            return Channel.BalanceNetworkModel();
        }

        public Task<List<Generator>> GeneratorOffCheck()
        {
            return Channel.GeneratorOffCheck();
        }

        public Task<List<long>> ListOfDisabledGenerators()
        {
            return Channel.ListOfDisabledGenerators();
        }

        public Task<List<Generator>> ListOffTurnedOffGenerators()
        {
            return Channel.ListOffTurnedOffGenerators();
        }

        public Task<float> UpdateThroughUI(long data)
        {
            return Channel.UpdateThroughUI(data);
        }
    }
}
