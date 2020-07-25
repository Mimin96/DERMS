using DERMSCommon;
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
        [OperationContract]
        TreeNode<NodeData> ConstructTree(NetworkModelTransfer networkModelTransfer);                                  // Should be used if this is the first pass and there is no pre-built trees
        
        [OperationContract]
        TreeNode<NodeData> ConstructTree(NetworkModelTransfer networkModelTransfer, TreeNode<NodeData> graphCached);  // Should be used when there is a pre-built tree
        
        [OperationContract]
        TreeNode<NodeData> UpdateGraphWithScadaValues(List<DataPoint> data, TreeNode<NodeData> graphCached);

    }
}
