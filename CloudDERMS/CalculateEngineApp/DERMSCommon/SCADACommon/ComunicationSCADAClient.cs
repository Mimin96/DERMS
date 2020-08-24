using DERMSCommon.DataModel.Core;
using DERMSCommon.SCADACommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon.SCADACommon
{
    public class ComunicationSCADAClient : ClientBase<IScadaCloudToScadaLocal>, IScadaCloudToScadaLocal
    {
        public ComunicationSCADAClient()
        {

        }

        public ComunicationSCADAClient(string endpoint) : base(endpoint)
        {

        }

        public Task AddorUpdateAnalogniKontejnerModelEntity(Dictionary<long, IdentifiedObject> dict)
        {
            return Channel.AddorUpdateAnalogniKontejnerModelEntity(dict);
        }

        public Task AddorUpdateDigitalniKontejnerModelEntity(Dictionary<long, IdentifiedObject> dict)
        {
            return Channel.AddorUpdateDigitalniKontejnerModelEntity(dict);
        }

        public Task AddorUpdateGidoviNaAdresuModelEntity(Dictionary<List<long>, ushort> dict)
        {
            return Channel.AddorUpdateGidoviNaAdresuModelEntity(dict);
        }

        public Task<string> GetAddress()
        {
            return Channel.GetAddress();
        }

        public Task<Dictionary<long, IdentifiedObject>> GetAnalogniKontejnerModel()
        {
            return Channel.GetAnalogniKontejnerModel();
        }

        public Task<Dictionary<long, IdentifiedObject>> GetDigitalniKontejnerModel()
        {
            return Channel.GetDigitalniKontejnerModel();
        }

        public Task<Dictionary<List<long>, ushort>> GetGidoviNaAdresuModel()
        {
            return Channel.GetGidoviNaAdresuModel();
        }

        public Task<bool> SendEndpoints(string endpoint)
        {
            return Channel.SendEndpoints(endpoint);
        }
    }
}
