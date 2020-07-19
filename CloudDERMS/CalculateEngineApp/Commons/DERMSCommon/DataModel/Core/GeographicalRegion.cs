using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DERMSCommon.DataModel.Core
{
    [DataContract]
    public class GeographicalRegion : IdentifiedObject
    {
        [DataMember]
        private List<long> regions = new List<long>();

        public List<long> Regions { get => regions; set => regions = value; }
        [DataMember]
        private float longitude;
        [DataMember]
        private float latitude;
        public float Longitude { get => longitude; set => longitude = value; }
        public float Latitude { get => latitude; set => latitude = value; }
        public GeographicalRegion(long globalId) : base(globalId)
        {

        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                GeographicalRegion x = (GeographicalRegion)obj;
                return ((x.longitude == this.longitude && x.latitude == this.latitude) && CompareHelper.CompareLists(x.Regions, this.Regions, true));
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
                case ModelCode.GEOGRAPHICALREGION_LONGITUDE:
                case ModelCode.GEOGRAPHICALREGION_LATITUDE:
                //case ModelCode.SWITCH_RETAINED:
                //case ModelCode.SWITCH_SWITCH_ON_COUNT:
                case ModelCode.GEOGRAPHICALREGION_SUBGEOREGS:
                    return true;

                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {
                case ModelCode.GEOGRAPHICALREGION_LONGITUDE:
                    prop.SetValue(longitude);
                    break;

                case ModelCode.GEOGRAPHICALREGION_LATITUDE:
                    prop.SetValue(latitude);
                    break;


                //case ModelCode.SWITCH_RETAINED:
                //    prop.SetValue(retained);
                //    break;
                //case ModelCode.SWITCH_SWITCH_ON_COUNT:
                //    prop.SetValue(switchOnCount);
                //    break;
                case ModelCode.GEOGRAPHICALREGION_SUBGEOREGS:
                    prop.SetValue(regions);
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
                case ModelCode.GEOGRAPHICALREGION_LATITUDE:
                    latitude = property.AsFloat();
                    break;

                case ModelCode.GEOGRAPHICALREGION_LONGITUDE:
                    longitude = property.AsFloat();
                    break;

                //case ModelCode.SWITCH_RETAINED:
                //    retained = property.AsBool();
                //    break;
                //case ModelCode.SWITCH_SWITCH_ON_COUNT:
                //    switchOnCount = property.AsInt();
                //    break;
                case ModelCode.GEOGRAPHICALREGION_SUBGEOREGS:
                    regions = property.AsReferences();
                    break;

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
                return (regions.Count > 0) || base.IsReferenced;
            }
        }

        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {
            if (regions != null && regions.Count > 0 && (refType == TypeOfReference.Target || refType == TypeOfReference.Both))
            {
                references[ModelCode.GEOGRAPHICALREGION_SUBGEOREGS] = regions.GetRange(0, regions.Count);
            }

            base.GetReferences(references, refType);
        }

        public override void AddReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.SUBGEOGRAPHICALREGION_GEOREG:
                    regions.Add(globalId);
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
                case ModelCode.SUBGEOGRAPHICALREGION_GEOREG:

                    if (regions.Contains(globalId))
                    {
                        regions.Remove(globalId);
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
