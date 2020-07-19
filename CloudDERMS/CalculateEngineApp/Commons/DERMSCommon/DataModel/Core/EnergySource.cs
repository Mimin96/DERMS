using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DERMSCommon.DataModel.Core
{
    [DataContract]
    public class EnergySource : ConductingEquipment
    {
        [DataMember]
        private float activePower;
        [DataMember]
        private float nominalVoltage;
        [DataMember]
        private float magnitudeVoltage;
        [DataMember]
        private EnergySourceType type;

        public float ActivePower { get => activePower; set => activePower = value; }
        public float NominalVoltage { get => nominalVoltage; set => nominalVoltage = value; }
        public float MagnitudeVoltage { get => magnitudeVoltage; set => magnitudeVoltage = value; }
        public EnergySourceType Type { get => type; set => type = value; }

        public EnergySource(long globalId) : base(globalId)
        {
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                EnergySource x = (EnergySource)obj;
                return (x.activePower == this.activePower && x.nominalVoltage == this.nominalVoltage && x.magnitudeVoltage == this.magnitudeVoltage && x.type == this.type);
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
                case ModelCode.ENERGYSOURCE_ACTIVEPOWER:
                case ModelCode.ENERGYSOURCE_VOLTAGE:
                case ModelCode.ENERGYSOURCE_MAGNITUDE:
                case ModelCode.ENERGYSOURCE_TYPE:
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

                case ModelCode.ENERGYSOURCE_ACTIVEPOWER:
                    prop.SetValue(activePower);
                    break;

                case ModelCode.ENERGYSOURCE_MAGNITUDE:
                    prop.SetValue(magnitudeVoltage);
                    break;
                case ModelCode.ENERGYSOURCE_VOLTAGE:
                    prop.SetValue(nominalVoltage);
                    break;
                case ModelCode.ENERGYSOURCE_TYPE:
                    prop.SetValue((short)type);
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

                case ModelCode.ENERGYSOURCE_ACTIVEPOWER:
                    activePower = property.AsFloat();
                    break;

                case ModelCode.ENERGYSOURCE_MAGNITUDE:
                    magnitudeVoltage = property.AsFloat();
                    break;
                case ModelCode.ENERGYSOURCE_VOLTAGE:
                    nominalVoltage = property.AsFloat();
                    break;
                case ModelCode.ENERGYSOURCE_TYPE:
                    type = (EnergySourceType)property.AsEnum();
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
            //if (baseVoltage != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            //{
            //	references[ModelCode.CONDEQ_BASVOLTAGE] = new List<long>();
            //	references[ModelCode.CONDEQ_BASVOLTAGE].Add(baseVoltage);
            //}

            base.GetReferences(references, refType);
        }

        #endregion IReference implementation
    }
}
