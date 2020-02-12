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
        public DayAhead CalculateDayAhead(Forecast forecast, long gid)
        {
            DayAhead dayAhead = new DayAhead();

            Substation substation = new Substation(gid);
            SubGeographicalRegion subGeoRegion = new SubGeographicalRegion(substation.SubGeoReg);

            if (this.GeneratorType.Equals(GeneratorType.Wind))
            {
                foreach (DarkSkyApi.Models.HourDataPoint dataPoint in forecast.Hourly.Hours.Take(24))
                {
                    WeatherForecast.HourDataPoint hourDataPoint = new WeatherForecast.HourDataPoint();
                    hourDataPoint.Time = dataPoint.Time.DateTime;

                    float minWindSpeed = 2;
                    float maxWindSpeed = 25;

                    float powerPercent = (dataPoint.WindSpeed - minWindSpeed) / (maxWindSpeed - minWindSpeed);
                    powerPercent += 0.2f;

                    hourDataPoint.ActivePower = considerP * powerPercent;
                    hourDataPoint.ReactivePower = considerP * powerPercent / 50;

                    dayAhead.Hourly.Add(hourDataPoint);
                    //TODO formula za windTurbine
                }
            }
            else if(this.GeneratorType.Equals(GeneratorType.Solar))
            {
                foreach (DarkSkyApi.Models.HourDataPoint dataPoint in forecast.Hourly.Hours.Take(24))
                {
                    WeatherForecast.HourDataPoint hourDataPoint = new WeatherForecast.HourDataPoint();
                    hourDataPoint.Time = dataPoint.Time.DateTime;
                    hourDataPoint.Time = hourDataPoint.Time.AddHours(forecast.TimeZoneOffset);

                    SPACalculator.SPAData spa = new SPACalculator.SPAData();
                    spa.SetUpData(subGeoRegion.Latitude, subGeoRegion.Longitude, hourDataPoint);

                    var result = SPACalculator.SPACalculate(ref spa);
                    double zenit = spa.Zenith;
                    double s = ConsiderP * (1 - dataPoint.CloudCover);
                    double test1 = Math.Cos((Math.PI / 100) * zenit);
                    double insolation = s * Math.Cos((Math.PI / 100) * zenit);
                    if(test1<0)
                    {
                        insolation = 0;
                    }
                    hourDataPoint.ActivePower = (float)insolation;
                    hourDataPoint.ReactivePower = (float)insolation / 50;

                    dayAhead.Hourly.Add(hourDataPoint);
                }

             }
            return dayAhead;
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
