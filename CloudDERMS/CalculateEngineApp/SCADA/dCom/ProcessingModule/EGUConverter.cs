using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessingModule
{
	public static class EGUConverter
	{
		public static double ConvertToEGU(double scalingFactor, double deviation, ushort rawValue)
		{
            return rawValue * scalingFactor + deviation; // ovde izmenio

        }

		public static ushort ConvertToRaw(double scalingFactor, double deviation, double eguValue)
		{
            return (ushort)((eguValue - deviation) / scalingFactor); // ovde izmenio

        }
	}
}
