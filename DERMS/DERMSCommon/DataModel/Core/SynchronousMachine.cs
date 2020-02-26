using DarkSkyApi.Models;
using DERMSCommon.WeatherForecast;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DERMSCommon.DataModel.Core
{
    [DataContract]
    public class Generator : RegulatingCondEq
    {
        [DataMember]
        private float maxQ;
        [DataMember]
        private float minQ;
        [DataMember]
        private float considerP;
        [DataMember]
        private GeneratorType generatorType;

        public float MaxQ { get => maxQ; set => maxQ = value; }
        public float MinQ { get => minQ; set => minQ = value; }
        public float ConsiderP { get => considerP; set => considerP = value; }
        public GeneratorType GeneratorType { get => generatorType; set => generatorType = value; }

        public Generator(long globalId) : base(globalId)
        {
        }
        public DayAhead CalculateDayAhead(Forecast forecast, long gid, Substation substation)
        {
            DayAhead dayAhead = new DayAhead();

            //Substation substation = new Substation(gid);
            SubGeographicalRegion subGeoRegion = new SubGeographicalRegion(substation.SubGeoReg);
            Random random = new Random();
            double lat, lon;
            ToLatLon(substation.Latitude, substation.Longitude, 34, out lat, out lon);
            substation.Latitude = (float)lat;
            substation.Longitude = (float)lon;
            float P = 0;

            if (this.GeneratorType.Equals(GeneratorType.Wind))
            {
                
                foreach (DarkSkyApi.Models.HourDataPoint dataPoint in forecast.Hourly.Hours.Take(24))
                {
                    WeatherForecast.HourDataPoint hourDataPoint = new WeatherForecast.HourDataPoint();
                    hourDataPoint.Time = dataPoint.Time.DateTime;

                    if (dataPoint.WindSpeed < 3.5)
                    {
                        P = 0;
                    }
                    else if (dataPoint.WindSpeed >= 3.5 && dataPoint.WindSpeed < 14)
                    {
                        P = (float)((dataPoint.WindSpeed - 3.5) * 0.035 );
                    }
                    else if (dataPoint.WindSpeed >= 14 && dataPoint.WindSpeed < 25)
                    {
                        P = ConsiderP;
                    }
                    else if (dataPoint.WindSpeed >= 25)
                    {
                        P = 0;
                    }


                    hourDataPoint.ActivePower = P;
                    hourDataPoint.ReactivePower = 0;

                    dayAhead.Hourly.Add(hourDataPoint);
                    //TODO formula za windTurbine
                }
            }
            else if (this.GeneratorType.Equals(GeneratorType.Solar))
            {
                
                foreach (DarkSkyApi.Models.HourDataPoint dataPoint in forecast.Hourly.Hours.Take(24))
                {
                    WeatherForecast.HourDataPoint hourDataPoint = new WeatherForecast.HourDataPoint();
                    hourDataPoint.Time = dataPoint.Time.DateTime;
                    hourDataPoint.Time = hourDataPoint.Time.AddHours(forecast.TimeZoneOffset);

                    SPACalculator.SPAData spa = new SPACalculator.SPAData();
                    spa.SetUpData(substation.Latitude, substation.Longitude, hourDataPoint);

                    var result = SPACalculator.SPACalculate(ref spa);
                    double zenit = spa.Zenith;
                    double s = ConsiderP * (1 - dataPoint.CloudCover);
                    double test1 = Math.Cos((Math.PI / 100) * zenit);
                    double insolation = s * Math.Cos((Math.PI / 100) * zenit);

                    insolation = 990 * (1 - dataPoint.CloudCover * dataPoint.CloudCover * dataPoint.CloudCover);
                    double TCell = dataPoint.Temperature + 0.025 * insolation;
                    if (TCell >= 25)
                    {
                        TCell = 25;
                    }

                    
                    P = (float)(ConsiderP * insolation * 0.00095 * (1 - 0.005 * (TCell - 25)));

                    hourDataPoint.ActivePower = P;
                    hourDataPoint.ReactivePower = 0;

                    dayAhead.Hourly.Add(hourDataPoint);
                }

            }
            return dayAhead;
        }

        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }



        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                Generator x = (Generator)obj;
                return (x.maxQ == this.maxQ && x.minQ == this.minQ && x.considerP == this.considerP && x.generatorType == this.generatorType);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #region IAccess implementation

        public override bool HasProperty(ModelCode property)
        {
            switch (property)
            {
                case ModelCode.GENERATOR_GENERATORTYPE:
                case ModelCode.GENERATOR_MAXQ:
                case ModelCode.GENERATOR_MINQ:
                case ModelCode.GENERATOR_CONSIDERP:
                    return true;

                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {
                case ModelCode.GENERATOR_GENERATORTYPE:
                    prop.SetValue((short)generatorType);
                    break;

                case ModelCode.GENERATOR_MAXQ:
                    prop.SetValue(maxQ);
                    break;

                case ModelCode.GENERATOR_MINQ:
                    prop.SetValue(minQ);
                    break;

                case ModelCode.GENERATOR_CONSIDERP:
                    prop.SetValue(considerP);
                    break;

                default:
                    base.GetProperty(prop);
                    break;
            }
        }

        public override void SetProperty(Property property)
        {
            switch (property.Id)
            {

                case ModelCode.GENERATOR_GENERATORTYPE:
                    generatorType = (GeneratorType)property.AsEnum();
                    break;
                case ModelCode.GENERATOR_MAXQ:
                    maxQ = property.AsFloat();
                    break;

                case ModelCode.GENERATOR_MINQ:
                    minQ = property.AsFloat();
                    break;

                case ModelCode.GENERATOR_CONSIDERP:
                    considerP = property.AsFloat();
                    break;

                default:
                    base.SetProperty(property);
                    break;
            }
        }

        #endregion IAccess implementation

        #region IReference implementation

        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {
            //if (powerSystemResource != 0 && (refType != TypeOfReference.Reference || refType != TypeOfReference.Both))
            //{
            //    references[ModelCode.MEASUREMENT_PSR] = new List<long>();
            //    references[ModelCode.MEASUREMENT_PSR].Add(powerSystemResource);
            //}

            base.GetReferences(references, refType);
        }

        #endregion IReference implementation
    }
}
