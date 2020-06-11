using dCom.Configuration;
using DERMSCommon.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DarkSkyApi;
using DarkSkyApi.Models;
using DERMSCommon;
using DERMSCommon.DataModel.Meas;
using System.Threading;
using Modbus.FunctionParameters;
using Modbus;
using Common;
using ProcessingModule;
using Innovative.SolarCalculator;

namespace dCom.Simulation
{
    public class WheaterSimulator : SCADACommunication
    {
        private HourDataPoint hourDataPoint = new HourDataPoint();
        private DarkSkyService darkSkyProxy;
        public WheaterSimulator()
        {
            // fa6d00664c0c9abf42654341ff91db31
            // e67254e31e12e23461c61e0fb0489142
            // ab42e06e054eb1164d36132c278edef9
            darkSkyProxy = new DarkSkyService("e67254e31e12e23461c61e0fb0489142");
        }

        public async void GetWeatherForecastAsyncSimulate()
        {

            foreach (KeyValuePair<long, IdentifiedObject> kvp in analogniStari)
            {
                Forecast result = await darkSkyProxy.GetTimeMachineWeatherAsync(((Analog)kvp.Value).Latitude, ((Analog)kvp.Value).Longitude, DateTime.Now, Unit.Auto);
                List<HourDataPoint> hourDataPoints = result.Hourly.Hours.ToList();

                DERMSCommon.WeatherForecast.WeatherForecast weatherForecast = new DERMSCommon.WeatherForecast.WeatherForecast(1001, 1, 1, 1, 1, DateTime.Now, "");
                foreach (HourDataPoint hdr in hourDataPoints)
                {
                    if (hdr.Time.Hour.Equals(DateTime.Now.Hour))
                        hourDataPoint = hdr;
                }
                float vrednost = 0;

                vrednost = CalculateHourAhead(((Analog)kvp.Value).Name, ((Analog)kvp.Value).NormalValue, ((Analog)kvp.Value).Latitude, ((Analog)kvp.Value).Latitude);
                foreach (KeyValuePair<List<long>, ushort> gidoviNaAdresu in GidoviNaAdresu)
                {
                    if (gidoviNaAdresu.Key[1] == (((Analog)kvp.Value).GlobalId) && ((Analog)kvp.Value).Description == "Simulation")
                    {


                        try
                        {
                            ModbusWriteCommandParameters p = new ModbusWriteCommandParameters(6, (byte)ModbusFunctionCode.WRITE_SINGLE_REGISTER, gidoviNaAdresu.Value, (ushort)vrednost, configuration);
                            Common.IModbusFunction fn = FunctionFactory.CreateModbusFunction(p);
                            commandExecutor.EnqueueCommand(fn);
                            ((Analog)kvp.Value).NormalValue = vrednost;
                            ModbusWriteCommandParameters p1 = new ModbusWriteCommandParameters(6, (byte)ModbusFunctionCode.WRITE_SINGLE_REGISTER, (ushort)(gidoviNaAdresu.Value - 1), (ushort)vrednost, configuration);
                            Common.IModbusFunction fn1 = FunctionFactory.CreateModbusFunction(p1);
                            commandExecutor.EnqueueCommand(fn1);
                            if (vrednost == 0)
                            {
                                ModbusWriteCommandParameters p12 = new ModbusWriteCommandParameters(6, (byte)ModbusFunctionCode.WRITE_SINGLE_REGISTER, (ushort)(gidoviNaAdresu.Value - 2), (ushort)vrednost, configuration);
                                Common.IModbusFunction fn12 = FunctionFactory.CreateModbusFunction(p12);
                                commandExecutor.EnqueueCommand(fn12);
                            }
                        }
                        catch (Exception ex)
                        {
                            string message = $"{ex.TargetSite.ReflectedType.Name}.{ex.TargetSite.Name}: {ex.Message}";

                        }
                    }

                }
            }

            //foreach (KeyValuePair<long, IdentifiedObject> kvp in digitalniStari)
            //{
            //    float vrednost = 0;
            //    List<Forecast> fo = new List<Forecast>();
            //    GetWeatherForecastAsync(((Discrete)kvp.Value).Latitude, ((Discrete)kvp.Value).Longitude);
            //    vrednost = CalculateHourAhead(((Discrete)kvp.Value).Name, ((Discrete)kvp.Value).NormalValue, ((Discrete)kvp.Value).Latitude, ((Discrete)kvp.Value).Latitude);
            //    foreach (KeyValuePair<List<long>, ushort> gidoviNaAdresu in GidoviNaAdresu)
            //    {
            //        if (gidoviNaAdresu.Key.Contains(((Discrete)kvp.Value).GlobalId))
            //        {
            //            ushort raw = 0;
            //            raw = EGUConverter.ConvertToRaw(2, 5, vrednost);
            //            ModbusWriteCommandParameters p = new ModbusWriteCommandParameters(6, (byte)ModbusFunctionCode.WRITE_SINGLE_COIL, gidoviNaAdresu.Value, raw, configuration);
            //            IModbusFunction fn = FunctionFactory.CreateModbusFunction(p);
            //            commandExecutor.EnqueueCommand(fn);
            //        }
            //    }
            //}


        }
    



  
            


        
        public float CalculateHourAhead(string tip, float ConsiderP, float longitude, float latitude)
        {




            float P = 0;

            if (tip == "Wind")
            {



                if (hourDataPoint.WindSpeed < 3.5)
                {
                    P = 0;
                }
                else if (hourDataPoint.WindSpeed >= 3.5 && hourDataPoint.WindSpeed < 14)
                {
                    P = (float)((hourDataPoint.WindSpeed - 3.5) * 0.035 * 1000);
                }
                else if (hourDataPoint.WindSpeed >= 14 && hourDataPoint.WindSpeed < 25)
                {
                    P = ConsiderP;
                }
                else if (hourDataPoint.WindSpeed >= 25)
                {
                    P = 0;
                }



                //TODO formula za windTurbine

            }
            else if (tip == "Solar")
            {


                double insolation = 0;

                insolation = 990 * (1 - hourDataPoint.CloudCover * hourDataPoint.CloudCover * hourDataPoint.CloudCover);
                double TCell = hourDataPoint.Temperature + 0.025 * insolation;
                if (TCell >= 25)
                {
                    TCell = 25;
                }


                P = (float)(ConsiderP * insolation * 0.00095 * (1 - 0.005 * (TCell - 25)));
                SolarTimes solarTimes = new SolarTimes(DateTime.Now, latitude, longitude);
                DateTime sunrise = solarTimes.Sunrise;
                DateTime sunset = solarTimes.Sunset;
                if (hourDataPoint.Time > sunset || hourDataPoint.Time < sunrise)
                    P = 0;




            }
            return P;
        }

    }
}
