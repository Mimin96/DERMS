using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Core;
using FTN.Services.NetworkModelService.DataModel.Meas;
using FTN.Services.NetworkModelService.DataModel.Wires;

namespace DERMSCommon.NMSCommuication
{
    [ServiceContract]
    public interface ISendDataFromNMSToCE
    {
        [OperationContract]
        [ServiceKnownType(typeof(Switch))]
        [ServiceKnownType(typeof(PROTECTED_SWITCH))]
        [ServiceKnownType(typeof(Conductor))]
        [ServiceKnownType(typeof(Breaker))]
        [ServiceKnownType(typeof(ACLineSegment))]
        [ServiceKnownType(typeof(Measurement))]
        [ServiceKnownType(typeof(Discrete))]
        [ServiceKnownType(typeof(Analog))]
        [ServiceKnownType(typeof(Terminal))]
        [ServiceKnownType(typeof(Generator))]
        [ServiceKnownType(typeof(Substation))]
        [ServiceKnownType(typeof(SubGeographicalRegion))]
        [ServiceKnownType(typeof(RegulatingCondEq))]
        [ServiceKnownType(typeof(PowerSystemResource))]
        [ServiceKnownType(typeof(FeederObject))]
        [ServiceKnownType(typeof(EquipmentContainer))]
        [ServiceKnownType(typeof(Equipment))]
        [ServiceKnownType(typeof(EnergySource))]
        [ServiceKnownType(typeof(EnergyConsumer))]
        [ServiceKnownType(typeof(ConnectivityNodeContainer))]
        [ServiceKnownType(typeof(ConnectivityNode))]
        [ServiceKnownType(typeof(ConductingEquipment))]
        [ServiceKnownType(typeof(GeographicalRegion))]
        void SendNetworkModel(NetworkModelTransfer networkModel);
    }
}
