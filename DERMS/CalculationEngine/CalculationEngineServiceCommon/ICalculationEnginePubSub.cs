﻿using DERMSCommon;
using DERMSCommon.WeatherForecast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServiceCommon
{
    [ServiceContract]
    public interface ICalculationEnginePubSub
    {
        [OperationContract]
        void SendScadaDataToUI(DataToUI data);
    }
    
}
