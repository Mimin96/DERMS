using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using DERMSCommon.DataModel.Core;
using DERMSCommon.DataModel.Meas;
using DERMSCommon.DataModel.Wires;
using FTN.Common;


namespace DERMSCommon.NMSCommuication
{
    [ServiceContract]
    public interface ISendDataFromNMSToCE
    {
        [OperationContract]
        [ServiceKnownType(typeof(DataModel.Wires.Switch))]
        [ServiceKnownType(typeof(PROTECTED_SWITCH))]
        [ServiceKnownType(typeof(Conductor))]
        [ServiceKnownType(typeof(Point))]
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
        bool SendNetworkModel(NetworkModelTransfer networkModel);

        [OperationContract]
        [ServiceKnownType(typeof(DataModel.Wires.Switch))]
        [ServiceKnownType(typeof(PROTECTED_SWITCH))]
        [ServiceKnownType(typeof(Conductor))]
        [ServiceKnownType(typeof(Point))]
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
        bool CheckForTM(NetworkModelTransfer networkModel);
    }
}
