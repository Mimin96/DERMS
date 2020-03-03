using DERMSCommon.WeatherForecast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon
{
	[DataContract]
	public class DataToUI
	{
		[DataMember]
		public Dictionary<long, DerForecastDayAhead> Data { get; set; }
		[DataMember]
		public Double Flexibility { get; set; }
		[DataMember]
		public int Topic { get; set; }
		public DataToUI()
		{
			Data = new Dictionary<long, DerForecastDayAhead>();
			Flexibility = 0;
			Topic = 0;
		}
	}
}
