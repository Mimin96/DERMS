using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DERMSCommon.DataModel.Core
{
    [DataContract]
    public class ConnectivityNode : IdentifiedObject
    {
        [DataMember]
        private List<long> terminals = new List<long>();
        [DataMember]
        private long container = 0;
        public List<long> Terminals { get => terminals; set => terminals = value; }
        public long Container { get => container; set => container = value; }
        public ConnectivityNode(long globalId) : base(globalId)
        {
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                ConnectivityNode x = (ConnectivityNode)obj;
                return (x.Container == this.Container && CompareHelper.CompareLists(x.Terminals, this.Terminals, true));
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
                //case ModelCode.SWITCH_RATED_CURRENT:
                //case ModelCode.SWITCH_RETAINED:
                //case ModelCode.SWITCH_SWITCH_ON_COUNT:
                case ModelCode.CONNECTIVITYNODE_TERMINALS:
                    return true;
                case ModelCode.CONNECTIVITYNODE_CONTAINER:
                    return true;

                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {
                //case ModelCode.SWITCH_NORMAL_OPEN:
                //    prop.SetValue(normalOpen);
                //    break;

                //case ModelCode.SWITCH_RATED_CURRENT:
                //    prop.SetValue((short)ratedCurrent);
                //    break;

                //case ModelCode.SWITCH_RETAINED:
                //    prop.SetValue(retained);
                //    break;
                //case ModelCode.SWITCH_SWITCH_ON_COUNT:
                //    prop.SetValue(switchOnCount);
                //    break;
                case ModelCode.CONNECTIVITYNODE_TERMINALS:
                    prop.SetValue(terminals);
                    break;
                case ModelCode.CONNECTIVITYNODE_CONTAINER:
                    prop.SetValue(container);
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

                //case ModelCode.SWITCH_RATED_CURRENT:
                //    ratedCurrent = (CurrentFlow)property.AsEnum();
                //    break;

                //case ModelCode.SWITCH_RETAINED:
                //    retained = property.AsBool();
                //    break;
                case ModelCode.CONNECTIVITYNODE_TERMINALS:
                    terminals = property.AsReferences();
                    break;
                case ModelCode.CONNECTIVITYNODE_CONTAINER:
                    container = property.AsReference();
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
                return (Terminals.Count > 0) || base.IsReferenced;
            }
        }

        

        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {
            if (Terminals != null && Terminals.Count > 0 && (refType == TypeOfReference.Target || refType == TypeOfReference.Both))
            {
                references[ModelCode.CONNECTIVITYNODE_TERMINALS] = Terminals.GetRange(0, Terminals.Count);
            }

            if (container != 0 && (refType != TypeOfReference.Reference || refType != TypeOfReference.Both))
            {
                references[ModelCode.CONNECTIVITYNODE_CONTAINER] = new List<long>();
                references[ModelCode.CONNECTIVITYNODE_CONTAINER].Add(container);
            }

            base.GetReferences(references, refType);
        }

        public override void AddReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.TERMINAL_CONNECTIVITY_NODE:
                    Terminals.Add(globalId);
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
                case ModelCode.TERMINAL_CONNECTIVITY_NODE:

                    if (Terminals.Contains(globalId))
                    {
                        Terminals.Remove(globalId);
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
