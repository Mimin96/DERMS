using DarkSkyApi.Models;
using Polenter.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DERMSCommon
{
    public class DarkSkyAPISmartCache
    {
        private string path, path_SCADA;

        public DarkSkyAPISmartCache()
        {
            path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, @"DarkSkyAPIMockCE.dat");
            path_SCADA = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, @"DarkSkyAPIMockSCADA.dat");
        }

        public void WriteToFile(Dictionary<long, Forecast> e)
        {
            var serializer = new SharpSerializer(true);
            serializer.Serialize(e, path);
        }

        public Dictionary<long, Forecast> ReadFromFile()
        {
            try
            {
                var serializer = new SharpSerializer(true);

                return (Dictionary<long, Forecast>)serializer.Deserialize(path);
            }
            catch
            {
                return new Dictionary<long, Forecast>();
            }
        }

        public void WriteToFile(Dictionary<long, List<HourDataPoint>> e)
        {
            var serializer = new SharpSerializer(true);
            serializer.Serialize(e, path_SCADA);
        }

        public Dictionary<long, List<HourDataPoint>> ReadFromFileDataPoint()
        {
            try
            {
                var serializer = new SharpSerializer(true);

                return (Dictionary<long, List<HourDataPoint>>)serializer.Deserialize(path_SCADA);
            }
            catch
            {
                return new Dictionary<long, List<HourDataPoint>>();
            }
        }
    }
}
