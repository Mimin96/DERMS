using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FTN.Services.NetworkModelService.DataModel.Core
{
    [DataContract]
    public class Terminal : IdentifiedObject
    {
        [DataMember]
        private long connectivityNode = 0;
        [DataMember]
        private long condEq = 0;
                
        public long ConnectivityNode { get => connectivityNode; set => connectivityNode = value; }
        public long CondEq { get => condEq; set => condEq = value; }

        public Terminal(long globalId) : base(globalId)
        {

        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                Terminal x = (Terminal)obj;
                return (x.condEq == this.condEq && x.connectivityNode == this.connectivityNode);              
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
                
                case ModelCode.TERMINAL_COND_EQ:
                case ModelCode.TERMINAL_CONNECTIVITY_NODE:
                    return true;
                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {                
                case ModelCode.TERMINAL_COND_EQ:
                    prop.SetValue(condEq);
                    break;
                case ModelCode.TERMINAL_CONNECTIVITY_NODE:
                    prop.SetValue(connectivityNode);
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
                case ModelCode.TERMINAL_COND_EQ:
                    condEq = property.AsReference();
                    break;
                case ModelCode.TERMINAL_CONNECTIVITY_NODE:
                    connectivityNode = property.AsReference();
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
            if (connectivityNode != 0 && (refType != TypeOfReference.Reference || refType != TypeOfReference.Both))
            {
                references[ModelCode.TERMINAL_CONNECTIVITY_NODE] = new List<long>();
                references[ModelCode.TERMINAL_CONNECTIVITY_NODE].Add(connectivityNode);
            }

            if (condEq != 0 && (refType != TypeOfReference.Reference || refType != TypeOfReference.Both))
            {
                references[ModelCode.TERMINAL_COND_EQ] = new List<long>();
                references[ModelCode.TERMINAL_COND_EQ].Add(condEq);
            }

            base.GetReferences(references, refType);
        }

        #endregion IReference implementation
    }
}
