using DERMSCommon;
using DERMSCommon.DataModel.Core;
using DERMSCommon.DataModel.Meas;
using DERMSCommon.DataModel.Wires;
using DERMSCommon.SCADACommon;
using DERMSCommon.UIModel.ThreeViewModel;
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
        [OperationContract]
        void SendScadaDataToUIDataPoint(List<DataPoint> data);

        [ServiceKnownType(typeof(Switch))]
        [ServiceKnownType(typeof(Point))]
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
        [ServiceKnownType(typeof(GeographicalRegion))]
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
        [ServiceKnownType(typeof(LinkedList<TreeNode<NodeData>>))]
        [OperationContract]
        void SendDataUI(TreeNode<NodeData> data, List<NetworkModelTreeClass> NetworkModelTreeClass);
    }
    
}
