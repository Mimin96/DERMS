namespace FTN.ESI.SIMES.CIM.CIMAdapter.Importer
{
	using FTN.Common;
    using System;

    /// <summary>
    /// PowerTransformerConverter has methods for populating
    /// ResourceDescription objects using PowerTransformerCIMProfile_Labs objects.
    /// </summary>
    public static class PowerTransformerConverter
	{

        #region Populate ResourceDescription
        public static void PopulateIdentifiedObjectProperties(FTN.IdentifiedObject cimIdentifiedObject, ResourceDescription rd)
        {

            if ((cimIdentifiedObject != null) && (rd != null))
            {
                if (cimIdentifiedObject.MRIDHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.IDOBJ_MRID, cimIdentifiedObject.MRID));
                }
                if (cimIdentifiedObject.DescriptionHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.IDOBJ_DESCRIPTION, cimIdentifiedObject.Description));
                }
                if (cimIdentifiedObject.NameHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.IDOBJ_NAME, cimIdentifiedObject.Name));
                }
            }
        }

        public static void PopulatePowerSystemResourceProperties(FTN.PowerSystemResource cimPowerSystemResource, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimPowerSystemResource != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateIdentifiedObjectProperties(cimPowerSystemResource, rd);
            }
        }


        public static void PopulateTerminalProperties(FTN.Terminal cimTerminal, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimTerminal != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateIdentifiedObjectProperties(cimTerminal, rd);

                if (cimTerminal.ConductingEquipmentHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimTerminal.ConductingEquipment.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimTerminal.GetType().ToString()).Append(" rdfID = \"").Append(cimTerminal.ID);
                        report.Report.Append("\" - Failed to set reference to ConductingEquipment: rdfID \"").Append(cimTerminal.ConductingEquipment.ID).AppendLine(" \" is not mapped to GID!");
                    }
                    rd.AddProperty(new Property(ModelCode.TERMINAL_COND_EQ, gid));
                }

                if (cimTerminal.ConnectivityNodeHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimTerminal.ConnectivityNode.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimTerminal.GetType().ToString()).Append(" rdfID = \"").Append(cimTerminal.ID);
                        report.Report.Append("\" - Failed to set reference to ConnectivityNode: rdfID \"").Append(cimTerminal.ConnectivityNode.ID).AppendLine(" \" is not mapped to GID!");
                    }
                    rd.AddProperty(new Property(ModelCode.TERMINAL_CONNECTIVITY_NODE, gid));
                }
            }
        }

        public static void PopulateGeneratorProperties(FTN.Generator cimGenerator, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimGenerator != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateRegulatingCondEqProperties(cimGenerator, rd, importHelper, report);

                if (cimGenerator.GeneratorTypeHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.GENERATOR_GENERATORTYPE, (short)GetDMSGeneratorType(cimGenerator.GeneratorType)));
                }

                if (cimGenerator.MaxFlexibilityHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.GENERATOR_MAXFLEX, cimGenerator.MaxFlexibility));
                }

                if (cimGenerator.MinFlexibilityHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.GENERATOR_MINFLEX, cimGenerator.MinFlexibility));
                }

                if (cimGenerator.FlexibilityHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.GENERATOR_FLEXIBILITY, cimGenerator.Flexibility));
                }

                if (cimGenerator.MaxQHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.GENERATOR_MAXQ, cimGenerator.MaxQ));
                }

                if (cimGenerator.MinQHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.GENERATOR_MINQ, cimGenerator.MinQ));
                }

                if (cimGenerator.CondenserPHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.GENERATOR_CONSIDERP, cimGenerator.CondenserP));
                }

                if (cimGenerator.EquipmentContainerHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimGenerator.EquipmentContainer.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimGenerator.GetType().ToString()).Append(" rdfID = \"").Append(cimGenerator.ID);
                        report.Report.Append("\" - Failed to set reference to EquipmentContainer: rdfID \"").Append(cimGenerator.EquipmentContainer.ID).AppendLine(" \" is not mapped to GID!");
                    }
                    rd.AddProperty(new Property(ModelCode.EQUIPMENT_CONTAINER, gid));
                }
                if (cimGenerator.BreakerHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimGenerator.Breaker.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimGenerator.GetType().ToString()).Append(" rdfID = \"").Append(cimGenerator.ID);
                        report.Report.Append("\" - Failed to set reference to Breaker: rdfID \"").Append(cimGenerator.Breaker.ID).AppendLine(" \" is not mapped to GID!");
                    }
                    rd.AddProperty(new Property(ModelCode.GENERATOR_BREAKER, gid));
                }
            }
        }

        public static void PopulateProtectedSwitchProperties(FTN.ProtectedSwitch cimProtectedSwitch, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimProtectedSwitch != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateSwitchProperties(cimProtectedSwitch, rd, importHelper, report);
            }
        }

        public static void PopulateMeasurementProperties(FTN.Measurement cimMeasurement, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimMeasurement != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateIdentifiedObjectProperties(cimMeasurement, rd);

                if (cimMeasurement.MeasurementTypeHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.MEASUREMENT_MEAS_TYPE, (short)GetDMSMeasurementType(cimMeasurement.MeasurementType)));
                }
            }
        }

        public static void PopulateConnectivityNodeContainerProperties(FTN.ConnectivityNodeContainer cimConnectivityNodeContainer, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimConnectivityNodeContainer != null) && (rd != null))
            {
                PowerTransformerConverter.PopulatePowerSystemResourceProperties(cimConnectivityNodeContainer, rd, importHelper, report);
            }
        }

        public static void PopulateEquipmentContainerProperties(FTN.EquipmentContainer cimEquipmentContainer, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimEquipmentContainer != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateConnectivityNodeContainerProperties(cimEquipmentContainer, rd, importHelper, report);

                if (cimEquipmentContainer.LongitudeHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.EQUIPMENTCONTAINER_LONGITUDE, cimEquipmentContainer.Longitude));
                }

                if (cimEquipmentContainer.LatitudeHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.EQUIPMENTCONTAINER_LATITUDE, cimEquipmentContainer.Latitude));
                }
            }
        }

        public static void PopulateAnalogProperties(FTN.Analog cimAnalog, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimAnalog != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateMeasurementProperties(cimAnalog, rd, importHelper, report);

                if (cimAnalog.MaxValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ANALOG_MAX_VALUE, cimAnalog.MaxValue));
                }

                if (cimAnalog.MinValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ANALOG_MIN_VALUE, cimAnalog.MinValue));
                }

                if (((FTN.Generator)cimAnalog.PowerSystemResource).CondenserPHasValue)
                {
                    if (cimAnalog.Description == "Commanding")
                        rd.AddProperty(new Property(ModelCode.ANALOG_NORMAL_VALUE, cimAnalog.NormalValue));
                    else
                        rd.AddProperty(new Property(ModelCode.ANALOG_NORMAL_VALUE, ((FTN.Generator)cimAnalog.PowerSystemResource).CondenserP));
                }

                if (((FTN.Generator)cimAnalog.PowerSystemResource).LatitudeHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.MEASUREMENT_LATITUDE, ((FTN.Generator)cimAnalog.PowerSystemResource).Latitude));
                }

                if (((FTN.Generator)cimAnalog.PowerSystemResource).LongitudeHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.MEASUREMENT_LONGITUDE, ((FTN.Generator)cimAnalog.PowerSystemResource).Longitude));
                }

                ////((FTN.Generator)cimAnalog.PowerSystemResource).Latitude////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                if (cimAnalog.PowerSystemResourceHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimAnalog.PowerSystemResource.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimAnalog.GetType().ToString()).Append(" rdfID = \"").Append(cimAnalog.ID);
                        report.Report.Append("\" - Failed to set reference to PowerSystemResource: rdfID \"").Append(cimAnalog.PowerSystemResource.ID).AppendLine(" \" is not mapped to GID!");
                    }
                    rd.AddProperty(new Property(ModelCode.MEASUREMENT_PSR, gid));
                }
            }
        }

        public static void PopulateDiscreteProperties(FTN.Discrete cimDiscrete, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimDiscrete != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateMeasurementProperties(cimDiscrete, rd, importHelper, report);

                if (cimDiscrete.MaxValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.DISCRETE_MAX_VALUE, cimDiscrete.MaxValue));
                }

                if (cimDiscrete.MinValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.DISCRETE_MIN_VALUE, cimDiscrete.MinValue));
                }

                if (cimDiscrete.NormalValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.DISCRETE_NORMAL_VALUE, cimDiscrete.NormalValue));
                }

                if (((FTN.Breaker)cimDiscrete.PowerSystemResource).LatitudeHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.MEASUREMENT_LATITUDE, ((FTN.Breaker)cimDiscrete.PowerSystemResource).Latitude));
                }

                if (((FTN.Breaker)cimDiscrete.PowerSystemResource).LongitudeHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.MEASUREMENT_LONGITUDE, ((FTN.Breaker)cimDiscrete.PowerSystemResource).Longitude));
                }

                if (cimDiscrete.PowerSystemResourceHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimDiscrete.PowerSystemResource.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimDiscrete.GetType().ToString()).Append(" rdfID = \"").Append(cimDiscrete.ID);
                        report.Report.Append("\" - Failed to set reference to PowerSystemResource: rdfID \"").Append(cimDiscrete.PowerSystemResource.ID).AppendLine(" \" is not mapped to GID!");
                    }
                    rd.AddProperty(new Property(ModelCode.MEASUREMENT_PSR, gid));
                }
            }
        }

        public static void PopulateBreakerProperties(FTN.Breaker cimBreaker, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimBreaker != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateProtectedSwitchProperties(cimBreaker, rd, importHelper, report);

                if (cimBreaker.EquipmentContainerHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimBreaker.EquipmentContainer.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimBreaker.GetType().ToString()).Append(" rdfID = \"").Append(cimBreaker.ID);
                        report.Report.Append("\" - Failed to set reference to EquipmentContainer: rdfID \"").Append(cimBreaker.EquipmentContainer.ID).AppendLine(" \" is not mapped to GID!");
                    }
                    rd.AddProperty(new Property(ModelCode.EQUIPMENT_CONTAINER, gid));
                }

            }
        }

        public static void PopulateRegulatingCondEqProperties(FTN.RegulatingCondEq cimRegulatingCondEq, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimRegulatingCondEq != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateConductingEquipmentProperties(cimRegulatingCondEq, rd, importHelper, report);
            }
        }

        public static void PopulatePointProperties(FTN.Point cimPoint, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimPoint != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateIdentifiedObjectProperties(cimPoint, rd);

                if (cimPoint.LongitudeHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.POINT_LONGITUDE, cimPoint.Longitude));
                }

                if (cimPoint.LatitudeHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.POINT_LATITUDE, cimPoint.Latitude));
                }

                if (cimPoint.LineHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimPoint.Line.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimPoint.GetType().ToString()).Append(" rdfID = \"").Append(cimPoint.ID);
                        report.Report.Append("\" - Failed to set reference to ACLINESEGMENT: rdfID \"").Append(cimPoint.Line.ID).AppendLine(" \" is not mapped to GID!");
                    }
                    rd.AddProperty(new Property(ModelCode.POINT_LINE, gid));
                }
            }
        }

        public static void PopulateGeographicalRegionProperties(FTN.GeographicalRegion cimGeographicalRegion, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimGeographicalRegion != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateIdentifiedObjectProperties(cimGeographicalRegion, rd);

                if (cimGeographicalRegion.LongitudeHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.GEOGRAPHICALREGION_LONGITUDE, cimGeographicalRegion.Longitude));
                }

                if (cimGeographicalRegion.LatitudeHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.GEOGRAPHICALREGION_LATITUDE, cimGeographicalRegion.Latitude));
                }
            }
        }

        public static void PopulateSubGeographicalRegionProperties(FTN.SubGeographicalRegion cimSubGeographicalRegion, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimSubGeographicalRegion != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateIdentifiedObjectProperties(cimSubGeographicalRegion, rd);

                if (cimSubGeographicalRegion.LongitudeHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.SUBGEOGRAPHICALREGION_LONGITUDE, cimSubGeographicalRegion.Longitude));
                }

                if (cimSubGeographicalRegion.LatitudeHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.SUBGEOGRAPHICALREGION_LATITUDE, cimSubGeographicalRegion.Latitude));
                }

                if (cimSubGeographicalRegion.RegionHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimSubGeographicalRegion.Region.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimSubGeographicalRegion.GetType().ToString()).Append(" rdfID = \"").Append(cimSubGeographicalRegion.ID);
                        report.Report.Append("\" - Failed to set reference to GeograhicalRegion: rdfID \"").Append(cimSubGeographicalRegion.Region.ID).AppendLine(" \" is not mapped to GID!");
                    }
                    rd.AddProperty(new Property(ModelCode.SUBGEOGRAPHICALREGION_GEOREG, gid));
                }
            }
        }

        public static void PopulateSubstationProperties(FTN.Substation cimSubstation, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimSubstation != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateEquipmentContainerProperties(cimSubstation, rd, importHelper, report);

                if (cimSubstation.RegionHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimSubstation.Region.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimSubstation.GetType().ToString()).Append(" rdfID = \"").Append(cimSubstation.ID);
                        report.Report.Append("\" - Failed to set reference to SubGeograhicalRegion: rdfID \"").Append(cimSubstation.Region.ID).AppendLine(" \" is not mapped to GID!");
                    }
                    rd.AddProperty(new Property(ModelCode.SUBSTATION_SUBGEOREG, gid));
                }
            }
        }

        public static void PopulateConnectivityNodeProperties(FTN.ConnectivityNode cimConnectivityNode, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimConnectivityNode != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateIdentifiedObjectProperties(cimConnectivityNode, rd);
            }
            if (cimConnectivityNode.ConnectivityNodeContainerHasValue)
            {
                long gid = importHelper.GetMappedGID(cimConnectivityNode.ConnectivityNodeContainer.ID);
                if (gid < 0)
                {
                    report.Report.Append("WARNING: Convert ").Append(cimConnectivityNode.GetType().ToString()).Append(" rdfID = \"").Append(cimConnectivityNode.ID);
                    report.Report.Append("\" - Failed to set reference to ConnectivityNodeContainer: rdfID \"").Append(cimConnectivityNode.ConnectivityNodeContainer.ID).AppendLine(" \" is not mapped to GID!");
                }
                rd.AddProperty(new Property(ModelCode.CONNECTIVITYNODE_CONTAINER, gid));
            }
        }
        public static void PopulateSwitchProperties(FTN.Switch cimSwitch, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimSwitch != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateConductingEquipmentProperties(cimSwitch, rd, importHelper, report);

                if (cimSwitch.NormalOpenHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.SWITCH_NORMAL_OPEN, cimSwitch.NormalOpen));
                }

                if (cimSwitch.FeederID1HasValue)
                {
                    rd.AddProperty(new Property(ModelCode.SWITCH_FEEDER_ID1, cimSwitch.FeederID1));
                }

                if (cimSwitch.FeederID2HasValue)
                {
                    rd.AddProperty(new Property(ModelCode.SWITCH_FEEDER_ID2, cimSwitch.FeederID2));
                }
            }
        }

        public static void PopulateEnergyConsumerProperties(FTN.EnergyConsumer cimEnergyConsumer, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimEnergyConsumer != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateConductingEquipmentProperties(cimEnergyConsumer, rd, importHelper, report);

                if (cimEnergyConsumer.PfixedHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ENERGYCONSUMER_PFIXED, cimEnergyConsumer.Pfixed));
                }

                if (cimEnergyConsumer.QfixedHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ENERGYCONSUMER_QFIXED, cimEnergyConsumer.Qfixed));
                }

                if (cimEnergyConsumer.EquipmentContainerHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimEnergyConsumer.EquipmentContainer.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimEnergyConsumer.GetType().ToString()).Append(" rdfID = \"").Append(cimEnergyConsumer.ID);
                        report.Report.Append("\" - Failed to set reference to EquipmentContainer: rdfID \"").Append(cimEnergyConsumer.EquipmentContainer.ID).AppendLine(" \" is not mapped to GID!");
                    }
                    rd.AddProperty(new Property(ModelCode.EQUIPMENT_CONTAINER, gid));
                }

            }
        }

        public static void PopulateEquipmentProperties(FTN.Equipment cimEquipment, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimEquipment != null) && (rd != null))
            {
                PowerTransformerConverter.PopulatePowerSystemResourceProperties(cimEquipment, rd, importHelper, report);
            }
        }

        public static void PopulateConductingEquipmentProperties(FTN.ConductingEquipment cimConductingEquipment, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimConductingEquipment != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateEquipmentProperties(cimConductingEquipment, rd, importHelper, report);

                if (cimConductingEquipment.LongitudeHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.CONDEQ_LONGITUDE, cimConductingEquipment.Longitude));
                }

                if (cimConductingEquipment.LatitudeHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.CONDEQ_LATITUDE, cimConductingEquipment.Latitude));
                }
            }
        }

        public static void PopulateConductorProperties(FTN.Conductor cimConductor, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimConductor != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateConductingEquipmentProperties(cimConductor, rd, importHelper, report);

                if (cimConductor.ConductorTypeHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.CONDUCTOR_TYPE, (short)GetDMSConductorType(cimConductor.ConductorType)));
                }
            }
        }

        public static void PopulateEnergySourceProperties(FTN.EnergySource cimEnergySource, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimEnergySource != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateConductingEquipmentProperties(cimEnergySource, rd, importHelper, report);

                if (cimEnergySource.ActivePowerHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ENERGYSOURCE_ACTIVEPOWER, cimEnergySource.ActivePower));
                }

                if (cimEnergySource.NominalVoltageHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ENERGYSOURCE_VOLTAGE, cimEnergySource.NominalVoltage));
                }

                if (cimEnergySource.VoltageMagnitudeHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ENERGYSOURCE_MAGNITUDE, cimEnergySource.VoltageMagnitude));
                }

                if (cimEnergySource.SourceTypeHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ENERGYSOURCE_TYPE, (short)GetDMSEnergySourceType(cimEnergySource.SourceType)));
                }

                if (cimEnergySource.EquipmentContainerHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimEnergySource.EquipmentContainer.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimEnergySource.GetType().ToString()).Append(" rdfID = \"").Append(cimEnergySource.ID);
                        report.Report.Append("\" - Failed to set reference to EquipmentContainer: rdfID \"").Append(cimEnergySource.EquipmentContainer.ID).AppendLine(" \" is not mapped to GID!");
                    }
                    rd.AddProperty(new Property(ModelCode.EQUIPMENT_CONTAINER, gid));
                }
            }
        }

        public static void PopulateACLineSegmentProperties(FTN.ACLineSegment cimACLineSegment, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimACLineSegment != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateConductorProperties(cimACLineSegment, rd, importHelper, report);

                if (cimACLineSegment.RatedCurrentHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ACLINESEGMENT_CURRENTFLOW, cimACLineSegment.RatedCurrent));
                }

                if (cimACLineSegment.FeederCableHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ACLINESEGMENT_FEEDERCABLE, cimACLineSegment.FeederCable));
                }

                if (cimACLineSegment.EquipmentContainerHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimACLineSegment.EquipmentContainer.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimACLineSegment.GetType().ToString()).Append(" rdfID = \"").Append(cimACLineSegment.ID);
                        report.Report.Append("\" - Failed to set reference to EquipmentContainer: rdfID \"").Append(cimACLineSegment.EquipmentContainer.ID).AppendLine(" \" is not mapped to GID!");
                    }
                    rd.AddProperty(new Property(ModelCode.EQUIPMENT_CONTAINER, gid));
                }
            }
        }

        #endregion Populate ResourceDescription

        #region Enums convert

        public static MeasurementType GetDMSMeasurementType(FTN.MeasurementType measurementType)
        {
            switch (measurementType)
            {
                case FTN.MeasurementType.ActivePower:
                    return MeasurementType.ActivePower;
                case FTN.MeasurementType.ReactivePower:
                    return MeasurementType.ReactivePower;
                case FTN.MeasurementType.Voltage:
                    return MeasurementType.Voltage;
                case FTN.MeasurementType.Discrete:
                    return MeasurementType.Discrete;
                default:
                    return MeasurementType.None;
            }
        }

        public static GeneratorType GetDMSGeneratorType(FTN.GeneratorType generatorType)
        {
            switch (generatorType)
            {
                case FTN.GeneratorType.Wind:
                    return GeneratorType.Wind;
                case FTN.GeneratorType.Solar:
                    return GeneratorType.Solar;
                case FTN.GeneratorType.Battery:
                    return GeneratorType.Battery;
                default:
                    return GeneratorType.None;
            }
        }

        public static ConductorType GetDMSConductorType(FTN.ConductorType conductorType)
        {
            switch (conductorType)
            {
                case FTN.ConductorType.BusSegment:
                    return ConductorType.BusSegment;
                case FTN.ConductorType.Cable:
                    return ConductorType.Cable;
                case FTN.ConductorType.OhCable:
                    return ConductorType.OhCable;
                case FTN.ConductorType.Overhead:
                    return ConductorType.Overhead;
                default:
                    return ConductorType.None;
            }
        }

        public static EnergySourceType GetDMSEnergySourceType(FTN.EnergySourceType sourceType)
        {
            switch (sourceType)
            {
                case FTN.EnergySourceType.Distribution:
                    return EnergySourceType.Distribution;
                case FTN.EnergySourceType.Interconnection:
                    return EnergySourceType.Interconnection;
                default:
                    return EnergySourceType.None;
            }
        }

        #endregion Enums convert
    }
}
