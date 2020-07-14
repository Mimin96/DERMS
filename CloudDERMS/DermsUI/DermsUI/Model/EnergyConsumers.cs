using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DermsUI.Model
{
    [XmlRoot("EnergyConsumer")]
    public class EnergyConsumers
    {
        private string id;
        private string name;
        private double x;
        private double y;

        public EnergyConsumers()
        {

        }

        [XmlElement("Id")]
        public string Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
            }
        }

        [XmlElement("Name")]
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        [XmlElement("X")]
        public double X
        {
            get
            {
                return x;
            }

            set
            {
                x = value;
            }
        }

        [XmlElement("Y")]
        public double Y
        {
            get
            {
                return y;
            }

            set
            {
                y = value;
            }
        }
    }

}
