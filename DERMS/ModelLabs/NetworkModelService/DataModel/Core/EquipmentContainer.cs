using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FTN.Services.NetworkModelService.DataModel.Core
{
    [DataContract]
    public class EquipmentContainer : ConnectivityNodeContainer
    {
        [DataMember]
        private List<long> equipments = new List<long>();
        [DataMember]
        private float longitude;
        [DataMember]
        private float latitude;

        public float Longitude { get => longitude; set => longitude = value; }
        public float Latitude { get => latitude; set => latitude = value; }
        public EquipmentContainer(long globalId) : base(globalId)
        {

        }

        public List<long> Equipments { get => equipments; set => equipments = value; }
        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                EquipmentContainer x = (EquipmentContainer)obj;
                return ((x.longitude == this.longitude && x.latitude == this.latitude) && CompareHelper.CompareLists(x.Equipments, this.Equipments, true));
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

        public override bool HasProperty(ModelCode t)
        {
            switch (t)
            {

                case ModelCode.EQUIPMENTCONTAINER_LONGITUDE:
                case ModelCode.EQUIPMENTCONTAINER_LATITUDE:
                case ModelCode.EQUIPMENTCONTAINER_EQUIPMENTS:
                    return true;

                default:
                    return base.HasProperty(t);
            }
        }

        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {
                case ModelCode.EQUIPMENTCONTAINER_LONGITUDE:
                    prop.SetValue(longitude);
                    break;

                case ModelCode.EQUIPMENTCONTAINER_LATITUDE:
                    prop.SetValue(latitude);
                    break;
                case ModelCode.EQUIPMENTCONTAINER_EQUIPMENTS:
                    prop.SetValue(equipments);
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

                case ModelCode.EQUIPMENTCONTAINER_LATITUDE:
                    latitude = property.AsFloat();
                    break;

                case ModelCode.EQUIPMENTCONTAINER_LONGITUDE:
                    longitude = property.AsFloat();
                    break;
                case ModelCode.EQUIPMENTCONTAINER_EQUIPMENTS:
                    equipments = property.AsReferences();
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
                return (equipments.Count > 0) || base.IsReferenced;
            }
        }

        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {
            if (equipments != null && equipments.Count > 0 && (refType == TypeOfReference.Target || refType == TypeOfReference.Both))
            {
                references[ModelCode.EQUIPMENTCONTAINER_EQUIPMENTS] = equipments.GetRange(0, equipments.Count);
            }

            base.GetReferences(references, refType);
        }

        public override void AddReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.EQUIPMENT_CONTAINER:
                    equipments.Add(globalId);
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
                case ModelCode.EQUIPMENT_CONTAINER:

                    if (equipments.Contains(globalId))
                    {
                        equipments.Remove(globalId);
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
