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
        private List<long> generatorsForOverclock = new List<long>();
        private Dictionary<long, bool> stateOfGenerator = new Dictionary<long, bool>();

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
                        foreach (KeyValuePair<long,bool> kvpGenerator in stateOfGenerator)
                        {
                            if (gr.Equipments.Contains(kvpGenerator.Key) && !kvpGenerator.Value)  // <- umesto flexibility treba generator.flexibility
                            {
                                generatorsForOverclock.Add(kvpGenerator.Key);
                                flexibility = true;
                                break;
                            }
                        }
                    }
                    foreach (long gen in generatorsForOverclock)
                    {
                        stateOfGenerator[gen] = !stateOfGenerator[gen];
                    }

                }
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

                        if(!stateOfGenerator.ContainsKey(generator.GlobalId))
                            stateOfGenerator.Add(generator.GlobalId, false);
                    }
                }
            }
            return generators;
        }
    }
}
