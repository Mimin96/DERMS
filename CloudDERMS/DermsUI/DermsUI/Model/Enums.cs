using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DermsUI.Model
{
    public class Enums
    {
        public enum Components 
        { 
            NMS, 
            SCADA, 
            CE, 
            TM, 
            UI 
        };

        public enum Content 
        { 
            WARRNING, 
            ERROR, 
            INFO 
        };

        public enum SupportedProfiles : byte
        {
            PowerTransformer = 0,
            VoltageRegulator,
            SwitchingEquipment,
            OverheadLines,
            UndergroundCables,
            ProtectionDevices
        };
    }
}
