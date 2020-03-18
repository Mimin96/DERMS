using CalculationEngineServiceCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dCom.Configuration
{
	public class SendListOfGeneratorsToScada : ISendListOfGeneratorsToScada
	{
		public void SendListOfGenerators(Dictionary<long, double> generators)
		{
			// LISTA GENERATORA KOJI SU PROMENILI FLEXIBILITY
		}
	}
}
