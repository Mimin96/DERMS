using FTN.ServiceContracts;
using FTN.Services.NetworkModelService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CloudCommon
{
    public class NMSCLoudClient : ClientBase<INetworkModelGDAContract>, INetworkModelGDAContract
    {
        protected static NetworkModelDeepCopy nm = null;
        public NMSCLoudClient()
        {
            nm = new NetworkModelDeepCopy();
        }

        public NMSCLoudClient(string endpoint) : base(endpoint)
        {
            nm = new NetworkModelDeepCopy();
        }

        public Task<FTN.Common.UpdateResult> ApplyUpdate(FTN.Common.Delta delta)
        {
            return Channel.ApplyUpdate(delta);
        }

        public FTN.Common.UpdateResult ApplyUpdate1(FTN.Common.Delta delta, bool b)
        {
            throw new NotImplementedException();
        }

        public int GetExtentValues(FTN.Common.ModelCode entityType, List<FTN.Common.ModelCode> propIds)
        {
            throw new NotImplementedException();
        }

        public int GetRelatedValues(long source, List<FTN.Common.ModelCode> propIds, FTN.Common.Association association)
        {
            throw new NotImplementedException();
        }

        public FTN.Common.ResourceDescription GetValues(long resourceId, List<FTN.Common.ModelCode> propIds)
        {
            throw new NotImplementedException();
        }

        public bool IteratorClose(int id)
        {
            throw new NotImplementedException();
        }

        public List<FTN.Common.ResourceDescription> IteratorNext(int n, int id)
        {
            throw new NotImplementedException();
        }

        public int IteratorResourcesLeft(int id)
        {
            throw new NotImplementedException();
        }

        public int IteratorResourcesTotal(int id)
        {
            throw new NotImplementedException();
        }

        public bool IteratorRewind(int id)
        {
            throw new NotImplementedException();
        }
        
    }
}
