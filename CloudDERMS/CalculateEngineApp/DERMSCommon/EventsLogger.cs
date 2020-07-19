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
            List<Event> temp;
            List<Event> listOfEvents = new List<Event>();

        public EventsLogger()
            {
                path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, @"SmartCache\Events.dat");
            }

        public void WriteToFile(Event e)
        {
            temp = ReadFromFile();
            temp.Add(e);
            


            using (var fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                using (var w = new StreamWriter(fs))
                {
                    var bw = new BinaryFormatter();
                    bw.Serialize(fs, temp);
                    w.Write(temp);

                }
            }
        }

        public List<Event> ReadFromFile()
        {
            try
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    var bw = new BinaryFormatter();
                    return (List<Event>)bw.Deserialize(fs);
                }
            }
            catch
            {
                return new List<Event>();
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
   

