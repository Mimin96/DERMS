using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessingModule
{
    public static class AlarmProcessor
    {

        public static AlarmType GetAlarmForAnalogPoint(double eguValue, IConfigItem configItem)
        {
            AlarmType alarm = AlarmType.NO_ALARM;
            if (CheckReasonability(eguValue, configItem))
            {
                alarm = AlarmType.REASONABILITY_FAILURE;
            }
            else if (eguValue > configItem.HighAlarm)
            {
                alarm = AlarmType.HIGH_ALARM;
            }
            else if (eguValue < configItem.LowAlarm)
            {
                alarm = AlarmType.LOW_ALARM;
            }

            return alarm;
        }

        private static bool CheckReasonability(double eguValue, IConfigItem configItem)
        {
            //ovde izmenio
            bool rez = false;

            if (eguValue > configItem.EGU_Max || eguValue < configItem.EGU_Min)
            {
                rez = true;
            }

            return rez;
        }

        public static AlarmType GetAlarmForDigitalPoint(ushort state, IConfigItem configItem)
        {
            //ovde izmenio
            AlarmType a;
            if (state == configItem.AbnormalValue)
            {
                a = AlarmType.ABNORMAL_VALUE;
            }
            else
            {
                a = AlarmType.NO_ALARM;
            }

            return a;
        }
    }
}
