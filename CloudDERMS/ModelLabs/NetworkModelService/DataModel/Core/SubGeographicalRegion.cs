
using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FTN.Services.NetworkModelService.DataModel.Core
{
    [DataContract]
    public class SubGeographicalRegion : IdentifiedObject
    {
        [DataMember]
        private long georeg = 0;
        [DataMember]
        private List<long> substations = new List<long>();
        public long GeoReg { get => georeg; set => georeg = value; }
        public List<long> Substations { get => substations; set => substations = value; }
        [DataMember]
        private float longitude;
        [DataMember]
        private float latitude;
        public float Longitude { get => longitude; set => longitude = value; }
        public float Latitude { get => latitude; set => latitude = value; }
        public SubGeographicalRegion(long globalId) : base(globalId)
        {
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                SubGeographicalRegion x = (SubGeographicalRegion)obj;
                return ((x.georeg == this.georeg && x.longitude == this.longitude && x.latitude == this.latitude) && CompareHelper.CompareLists(x.Substations, this.Substations, true)
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
                case ModelCode.SUBGEOGRAPHICALREGION_LONGITUDE:
                case ModelCode.SUBGEOGRAPHICALREGION_LATITUDE:
                case ModelCode.SUBGEOGRAPHICALREGION_SUBSTATIONS:
                case ModelCode.SUBGEOGRAPHICALREGION_GEOREG:
                    return true;

                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {
                case ModelCode.SUBGEOGRAPHICALREGION_LONGITUDE:
                    prop.SetValue(longitude);
                    break;

                case ModelCode.SUBGEOGRAPHICALREGION_LATITUDE:
                    prop.SetValue(latitude);
                    break;

                //case ModelCode.SWITCH_RETAINED:
                //    prop.SetValue(retained);
                //    break;
                case ModelCode.SUBGEOGRAPHICALREGION_SUBSTATIONS:
                    prop.SetValue(Substations);
                    break;
                case ModelCode.SUBGEOGRAPHICALREGION_GEOREG:
                    prop.SetValue(georeg);
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

                case ModelCode.SUBGEOGRAPHICALREGION_LATITUDE:
                    latitude = property.AsFloat();
                    break;

                case ModelCode.SUBGEOGRAPHICALREGION_LONGITUDE:
                    longitude = property.AsFloat();
                    break;
                case ModelCode.SUBGEOGRAPHICALREGION_SUBSTATIONS:
                    Substations = property.AsReferences();
                    break;
                case ModelCode.SUBGEOGRAPHICALREGION_GEOREG:
                    georeg = property.AsReference();
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
                return (substations.Count > 0) || base.IsReferenced;
            }
        }

        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {
            if (georeg != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.SUBGEOGRAPHICALREGION_GEOREG] = new List<long>();
                references[ModelCode.SUBGEOGRAPHICALREGION_GEOREG].Add(georeg);
            }

            if (substations != null && substations.Count > 0 && (refType == TypeOfReference.Target || refType == TypeOfReference.Both))
            {
                references[ModelCode.SUBGEOGRAPHICALREGION_SUBSTATIONS] = substations.GetRange(0, substations.Count);
            }

            base.GetReferences(references, refType);
        }

        public override void AddReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.SUBSTATION_SUBGEOREG:
                    substations.Add(globalId);
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
                case ModelCode.SUBSTATION_SUBGEOREG:

                    if (substations.Contains(globalId))
                    {
                        substations.Remove(globalId);
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
