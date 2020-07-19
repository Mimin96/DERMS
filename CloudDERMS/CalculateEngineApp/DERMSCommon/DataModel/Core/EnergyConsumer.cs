using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DERMSCommon.DataModel.Core
{
    [DataContract]
    public class EnergyConsumer : ConductingEquipment
    {
        [DataMember]
        private float qFixed;
        [DataMember]
        private float pFixed;
        public EnergyConsumer(long globalId) : base(globalId)
        {
        }

        
        public float QFixed { get => qFixed; set => qFixed = value; }
        public float PFixed { get => pFixed; set => pFixed = value; }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                EnergyConsumer x = (EnergyConsumer)obj;
                return (x.qFixed == this.qFixed && x.pFixed == this.pFixed);
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
                
                case ModelCode.ENERGYCONSUMER_PFIXED:
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

                
                case ModelCode.ENERGYCONSUMER_PFIXED:
                    prop.SetValue(pFixed);
                    break;
                case ModelCode.ENERGYCONSUMER_QFIXED:
                    prop.SetValue(qFixed);
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

                
                case ModelCode.ENERGYCONSUMER_PFIXED:
                    pFixed = property.AsFloat();
                    break;
                case ModelCode.ENERGYCONSUMER_QFIXED:
                    qFixed = property.AsFloat();
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
            //if (baseVoltage != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            //{
            //	references[ModelCode.CONDEQ_BASVOLTAGE] = new List<long>();
            //	references[ModelCode.CONDEQ_BASVOLTAGE].Add(baseVoltage);
            //}

            base.GetReferences(references, refType);
        }

        #endregion IReference implementation
    }
}
