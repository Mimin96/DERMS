using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FTN.Services.NetworkModelService.DataModel.Wires
{
    [DataContract]
    public class ACLineSegment : Conductor
    {
        [DataMember]
        private float currentFlow;
        [DataMember]
        private bool feederCable;

        public float CurrentFlow { get => currentFlow; set => currentFlow = value; }
        public bool FeederCable { get => feederCable; set => feederCable = value; }

        public ACLineSegment(long globalId) : base(globalId)
        {
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                ACLineSegment x = (ACLineSegment)obj;
                return (x.currentFlow == this.currentFlow && x.feederCable == this.feederCable);
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
                case ModelCode.ACLINESEGMENT_CURRENTFLOW:
                case ModelCode.ACLINESEGMENT_FEEDERCABLE:
                //case ModelCode.SWITCH_RETAINED:
                //case ModelCode.SWITCH_SWITCH_ON_COUNT:
                //case ModelCode.SWITCH_SWITCH_ON_DATE:
                //    return true;

                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {
                case ModelCode.ACLINESEGMENT_CURRENTFLOW:
                    prop.SetValue(currentFlow);
                    break;

                case ModelCode.ACLINESEGMENT_FEEDERCABLE:
                    prop.SetValue(feederCable);
                    break;

                //case ModelCode.SWITCH_RETAINED:
                //    prop.SetValue(retained);
                //    break;
                //case ModelCode.SWITCH_SWITCH_ON_COUNT:
                //    prop.SetValue(switchOnCount);
                //    break;
                //case ModelCode.SWITCH_SWITCH_ON_DATE:
                //    prop.SetValue(switchOnDate);
                //    break;

                default:
                    base.GetProperty(prop);
                    break;
            }
        }

        public override void SetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.ACLINESEGMENT_FEEDERCABLE:
                    feederCable = property.AsBool();
                    break;

                case ModelCode.ACLINESEGMENT_CURRENTFLOW:
                    currentFlow = property.AsFloat();
                    break;

                //case ModelCode.SWITCH_RETAINED:
                //    retained = property.AsBool();
                //    break;
                //case ModelCode.SWITCH_SWITCH_ON_COUNT:
                //    switchOnCount = property.AsInt();
                //    break;
                //case ModelCode.SWITCH_SWITCH_ON_DATE:
                //    switchOnDate = property.AsDateTime();
                //    break;

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
