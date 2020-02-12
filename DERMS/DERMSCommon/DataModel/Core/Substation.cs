using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DERMSCommon.DataModel.Core
{
    [DataContract]
    public class Substation : EquipmentContainer
    {
        [DataMember]
        private long subgeoreg = 0;

        public long SubGeoReg { get => subgeoreg; set => subgeoreg = value; }

        public Substation(long globalId) : base(globalId)
        {
        }

        

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                Substation x = (Substation)obj;
                return ((x.subgeoreg == this.subgeoreg)
                        /*(x.normallyInService == this.normallyInService)*/);
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
                case ModelCode.SUBSTATION_SUBGEOREG:
                    return true;

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
                case ModelCode.SUBSTATION_SUBGEOREG:
                    prop.SetValue(subgeoreg);
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

                //case ModelCode.SWITCH_RETAINED:
                //    retained = property.AsBool();
                //    break;
                //case ModelCode.SWITCH_SWITCH_ON_COUNT:
                //    switchOnCount = property.AsInt();
                //    break;
                case ModelCode.SUBSTATION_SUBGEOREG:
                    subgeoreg = property.AsReference();
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
            if (subgeoreg != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.SUBSTATION_SUBGEOREG] = new List<long>();
                references[ModelCode.SUBSTATION_SUBGEOREG].Add(subgeoreg);
            }

            base.GetReferences(references, refType);
        }


        #endregion IReference implementation
    }
    
}
