using DERMSCommon.UIModel.ThreeViewModel;
using DERMSCommon.WeatherForecast;
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
    public class DataToUI
    {
        #region DerForecastDayAhead
        [DataMember]
        public Dictionary<long, DerForecastDayAhead> Data { get; set; }
        #endregion

        #region Flexibility
        [DataMember]
        public Double Flexibility { get; set; }
        [DataMember]
        public FlexibilityIncDec FlexibilityIncDec { get; set; }
        [DataMember]
        public int Topic { get; set; }
        [DataMember]
        public long Gid { get; set; }
        #endregion

        #region CE To Scada
        [DataMember]
        public Dictionary<long, double> DataFromCEToScada { get; set; }
        #endregion

        #region NetworkModelTreeClass
        [DataMember]
        public List<NetworkModelTreeClass> NetworkModelTreeClass { get; set; }
        #endregion

        public DataToUI()
        {
            Data = new Dictionary<long, DerForecastDayAhead>();
            Flexibility = 0;
            Topic = 0;
            Gid = 0;
            FlexibilityIncDec = FlexibilityIncDec.Default;
            DataFromCEToScada = new Dictionary<long, double>();
            NetworkModelTreeClass = new List<NetworkModelTreeClass>();
        }
    }
}
