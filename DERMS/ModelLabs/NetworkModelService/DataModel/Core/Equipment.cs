using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using FTN.Common;

namespace FTN.Services.NetworkModelService.DataModel.Core
{
    [DataContract]
	public class Equipment : PowerSystemResource
	{
        [DataMember]
        private long container = 0;
		//private bool normallyInService;
						
		public Equipment(long globalId) : base(globalId) 
		{
		}		

        public long Container { get => container; set => container = value; }

        //public bool NormallyInService { get => normallyInService; set => normallyInService = value; }

        public override bool Equals(object obj)
		{
			if (base.Equals(obj))
			{
				Equipment x = (Equipment)obj;
				return ((x.container == this.container)
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
				case ModelCode.EQUIPMENT_CONTAINER:
		
					return true;
				default:
					return base.HasProperty(property);
			}
		}

		public override void GetProperty(Property property)
		{
			switch (property.Id)
			{

                case ModelCode.EQUIPMENT_CONTAINER:
                    property.SetValue(container);
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

                case ModelCode.EQUIPMENT_CONTAINER:
                    container = property.AsReference();
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
            if (container != 0 && (refType != TypeOfReference.Reference || refType != TypeOfReference.Both))
            {
                references[ModelCode.EQUIPMENT_CONTAINER] = new List<long>();
                references[ModelCode.EQUIPMENT_CONTAINER].Add(container);
            }

            base.GetReferences(references, refType);
        }

        #endregion IReference implementation
    }
}

