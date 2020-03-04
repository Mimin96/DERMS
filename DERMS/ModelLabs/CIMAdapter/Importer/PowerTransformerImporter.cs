using System;
using System.Collections.Generic;
using CIM.Model;
using FTN.Common;
using FTN.ESI.SIMES.CIM.CIMAdapter.Manager;
using TelventDMS.Services.NetworkModelService.TestClient.Tests;

namespace FTN.ESI.SIMES.CIM.CIMAdapter.Importer
{
	/// <summary>
	/// PowerTransformerImporter
	/// </summary>
	public class PowerTransformerImporter
	{
		/// <summary> Singleton </summary>
		private static PowerTransformerImporter ptImporter = null;
		private static object singletoneLock = new object();

        public static TestGda test = new TestGda();
        private ConcreteModel concreteModel;
		private Delta delta;
		private ImportHelper importHelper;
		private TransformAndLoadReport report;


		#region Properties
		public static PowerTransformerImporter Instance
		{
			get
			{
				if (ptImporter == null)
				{
					lock (singletoneLock)
					{
						if (ptImporter == null)
						{
							ptImporter = new PowerTransformerImporter();
							ptImporter.Reset();
						}
					}
				}
				return ptImporter;
			}
		}

		public Delta NMSDelta
		{
			get 
			{
				return delta;
			}
		}
		#endregion Properties


		public void Reset()
		{
			concreteModel = null;
			delta = new Delta();
			importHelper = new ImportHelper();
			report = null;
		}

		public TransformAndLoadReport CreateNMSDelta(ConcreteModel cimConcreteModel)
		{
			LogManager.Log("Importing PowerTransformer Elements...", LogLevel.Info);
			report = new TransformAndLoadReport();
			concreteModel = cimConcreteModel;
			delta.ClearDeltaOperations();

			if ((concreteModel != null) && (concreteModel.ModelMap != null))
			{
				try
				{
					// convert into DMS elements
					ConvertModelAndPopulateDelta();
				}
				catch (Exception ex)
				{
					string message = string.Format("{0} - ERROR in data import - {1}", DateTime.Now, ex.Message);
					LogManager.Log(message);
					report.Report.AppendLine(ex.Message);
					report.Success = false;
				}
			}
			LogManager.Log("Importing PowerTransformer Elements - END.", LogLevel.Info);
			return report;
		}

		/// <summary>
		/// Method performs conversion of network elements from CIM based concrete model into DMS model.
		/// </summary>
		private void ConvertModelAndPopulateDelta()
		{
			LogManager.Log("Loading elements and creating delta...", LogLevel.Info);

            //// import all concrete model types (DMSType enum)
            //ImportBaseVoltages();
            
            ImportGeographicalRegions();
            ImportSubGeographicalRegions();
            //ImportFeederObjects();
            ImportSubstations();
            ImportBreakers();           
            ImportGenerators();
            ImportEnergyConsumers();
            ImportEnergySources();
            ImportACLineSegments();
            ImportPoints();
            ImportConnectivityNodes();
            ImportTerminals();
            ImportAnalogs();
            ImportDiscretes();

			LogManager.Log("Loading elements and creating delta completed.", LogLevel.Info);
		}

        #region Import

        private void ImportPoints()
        {
            bool isUpdated = false;
            SortedDictionary<string, ResourceDescription> mrids = test.GetAllMrids(ModelCode.IDOBJ_MRID, ModelCode.POINT);
            SortedDictionary<string, object> cimPoints = concreteModel.GetAllObjectsOfType("FTN.Point");
            if (cimPoints != null)
            {
                foreach (KeyValuePair<string, object> cimPointPair in cimPoints)
                {
                    FTN.Point cimPoint = cimPointPair.Value as FTN.Point;

                    ResourceDescription rd = new ResourceDescription();
                    PowerTransformerConverter.PopulatePointProperties(cimPoint, rd, importHelper, report);
                    ResourceDescription rdResult = null;
                    if (mrids.ContainsKey(cimPoint.MRID))
                    {
                        rd.Id = mrids[cimPoint.MRID].Id;
                        rdResult = new ResourceDescription(mrids[cimPoint.MRID].Id);
                        importHelper.DefineIDMapping(cimPoint.ID, rdResult.Id);
                        for (int i = 0; i < mrids[cimPoint.MRID].Properties.Count; i++)
                        {
                            if (mrids[cimPoint.MRID].Properties[i].Id == ModelCode.IDOBJ_GID && mrids[cimPoint.MRID].Properties[i].Type != PropertyType.Reference)
                                continue;
                            for (int j = 0; j < rd.Properties.Count; j++)
                                if (mrids[cimPoint.MRID].Properties[i].Id == rd.Properties[j].Id)
                                {
                                    if (!rd.Properties[j].PropertyValue.StringValue.Equals(mrids[cimPoint.MRID].Properties[i].PropertyValue.StringValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.LongValue.Equals(mrids[cimPoint.MRID].Properties[i].PropertyValue.LongValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.FloatValue.Equals(mrids[cimPoint.MRID].Properties[i].PropertyValue.FloatValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                }
                        }

                        if (isUpdated)
                        {
                            report.Report.Append("Point ID = ").Append(cimPoint.ID).Append(" SUCCESSFULLY updated GID = ").AppendLine(mrids[cimPoint.MRID].Id.ToString());
                            isUpdated = false;
                        }

                        delta.AddDeltaOperation(DeltaOpType.Update, rdResult, true);
                    }
                    else
                    {
                        rd = CreatePointResourceDescription(cimPoint);
                        if (rd != null)
                        {
                            delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                            report.Report.Append("Point ID = ").Append(cimPoint.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                        }
                        else
                        {
                            report.Report.Append("Point ID = ").Append(cimPoint.ID).AppendLine(" FAILED to be converted");
                        }
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreatePointResourceDescription(FTN.Point cimPoint)
        {
            ResourceDescription rd = null;
            if (cimPoint != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.POINT, importHelper.CheckOutIndexForDMSType(DMSType.POINT));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimPoint.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulatePointProperties(cimPoint, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportBreakers()
        {
            bool isUpdated = false;
            SortedDictionary<string, ResourceDescription> mrids = test.GetAllMrids(ModelCode.IDOBJ_MRID, ModelCode.BREAKER);
            SortedDictionary<string, object> cimBreakers = concreteModel.GetAllObjectsOfType("FTN.Breaker");
            if (cimBreakers != null)
            {
                foreach (KeyValuePair<string, object> cimBreakerPair in cimBreakers)
                {
                    FTN.Breaker cimBreaker = cimBreakerPair.Value as FTN.Breaker;

                    ResourceDescription rd = new ResourceDescription();
                    PowerTransformerConverter.PopulateBreakerProperties(cimBreaker, rd, importHelper, report);
                    ResourceDescription rdResult = null;
                    if (mrids.ContainsKey(cimBreaker.MRID))
                    {
                        rd.Id = mrids[cimBreaker.MRID].Id;
                        rdResult = new ResourceDescription(mrids[cimBreaker.MRID].Id);
                        importHelper.DefineIDMapping(cimBreaker.ID, rdResult.Id);
                        for (int i = 0; i < mrids[cimBreaker.MRID].Properties.Count; i++)
                        {
                            if (mrids[cimBreaker.MRID].Properties[i].Id == ModelCode.IDOBJ_GID && mrids[cimBreaker.MRID].Properties[i].Type != PropertyType.Reference)
                                continue;
                            for (int j = 0; j < rd.Properties.Count; j++)
                                if (mrids[cimBreaker.MRID].Properties[i].Id == rd.Properties[j].Id)
                                {
                                    if (!rd.Properties[j].PropertyValue.StringValue.Equals(mrids[cimBreaker.MRID].Properties[i].PropertyValue.StringValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.LongValue.Equals(mrids[cimBreaker.MRID].Properties[i].PropertyValue.LongValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.FloatValue.Equals(mrids[cimBreaker.MRID].Properties[i].PropertyValue.FloatValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                }
                        }

                        if (isUpdated)
                        {
                            report.Report.Append("Breaker ID = ").Append(cimBreaker.ID).Append(" SUCCESSFULLY updated GID = ").AppendLine(mrids[cimBreaker.MRID].Id.ToString());
                            isUpdated = false;
                        }

                        delta.AddDeltaOperation(DeltaOpType.Update, rdResult, true);
                    }
                    else
                    {
                        rd = CreateBreakerResourceDescription(cimBreaker);
                        if (rd != null)
                        {
                            delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                            report.Report.Append("Breaker ID = ").Append(cimBreaker.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                        }
                        else
                        {
                            report.Report.Append("Breaker ID = ").Append(cimBreaker.ID).AppendLine(" FAILED to be converted");
                        }
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateBreakerResourceDescription(FTN.Breaker cimBreaker)
        {
            ResourceDescription rd = null;
            if (cimBreaker != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.BREAKER, importHelper.CheckOutIndexForDMSType(DMSType.BREAKER));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimBreaker.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateBreakerProperties(cimBreaker, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportGeographicalRegions()
        {
            bool isUpdated = false;
            SortedDictionary<string, ResourceDescription> mrids = test.GetAllMrids(ModelCode.IDOBJ_MRID, ModelCode.GEOGRAPHICALREGION);
            SortedDictionary<string, object> cimGeographicalRegions = concreteModel.GetAllObjectsOfType("FTN.GeographicalRegion");
            if (cimGeographicalRegions != null)
            {
                foreach (KeyValuePair<string, object> cimGeographicalRegionPair in cimGeographicalRegions)
                {
                    FTN.GeographicalRegion cimGeographicalRegion = cimGeographicalRegionPair.Value as FTN.GeographicalRegion;

                    ResourceDescription rd = new ResourceDescription();
                    PowerTransformerConverter.PopulateGeographicalRegionProperties(cimGeographicalRegion, rd, importHelper, report);
                    ResourceDescription rdResult = null;

                    if (mrids.ContainsKey(cimGeographicalRegion.MRID))
                    {
                        rd.Id = mrids[cimGeographicalRegion.MRID].Id;
                        rdResult = new ResourceDescription(mrids[cimGeographicalRegion.MRID].Id);
                        importHelper.DefineIDMapping(cimGeographicalRegion.ID, rdResult.Id);
                        for (int i = 0; i < mrids[cimGeographicalRegion.MRID].Properties.Count; i++)
                        {
                            if (mrids[cimGeographicalRegion.MRID].Properties[i].Id == ModelCode.IDOBJ_GID && mrids[cimGeographicalRegion.MRID].Properties[i].Type != PropertyType.Reference)
                                continue;
                            for (int j = 0; j < rd.Properties.Count; j++)
                                if (mrids[cimGeographicalRegion.MRID].Properties[i].Id == rd.Properties[j].Id)
                                {
                                    if (!rd.Properties[j].PropertyValue.StringValue.Equals(mrids[cimGeographicalRegion.MRID].Properties[i].PropertyValue.StringValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.LongValue.Equals(mrids[cimGeographicalRegion.MRID].Properties[i].PropertyValue.LongValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.FloatValue.Equals(mrids[cimGeographicalRegion.MRID].Properties[i].PropertyValue.FloatValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                }
                        }

                        if (isUpdated)
                        {
                            report.Report.Append("GeographicalRegion ID = ").Append(cimGeographicalRegion.ID).Append(" SUCCESSFULLY updated GID = ").AppendLine(mrids[cimGeographicalRegion.MRID].Id.ToString());
                            isUpdated = false;
                        }

                        
                        delta.AddDeltaOperation(DeltaOpType.Update, rdResult, true);
                    }
                    else
                    {
                        rd = CreateGeographicalRegionResourceDescription(cimGeographicalRegion);
                        if (rd != null)
                        {
                            delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                            report.Report.Append("GeographicalRegion ID = ").Append(cimGeographicalRegion.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                        }
                        else
                        {
                            report.Report.Append("GeographicalRegion ID = ").Append(cimGeographicalRegion.ID).AppendLine(" FAILED to be converted");
                        }
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateGeographicalRegionResourceDescription(FTN.GeographicalRegion cimGeographicalRegion)
        {
            ResourceDescription rd = null;
            if (cimGeographicalRegion != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.GEOGRAPHICALREGION, importHelper.CheckOutIndexForDMSType(DMSType.GEOGRAPHICALREGION));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimGeographicalRegion.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateGeographicalRegionProperties(cimGeographicalRegion, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportSubGeographicalRegions()
        {
            bool isUpdated = false;
            SortedDictionary<string, ResourceDescription> mrids = test.GetAllMrids(ModelCode.IDOBJ_MRID, ModelCode.SUBGEOGRAPHICALREGION);
            SortedDictionary<string, object> cimSubGeographicalRegions = concreteModel.GetAllObjectsOfType("FTN.SubGeographicalRegion");
            if (cimSubGeographicalRegions != null)
            {
                foreach (KeyValuePair<string, object> cimSubGeographicalRegionPair in cimSubGeographicalRegions)
                {
                    FTN.SubGeographicalRegion cimSubGeographicalRegion = cimSubGeographicalRegionPair.Value as FTN.SubGeographicalRegion;

                    ResourceDescription rdResult = null;
                    ResourceDescription rd = new ResourceDescription();
                    PowerTransformerConverter.PopulateSubGeographicalRegionProperties(cimSubGeographicalRegion, rd, importHelper, report);
                    if (mrids.ContainsKey(cimSubGeographicalRegion.MRID))
                    {
                        rd.Id = mrids[cimSubGeographicalRegion.MRID].Id;
                        rdResult = new ResourceDescription(mrids[cimSubGeographicalRegion.MRID].Id);
                        importHelper.DefineIDMapping(cimSubGeographicalRegion.ID, rdResult.Id);

                        for (int i = 0; i < mrids[cimSubGeographicalRegion.MRID].Properties.Count; i++)
                        {
                            if (mrids[cimSubGeographicalRegion.MRID].Properties[i].Id == ModelCode.IDOBJ_GID && mrids[cimSubGeographicalRegion.MRID].Properties[i].Type != PropertyType.Reference)
                                continue;
                            for (int j = 0; j < rd.Properties.Count; j++)
                                if (mrids[cimSubGeographicalRegion.MRID].Properties[i].Id == rd.Properties[j].Id)
                                {
                                    if (!rd.Properties[j].PropertyValue.StringValue.Equals(mrids[cimSubGeographicalRegion.MRID].Properties[i].PropertyValue.StringValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.LongValue.Equals(mrids[cimSubGeographicalRegion.MRID].Properties[i].PropertyValue.LongValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.FloatValue.Equals(mrids[cimSubGeographicalRegion.MRID].Properties[i].PropertyValue.FloatValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                }
                        }

                        if (isUpdated)
                        {
                            report.Report.Append("SubGeographicalRegion ID = ").Append(cimSubGeographicalRegion.ID).Append(" SUCCESSFULLY updated GID = ").AppendLine(mrids[cimSubGeographicalRegion.MRID].Id.ToString());
                            isUpdated = false;
                        }

                        delta.AddDeltaOperation(DeltaOpType.Update, rdResult, true);
                    }
                    else
                    {                        
                        rd = CreateSubGeographicalRegionResourceDescription(cimSubGeographicalRegion);
                        if (rd != null)
                        {
                            delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                            report.Report.Append("SubGeographicalRegion ID = ").Append(cimSubGeographicalRegion.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                        }
                        else
                        {
                            report.Report.Append("SubGeographicalRegion ID = ").Append(cimSubGeographicalRegion.ID).AppendLine(" FAILED to be converted");
                        }
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateSubGeographicalRegionResourceDescription(FTN.SubGeographicalRegion cimSubGeographicalRegion)
        {
            ResourceDescription rd = null;
            if (cimSubGeographicalRegion != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.SUBGEOGRAPHICALREGION, importHelper.CheckOutIndexForDMSType(DMSType.SUBGEOGRAPHICALREGION));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimSubGeographicalRegion.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateSubGeographicalRegionProperties(cimSubGeographicalRegion, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportEnergyConsumers()
        {
            bool isUpdated = false;
            SortedDictionary<string, ResourceDescription> mrids = test.GetAllMrids(ModelCode.IDOBJ_MRID, ModelCode.ENERGYCONSUMER);
            SortedDictionary<string, object> cimEnergyConsumers = concreteModel.GetAllObjectsOfType("FTN.EnergyConsumer");
            if (cimEnergyConsumers != null)
            {
                foreach (KeyValuePair<string, object> cimEnergyConsumerPair in cimEnergyConsumers)
                {
                    FTN.EnergyConsumer cimEnergyConsumer = cimEnergyConsumerPair.Value as FTN.EnergyConsumer;
                    ResourceDescription rd = new ResourceDescription();
                    PowerTransformerConverter.PopulateEnergyConsumerProperties(cimEnergyConsumer, rd, importHelper, report);
                    ResourceDescription rdResult = null;

                    if (mrids.ContainsKey(cimEnergyConsumer.MRID))
                    {
                        rd.Id = mrids[cimEnergyConsumer.MRID].Id;
                        rdResult = new ResourceDescription(mrids[cimEnergyConsumer.MRID].Id);
                        importHelper.DefineIDMapping(cimEnergyConsumer.ID, rdResult.Id);
                        for (int i = 0; i < mrids[cimEnergyConsumer.MRID].Properties.Count; i++)
                        {
                            if (mrids[cimEnergyConsumer.MRID].Properties[i].Id == ModelCode.IDOBJ_GID && mrids[cimEnergyConsumer.MRID].Properties[i].Type != PropertyType.Reference)
                                continue;
                            for (int j = 0; j < rd.Properties.Count; j++)
                                if (mrids[cimEnergyConsumer.MRID].Properties[i].Id == rd.Properties[j].Id)
                                {
                                    if (!rd.Properties[j].PropertyValue.StringValue.Equals(mrids[cimEnergyConsumer.MRID].Properties[i].PropertyValue.StringValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.LongValue.Equals(mrids[cimEnergyConsumer.MRID].Properties[i].PropertyValue.LongValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.FloatValue.Equals(mrids[cimEnergyConsumer.MRID].Properties[i].PropertyValue.FloatValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                }
                        }

                        if (isUpdated)
                        {
                            report.Report.Append("EnergyConsumer ID = ").Append(cimEnergyConsumer.ID).Append(" SUCCESSFULLY updated GID = ").AppendLine(mrids[cimEnergyConsumer.MRID].Id.ToString());
                            isUpdated = false;
                        }


                        delta.AddDeltaOperation(DeltaOpType.Update, rdResult, true);
                    }
                    else
                    {                        
                        rd = CreateEnergyConsumerResourceDescription(cimEnergyConsumer);
                        if (rd != null)
                        {
                            delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                            report.Report.Append("EnergyConsumer ID = ").Append(cimEnergyConsumer.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                        }
                        else
                        {
                            report.Report.Append("EnergyConsumer ID = ").Append(cimEnergyConsumer.ID).AppendLine(" FAILED to be converted");
                        }
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateEnergyConsumerResourceDescription(FTN.EnergyConsumer cimEnergyConsumer)
        {
            ResourceDescription rd = null;
            if (cimEnergyConsumer != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.ENERGYCONSUMER, importHelper.CheckOutIndexForDMSType(DMSType.ENERGYCONSUMER));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimEnergyConsumer.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateEnergyConsumerProperties(cimEnergyConsumer, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportConnectivityNodes()
        {
            bool isUpdated = false;
            SortedDictionary<string, ResourceDescription> mrids = test.GetAllMrids(ModelCode.IDOBJ_MRID, ModelCode.CONNECTIVITYNODE);
            SortedDictionary<string, object> cimConnectivityNodes = concreteModel.GetAllObjectsOfType("FTN.ConnectivityNode");
            if (cimConnectivityNodes != null)
            {
                foreach (KeyValuePair<string, object> cimConnectivityNodePair in cimConnectivityNodes)
                {
                    FTN.ConnectivityNode cimConnectivityNode = cimConnectivityNodePair.Value as FTN.ConnectivityNode;

                    ResourceDescription rd = new ResourceDescription();
                    PowerTransformerConverter.PopulateConnectivityNodeProperties(cimConnectivityNode, rd, importHelper, report);
                    ResourceDescription rdResult = null;

                    if (mrids.ContainsKey(cimConnectivityNode.MRID))
                    {
                        rd.Id = mrids[cimConnectivityNode.MRID].Id;
                        rdResult = new ResourceDescription(mrids[cimConnectivityNode.MRID].Id);
                        importHelper.DefineIDMapping(cimConnectivityNode.ID, rdResult.Id);
                        for (int i = 0; i < mrids[cimConnectivityNode.MRID].Properties.Count; i++)
                        {
                            if (mrids[cimConnectivityNode.MRID].Properties[i].Id == ModelCode.IDOBJ_GID && mrids[cimConnectivityNode.MRID].Properties[i].Type != PropertyType.Reference)
                                continue;
                            for (int j = 0; j < rd.Properties.Count; j++)
                                if (mrids[cimConnectivityNode.MRID].Properties[i].Id == rd.Properties[j].Id)
                                {
                                    if (!rd.Properties[j].PropertyValue.StringValue.Equals(mrids[cimConnectivityNode.MRID].Properties[i].PropertyValue.StringValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.LongValue.Equals(mrids[cimConnectivityNode.MRID].Properties[i].PropertyValue.LongValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.FloatValue.Equals(mrids[cimConnectivityNode.MRID].Properties[i].PropertyValue.FloatValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                }
                        }

                        if (isUpdated)
                        {
                            report.Report.Append("ConnectivityNode ID = ").Append(cimConnectivityNode.ID).Append(" SUCCESSFULLY updated GID = ").AppendLine(mrids[cimConnectivityNode.MRID].Id.ToString());
                            isUpdated = false;
                        }
                        
                        delta.AddDeltaOperation(DeltaOpType.Update, rdResult, true);
                    }
                    else
                    {                     
                        rd = CreateConnectivityNodeResourceDescription(cimConnectivityNode);
                        if (rd != null)
                        {
                            delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                            report.Report.Append("ConnectivityNode ID = ").Append(cimConnectivityNode.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                        }
                        else
                        {
                            report.Report.Append("ConnectivityNode ID = ").Append(cimConnectivityNode.ID).AppendLine(" FAILED to be converted");
                        }
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateConnectivityNodeResourceDescription(FTN.ConnectivityNode cimConnectivityNode)
        {
            ResourceDescription rd = null;
            if (cimConnectivityNode != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.CONNECTIVITYNODE, importHelper.CheckOutIndexForDMSType(DMSType.CONNECTIVITYNODE));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimConnectivityNode.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateConnectivityNodeProperties(cimConnectivityNode, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportTerminals()
        {
            bool isUpdated = false;
            SortedDictionary<string, ResourceDescription> mrids = test.GetAllMrids(ModelCode.IDOBJ_MRID, ModelCode.TERMINAL);
            SortedDictionary<string, object> cimTerminals = concreteModel.GetAllObjectsOfType("FTN.Terminal");
            if (cimTerminals != null)
            {
                foreach (KeyValuePair<string, object> cimTerminalPair in cimTerminals)
                {
                    FTN.Terminal cimTerminal = cimTerminalPair.Value as FTN.Terminal;

                    ResourceDescription rd = new ResourceDescription();
                    PowerTransformerConverter.PopulateTerminalProperties(cimTerminal, rd, importHelper, report);
                    ResourceDescription rdResult = null;

                    if (mrids.ContainsKey(cimTerminal.MRID))
                    {
                        rd.Id = mrids[cimTerminal.MRID].Id;
                        rdResult = new ResourceDescription(mrids[cimTerminal.MRID].Id);
                        importHelper.DefineIDMapping(cimTerminal.ID, rdResult.Id);
                        for (int i = 0; i < mrids[cimTerminal.MRID].Properties.Count; i++)
                        {
                            if (mrids[cimTerminal.MRID].Properties[i].Id == ModelCode.IDOBJ_GID && mrids[cimTerminal.MRID].Properties[i].Type != PropertyType.Reference)
                                continue;
                            for (int j = 0; j < rd.Properties.Count; j++)
                                if (mrids[cimTerminal.MRID].Properties[i].Id == rd.Properties[j].Id)
                                {
                                    if (!rd.Properties[j].PropertyValue.StringValue.Equals(mrids[cimTerminal.MRID].Properties[i].PropertyValue.StringValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.LongValue.Equals(mrids[cimTerminal.MRID].Properties[i].PropertyValue.LongValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.FloatValue.Equals(mrids[cimTerminal.MRID].Properties[i].PropertyValue.FloatValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                }
                        }

                        if (isUpdated)
                        {
                            report.Report.Append("Terminal ID = ").Append(cimTerminal.ID).Append(" SUCCESSFULLY updated GID = ").AppendLine(mrids[cimTerminal.MRID].Id.ToString());
                            isUpdated = false;
                        }


                        delta.AddDeltaOperation(DeltaOpType.Update, rdResult, true);
                    }
                    else
                    {
                        rd = CreateTerminalResourceDescription(cimTerminal);
                        if (rd != null)
                        {
                            delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                            report.Report.Append("Terminal ID = ").Append(cimTerminal.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                        }
                        else
                        {
                            report.Report.Append("Terminal ID = ").Append(cimTerminal.ID).AppendLine(" FAILED to be converted");
                        }
                    }
                }
                report.Report.AppendLine();
            }
        }
        private ResourceDescription CreateTerminalResourceDescription(FTN.Terminal cimTerminal)
        {
            ResourceDescription rd = null;
            if (cimTerminal != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.TERMINAL, importHelper.CheckOutIndexForDMSType(DMSType.TERMINAL));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimTerminal.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateTerminalProperties(cimTerminal, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportSubstations()
        {
            bool isUpdated = false;
            SortedDictionary<string, ResourceDescription> mrids = test.GetAllMrids(ModelCode.IDOBJ_MRID, ModelCode.SUBSTATION);
            SortedDictionary<string, object> cimSubstations = concreteModel.GetAllObjectsOfType("FTN.Substation");
            if (cimSubstations != null)
            {
                foreach (KeyValuePair<string, object> cimSubstationPair in cimSubstations)
                {
                    FTN.Substation cimSubstation = cimSubstationPair.Value as FTN.Substation;

                    ResourceDescription rd = new ResourceDescription();
                    PowerTransformerConverter.PopulateSubstationProperties(cimSubstation, rd, importHelper, report);
                    ResourceDescription rdResult = null;

                    if (mrids.ContainsKey(cimSubstation.MRID))
                    {
                        rd.Id = mrids[cimSubstation.MRID].Id;
                        rdResult = new ResourceDescription(mrids[cimSubstation.MRID].Id);
                        importHelper.DefineIDMapping(cimSubstation.ID, rdResult.Id);
                        for (int i = 0; i < mrids[cimSubstation.MRID].Properties.Count; i++)
                        {
                            if (mrids[cimSubstation.MRID].Properties[i].Id == ModelCode.IDOBJ_GID && mrids[cimSubstation.MRID].Properties[i].Type != PropertyType.Reference)
                                continue;
                            for (int j = 0; j < rd.Properties.Count; j++)
                                if (mrids[cimSubstation.MRID].Properties[i].Id == rd.Properties[j].Id)
                                {
                                    if (!rd.Properties[j].PropertyValue.StringValue.Equals(mrids[cimSubstation.MRID].Properties[i].PropertyValue.StringValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.LongValue.Equals(mrids[cimSubstation.MRID].Properties[i].PropertyValue.LongValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.FloatValue.Equals(mrids[cimSubstation.MRID].Properties[i].PropertyValue.FloatValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                }
                        }

                        if (isUpdated)
                        {
                            report.Report.Append("Substation ID = ").Append(cimSubstation.ID).Append(" SUCCESSFULLY updated GID = ").AppendLine(mrids[cimSubstation.MRID].Id.ToString());
                            isUpdated = false;
                        }


                        delta.AddDeltaOperation(DeltaOpType.Update, rdResult, true);
                    }
                    else
                    {
                        rd = CreateSubstationResourceDescription(cimSubstation);
                        if (rd != null)
                        {
                            delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                            report.Report.Append("Substation ID = ").Append(cimSubstation.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                        }
                        else
                        {
                            report.Report.Append("Substation ID = ").Append(cimSubstation.ID).AppendLine(" FAILED to be converted");
                        }
                    }
                }
                report.Report.AppendLine();
            }
        }
        private ResourceDescription CreateSubstationResourceDescription(FTN.Substation cimSubstation)
        {
            ResourceDescription rd = null;
            if (cimSubstation != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.SUBSTATION, importHelper.CheckOutIndexForDMSType(DMSType.SUBSTATION));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimSubstation.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateSubstationProperties(cimSubstation, rd, importHelper, report);
            }
            return rd;
        }        

        private void ImportACLineSegments()
        {
            bool isUpdated = false;
            SortedDictionary<string, ResourceDescription> mrids = test.GetAllMrids(ModelCode.IDOBJ_MRID, ModelCode.ACLINESEGMENT);
            SortedDictionary<string, object> cimACLineSegments = concreteModel.GetAllObjectsOfType("FTN.ACLineSegment");
            if (cimACLineSegments != null)
            {
                foreach (KeyValuePair<string, object> cimACLineSegmentPair in cimACLineSegments)
                {
                    FTN.ACLineSegment cimACLineSegment = cimACLineSegmentPair.Value as FTN.ACLineSegment;

                    ResourceDescription rd = new ResourceDescription();
                    PowerTransformerConverter.PopulateACLineSegmentProperties(cimACLineSegment, rd, importHelper, report);
                    ResourceDescription rdResult = null;

                    if (mrids.ContainsKey(cimACLineSegment.MRID))
                    {
                        rd.Id = mrids[cimACLineSegment.MRID].Id;
                        rdResult = new ResourceDescription(mrids[cimACLineSegment.MRID].Id);
                        importHelper.DefineIDMapping(cimACLineSegment.ID, rdResult.Id);
                        for (int i = 0; i < mrids[cimACLineSegment.MRID].Properties.Count; i++)
                        {
                            if (mrids[cimACLineSegment.MRID].Properties[i].Id == ModelCode.IDOBJ_GID && mrids[cimACLineSegment.MRID].Properties[i].Type != PropertyType.Reference)
                                continue;
                            for (int j = 0; j < rd.Properties.Count; j++)
                                if (mrids[cimACLineSegment.MRID].Properties[i].Id == rd.Properties[j].Id)
                                {
                                    if (!rd.Properties[j].PropertyValue.StringValue.Equals(mrids[cimACLineSegment.MRID].Properties[i].PropertyValue.StringValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.LongValue.Equals(mrids[cimACLineSegment.MRID].Properties[i].PropertyValue.LongValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.FloatValue.Equals(mrids[cimACLineSegment.MRID].Properties[i].PropertyValue.FloatValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                }
                        }

                        if (isUpdated)
                        {
                            report.Report.Append("ACLineSegment ID = ").Append(cimACLineSegment.ID).Append(" SUCCESSFULLY updated GID = ").AppendLine(mrids[cimACLineSegment.MRID].Id.ToString());
                            isUpdated = false;
                        }


                        delta.AddDeltaOperation(DeltaOpType.Update, rdResult, true);
                    }
                    else
                    {
                        rd = CreateACLineSegmentResourceDescription(cimACLineSegment);
                        if (rd != null)
                        {
                            delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                            report.Report.Append("ACLineSegment ID = ").Append(cimACLineSegment.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                        }
                        else
                        {
                            report.Report.Append("ACLineSegment ID = ").Append(cimACLineSegment.ID).AppendLine(" FAILED to be converted");
                        }
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateACLineSegmentResourceDescription(FTN.ACLineSegment cimACLineSegment)
        {
            ResourceDescription rd = null;
            if (cimACLineSegment != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.ACLINESEGMENT, importHelper.CheckOutIndexForDMSType(DMSType.ACLINESEGMENT));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimACLineSegment.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateACLineSegmentProperties(cimACLineSegment, rd, importHelper, report);
            }
            return rd;
        }
        private void ImportEnergySources()
        {
            bool isUpdated = false;
            SortedDictionary<string, ResourceDescription> mrids = test.GetAllMrids(ModelCode.IDOBJ_MRID, ModelCode.ENERGYSOURCE);
            SortedDictionary<string, object> cimEnergySources = concreteModel.GetAllObjectsOfType("FTN.EnergySource");
            if (cimEnergySources != null)
            {
                foreach (KeyValuePair<string, object> cimEnergySourcePair in cimEnergySources)
                {
                    FTN.EnergySource cimEnergySource = cimEnergySourcePair.Value as FTN.EnergySource;

                    ResourceDescription rd = new ResourceDescription();
                    PowerTransformerConverter.PopulateEnergySourceProperties(cimEnergySource, rd, importHelper, report);
                    ResourceDescription rdResult = null;

                    if (mrids.ContainsKey(cimEnergySource.MRID))
                    {
                        rd.Id = mrids[cimEnergySource.MRID].Id;
                        rdResult = new ResourceDescription(mrids[cimEnergySource.MRID].Id);
                        importHelper.DefineIDMapping(cimEnergySource.ID, rdResult.Id);
                        for (int i = 0; i < mrids[cimEnergySource.MRID].Properties.Count; i++)
                        {
                            if (mrids[cimEnergySource.MRID].Properties[i].Id == ModelCode.IDOBJ_GID && mrids[cimEnergySource.MRID].Properties[i].Type != PropertyType.Reference)
                                continue;
                            for (int j = 0; j < rd.Properties.Count; j++)
                                if (mrids[cimEnergySource.MRID].Properties[i].Id == rd.Properties[j].Id)
                                {
                                    if (!rd.Properties[j].PropertyValue.StringValue.Equals(mrids[cimEnergySource.MRID].Properties[i].PropertyValue.StringValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.LongValue.Equals(mrids[cimEnergySource.MRID].Properties[i].PropertyValue.LongValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.FloatValue.Equals(mrids[cimEnergySource.MRID].Properties[i].PropertyValue.FloatValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                }
                        }

                        if (isUpdated)
                        {
                            report.Report.Append("EnergySource ID = ").Append(cimEnergySource.ID).Append(" SUCCESSFULLY updated GID = ").AppendLine(mrids[cimEnergySource.MRID].Id.ToString());
                            isUpdated = false;
                        }


                        delta.AddDeltaOperation(DeltaOpType.Update, rdResult, true);
                    }
                    else
                    {
                        rd = CreateEnergySourceResourceDescription(cimEnergySource);
                        if (rd != null)
                        {
                            delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                            report.Report.Append("EnergySource ID = ").Append(cimEnergySource.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                        }
                        else
                        {
                            report.Report.Append("EnergySource ID = ").Append(cimEnergySource.ID).AppendLine(" FAILED to be converted");
                        }
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateEnergySourceResourceDescription(FTN.EnergySource cimEnergySource)
        {
            ResourceDescription rd = null;
            if (cimEnergySource != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.ENEGRYSOURCE, importHelper.CheckOutIndexForDMSType(DMSType.ENEGRYSOURCE));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimEnergySource.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateEnergySourceProperties(cimEnergySource, rd, importHelper, report);
            }
            return rd;
        }
        private void ImportGenerators()
        {
            bool isUpdated = false;
            SortedDictionary<string, ResourceDescription> mrids = test.GetAllMrids(ModelCode.IDOBJ_MRID, ModelCode.GENERATOR);
            SortedDictionary<string, object> cimGenerators = concreteModel.GetAllObjectsOfType("FTN.Generator");
            if (cimGenerators != null)
            {
                foreach (KeyValuePair<string, object> cimGeneratorPair in cimGenerators)
                {
                    FTN.Generator cimGenerator = cimGeneratorPair.Value as FTN.Generator;
                    
                    ResourceDescription rd = new ResourceDescription();
                    PowerTransformerConverter.PopulateGeneratorProperties(cimGenerator, rd, importHelper, report);
                    ResourceDescription rdResult = null;
                    if (mrids.ContainsKey(cimGenerator.MRID))
                    {
                        rd.Id = mrids[cimGenerator.MRID].Id;
                        rdResult = new ResourceDescription(mrids[cimGenerator.MRID].Id);
                        importHelper.DefineIDMapping(cimGenerator.ID, rdResult.Id);
                        for (int i = 0; i < mrids[cimGenerator.MRID].Properties.Count; i++)
                        {
                            if (mrids[cimGenerator.MRID].Properties[i].Id == ModelCode.IDOBJ_GID && mrids[cimGenerator.MRID].Properties[i].Type != PropertyType.Reference)
                                continue;
                            for (int j = 0; j < rd.Properties.Count; j++)
                                if (mrids[cimGenerator.MRID].Properties[i].Id == rd.Properties[j].Id)
                                {
                                    if (!rd.Properties[j].PropertyValue.StringValue.Equals(mrids[cimGenerator.MRID].Properties[i].PropertyValue.StringValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.LongValue.Equals(mrids[cimGenerator.MRID].Properties[i].PropertyValue.LongValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.FloatValue.Equals(mrids[cimGenerator.MRID].Properties[i].PropertyValue.FloatValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                }
                        }

                        if (isUpdated)
                        {
                            report.Report.Append("Generator ID = ").Append(cimGenerator.ID).Append(" SUCCESSFULLY updated GID = ").AppendLine(mrids[cimGenerator.MRID].Id.ToString());
                            isUpdated = false;
                        }


                        delta.AddDeltaOperation(DeltaOpType.Update, rdResult, true);
                    }
                    else
                    {
                        rd = CreateGeneratorResourceDescription(cimGenerator);
                        if (rd != null)
                        {
                            delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                            report.Report.Append("Generator ID = ").Append(cimGenerator.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                        }
                        else
                        {
                            report.Report.Append("Generator ID = ").Append(cimGenerator.ID).AppendLine(" FAILED to be converted");
                        }
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateGeneratorResourceDescription(FTN.Generator cimGenerator)
        {
            ResourceDescription rd = null;
            if (cimGenerator != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.GENERATOR, importHelper.CheckOutIndexForDMSType(DMSType.GENERATOR));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimGenerator.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateGeneratorProperties(cimGenerator, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportAnalogs()
        {
            bool isUpdated = false;
            SortedDictionary<string, ResourceDescription> mrids = test.GetAllMrids(ModelCode.IDOBJ_MRID, ModelCode.ANALOG);
            SortedDictionary<string, object> cimAnalogs = concreteModel.GetAllObjectsOfType("FTN.Analog");
            if (cimAnalogs != null)
            {
                foreach (KeyValuePair<string, object> cimAnalogPair in cimAnalogs)
                {
                    FTN.Analog cimAnalog = cimAnalogPair.Value as FTN.Analog;

                    ResourceDescription rd = new ResourceDescription();
                    PowerTransformerConverter.PopulateAnalogProperties(cimAnalog, rd, importHelper, report);
                    ResourceDescription rdResult = null;
                    if (mrids.ContainsKey(cimAnalog.MRID))
                    {
                        rd.Id = mrids[cimAnalog.MRID].Id;
                        rdResult = new ResourceDescription(mrids[cimAnalog.MRID].Id);
                        importHelper.DefineIDMapping(cimAnalog.ID, rdResult.Id);
                        for (int i = 0; i < mrids[cimAnalog.MRID].Properties.Count; i++)
                        {
                            if (mrids[cimAnalog.MRID].Properties[i].Id == ModelCode.IDOBJ_GID && mrids[cimAnalog.MRID].Properties[i].Type != PropertyType.Reference)
                                continue;
                            for (int j = 0; j < rd.Properties.Count; j++)
                                if (mrids[cimAnalog.MRID].Properties[i].Id == rd.Properties[j].Id)
                                {
                                    if (!rd.Properties[j].PropertyValue.StringValue.Equals(mrids[cimAnalog.MRID].Properties[i].PropertyValue.StringValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.LongValue.Equals(mrids[cimAnalog.MRID].Properties[i].PropertyValue.LongValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.FloatValue.Equals(mrids[cimAnalog.MRID].Properties[i].PropertyValue.FloatValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                }
                        }

                        if (isUpdated)
                        {
                            report.Report.Append("Analog ID = ").Append(cimAnalog.ID).Append(" SUCCESSFULLY updated GID = ").AppendLine(mrids[cimAnalog.MRID].Id.ToString());
                            isUpdated = false;
                        }


                        delta.AddDeltaOperation(DeltaOpType.Update, rdResult, true);
                    }
                    else
                    {
                        rd = CreateAnalogResourceDescription(cimAnalog);
                        if (rd != null)
                        {
                            delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                            report.Report.Append("Analog ID = ").Append(cimAnalog.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                        }
                        else
                        {
                            report.Report.Append("Analog ID = ").Append(cimAnalog.ID).AppendLine(" FAILED to be converted");
                        }
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateAnalogResourceDescription(FTN.Analog cimAnalog)
        {
            ResourceDescription rd = null;
            if (cimAnalog != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.ANALOG, importHelper.CheckOutIndexForDMSType(DMSType.ANALOG));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimAnalog.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateAnalogProperties(cimAnalog, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportDiscretes()
        {
            bool isUpdated = false;
            SortedDictionary<string, ResourceDescription> mrids = test.GetAllMrids(ModelCode.IDOBJ_MRID, ModelCode.DISCRETE);
            SortedDictionary<string, object> cimDiscretes = concreteModel.GetAllObjectsOfType("FTN.Discrete");
            if (cimDiscretes != null)
            {
                foreach (KeyValuePair<string, object> cimDiscretePair in cimDiscretes)
                {
                    FTN.Discrete cimDiscrete = cimDiscretePair.Value as FTN.Discrete;

                    ResourceDescription rd = new ResourceDescription();
                    PowerTransformerConverter.PopulateDiscreteProperties(cimDiscrete, rd, importHelper, report);
                    ResourceDescription rdResult = null;
                    if (mrids.ContainsKey(cimDiscrete.MRID))
                    {
                        rd.Id = mrids[cimDiscrete.MRID].Id;
                        rdResult = new ResourceDescription(mrids[cimDiscrete.MRID].Id);
                        importHelper.DefineIDMapping(cimDiscrete.ID, rdResult.Id);
                        for (int i = 0; i < mrids[cimDiscrete.MRID].Properties.Count; i++)
                        {
                            if (mrids[cimDiscrete.MRID].Properties[i].Id == ModelCode.IDOBJ_GID && mrids[cimDiscrete.MRID].Properties[i].Type != PropertyType.Reference)
                                continue;
                            for (int j = 0; j < rd.Properties.Count; j++)
                                if (mrids[cimDiscrete.MRID].Properties[i].Id == rd.Properties[j].Id)
                                {
                                    if (!rd.Properties[j].PropertyValue.StringValue.Equals(mrids[cimDiscrete.MRID].Properties[i].PropertyValue.StringValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.LongValue.Equals(mrids[cimDiscrete.MRID].Properties[i].PropertyValue.LongValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                    if (!rd.Properties[j].PropertyValue.FloatValue.Equals(mrids[cimDiscrete.MRID].Properties[i].PropertyValue.FloatValue))
                                    {
                                        rdResult.Properties.Add(rd.Properties[j]);
                                        isUpdated = true;
                                        break;
                                    }
                                }
                        }

                        if (isUpdated)
                        {
                            report.Report.Append("Discrete ID = ").Append(cimDiscrete.ID).Append(" SUCCESSFULLY updated GID = ").AppendLine(mrids[cimDiscrete.MRID].Id.ToString());
                            isUpdated = false;
                        }


                        delta.AddDeltaOperation(DeltaOpType.Update, rdResult, true);
                    }
                    else
                    {
                        rd = CreateDiscreteResourceDescription(cimDiscrete);
                        if (rd != null)
                        {
                            delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                            report.Report.Append("Discrete ID = ").Append(cimDiscrete.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                        }
                        else
                        {
                            report.Report.Append("Discrete ID = ").Append(cimDiscrete.ID).AppendLine(" FAILED to be converted");
                        }
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateDiscreteResourceDescription(FTN.Discrete cimDiscrete)
        {
            ResourceDescription rd = null;
            if (cimDiscrete != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.DISCRETE, importHelper.CheckOutIndexForDMSType(DMSType.DISCRETE));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimDiscrete.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateDiscreteProperties(cimDiscrete, rd, importHelper, report);
            }
            return rd;
        }
        
		#endregion Import
	}
}

