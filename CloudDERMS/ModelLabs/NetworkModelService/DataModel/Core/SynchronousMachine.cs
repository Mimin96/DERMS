using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FTN.Services.NetworkModelService.DataModel.Core
{
    [DataContract]
    public class Generator : RegulatingCondEq
    {
        [DataMember]
        private float maxQ;
        [DataMember]
        private float minQ;
        [DataMember]
        private float considerP;
        [DataMember]
        private GeneratorType generatorType;
      
        public float MaxQ { get => maxQ; set => maxQ = value; }
        public float MinQ { get => minQ; set => minQ = value; }
        public float ConsiderP { get => considerP; set => considerP = value; }
        public GeneratorType GeneratorType { get => generatorType; set => generatorType = value; }

        public Generator(long globalId) : base(globalId)
        {
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                Generator x = (Generator)obj;
                return (x.maxQ == this.maxQ && x.minQ == this.minQ && x.considerP == this.considerP && x.generatorType == this.generatorType);
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
                case ModelCode.GENERATOR_GENERATORTYPE:
                case ModelCode.GENERATOR_MAXQ:
                case ModelCode.GENERATOR_MINQ:
                case ModelCode.GENERATOR_CONSIDERP:
                    return true;

                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {
                case ModelCode.GENERATOR_GENERATORTYPE:
                    prop.SetValue((short)generatorType);
                    break;

                case ModelCode.GENERATOR_MAXQ:
                    prop.SetValue(maxQ);
                    break;

                case ModelCode.GENERATOR_MINQ:
                    prop.SetValue(minQ);
                    break;
                
                case ModelCode.GENERATOR_CONSIDERP:
                    prop.SetValue(considerP);
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

                case ModelCode.GENERATOR_GENERATORTYPE:
                    generatorType = (GeneratorType)property.AsEnum();
                    break;
                case ModelCode.GENERATOR_MAXQ:
                    maxQ = property.AsFloat();
                    break;

                case ModelCode.GENERATOR_MINQ:
                    minQ = property.AsFloat();
                    break;                
                
                case ModelCode.GENERATOR_CONSIDERP:
                    considerP = property.AsFloat();
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
            //if (powerSystemResource != 0 && (refType != TypeOfReference.Reference || refType != TypeOfReference.Both))
            //{
            //    references[ModelCode.MEASUREMENT_PSR] = new List<long>();
            //    references[ModelCode.MEASUREMENT_PSR].Add(powerSystemResource);
            //}

            base.GetReferences(references, refType);
        }

        #endregion IReference implementation
    }
}
