using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon.DataModel.Core
{
    [DataContract]
    public class Point : IdentifiedObject
    {
        [DataMember]
        private long line = 0;
        [DataMember]
        private List<long> substations = new List<long>();
        public long Line { get => line; set => line = value; }
        [DataMember]
        private float longitude;
        [DataMember]
        private float latitude;
        public float Longitude { get => longitude; set => longitude = value; }
        public float Latitude { get => latitude; set => latitude = value; }
        public Point(long globalId) : base(globalId)
        {
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                Point x = (Point)obj;
                return ((x.line == this.line && x.longitude == this.longitude && x.latitude == this.latitude)
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
                case ModelCode.POINT_LINE:
                case ModelCode.POINT_LATITUDE:
                case ModelCode.POINT_LONGITUDE:
                    return true;

                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {
                case ModelCode.POINT_LONGITUDE:
                    prop.SetValue(longitude);
                    break;

                case ModelCode.POINT_LATITUDE:
                    prop.SetValue(latitude);
                    break;

                case ModelCode.POINT_LINE:
                    prop.SetValue(line);
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

                case ModelCode.POINT_LATITUDE:
                    latitude = property.AsFloat();
                    break;

                case ModelCode.POINT_LONGITUDE:
                    longitude = property.AsFloat();
                    break;

                case ModelCode.POINT_LINE:
                    line = property.AsReference();
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
            if (line != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.POINT_LINE] = new List<long>();
                references[ModelCode.POINT_LINE].Add(line);
            }           

            base.GetReferences(references, refType);
        }        

        #endregion IReference implementation        
    }
}
