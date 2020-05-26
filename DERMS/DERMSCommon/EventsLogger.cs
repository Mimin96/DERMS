using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon { 

        public class EventsLogger
        {
            private string path;

            public EventsLogger()
            {
                path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, @"SmartCache\Events.dat");
            }

        public void WriteToFile(Event e)
        {

            string eventmess = e.Message + "  " + e.Component + "  " + e.DateTime + "\n";

            using (StreamWriter w = File.AppendText(path))
            {
                w.Write(eventmess);
            }

        }
    }
    //public void WriteToFile(Event e)
    //{

    //    using (var fs = new FileStream(path, FileMode.OpenOrCreate))
    //    {
    //        using (var w = new StreamWriter(fs))
    //        {
    //        //var bw = new BinaryFormatter();
    //        //bw.Serialize(fs, e);

    //            w.Write(e.DateTime);
    //        }
    //    }
    //}

    //public Dictionary<long, List<DataPoint>> ReadFromFile()
    //{
    //    using (var fs = new FileStream(path, FileMode.Open))
    //    {
    //        var bw = new BinaryFormatter();
    //        return (Dictionary<long, List<DataPoint>>)bw.Deserialize(fs);
    //    }
    //}

    //public void DeleteSmartCache()
    //{
    //    try
    //    {
    //        if (File.Exists(path))
    //        {
    //            File.Delete(path);
    //        }
    //    }
    //    catch (IOException ioExp)
    //    {
    //        Console.WriteLine(ioExp.Message);
    //    }
    //}
}
   

