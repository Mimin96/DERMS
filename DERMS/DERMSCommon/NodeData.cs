using DERMSCommon.DataModel.Core;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DERMSCommon.Enums;

namespace DERMSCommon
{
    public class NodeData
    {

        private IdentifiedObject idenetifiedObj;
        private bool isRoot;
        private Energized energized;
        private DMSType type;
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
        }

        public IdentifiedObject IdentifiedObject { get { return idenetifiedObj; } set { idenetifiedObj = value; } }
        public Energized Energized { get { return energized; } set { energized = value; } }
        public DMSType Type { get { return type; } set { type = value; } }
        public float Value { get { return val; } set { val = value; } }
    }
}
