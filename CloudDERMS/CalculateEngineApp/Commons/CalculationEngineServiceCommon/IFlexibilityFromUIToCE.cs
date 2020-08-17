﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using static DERMSCommon.Enums;

namespace CalculationEngineServiceCommon
{
    [ServiceContract]
    public interface IFlexibilityFromUIToCE
    {
        [OperationContract]
		Task UpdateFlexibilityFromUIToCE(double valueKW, FlexibilityIncDec incOrDec, long gid);

        [OperationContract]
        Task ChangeBreakerStatus(long GID, bool NormalOpen);
    }
}