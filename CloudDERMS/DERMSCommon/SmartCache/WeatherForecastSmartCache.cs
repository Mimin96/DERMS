using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon.SmartCache
{
    public class WeatherForecastSmartCache
    {
        private string path;

        public WeatherForecastSmartCache()
        {
            path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, @"SmartCache\WeatherForecast_SmartCache.dat");
        }
        public void WriteToFile(List<WeatherForecast.WeatherForecast> listOfWeatherForecast)
        {
            Dictionary<long, List<WeatherForecast.WeatherForecast>> dictionaryWeatherForecast = new Dictionary<long, List<WeatherForecast.WeatherForecast>>();
            List<WeatherForecast.WeatherForecast> temp = new List<WeatherForecast.WeatherForecast>();

            foreach (WeatherForecast.WeatherForecast d in listOfWeatherForecast)
            {
                if (!dictionaryWeatherForecast.ContainsKey(d.Gid))
                {
                    foreach (var d1 in listOfWeatherForecast)
                    {
                        if (d1.Gid == d.Gid)
                        {
                            temp.Add(d1);
                        }
                    }
                }

                dictionaryWeatherForecast.Add(d.Gid, temp);
                temp.Clear();
            }
            using (var fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                using (var w = new StreamWriter(fs))
                {
                    var bw = new BinaryFormatter();
                    bw.Serialize(fs, dictionaryWeatherForecast);
                    w.Write(dictionaryWeatherForecast);
                }
            }
        }

        public Dictionary<long, List<WeatherForecast.WeatherForecast>> ReadFromFile()
        {
            using (var fs = new FileStream(path, FileMode.Open))
            {
                var bw = new BinaryFormatter();
                return (Dictionary<long, List<WeatherForecast.WeatherForecast>>)bw.Deserialize(fs);
            }
        }

        public void DeleteSmartCache()
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (IOException ioExp)
            {
                Console.WriteLine(ioExp.Message);
            }

        }
    }
}
