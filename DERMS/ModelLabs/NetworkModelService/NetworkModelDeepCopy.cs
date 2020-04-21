using DERMSCommon.DataModel.Core;
using DERMSCommon.NMSCommuication;
using FTN.Common;
using FTN.Services.NetworkModelService.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.NetworkModelService
{
    public class NetworkModelDeepCopy : INetworkModelDeepCopy
    {
        NetworkModel networkModel = null;
        NetworkModel networkModelCopy = null;
        Delta lastDelta = null;

        CommunicationWithCE proxyFromNMSToCE;
        CommunicationWithScada proxyFromNMSToScada;
        CommunicationWithTM proxyFromNMSToTM;
        NetworkModelTransfer networkModelTransfer;
        SignalsTransfer signalsTransfer;
        public NetworkModelDeepCopy()
        {
            networkModel = new NetworkModel();
        }

        public void StartService()
        {
            proxyFromNMSToTM = new CommunicationWithTM();
            proxyFromNMSToCE = new CommunicationWithCE();
            proxyFromNMSToScada = new CommunicationWithScada();
            bool result;
            bool result1;

            proxyFromNMSToTM.Open();
            proxyFromNMSToTM.sendToTM.Enlist("net.tcp://localhost:19506/ITransactionCheck");

            proxyFromNMSToCE.Open();

            DataForSendingToCEandSCADA();
            networkModelTransfer.InitState = true;


            result1 = proxyFromNMSToCE.sendToCE.CheckForTM(networkModelTransfer);

            proxyFromNMSToScada.Open();
            result = proxyFromNMSToScada.sendToScada.CheckForTM(signalsTransfer);

            proxyFromNMSToTM.sendToTM.FinishList(result && result1);
        }

        #region Find

        public bool EntityExists(long globalId)
        {
            return networkModel.EntityExists(globalId);
        }

        public IdentifiedObject GetEntity(long globalId)
        {
            return networkModel.GetEntity(globalId);
        }

        #endregion Find

        #region GDA query

        /// <summary>
        /// Gets resource description for entity requested by globalId.
        /// </summary>
        /// <param name="globalId">Id of the entity</param>
        /// <param name="properties">List of requested properties</param>		
        /// <returns>Resource description of the specified entity</returns>
        public ResourceDescription GetValues(long globalId, List<ModelCode> properties)
        {
            return networkModel.GetValues(globalId, properties);
        }

        /// <summary>
        /// Gets resource iterator that holds descriptions for all entities of the specified type.
        /// </summary>		
        /// <param name="type">Type of entity that is requested</param>
        /// <param name="properties">List of requested properties</param>		
        /// <returns>Resource iterator for the requested entities</returns>
        public ResourceIterator GetExtentValues(ModelCode entityType, List<ModelCode> properties)
        {
            return networkModel.GetExtentValues(entityType, properties);
        }

        /// <summary>
        /// Gets resource iterator that holds descriptions for all entities related to specified source.
        /// </summary>
        /// <param name="contextId">Context Id</param>
        /// <param name="properties">List of requested properties</param>
        /// <param name="association">Relation between source and entities that should be returned</param>
        /// <param name="source">Id of entity that is start for association search</param>
        /// <param name="typeOfQuery">Query type choice(global or local)</param>
        /// <returns>Resource iterator for the requested entities</returns>
        public ResourceIterator GetRelatedValues(long source, List<ModelCode> properties, Association association)
        {
            return networkModel.GetRelatedValues(source, properties, association);
        }

        #endregion GDA query	       

        public UpdateResult Apply(Delta delta)
        {

            lastDelta = delta;
            networkModelCopy = new NetworkModel(networkModel);
            UpdateResult updateResult = networkModel.ApplyDelta(delta);

            if (updateResult.Result == ResultType.Failed)
            {
                networkModel = networkModelCopy;
                return updateResult;
            }

            return updateResult;
        }

        private void DataForSendingToCEandSCADA()
        {
            Dictionary<DMSType, Dictionary<long, IdentifiedObject>> insertCE = new Dictionary<DMSType, Dictionary<long, IdentifiedObject>>();
            Dictionary<DMSType, Dictionary<long, IdentifiedObject>> updateCE = new Dictionary<DMSType, Dictionary<long, IdentifiedObject>>();
            Dictionary<DMSType, Dictionary<long, IdentifiedObject>> deleteCE = new Dictionary<DMSType, Dictionary<long, IdentifiedObject>>();
            Dictionary<int, Dictionary<long, IdentifiedObject>> insertSCADA = new Dictionary<int, Dictionary<long, IdentifiedObject>>();
            Dictionary<int, Dictionary<long, IdentifiedObject>> updateSCADA = new Dictionary<int, Dictionary<long, IdentifiedObject>>();
            Dictionary<int, Dictionary<long, IdentifiedObject>> deleteSCADA = new Dictionary<int, Dictionary<long, IdentifiedObject>>();

            foreach (DMSType dmst in networkModel.networkDataModel.Keys)
            {
                Container container = networkModel.networkDataModel[dmst];

                foreach (long key in container.Entities.Keys)
                {
                    //insert
                    if (networkModel.insert.Contains(key))
                    {
                        if (!insertCE.ContainsKey(dmst))
                        {
                            Dictionary<long, IdentifiedObject> helper = new Dictionary<long, IdentifiedObject>();
                            helper.Add(key, container.Entities[key]);
                            insertCE[dmst] = helper;
                            //if (dmst == DMSType.ANALOG || dmst == DMSType.DISCRETE)
                            if (dmst == DMSType.ANALOG)
                                insertSCADA[1] = helper;
                            else if (dmst == DMSType.DISCRETE)
                                insertSCADA[0] = helper;
                        }
                        else
                        {
                            insertCE[dmst].Add(key, container.Entities[key]);
                        }
                    }

                    //update
                    if (networkModel.update.Contains(key))
                    {
                        if (!updateCE.ContainsKey(dmst))
                        {
                            Dictionary<long, IdentifiedObject> helper = new Dictionary<long, IdentifiedObject>();
                            helper.Add(key, container.Entities[key]);
                            updateCE[dmst] = helper;
                            if (dmst == DMSType.ANALOG)
                                updateSCADA[1] = helper;
                            else if (dmst == DMSType.DISCRETE)
                                updateSCADA[0] = helper;
                        }
                        else
                        {
                            updateCE[dmst].Add(key, container.Entities[key]);
                        }
                    }

                    //delete
                    if (networkModel.delete.Contains(key))
                    {
                        if (!deleteCE.ContainsKey(dmst))
                        {
                            Dictionary<long, IdentifiedObject> helper = new Dictionary<long, IdentifiedObject>();
                            helper.Add(key, container.Entities[key]);
                            deleteCE[dmst] = helper;
                            if (dmst == DMSType.ANALOG)
                                deleteSCADA[1] = helper;
                            else if (dmst == DMSType.DISCRETE)
                                deleteSCADA[0] = helper;
                        }
                        else
                        {
                            deleteCE[dmst].Add(key, container.Entities[key]);
                        }
                    }
                }
            }

            networkModelTransfer = new NetworkModelTransfer(insertCE, updateCE, deleteCE);
            signalsTransfer = new SignalsTransfer(insertSCADA, updateSCADA, deleteSCADA);
        }

        public void Commit()
        {
            if (lastDelta != null)
                networkModel.SaveDelta(lastDelta);
        }

        public void Rollback()
        {
            networkModel = networkModelCopy;
        }

    }
}



