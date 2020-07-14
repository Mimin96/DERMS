using DERMSCommon.DataModel.Core;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DERMSCommon.DataModel.Wires
{
    [DataContract]
    public class ACLineSegment : Conductor
    {
        [DataMember]
        private float currentFlow;
        [DataMember]
        private bool feederCable;
        [DataMember]
        private List<long> points = new List<long>();
        public float CurrentFlow { get => currentFlow; set => currentFlow = value; }
        public bool FeederCable { get => feederCable; set => feederCable = value; }
        public List<long> Points { get => points; set => points = value; }

        public ACLineSegment(long globalId) : base(globalId)
        {
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                ACLineSegment x = (ACLineSegment)obj;
                return (x.currentFlow == this.currentFlow && x.feederCable == this.feederCable && CompareHelper.CompareLists(x.Points, this.Points, true));
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
                case ModelCode.ACLINESEGMENT_POINTS:
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

                case ModelCode.ACLINESEGMENT_POINTS:
                    prop.SetValue(points);
                    break;
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

                case ModelCode.ACLINESEGMENT_POINTS:
                    points = property.AsReferences();
                    break;
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

        public override bool IsReferenced
        {
            get
            {
                return (points.Count > 0) || base.IsReferenced;
            }
        }

        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {       
            if (points != null && points.Count > 0 && (refType == TypeOfReference.Target || refType == TypeOfReference.Both))
            {
                references[ModelCode.ACLINESEGMENT_POINTS] = points.GetRange(0, points.Count);
            }

            base.GetReferences(references, refType);
        }

        public override void AddReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.POINT_LINE:
                    points.Add(globalId);
                    break;

                default:
                    base.AddReference(referenceId, globalId);
                    break;
            }
        }

        public override void RemoveReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.POINT_LINE:

                    if (points.Contains(globalId))
                    {
                        points.Remove(globalId);
                    }
                    else
                    {
                        CommonTrace.WriteTrace(CommonTrace.TraceWarning, "Entity (GID = 0x{0:x16}) doesn't contain reference 0x{1:x16}.", this.GlobalId, globalId);
                    }

                    break;
                default:
                    base.RemoveReference(referenceId, globalId);
                    break;
            }
        }

        #endregion IReference implementation
    }
}
