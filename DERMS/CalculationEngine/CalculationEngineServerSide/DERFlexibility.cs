﻿using DERMSCommon.DataModel.Core;
using DERMSCommon.NMSCommuication;
using DERMSCommon.WeatherForecast;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineService
{
    public class DERFlexibility
    {
        private NetworkModelTransfer networkModel;
        private List<long> generatorsForOverclock = new List<long>();
        private Dictionary<long, bool> stateOfGenerator = new Dictionary<long, bool>();

		public DERFlexibility() { }

		public DERFlexibility(NetworkModelTransfer networkModel)
        {
            this.networkModel = networkModel;
        }

		public bool CheckFlexibility(long gid)
		{
			bool flexibility = true;
			List<Generator> allGenerators = new List<Generator>();
			allGenerators = GetGenerators();

			foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
			{
				foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
				{
					if (kvpDic.Key.Equals(gid))
					{
						var type = kvpDic.Value.GetType();
						if (type.Name.Equals("Substation"))
						{
							var gr = (Substation)kvpDic.Value;
							foreach (KeyValuePair<long, bool> kvpGenerator in stateOfGenerator)
							{
								if (gr.Equipments.Contains(kvpGenerator.Key) && !kvpGenerator.Value)  // <- umesto flexibility treba generator.flexibility
								{
									generatorsForOverclock.Add(kvpGenerator.Key);
									flexibility = true;
									break;
								}
							}
						}
						else if (type.Name.Equals("GeographicalRegion"))
						{
							var gr = (GeographicalRegion)kvpDic.Value;
							foreach (long s in gr.Regions)
							{
								foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp1 in networkModel.Insert)
								{
									foreach (KeyValuePair<long, IdentifiedObject> kvpDic1 in kvp.Value)
									{
										if (kvpDic1.Key.Equals(s))
										{
											SubGeographicalRegion sgr = (SubGeographicalRegion)kvpDic1.Value;
											foreach (long sub in sgr.Substations)
											{
												foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp2 in networkModel.Insert)
												{
													foreach (KeyValuePair<long, IdentifiedObject> kvpDic2 in kvp.Value)
													{
														Substation substation = (Substation)kvpDic2.Value;
														foreach (KeyValuePair<long, bool> kvpGenerator in stateOfGenerator)
														{
															if (substation.Equipments.Contains(kvpGenerator.Key) && !kvpGenerator.Value)  // <- umesto flexibility treba generator.flexibility
															{
																generatorsForOverclock.Add(kvpGenerator.Key);
																flexibility = true;
																break;
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
						else if (type.Name.Equals("SubGeographicalRegion"))
						{
							SubGeographicalRegion sgr = (SubGeographicalRegion)kvpDic.Value;
							foreach (long sub in sgr.Substations)
							{
								foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp2 in networkModel.Insert)
								{
									foreach (KeyValuePair<long, IdentifiedObject> kvpDic2 in kvp.Value)
									{
										Substation substation = (Substation)kvpDic2.Value;
										foreach (KeyValuePair<long, bool> kvpGenerator in stateOfGenerator)
										{
											if (substation.Equipments.Contains(kvpGenerator.Key) && !kvpGenerator.Value)  // <- umesto flexibility treba generator.flexibility
											{
												generatorsForOverclock.Add(kvpGenerator.Key);
												flexibility = true;
												break;
											}
										}
									}
								}
							}
						}
						foreach (long gen in generatorsForOverclock)
						{
							stateOfGenerator[gen] = !stateOfGenerator[gen];
						}
					}
				}
			}

			return flexibility;
		}

		public void TurnOnFlexibility(double flexibilityValue, Dictionary<long, DerForecastDayAhead> derForcast, long gid)
		{
			foreach (HourDataPoint datapoint in derForcast[gid].Production.Hourly)
			{
				datapoint.ActivePower += (float)flexibilityValue;
			}
		}

		public bool CheckFlexibilityForManualCommanding(long gid, Dictionary<long, IdentifiedObject> model)
		{
			bool flexibility = false;
			List<Generator> allGenerators = new List<Generator>();
			allGenerators = GetGeneratorsForManualCommand(model);
			var type = model[gid].GetType();
			if (type.Name.Equals("Substation"))
			{
				Substation substation = (Substation)model[gid];
				foreach (KeyValuePair<long, bool> kvpGenerator in stateOfGenerator)
				{
					if (substation.Equipments.Contains(kvpGenerator.Key) && !kvpGenerator.Value)  // <- umesto flexibility treba generator.flexibility
					{
						generatorsForOverclock.Add(kvpGenerator.Key);
						flexibility = true;
						break;
					}
				}
			}
			else if (type.Name.Equals("GeographicalRegion"))
			{
				GeographicalRegion gr = (GeographicalRegion)model[gid];
				foreach (long s in gr.Regions)
				{
					SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)model[s];
					foreach (long sub in subGeographicalRegion.Substations)
					{
						Substation substation = (Substation)model[sub];
						foreach (KeyValuePair<long, bool> kvpGenerator in stateOfGenerator)
						{
							if (substation.Equipments.Contains(kvpGenerator.Key) && !kvpGenerator.Value)  // <- umesto flexibility treba generator.flexibility
							{
								generatorsForOverclock.Add(kvpGenerator.Key);
								flexibility = true;
								break;
							}
						}
					}
				}
			}
			else if (type.Name.Equals("SubGeographicalRegion"))
			{
				SubGeographicalRegion sgr = (SubGeographicalRegion)model[gid];
				foreach (long sub in sgr.Substations)
				{
					Substation substation = (Substation)model[sub];
					foreach (KeyValuePair<long, bool> kvpGenerator in stateOfGenerator)
					{
						if (substation.Equipments.Contains(kvpGenerator.Key) && !kvpGenerator.Value)  // <- umesto flexibility treba generator.flexibility
						{
							generatorsForOverclock.Add(kvpGenerator.Key);
							flexibility = true;
							break;
						}
					}
				}
			}
			else if (type.Name.Equals("Generator"))
			{
				Generator generator = (Generator)model[gid];
				foreach (KeyValuePair<long, bool> kvpGenerator in stateOfGenerator)
				{
					if (generator.GlobalId.Equals(kvpGenerator.Key) && !kvpGenerator.Value)  // TurnOnFlexibilityForManualCommanding NE MOZE SE PROSLEDJIVATI GID GENERATORA
					{
						generatorsForOverclock.Add(kvpGenerator.Key);
						flexibility = true;
						break;
					}
				}
			}

			foreach (long gen in generatorsForOverclock)
			{
				stateOfGenerator[gen] = !stateOfGenerator[gen];
			}

			return flexibility;
		}

		public Dictionary<long, double> TurnOnFlexibilityForGeoRegion(double flexibilityValue, Dictionary<long, DerForecastDayAhead> derForcast, long gid, Dictionary<long, IdentifiedObject> affectedEntities)
		{
			Dictionary<long, double> ret = new Dictionary<long, double>();

			GeographicalRegion geographicalRegion = (GeographicalRegion)affectedEntities[gid];
			int numOfHour = -1;
			bool finished = false;
			if (!affectedEntities.Count.Equals(0))
			{
				if (flexibilityValue > 0)
				{
					foreach (HourDataPoint datapoint in derForcast[gid].Production.Hourly)
					{
						numOfHour++;
						finished = false;
						double productionHour = datapoint.ActivePower * (flexibilityValue / 100); // RACUNAMO KOLIKO BI TREBALA DA SE POVECA PROZIVODNJA PO SATU
						foreach (IdentifiedObject io in affectedEntities.Values)
						{
							if (!finished)
							{
								if (io.GetType().Name.Equals("SubGeographicalRegion"))
								{
									SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)io;
									foreach (long sub in subGeographicalRegion.Substations)
									{
										if (!finished)
										{
											Substation substation = (Substation)affectedEntities[sub];
											foreach (long gen in substation.Equipments)
											{
												if (affectedEntities.ContainsKey(gen))
												{
													Generator generator = (Generator)affectedEntities[gen];
													if (generator.MaxFlexibility > 0)
													{
														double genProduction = 0;
														double genProductionInPercent = 0;
														double contition = productionHour - (derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MaxFlexibility / 100));
														if (contition > 0) // POSTAVIMO PROIZVODNJU GENERATORA NA MAX I NASTAVLJAMO DALJE DA POVECAVAMO OSTALE GENERATORE
														{
															genProduction = derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MaxFlexibility / 100);
															genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
															productionHour -= genProduction;
														}
														else if (contition < 0) // POVECAMO PROIZVODNJU GENERATORA I ZADOVOLJEN JE FLEXIBILITY GEOREGIONA
														{
															genProduction = productionHour; //DOBIJEMO ZA KOLIKO KW TREBA POVECATI PROIZVODNJU ODREDJENOG GENERATORA
															genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
															productionHour -= genProduction;
															finished = true;
														}
														else // ZNACI DA JE ZADOVOLJEN FLEXIBILITY REGIONA
														{
															genProduction = derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MaxFlexibility / 100);
															genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
															productionHour -= genProduction;
															finished = true;
														}

														derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower += (float)genProduction; // POVECAMO PROIZVODNJU GENERATORA NA MAX
														derForcast[substation.GlobalId].Production.Hourly[numOfHour].ActivePower += (float)genProduction; // POVECAMO PROIZVODNJU SUBSTATIONA ZA ONOLIKO ZA KOLIKO SE POVECALA PROIZVODNJA GENERATORA
														derForcast[subGeographicalRegion.GlobalId].Production.Hourly[numOfHour].ActivePower += (float)genProduction; // POVECAMO PROIZVODNJU SUBREGIONA ZA ONOLIKO ZA KOLIKO SE POVECALA PROIZVODNJA GENERATORA
														derForcast[geographicalRegion.GlobalId].Production.Hourly[numOfHour].ActivePower += (float)genProduction; // PROVERITI KAKO SE MENJA PRODUCION GEOREGIONA KAD IMA VISE SUBREGIONA

														if (!ret.ContainsKey(generator.GlobalId) && !Double.IsNaN(genProductionInPercent))
															ret.Add(generator.GlobalId, genProductionInPercent);


														if (finished)
															break;

													}
												}

											}
										}
										else
										{
											break;
										}
									}
								}
							}
							else
							{
								break;
							}
						}
					}
				}
				else if (flexibilityValue < 0)
				{
					foreach (HourDataPoint datapoint in derForcast[gid].Production.Hourly)
					{
						numOfHour++;
						finished = false;
						double productionHour = -1 * datapoint.ActivePower * (flexibilityValue / 100); // RACUNAMO KOLIKO BI TREBALA DA SE SMANJI PROZIVODNJA PO SATU
						foreach (IdentifiedObject io in affectedEntities.Values)
						{
							if (!finished)
							{
								if (io.GetType().Name.Equals("SubGeographicalRegion"))
								{
									SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)io;
									foreach (long sub in subGeographicalRegion.Substations)
									{
										if (!finished)
										{
											Substation substation = (Substation)affectedEntities[sub];
											foreach (long gen in substation.Equipments)
											{
												if (affectedEntities.ContainsKey(gen))
												{
													Generator generator = (Generator)affectedEntities[gen];
													if (generator.MinFlexibility > 0)
													{
														double genProduction = 0;
														double genProductionInPercent = 0;
														double contition = productionHour - (derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MinFlexibility / 100));
														if (contition > 0) // POSTAVIMO PROIZVODNJU GENERATORA NA MIN I NASTAVLJAMO DALJE DA SMANJUJEMO PROIZVODNJU OSTALIH GENERATORA
														{
															genProduction = derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MinFlexibility / 100);
															genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
															productionHour -= genProduction;
														}
														else if (contition < 0) // POVECAMO PROIZVODNJU GENERATORA I ZADOVOLJEN JE FLEXIBILITY GEOREGIONA
														{
															genProduction = productionHour; //DOBIJEMO ZA KOLIKO KW TREBA POVECATI PROIZVODNJU ODREDJENOG GENERATORA
															genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
															productionHour -= genProduction;
															finished = true;
														}
														else // ZNACI DA JE ZADOVOLJEN FLEXIBILITY REGIONA
														{
															genProduction = derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MinFlexibility / 100);
															genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
															productionHour -= genProduction;
															finished = true;
														}

														derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower -= (float)genProduction; // POVECAMO PROIZVODNJU GENERATORA NA MAX
														derForcast[substation.GlobalId].Production.Hourly[numOfHour].ActivePower -= (float)genProduction; // POVECAMO PROIZVODNJU SUBSTATIONA ZA ONOLIKO ZA KOLIKO SE POVECALA PROIZVODNJA GENERATORA
														derForcast[subGeographicalRegion.GlobalId].Production.Hourly[numOfHour].ActivePower -= (float)genProduction; // POVECAMO PROIZVODNJU SUBREGIONA ZA ONOLIKO ZA KOLIKO SE POVECALA PROIZVODNJA GENERATORA
														derForcast[geographicalRegion.GlobalId].Production.Hourly[numOfHour].ActivePower -= (float)genProduction; // PROVERITI KAKO SE MENJA PRODUCION GEOREGIONA KAD IMA VISE SUBREGIONA

														if (!ret.ContainsKey(generator.GlobalId) && !Double.IsNaN(genProductionInPercent))
															ret.Add(generator.GlobalId, -1 * genProductionInPercent);

														if (finished)
															break;
													}
												}

											}
										}
										else
										{
											break;
										}
									}
								}
							}
							else
							{
								break;
							}
						}
					}
				}
			}

			return ret;

		}

		public Dictionary<long, double> TurnOnFlexibilityForSubGeoRegion(double flexibilityValue, Dictionary<long, DerForecastDayAhead> derForcast, long gid, Dictionary<long, IdentifiedObject> affectedEntities)
		{
			Dictionary<long, double> ret = new Dictionary<long, double>();

			SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)affectedEntities[gid];
			GeographicalRegion geographicalRegion = null;
			int numOfHour = -1;
			bool finished = false;
			if (!affectedEntities.Count.Equals(0))
			{
				foreach (IdentifiedObject io in affectedEntities.Values)
				{
					if (io.GetType().Name.Equals("GeographicalRegion"))
					{
						geographicalRegion = (GeographicalRegion)io;
						break;
					}
				}

				if (flexibilityValue > 0)
				{
					foreach (HourDataPoint datapoint in derForcast[gid].Production.Hourly)
					{
						numOfHour++;
						finished = false;

						double productionHour = datapoint.ActivePower * (flexibilityValue / 100); // RACUNAMO KOLIKO BI TREBALA DA SE POVECA PROZIVODNJA PO SATU
						foreach (IdentifiedObject io in affectedEntities.Values)
						{
							if (!finished)
							{
								if (io.GetType().Name.Equals("Substation"))
								{
									Substation substation = (Substation)io;
									foreach (long gen in substation.Equipments)
									{
										if (affectedEntities.ContainsKey(gen))
										{
											Generator generator = (Generator)affectedEntities[gen];
											if (generator.MaxFlexibility > 0)
											{
												double genProduction = 0;
												double genProductionInPercent = 0;
												double contition = productionHour - (derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MaxFlexibility / 100));
												if (contition > 0) // POSTAVIMO PROIZVODNJU GENERATORA NA MAX I NASTAVLJAMO DALJE DA POVECAVAMO OSTALE GENERATORE
												{
													genProduction = derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MaxFlexibility / 100);
													genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
													productionHour -= genProduction;
												}
												else if (contition < 0) // POVECAMO PROIZVODNJU GENERATORA I ZADOVOLJEN JE FLEXIBILITY GEOREGIONA
												{
													genProduction = productionHour; //DOBIJEMO ZA KOLIKO KW TREBA POVECATI PROIZVODNJU ODREDJENOG GENERATORA
													genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
													productionHour -= genProduction;
													finished = true;
												}
												else // ZNACI DA JE ZADOVOLJEN FLEXIBILITY REGIONA
												{
													genProduction = derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MaxFlexibility / 100);
													genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
													productionHour -= genProduction;
													finished = true;
												}

												derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower += (float)genProduction; // POVECAMO PROIZVODNJU GENERATORA NA MAX
												derForcast[substation.GlobalId].Production.Hourly[numOfHour].ActivePower += (float)genProduction; // POVECAMO PROIZVODNJU SUBSTATIONA ZA ONOLIKO ZA KOLIKO SE POVECALA PROIZVODNJA GENERATORA
												derForcast[subGeographicalRegion.GlobalId].Production.Hourly[numOfHour].ActivePower += (float)genProduction; // POVECAMO PROIZVODNJU SUBREGIONA ZA ONOLIKO ZA KOLIKO SE POVECALA PROIZVODNJA GENERATORA
												derForcast[geographicalRegion.GlobalId].Production.Hourly[numOfHour].ActivePower += (float)genProduction; // PROVERITI KAKO SE MENJA PRODUCION GEOREGIONA KAD IMA VISE SUBREGIONA

												if (!ret.ContainsKey(generator.GlobalId) && !Double.IsNaN(genProductionInPercent))
													ret.Add(generator.GlobalId, genProductionInPercent);

												if (finished)
													break;
											}
										}

									}
								}
							}
							else
							{
								break;
							}
						}
					}
				}
				else if (flexibilityValue < 0)
				{
					foreach (HourDataPoint datapoint in derForcast[gid].Production.Hourly)
					{
						numOfHour++;
						finished = false;

						double productionHour = -1 * datapoint.ActivePower * (flexibilityValue / 100); // RACUNAMO KOLIKO BI TREBALA DA SE POVECA PROZIVODNJA PO SATU
						foreach (IdentifiedObject io in affectedEntities.Values)
						{
							if (!finished)
							{
								if (io.GetType().Name.Equals("Substation"))
								{
									Substation substation = (Substation)io;
									foreach (long gen in substation.Equipments)
									{
										if (affectedEntities.ContainsKey(gen))
										{
											Generator generator = (Generator)affectedEntities[gen];
											if (generator.MaxFlexibility > 0)
											{
												double genProduction = 0;
												double genProductionInPercent = 0;
												double contition = productionHour - (derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MinFlexibility / 100));
												if (contition > 0) // POSTAVIMO PROIZVODNJU GENERATORA NA MIN I NASTAVLJAMO DALJE DA SMANJIMO PROIZVODNJU OSTALIH GENERATORA
												{
													genProduction = derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MinFlexibility / 100);
													genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
													productionHour -= genProduction;
												}
												else if (contition < 0) // SMANJIMO PROIZVODNJU GENERATORA I ZADOVOLJEN JE FLEXIBILITY GEOREGIONA
												{
													genProduction = productionHour; //DOBIJEMO ZA KOLIKO KW TREBA SMANJITI PROIZVODNJU ODREDJENOG GENERATORA
													genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
													productionHour -= genProduction;
													finished = true;
												}
												else // ZNACI DA JE ZADOVOLJEN FLEXIBILITY REGIONA
												{
													genProduction = derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MinFlexibility / 100);
													genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
													productionHour -= genProduction;
													finished = true;
												}

												derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower -= (float)genProduction; // SMANJIMO PROIZVODNJU GENERATORA NA MAX
												derForcast[substation.GlobalId].Production.Hourly[numOfHour].ActivePower -= (float)genProduction; // SMANJIMO PROIZVODNJU SUBSTATIONA ZA ONOLIKO ZA KOLIKO SE POVECALA PROIZVODNJA GENERATORA
												derForcast[subGeographicalRegion.GlobalId].Production.Hourly[numOfHour].ActivePower -= (float)genProduction; // SMANJIMO PROIZVODNJU SUBREGIONA ZA ONOLIKO ZA KOLIKO SE POVECALA PROIZVODNJA GENERATORA
												derForcast[geographicalRegion.GlobalId].Production.Hourly[numOfHour].ActivePower -= (float)genProduction; // SMANJIMO KAKO SE MENJA PRODUCION GEOREGIONA KAD IMA VISE SUBREGIONA

												if (!ret.ContainsKey(generator.GlobalId) && !Double.IsNaN(genProductionInPercent))
													ret.Add(generator.GlobalId, -1 * genProductionInPercent);

												if (finished)
													break;
											}
										}

									}
								}
							}
							else
							{
								break;
							}
						}
					}
				}

			}

			return ret;
		}

		public Dictionary<long, double> TurnOnFlexibilityForSubstation(double flexibilityValue, Dictionary<long, DerForecastDayAhead> derForcast, long gid, Dictionary<long, IdentifiedObject> affectedEntities)
		{
			Dictionary<long, double> ret = new Dictionary<long, double>();

			SubGeographicalRegion subGeographicalRegion = null;
			GeographicalRegion geographicalRegion = null;
			Substation substation = (Substation)affectedEntities[gid];
			int numOfHour = -1;
			bool finished = false;
			if (!affectedEntities.Count.Equals(0))
			{
				foreach (IdentifiedObject io in affectedEntities.Values)
				{
					if (io.GetType().Name.Equals("GeographicalRegion"))
					{
						geographicalRegion = (GeographicalRegion)io;
						break;
					}
				}

				foreach (IdentifiedObject io in affectedEntities.Values)
				{
					if (io.GetType().Name.Equals("SubGeographicalRegion"))
					{
						subGeographicalRegion = (SubGeographicalRegion)io;
						break;
					}
				}

				if (flexibilityValue > 0)
				{
					foreach (HourDataPoint datapoint in derForcast[gid].Production.Hourly)
					{
						numOfHour++;
						finished = false;
						double productionHour = datapoint.ActivePower * (flexibilityValue / 100); // RACUNAMO KOLIKO BI TREBALA DA SE POVECA PROZIVODNJA PO SATU
						foreach (IdentifiedObject io in affectedEntities.Values)
						{
							if (!finished)
							{
								if (io.GetType().Name.Equals("Substation"))
								{
									foreach (long gen in substation.Equipments)
									{
										if (affectedEntities.ContainsKey(gen))
										{
											Generator generator = (Generator)affectedEntities[gen];
											if (generator.MaxFlexibility > 0)
											{
												double genProduction = 0;
												double genProductionInPercent = 0;
												double contition = productionHour - (derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MaxFlexibility / 100));
												if (contition > 0) // POSTAVIMO PROIZVODNJU GENERATORA NA MAX I NASTAVLJAMO DALJE DA POVECAVAMO OSTALE GENERATORE
												{
													genProduction = derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MaxFlexibility / 100);
													genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
													productionHour -= genProduction;
												}
												else if (contition < 0) // POVECAMO PROIZVODNJU GENERATORA I ZADOVOLJEN JE FLEXIBILITY GEOREGIONA
												{
													genProduction = productionHour; //DOBIJEMO ZA KOLIKO KW TREBA POVECATI PROIZVODNJU ODREDJENOG GENERATORA
													genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
													productionHour -= genProduction;
													finished = true;
												}
												else // ZNACI DA JE ZADOVOLJEN FLEXIBILITY REGIONA
												{
													genProduction = derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MaxFlexibility / 100);
													genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
													productionHour -= genProduction;
													finished = true;
												}

												derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower += (float)genProduction; // POVECAMO PROIZVODNJU GENERATORA NA MAX
												derForcast[substation.GlobalId].Production.Hourly[numOfHour].ActivePower += (float)genProduction; // POVECAMO PROIZVODNJU SUBSTATIONA ZA ONOLIKO ZA KOLIKO SE POVECALA PROIZVODNJA GENERATORA
												derForcast[subGeographicalRegion.GlobalId].Production.Hourly[numOfHour].ActivePower += (float)genProduction; // POVECAMO PROIZVODNJU SUBREGIONA ZA ONOLIKO ZA KOLIKO SE POVECALA PROIZVODNJA GENERATORA
												derForcast[geographicalRegion.GlobalId].Production.Hourly[numOfHour].ActivePower += (float)genProduction; // PROVERITI KAKO SE MENJA PRODUCION GEOREGIONA KAD IMA VISE SUBREGIONA

												if (!ret.ContainsKey(generator.GlobalId) && !Double.IsNaN(genProductionInPercent))
													ret.Add(generator.GlobalId, genProductionInPercent);

												if (finished)
													break;
											}
										}

									}
								}
							}
							else
							{
								break;
							}
						}
					}
				}
				else if (flexibilityValue < 0)
				{
					foreach (HourDataPoint datapoint in derForcast[gid].Production.Hourly)
					{
						numOfHour++;
						finished = false;
						double productionHour = -1 * datapoint.ActivePower * (flexibilityValue / 100); // RACUNAMO KOLIKO BI TREBALA DA SE SMANJI PROZIVODNJA PO SATU
						foreach (IdentifiedObject io in affectedEntities.Values)
						{
							if (!finished)
							{
								if (io.GetType().Name.Equals("Substation"))
								{
									foreach (long gen in substation.Equipments)
									{
										if (affectedEntities.ContainsKey(gen))
										{
											Generator generator = (Generator)affectedEntities[gen];
											if (generator.MinFlexibility > 0)
											{
												double genProduction = 0;
												double genProductionInPercent = 0;
												double contition = productionHour - (derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MinFlexibility / 100));
												if (contition > 0) // POSTAVIMO PROIZVODNJU GENERATORA NA MIN I NASTAVLJAMO DALJE DA SMANJIMO PROIZVODNJU GENERATORA
												{
													genProduction = derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MinFlexibility / 100);
													genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
													productionHour -= genProduction;
												}
												else if (contition < 0) // SMANJIMO PROIZVODNJU GENERATORA I ZADOVOLJEN JE FLEXIBILITY GEOREGIONA
												{
													genProduction = productionHour; //DOBIJEMO ZA KOLIKO KW TREBA SMANJITI PROIZVODNJU ODREDJENOG GENERATORA
													genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
													productionHour -= genProduction;
													finished = true;
												}
												else // ZNACI DA JE ZADOVOLJEN FLEXIBILITY REGIONA
												{
													genProduction = derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MinFlexibility / 100);
													genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
													productionHour -= genProduction;
													finished = true;
												}

												derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower -= (float)genProduction; // SMANJIMO PROIZVODNJU GENERATORA NA MAX
												derForcast[substation.GlobalId].Production.Hourly[numOfHour].ActivePower -= (float)genProduction; // SMANJIMO PROIZVODNJU SUBSTATIONA ZA ONOLIKO ZA KOLIKO SE POVECALA PROIZVODNJA GENERATORA
												derForcast[subGeographicalRegion.GlobalId].Production.Hourly[numOfHour].ActivePower -= (float)genProduction; // SMANJIMO PROIZVODNJU SUBREGIONA ZA ONOLIKO ZA KOLIKO SE POVECALA PROIZVODNJA GENERATORA
												derForcast[geographicalRegion.GlobalId].Production.Hourly[numOfHour].ActivePower -= (float)genProduction; // SMANJIMO KAKO SE MENJA PRODUCION GEOREGIONA KAD IMA VISE SUBREGIONA

												if (!ret.ContainsKey(generator.GlobalId) && !Double.IsNaN(genProductionInPercent))
													ret.Add(generator.GlobalId, -1 * genProductionInPercent);

												if (finished)
													break;
											}
										}

									}
								}
							}
							else
							{
								break;
							}
						}
					}
				}

			}

			return ret;
		}

		public Dictionary<long, double> TurnOnFlexibilityForGenerator(double flexibilityValue, Dictionary<long, DerForecastDayAhead> derForcast, long gid, Dictionary<long, IdentifiedObject> affectedEntities)
		{
			Dictionary<long, double> ret = new Dictionary<long, double>();

			SubGeographicalRegion subGeographicalRegion = null;
			GeographicalRegion geographicalRegion = null;
			Substation substation = null;
			Generator generator = (Generator)affectedEntities[gid];
			int numOfHour = -1;
			bool finished = false;
			if (!affectedEntities.Count.Equals(0))
			{
				foreach (IdentifiedObject io in affectedEntities.Values)
				{
					if (io.GetType().Name.Equals("GeographicalRegion"))
					{
						geographicalRegion = (GeographicalRegion)io;
						break;
					}
				}

				foreach (IdentifiedObject io in affectedEntities.Values)
				{
					if (io.GetType().Name.Equals("SubGeographicalRegion"))
					{
						subGeographicalRegion = (SubGeographicalRegion)io;
						break;
					}
				}

				foreach (IdentifiedObject io in affectedEntities.Values)
				{
					if (io.GetType().Name.Equals("Substation"))
					{
						substation = (Substation)io;
						break;
					}
				}

				if (flexibilityValue > 0)
				{
					foreach (HourDataPoint datapoint in derForcast[gid].Production.Hourly)
					{
						numOfHour++;
						finished = false;
						double productionHour = datapoint.ActivePower * (flexibilityValue / 100); // RACUNAMO KOLIKO BI TREBALA DA SE POVECA PROZIVODNJA PO SATU
						foreach (IdentifiedObject io in affectedEntities.Values)
						{
							if (!finished)
							{
								if (generator.MaxFlexibility > 0)
								{
									double genProduction = 0;
									double genProductionInPercent = 0;
									double contition = productionHour - (derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MaxFlexibility / 100));
									if (contition > 0) // POSTAVIMO PROIZVODNJU GENERATORA NA MAX I NASTAVLJAMO DALJE DA POVECAVAMO OSTALE GENERATORE
									{
										genProduction = derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MaxFlexibility / 100);
										genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
										productionHour -= genProduction;
									}
									else if (contition < 0) // POVECAMO PROIZVODNJU GENERATORA I ZADOVOLJEN JE FLEXIBILITY GEOREGIONA
									{
										genProduction = productionHour; //DOBIJEMO ZA KOLIKO KW TREBA POVECATI PROIZVODNJU ODREDJENOG GENERATORA
										genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
										productionHour -= genProduction;
										finished = true;
									}
									else // ZNACI DA JE ZADOVOLJEN FLEXIBILITY REGIONA
									{
										genProduction = derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MaxFlexibility / 100);
										genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
										productionHour -= genProduction;
										finished = true;
									}

									derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower += (float)genProduction; // POVECAMO PROIZVODNJU GENERATORA NA MAX
									derForcast[substation.GlobalId].Production.Hourly[numOfHour].ActivePower += (float)genProduction; // POVECAMO PROIZVODNJU SUBSTATIONA ZA ONOLIKO ZA KOLIKO SE POVECALA PROIZVODNJA GENERATORA
									derForcast[subGeographicalRegion.GlobalId].Production.Hourly[numOfHour].ActivePower += (float)genProduction; // POVECAMO PROIZVODNJU SUBREGIONA ZA ONOLIKO ZA KOLIKO SE POVECALA PROIZVODNJA GENERATORA
									derForcast[geographicalRegion.GlobalId].Production.Hourly[numOfHour].ActivePower += (float)genProduction; // PROVERITI KAKO SE MENJA PRODUCION GEOREGIONA KAD IMA VISE SUBREGIONA

									if (!ret.ContainsKey(generator.GlobalId) && !Double.IsNaN(genProductionInPercent))
										ret.Add(generator.GlobalId, genProductionInPercent);

									if (finished)
										break;
								}
							}
							else
							{
								break;
							}
						}
					}
				}
				else if (flexibilityValue < 0)
				{
					foreach (HourDataPoint datapoint in derForcast[gid].Production.Hourly)
					{
						numOfHour++;
						finished = false;
						double productionHour = -1 * datapoint.ActivePower * (flexibilityValue / 100); // RACUNAMO KOLIKO BI TREBALA DA SE SMANJI PROZIVODNJA PO SATU
						foreach (IdentifiedObject io in affectedEntities.Values)
						{
							if (!finished)
							{
								if (generator.MinFlexibility > 0)
								{
									double genProduction = 0;
									double genProductionInPercent = 0;
									double contition = productionHour - (derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MinFlexibility / 100));
									if (contition > 0) // POSTAVIMO PROIZVODNJU GENERATORA NA MIN I NASTAVLJAMO DALJE DA SMANJIMO PROIZVODNJU GENERATORA
									{
										genProduction = derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MinFlexibility / 100);
										genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
										productionHour -= genProduction;
									}
									else if (contition < 0) // SMANJIMO PROIZVODNJU GENERATORA I ZADOVOLJEN JE FLEXIBILITY GEOREGIONA
									{
										genProduction = productionHour; //DOBIJEMO ZA KOLIKO KW TREBA SMANJITI PROIZVODNJU ODREDJENOG GENERATORA
										genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
										productionHour -= genProduction;
										finished = true;
									}
									else // ZNACI DA JE ZADOVOLJEN FLEXIBILITY REGIONA
									{
										genProduction = derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower * (generator.MinFlexibility / 100);
										genProductionInPercent = (100 * genProduction) / derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower;
										productionHour -= genProduction;
										finished = true;
									}

									derForcast[generator.GlobalId].Production.Hourly[numOfHour].ActivePower -= (float)genProduction; // SMANJIMO PROIZVODNJU GENERATORA NA MAX
									derForcast[substation.GlobalId].Production.Hourly[numOfHour].ActivePower -= (float)genProduction; // SMANJIMO PROIZVODNJU SUBSTATIONA ZA ONOLIKO ZA KOLIKO SE POVECALA PROIZVODNJA GENERATORA
									derForcast[subGeographicalRegion.GlobalId].Production.Hourly[numOfHour].ActivePower -= (float)genProduction; // SMANJIMO PROIZVODNJU SUBREGIONA ZA ONOLIKO ZA KOLIKO SE POVECALA PROIZVODNJA GENERATORA
									derForcast[geographicalRegion.GlobalId].Production.Hourly[numOfHour].ActivePower -= (float)genProduction; // SMANJIMO KAKO SE MENJA PRODUCION GEOREGIONA KAD IMA VISE SUBREGIONA

									if (!ret.ContainsKey(generator.GlobalId) && !Double.IsNaN(genProductionInPercent))
										ret.Add(generator.GlobalId, -1 * genProductionInPercent);

									if (finished)
										break;
								}
							}
							else
							{
								break;
							}
						}
					}
				}

			}

			return ret;
		}

		public List<Generator> GetGenerators()
		{
			List<Generator> generators = new List<Generator>();
			foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
			{
				foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
				{
					var type = kvpDic.Value.GetType();
					if (type.Name.Equals("Generator"))
					{
						var generator = (Generator)kvpDic.Value;
						generators.Add(generator);

						if (!stateOfGenerator.ContainsKey(generator.GlobalId))
							stateOfGenerator.Add(generator.GlobalId, false);
					}
				}
			}
			return generators;
		}

		public List<Generator> GetGeneratorsForManualCommand(Dictionary<long, IdentifiedObject> nmsModel)
		{
			List<Generator> generators = new List<Generator>();

			foreach (IdentifiedObject io in nmsModel.Values)
			{
				var type = io.GetType();
				if (type.Name.Equals("Generator"))
				{
					var generator = (Generator)io;
					generators.Add(generator);

					if (!stateOfGenerator.ContainsKey(generator.GlobalId))
						stateOfGenerator.Add(generator.GlobalId, false);
				}
			}

			return generators;
		}
	}
}
