using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FTN.Services.NetworkModelService.DataModel.Core
{
    [DataContract]
    public class FeederObject : EquipmentContainer
    {
        public FeederObject(long globalId) : base(globalId)
        {
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
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
                //case ModelCode.SWITCH_NORMAL_OPEN:
                //    prop.SetValue(normalOpen);
                //    break;

                //case ModelCode.SWITCH_RATED_CURRENT:
                //    prop.SetValue((short)ratedCurrent);
                //    break;

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
                //case ModelCode.SWITCH_NORMAL_OPEN:
                //    normalOpen = property.AsBool();
                //    break;

                //case ModelCode.SWITCH_RATED_CURRENT:
                //    ratedCurrent = (CurrentFlow)property.AsEnum();
                //    break;

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
