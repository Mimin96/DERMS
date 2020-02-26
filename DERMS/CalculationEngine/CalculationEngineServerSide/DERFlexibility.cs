using DERMSCommon.DataModel.Core;
using DERMSCommon.NMSCommuication;
using DERMSCommon.WeatherForecast;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineService
{
    public class DERFlexibility
    {
        private NetworkModelTransfer networkModel;
        private List<Generator> generatorsForOverclock = new List<Generator>();

        public DERFlexibility(NetworkModelTransfer networkModel)
        {
            this.networkModel = networkModel;
        }

        public bool CheckFlexibility(long gid)
        {
            bool flexibility = true;
            List<Generator> allGenerators = new List<Generator>();
            allGenerators = GetGenerators();

            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("Substation"))
                    {
                        var gr = (Substation)kvpDic.Value;
                        foreach (Generator generator in allGenerators)
                        {
                            if (gr.Equipments.Contains(generator.GlobalId) && flexibility)  // <- umesto flexibility treba generator.flexibility
                            {
                                generatorsForOverclock.Add(generator);

                            }
                        }
                    }

                }
            }
            if (generatorsForOverclock.Count != 0)
            {
                flexibility = true;
            }
            return flexibility;
        }

        public void TurnOnFlexibility(double flexibilityValue, Dictionary<long, DerForecastDayAhead> derForcast, long gid)
        {

            foreach (HourDataPoint datapoint in derForcast[gid].Production.Hourly)
            {
                datapoint.ActivePower += (float)flexibilityValue;

            }
        }

        public List<Generator> GetGenerators()
        {
            List<Generator> generators = new List<Generator>();
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("Generator"))
                    {
                        var generator = (Generator)kvpDic.Value;
                        generators.Add(generator);
                    }
                }
            }
            return generators;
        }
    }
}
