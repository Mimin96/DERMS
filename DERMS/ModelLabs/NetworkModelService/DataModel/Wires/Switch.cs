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
    public class Switch : ConductingEquipment 
    {
        [DataMember]
        private bool normalOpen;
        [DataMember]
        private string feederId1;
        [DataMember]
        private string feederId2;

        public Switch(long globalId) : base(globalId)
        {
        }

        public bool NormalOpen { get => normalOpen; set => normalOpen = value; }
        public string FeederId1 { get => feederId1; set => feederId1 = value; }
        public string FeederId2 { get => feederId2; set => feederId2 = value; }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                Switch x = (Switch)obj;
                return (x.normalOpen == this.normalOpen && x.feederId1 == this.feederId1 && x.feederId2 == this.feederId2);
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
                case ModelCode.SWITCH_NORMAL_OPEN:
                case ModelCode.SWITCH_FEEDER_ID1:
                case ModelCode.SWITCH_FEEDER_ID2:
                    return true;

                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {
                case ModelCode.SWITCH_NORMAL_OPEN:
                    prop.SetValue(normalOpen);
                    break;

                case ModelCode.SWITCH_FEEDER_ID1:
                    prop.SetValue(feederId1);
                    break;

                case ModelCode.SWITCH_FEEDER_ID2:
                    prop.SetValue(feederId2);
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
                case ModelCode.SWITCH_NORMAL_OPEN:
                    normalOpen = property.AsBool();
                    break;

                case ModelCode.SWITCH_FEEDER_ID1:
                    feederId1 = property.AsString();
                    break;

                case ModelCode.SWITCH_FEEDER_ID2:
                    feederId2 = property.AsString();
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
