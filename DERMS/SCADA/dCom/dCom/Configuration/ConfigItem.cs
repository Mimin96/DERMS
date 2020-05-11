using Common;
using System;
using System.Collections.Generic;

namespace dCom.Configuration
{
    internal class ConfigItem : IConfigItem
    {
        #region Fields

        private PointType registryType;
        private ushort numberOfRegisters;
        private ushort startAddress;
        private ushort decimalSeparatorPlace;
        private int minValue;
        private int maxValue;
        private int defaultValue;
        private string processingType;
        private string description;
        private int acquisitionInterval;
        private double scalingFactor;
        private double deviation;
        private double egu_max;
        private double egu_min;
        private ushort abnormalValue;
        private double highAlarm;
        private double lowAlarm;
        private long gid;
        private long gidGeneratora;
        #endregion Fields

        #region Properties

        public PointType RegistryType
        {
            get
            {
                return registryType;
            }

            set
            {
                registryType = value;
            }
        }
        public long Gid
        {
            get
            {
                return gid;
            }

            set
            {
                gid = value;
            }
        }
        public long GidGeneratora
        {
            get
            {
                return gidGeneratora;
            }

            set
            {
                gidGeneratora = value;
            }
        }
        public ushort NumberOfRegisters
        {
            get
            {
                return numberOfRegisters;
            }

            set
            {
                numberOfRegisters = value;
            }
        }

        public ushort StartAddress
        {
            get
            {
                return startAddress;
            }

            set
            {
                startAddress = value;
            }
        }

        public ushort DecimalSeparatorPlace
        {
            get
            {
                return decimalSeparatorPlace;
            }

            set
            {
                decimalSeparatorPlace = value;
            }
        }

        public int MinValue
        {
            get
            {
                return minValue;
            }

            set
            {
                minValue = value;
            }
        }

        public int MaxValue
        {
            get
            {
                return maxValue;
            }

            set
            {
                maxValue = value;
            }
        }

        public int DefaultValue
        {
            get
            {
                return defaultValue;
            }

            set
            {
                defaultValue = value;
            }
        }

        public string ProcessingType
        {
            get
            {
                return processingType;
            }

            set
            {
                processingType = value;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                description = value;
            }
        }

        public int AcquisitionInterval
        {
            get
            {
                return acquisitionInterval;
            }

            set
            {
                acquisitionInterval = value;
            }
        }

        public double ScaleFactor
        {
            get
            {
                return scalingFactor;
            }
            set
            {
                scalingFactor = value;
            }
        }

        public double Deviation
        {
            get
            {
                return deviation;
            }

            set
            {
                deviation = value;
            }
        }

        public double EGU_Max
        {
            get
            {
                return egu_max;
            }

            set
            {
                egu_max = value;
            }
        }

        public double EGU_Min
        {
            get
            {
                return egu_min;
            }

            set
            {
                egu_min = value;
            }
        }

        public ushort AbnormalValue
        {
            get
            {
                return abnormalValue;
            }

            set
            {
                abnormalValue = value;
            }
        }

        public double HighAlarm
        {
            get
            {
                return highAlarm;
            }

            set
            {
                highAlarm = value;
            }
        }

        public double LowAlarm
        {
            get
            {
                return lowAlarm;
            }

            set
            {
                lowAlarm = value;
            }
        }



        #endregion Properties
        public ConfigItem()
        {
            DecimalSeparatorPlace = 0;
            AcquisitionInterval = 5;

        }
        public ConfigItem(List<string> configurationParameters)
        {
            RegistryType = GetRegistryType(configurationParameters[0]);
            int temp;
            double doubleTemp;
            Int32.TryParse(configurationParameters[1], out temp);
            NumberOfRegisters = (ushort)temp;
            Int32.TryParse(configurationParameters[2], out temp);
            StartAddress = (ushort)temp;
            Int32.TryParse(configurationParameters[3], out temp);
            DecimalSeparatorPlace = (ushort)temp;
            Int32.TryParse(configurationParameters[4], out temp);
            MinValue = (ushort)temp;
            Int32.TryParse(configurationParameters[5], out temp);
            MaxValue = (ushort)temp;
            Int32.TryParse(configurationParameters[6], out temp);
            DefaultValue = (ushort)temp;
            ProcessingType = configurationParameters[7];
            Description = configurationParameters[8].TrimStart('@');
            Int32.TryParse(configurationParameters[9], out temp);
            Double.TryParse(configurationParameters[19], out doubleTemp);
            //Gid = doubleTemp;
            AcquisitionInterval = temp;
            if (configurationParameters[10] != "#")
            {
                Double.TryParse(configurationParameters[10], out doubleTemp);
                ScaleFactor = doubleTemp;
            }
            if (configurationParameters[11] != "#")
            {
                Double.TryParse(configurationParameters[11], out doubleTemp);
                Deviation = doubleTemp;
            }
            if (configurationParameters[12] != "#")
            {
                Double.TryParse(configurationParameters[12], out doubleTemp);
                EGU_Min = doubleTemp;
            }
            if (configurationParameters[13] != "#")
            {
                Double.TryParse(configurationParameters[13], out doubleTemp);
                EGU_Max = doubleTemp;
            }
            if (configurationParameters[14] != "#")
            {
                Int32.TryParse(configurationParameters[14], out temp);
                AbnormalValue = (ushort)temp;
            }

            if (configurationParameters[15] != "#")
            {
                Int32.TryParse(configurationParameters[16], out temp);
                HighAlarm = (double)temp;
            }

            if (configurationParameters[16] != "#")
            {
                Int32.TryParse(configurationParameters[15], out temp);
                LowAlarm = (double)temp;
            }
        }

        private PointType GetRegistryType(string registryTypeName)
        {
            PointType registryType;
            switch (registryTypeName)
            {
                case "DO_REG":
                    registryType = PointType.DIGITAL_OUTPUT;
                    break;

                case "DI_REG":
                    registryType = PointType.DIGITAL_INPUT;
                    break;

                case "IN_REG":
                    registryType = PointType.ANALOG_INPUT;
                    break;

                case "HR_INT":
                    registryType = PointType.ANALOG_OUTPUT;
                    break;

                default:
                    registryType = PointType.HR_LONG;
                    break;
            }
            return registryType;
        }
    }
}