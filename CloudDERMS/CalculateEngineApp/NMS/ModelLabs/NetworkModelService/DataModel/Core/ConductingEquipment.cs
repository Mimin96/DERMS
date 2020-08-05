
using FTN.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace FTN.Services.NetworkModelService.DataModel.Core
{
    [DataContract]
	public class ConductingEquipment : Equipment
	{
        [DataMember]
        private List<long> terminals = new List<long>();
        public List<long> Terminals { get => terminals; set => terminals = value; }
        //private PhaseCode phases;
        [DataMember]
        private float longitude;
        [DataMember]
        private float latitude;
        public float Longitude { get => longitude; set => longitude = value; }
        public float Latitude { get => latitude; set => latitude = value; }
        //private long baseVoltage = 0;

        public ConductingEquipment(long globalId) : base(globalId) 
		{
		}

        //public PhaseCode Phases
        //{
        //	get
        //	{
        //		return phases;
        //	}

        //	set
        //	{
        //		phases = value;
        //	}
        //}

        //public float RatedVoltage
        //{
        //	get { return ratedVoltage; }
        //	set { ratedVoltage = value; }
        //}

        //public long BaseVoltage
        //{
        //	get { return baseVoltage; }
        //	set { baseVoltage = value; }
        //}

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                ConductingEquipment x = (ConductingEquipment)obj;
                return (CompareHelper.CompareLists(x.Terminals, this.Terminals, true) && x.longitude == this.longitude && x.latitude == this.latitude);
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
                case ModelCode.CONDEQ_LATITUDE:
                case ModelCode.CONDEQ_LONGITUDE:
                case ModelCode.CONDEQ_TERMINALS:
                    return true;

                default:
					return base.HasProperty(property);
			}
		}

		public override void GetProperty(Property prop)
		{
			switch (prop.Id)
			{
                case ModelCode.CONDEQ_LONGITUDE:
                    prop.SetValue(longitude);
                    break;

                case ModelCode.CONDEQ_LATITUDE:
                    prop.SetValue(latitude);
                    break;

                case ModelCode.CONDEQ_TERMINALS:
                    prop.SetValue(terminals);
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
                case ModelCode.CONDEQ_LATITUDE:
                    latitude = property.AsFloat();
                    break;

                case ModelCode.CONDEQ_LONGITUDE:
                    longitude = property.AsFloat();
                    break;

                case ModelCode.CONDEQ_TERMINALS:
                    terminals = property.AsReferences();
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
                return (terminals.Count > 0) || base.IsReferenced;
            }
        }

        

        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {
            if (terminals != null && terminals.Count > 0 && (refType == TypeOfReference.Target || refType == TypeOfReference.Both))
            {
                references[ModelCode.CONDEQ_TERMINALS] = terminals.GetRange(0, Terminals.Count);
            }

            base.GetReferences(references, refType);
        }

        public override void AddReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.TERMINAL_COND_EQ:
                    terminals.Add(globalId);
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
                case ModelCode.TERMINAL_COND_EQ:

                    if (terminals.Contains(globalId))
                    {
                        terminals.Remove(globalId);
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
