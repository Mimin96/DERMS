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
    public class Discrete : Measurement
    {
        [DataMember]
        private int maxValue;
        [DataMember]
        private int minValue;
        [DataMember]
        private int normalValue;
                
        public int MaxValue { get => maxValue; set => maxValue = value; }
        public int MinValue { get => minValue; set => minValue = value; }
        public int NormalValue { get => normalValue; set => normalValue = value; }

        public Discrete(long globalId) : base(globalId)
        {
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                Discrete x = (Discrete)obj;
                return (x.maxValue == this.maxValue && x.minValue == this.minValue && x.normalValue == this.normalValue);
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
                case ModelCode.DISCRETE_MAX_VALUE:
                case ModelCode.DISCRETE_MIN_VALUE:
                case ModelCode.DISCRETE_NORMAL_VALUE:
                    return true;

                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {
                case ModelCode.DISCRETE_MAX_VALUE:
                    prop.SetValue(maxValue);
                    break;

                case ModelCode.DISCRETE_MIN_VALUE:
                    prop.SetValue(minValue);
                    break;
                case ModelCode.DISCRETE_NORMAL_VALUE:
                    prop.SetValue(normalValue);
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
                case ModelCode.DISCRETE_MAX_VALUE:
                    maxValue = property.AsInt();
                    break;

                case ModelCode.DISCRETE_MIN_VALUE:
                    minValue = property.AsInt();
                    break;
                case ModelCode.DISCRETE_NORMAL_VALUE:
                    normalValue = property.AsInt();
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
