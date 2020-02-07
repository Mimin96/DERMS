using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FTN.Services.NetworkModelService.DataModel.Meas
{
    [DataContract]
    public class Measurement : IdentifiedObject
    {
        [DataMember]
        private MeasurementType measurementType;
        [DataMember]
        private long powerSystemResource = 0;

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
                return (x.measurementType == this.measurementType && x.powerSystemResource == this.powerSystemResource);
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
                //case ModelCode.SWITCH_NORMAL_OPEN:
                //case ModelCode.SWITCH_RATED_CURRENT:
                
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
                //case ModelCode.ENERGYCONSUMER_CONSUMER_COUNT:
                //    prop.SetValue(normalOpen);
                //    break;

                //case ModelCode.SWITCH_RATED_CURRENT:
                //    prop.SetValue((short)ratedCurrent);
                //    break;
               
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
                //case ModelCode.SWITCH_NORMAL_OPEN:
                //    normalOpen = property.AsBool();
                //    break;

                //case ModelCode.SWITCH_RATED_CURRENT:
                //    ratedCurrent = (CurrentFlow)property.AsEnum();
                //    break;
                
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
