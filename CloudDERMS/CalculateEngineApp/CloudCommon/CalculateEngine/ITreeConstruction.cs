using DERMSCommon;
using DERMSCommon.DataModel.Core;
using DERMSCommon.DataModel.Meas;
using DERMSCommon.DataModel.Wires;
using DERMSCommon.NMSCommuication;
using DERMSCommon.SCADACommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CloudCommon.CalculateEngine
{
    [ServiceContract]
    public interface ITreeConstruction
    {
        // Methods shoud return finished trees
        // Calculation of Flexibility removed from this code, should be moved somewhere else
        // Removed UpdateNewDataPoitns
        // List<object> da bude povratna vrednost za Construct tree da bi cratio graph cached i NetworkModelTreeClass
       // [OperationContract]
       // TreeNode<NodeData> ConstructTree(NetworkModelTransfer networkModelTransfer);                                  // Should be used if this is the first pass and there is no pre-built trees
        
        [OperationContract]
        [ServiceKnownType(typeof(Switch))]
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
        TreeNode<NodeData> ConstructTree(NetworkModelTransfer networkModelTransfer, TreeNode<NodeData> graphCached);  // Should be used when there is a pre-built tree
        
        [OperationContract]
        Task<TreeNode<NodeData>> UpdateGraphWithScadaValues(List<DataPoint> data, TreeNode<NodeData> graphCached);

        [OperationContract]
        [ServiceKnownType(typeof(DERMSCommon.DataModel.Wires.Switch))]
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
        Task<TreeNode<NodeData>> ConstructTree1(NetworkModelTransfer networkModelTransfer);

    }
}
