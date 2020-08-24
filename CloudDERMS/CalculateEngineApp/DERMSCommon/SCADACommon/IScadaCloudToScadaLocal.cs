using DERMSCommon.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon.SCADACommon
{
    [ServiceContract]
    public interface IScadaCloudToScadaLocal
    {
        [OperationContract]
        Task<bool> SendEndpoints(string endpoint);
        [OperationContract]
        Task AddorUpdateAnalogniKontejnerModelEntity(Dictionary<long, IdentifiedObject> dict);
        [OperationContract]
        Task AddorUpdateDigitalniKontejnerModelEntity(Dictionary<long, IdentifiedObject> dict);
        [OperationContract]
        Task AddorUpdateGidoviNaAdresuModelEntity(Dictionary<List<long>, ushort> dict);
        [OperationContract]
        Task<Dictionary<long, IdentifiedObject>> GetAnalogniKontejnerModel();
        [OperationContract]
        Task<Dictionary<long, IdentifiedObject>> GetDigitalniKontejnerModel();
        [OperationContract]
        Task<Dictionary<List<long>, ushort>> GetGidoviNaAdresuModel();
        [OperationContract]
        Task<string> GetAddress();
    }
}
