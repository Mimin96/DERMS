using DERMSCommon.DataModel.Core;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DERMSCommon.DataModel.Meas
{
    [DataContract]
    public class Measurement : IdentifiedObject
    {
        [DataMember]
        private MeasurementType measurementType;
        [DataMember]
        private long powerSystemResource = 0;
        [DataMember]
        private float longitude;
        [DataMember]
        private float latitude;
        public float Longitude { get => longitude; set => longitude = value; }
        public float Latitude { get => latitude; set => latitude = value; }

        public MeasurementType MeasurementType { get => measurementType; set => measurementType = value; }
        public long PowerSystemResource { get => powerSystemResource; set => powerSystemResource = value; }

        public Measurement(long globalId) : base(globalId)
        {
        }

        
        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                Measurement x = (Measurement)obj;
                return (x.measurementType == this.measurementType && x.powerSystemResource == this.powerSystemResource && x.latitude == this.latitude && x.longitude == this.longitude);
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
                case ModelCode.MEASUREMENT_LATITUDE:
                case ModelCode.MEASUREMENT_LONGITUDE:
                case ModelCode.MEASUREMENT_MEAS_TYPE:
                case ModelCode.ENERGYCONSUMER_QFIXED:
                    return true;

                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {
                case ModelCode.MEASUREMENT_LATITUDE:
                    prop.SetValue(latitude);
                    break;

                case ModelCode.MEASUREMENT_LONGITUDE:
                    prop.SetValue(longitude);
                    break;

                case ModelCode.MEASUREMENT_MEAS_TYPE:
                    prop.SetValue((short)measurementType);
                    break;
                case ModelCode.MEASUREMENT_PSR:
                    prop.SetValue(powerSystemResource);
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
                case ModelCode.MEASUREMENT_LATITUDE:
                    latitude = property.AsFloat();
                    break;

                case ModelCode.MEASUREMENT_LONGITUDE:
                    longitude = property.AsFloat();
                    break;

                case ModelCode.MEASUREMENT_MEAS_TYPE:
                    measurementType = (MeasurementType)property.AsEnum();
                    break;
                case ModelCode.MEASUREMENT_PSR:
                    powerSystemResource = property.AsReference();
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
            if (powerSystemResource != 0 && (refType != TypeOfReference.Reference || refType != TypeOfReference.Both))
            {
                references[ModelCode.MEASUREMENT_PSR] = new List<long>();
                references[ModelCode.MEASUREMENT_PSR].Add(powerSystemResource);
            }

            base.GetReferences(references, refType);
        }

        #endregion IReference implementation
    }
}
