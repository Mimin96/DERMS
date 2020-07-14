using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServiceCommon
{
    [DataContract]
    public class DataFromScadaToCE
    {
        [DataMember]
        long Gid { get; set; }

        [DataMember]
        TypeOfSignalEnum TypeOfSignal { get; set; }

        [DataMember]
        long Value { get; set; }

        public DataFromScadaToCE(long gid,TypeOfSignalEnum type,long value)
        {
            Gid = gid;
            TypeOfSignal = type;
            Value = value;
        }
    }
}
