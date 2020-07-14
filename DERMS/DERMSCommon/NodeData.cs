using DERMSCommon.DataModel.Core;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static DERMSCommon.Enums;

namespace DERMSCommon
{
    [DataContract]
    public class NodeData
    {
        [DataMember]
        private IdentifiedObject idenetifiedObj;
        [DataMember]
        private bool isRoot;
        [DataMember]
        private Energized energized;
        [DataMember]
        private DMSType type;
        [DataMember]
        private float val;

        public NodeData(IdentifiedObject identified, DMSType type, bool isroot)
        {
            this.idenetifiedObj = identified;
            this.isRoot = isroot;
            this.energized = Energized.NotEnergized;
            this.type = type;
        }

        public NodeData(IdentifiedObject identified, bool isroot)
        {
            this.idenetifiedObj = identified;
            this.isRoot = isroot;
            this.energized = Energized.NotEnergized;
            this.type = DMSType.GEOGRAPHICALREGION;
        }

        public IdentifiedObject IdentifiedObject { get { return idenetifiedObj; } set { idenetifiedObj = value; } }
        public Energized Energized { get { return energized; } set { energized = value; } }
        public DMSType Type { get { return type; } set { type = value; } }
        public float Value { get { return val; } set { val = value; } }
    }
}
