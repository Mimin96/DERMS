using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using FTN.Common;



namespace DERMSCommon.DataModel.Core
{
    [DataContract]
	public class PowerSystemResource : IdentifiedObject
	{
        [DataMember]
        private List<long> measurements = new List<long>();        
        public List<long> Measurements { get => measurements; set => measurements = value; }

        public PowerSystemResource(long globalId)
			: base(globalId)
		{
		}

        //public string CustomType
        //{
        //	get 
        //	{ 
        //		return customType; 
        //	}

        //	set 
        //	{
        //		customType = value; 
        //	}
        //}

        //public long Location
        //{
        //	get
        //	{
        //		return location;
        //	}

        //	set
        //	{
        //		location = value;
        //	}
        //}

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                PowerSystemResource x = (PowerSystemResource)obj;
                return CompareHelper.CompareLists(x.Measurements, this.Measurements, true);
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
                //case ModelCode.PSR_CUSTOMTYPE:
                case ModelCode.PSR_MEASUREMENTS:
                    return true;

                default:
					return base.HasProperty(property);
			}
		}

		public override void GetProperty(Property property)
		{
			switch (property.Id)
			{
                //case ModelCode.PSR_CUSTOMTYPE:					
                //	property.SetValue(customType);
                //	break;

                case ModelCode.PSR_MEASUREMENTS:
                    property.SetValue(measurements);
                    break;

                default:
					base.GetProperty(property);
					break;
			}
		}

		public override void SetProperty(Property property)
		{
			switch (property.Id)
			{
                //case ModelCode.PSR_CUSTOMTYPE:
                //	customType = property.AsString();
                //	break;

                case ModelCode.PSR_MEASUREMENTS:
                    measurements = property.AsReferences();
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
                return (measurements.Count > 0) || base.IsReferenced;
            }
        }
        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
		{
            if (measurements != null && measurements.Count > 0 && (refType == TypeOfReference.Target || refType == TypeOfReference.Both))
            {
                references[ModelCode.PSR_MEASUREMENTS] = measurements.GetRange(0, measurements.Count);
            }

            base.GetReferences(references, refType);			
		}

        public override void AddReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.MEASUREMENT_PSR:
                    measurements.Add(globalId);
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
                case ModelCode.MEASUREMENT_PSR:

                    if (measurements.Contains(globalId))
                    {                        
                        measurements.Remove(globalId);
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
