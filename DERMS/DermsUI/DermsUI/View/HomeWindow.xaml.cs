using DermsUI.Model;
using DermsUI.ViewModel;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using DermsUI.Model;
using DermsUI.ViewModel;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.ObjectModel;
using GMap.NET.WindowsForms.ToolTips;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Drawing;

namespace DermsUI.View
{
    /// <summary>
    /// Interaction logic for HomeWindow.xaml
    /// </summary>
    public partial class HomeWindow : MetroWindow
    {

        //Region*, SubRegion*, ACLineSegment*, Substation, Breaker, EnergySource, EnergyConsumer, Generator to mi sve treba

        public string filePath = System.IO.Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, @"DermsElements.xml");
        //public string filePath = @"C:\Users\Mimin\Desktop\Master\5.2.2020\DERMS\DERMS\DermsElements.xml";
        public static ObservableCollection<SubstationEntity> entitiesSustation = new ObservableCollection<SubstationEntity>();
        public static ObservableCollection<Breakers> entitiesBreaker = new ObservableCollection<Breakers>();
        public static ObservableCollection<EnergySources> entitiesEnergySource = new ObservableCollection<EnergySources>();
        public static ObservableCollection<EnergyConsumers> entitiesEnergyConsumer = new ObservableCollection<EnergyConsumers>();
        public static ObservableCollection<Generator> entitiesGenerator = new ObservableCollection<Generator>();

        public HomeWindow()
        {
            InitializeComponent();

            ParseAll(filePath);

            GMapp.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            GMapp.ShowCenter = true;

            GMapp.DragButton = System.Windows.Forms.MouseButtons.Left;
            GMapp.Position = new PointLatLng(45.2516700, 19.8369400);

        }

        private void CheckBoxEntity()
        {
            GMapp.Overlays.Clear();

            if (CheckBoxAllEntity.IsChecked == true) //subs
            {
                GMapp.Refresh();
                GMapp.ReloadMap();
                GMapp.Show();

                Substations();
                Generator();
                Breakers();
                EnergyConsumers();
                EnergySources();

            }

            if (CheckBoxBreakerEntity.IsChecked == true)
            {
                GMapp.Refresh();
                GMapp.ReloadMap();
                GMapp.Show();
                Breakers();
            }

            if (CheckBoxEnergyConsumerEntity.IsChecked == true)
            {
                GMapp.Refresh();
                GMapp.ReloadMap();
                GMapp.Show();
                EnergyConsumers();
            }

            if (CheckBoxEnergySourceEntity.IsChecked == true)
            {
                GMapp.Refresh();
                GMapp.ReloadMap();
                GMapp.Show();
                EnergySources();
            }

            if (CheckBoxGeneratorEntity.IsChecked == true)
            {
                GMapp.Refresh();
                GMapp.ReloadMap();
                GMapp.Show();
                Generator();
            }


            if (CheckBoxSubstitutionEntuty.IsChecked == true)
            {
                GMapp.Refresh();
                GMapp.ReloadMap();
                GMapp.Show();
                Substations();
            }

        }

        private void Generator()
        {
            foreach (Generator g in entitiesGenerator)
            {
                GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(g.X, g.Y), GMarkerGoogleType.green_dot); //moze i icon ako treba sa Bitmap
                GMapOverlay markersOverlay1 = new GMapOverlay("markers");
                this.GMapp.Overlays.Add(markersOverlay1);
                markersOverlay1.Markers.Add(marker);
            }
        }

        private void EnergyConsumers()
        {
            foreach (EnergyConsumers se in entitiesEnergyConsumer)
            {
                GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(se.X, se.Y), GMarkerGoogleType.white_small); //moze i icon ako treba sa Bitmap
                GMapOverlay markersOverlay1 = new GMapOverlay("markers");
                this.GMapp.Overlays.Add(markersOverlay1);
                markersOverlay1.Markers.Add(marker);
            }
        }

        private void EnergySources()
        {
            foreach (EnergySources se in entitiesEnergySource)
            {
                GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(se.X, se.Y), GMarkerGoogleType.black_small); //moze i icon ako treba sa Bitmap
                GMapOverlay markersOverlay1 = new GMapOverlay("markers");
                this.GMapp.Overlays.Add(markersOverlay1);
                markersOverlay1.Markers.Add(marker);
            }
        }

        private void Breakers()
        {
            foreach (Breakers se in entitiesBreaker)
            {
                GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(se.X, se.Y), GMarkerGoogleType.blue); //moze i icon ako treba sa Bitmap
                GMapOverlay markersOverlay1 = new GMapOverlay("markers");
                this.GMapp.Overlays.Add(markersOverlay1);
                markersOverlay1.Markers.Add(marker);
            }
        }

        private void Substations()
        {
            foreach (SubstationEntity se in entitiesSustation)
            {
                GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(se.X, se.Y), GMarkerGoogleType.red); //moze i icon ako treba sa Bitmap
                GMapOverlay markersOverlay1 = new GMapOverlay("markers");
                this.GMapp.Overlays.Add(markersOverlay1);
                markersOverlay1.Markers.Add(marker);
            }
        }

        public void ParseAll(string filePath)
        {
            XElement xelement = XElement.Load(filePath);
            IEnumerable<XElement> netModElements = xelement.Elements();

            double lat, lon;
            XmlSerializer serializer = null;

            // Read the entire XML
            foreach (var netModElement in netModElements)
            {
                foreach (var entity in netModElement.Elements())
                {
                    var temp = entity.ToString();

                    using (TextReader reader = new StringReader(temp))
                    {
                        SubstationEntity resultSubstations = null;
                        Breakers resultBreakers = null;
                        EnergySources resultEnergySources = null;
                        EnergyConsumers resultEnergyConsumers = null;
                        Generator resultGenerators = null;

                        switch (entity.Name.ToString())
                        {
                            case "SubstationEntity":
                                serializer = new XmlSerializer(typeof(SubstationEntity));
                                resultSubstations = (SubstationEntity)serializer.Deserialize(reader);
                                ToLatLon(((SubstationEntity)resultSubstations).X, ((SubstationEntity)resultSubstations).Y, 34, out lat, out lon);
                                ((SubstationEntity)resultSubstations).X = lat;
                                ((SubstationEntity)resultSubstations).Y = lon;
                                break;
                            case "Breaker":
                                serializer = new XmlSerializer(typeof(Breakers));
                                resultBreakers = (Breakers)serializer.Deserialize(reader);
                                ToLatLon(((Breakers)resultBreakers).X, ((Breakers)resultBreakers).Y, 34, out lat, out lon);
                                ((Breakers)resultBreakers).X = lat;
                                ((Breakers)resultBreakers).Y = lon;
                                break;
                            case "EnergySource":
                                serializer = new XmlSerializer(typeof(EnergySources));
                                resultEnergySources = (EnergySources)serializer.Deserialize(reader);
                                ToLatLon(((EnergySources)resultEnergySources).X, ((EnergySources)resultEnergySources).Y, 34, out lat, out lon);
                                ((EnergySources)resultEnergySources).X = lat;
                                ((EnergySources)resultEnergySources).Y = lon;
                                break;
                            case "EnergyConsumer":
                                serializer = new XmlSerializer(typeof(EnergyConsumers));
                                resultEnergyConsumers = (EnergyConsumers)serializer.Deserialize(reader);
                                ToLatLon(((EnergyConsumers)resultEnergyConsumers).X, ((EnergyConsumers)resultEnergyConsumers).Y, 34, out lat, out lon);
                                ((EnergyConsumers)resultEnergyConsumers).X = lat;
                                ((EnergyConsumers)resultEnergyConsumers).Y = lon;
                                break;
                            case "Generator":
                                serializer = new XmlSerializer(typeof(Generator));
                                resultGenerators = (Generator)serializer.Deserialize(reader);
                                ToLatLon(((Generator)resultGenerators).X, ((Generator)resultGenerators).Y, 34, out lat, out lon);
                                ((Generator)resultGenerators).X = lat;
                                ((Generator)resultGenerators).Y = lon;
                                break;
                        }

                        if (resultSubstations != null)
                        {
                            entitiesSustation.Add(resultSubstations);

                        }
                        else if (resultBreakers != null)
                        {
                            entitiesBreaker.Add(resultBreakers);
                        }
                        else if (resultEnergySources != null)
                        {
                            entitiesEnergySource.Add(resultEnergySources);
                        }
                        else if (resultEnergyConsumers != null)
                        {

                            entitiesEnergyConsumer.Add(resultEnergyConsumers);
                        }
                        else if (resultGenerators != null)
                        {

                            entitiesGenerator.Add(resultGenerators);
                        }
                    }
                }
            }
        }

        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }

        private void RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var sample = (SampleVm)((Border)sender).DataContext;
            var hvm = (HomeViewModel)DataContext;
            hvm.Content = (UserControl)Activator.CreateInstance(sample.Content);
            hvm.IsMenuOpen = false;
        }

        private void UIElement_OnMouseDown2(object sender, MouseButtonEventArgs e)
        {
            var sample = (SampleVm)((Border)sender).DataContext;
            var hvm = (HomeViewModel)DataContext;
            hvm.Content2 = (UserControl)Activator.CreateInstance(sample.Content);
            hvm.IsMenuOpen2 = false;
        }

        private void UIElement_OnMouseDown3(object sender, MouseButtonEventArgs e)
        {
            var sample = (SampleVm)((Border)sender).DataContext;
            var hvm = (HomeViewModel)DataContext;
            hvm.Content3 = (UserControl)Activator.CreateInstance(sample.Content);
            hvm.IsMenuOpen3 = false;
        }

        private void UIElement_OnMouseDown4(object sender, MouseButtonEventArgs e)
        {
            var sample = (SampleVm)((Border)sender).DataContext;
            var hvm = (HomeViewModel)DataContext;
            hvm.Content4 = (UserControl)Activator.CreateInstance(sample.Content);
            hvm.IsMenuOpen4 = false;
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            CheckBoxEntity();
        }

        private void OptionsButton_Copy_Click(object sender, RoutedEventArgs e)
        {
            this.GMapp.Overlays.Clear();
            GMapp.ReloadMap();
            GMapp.Refresh();

            CheckBoxSubstitutionEntuty.IsChecked = false;
            CheckBoxGeneratorEntity.IsChecked = false;
            CheckBoxAllEntity.IsChecked = false;
            CheckBoxBreakerEntity.IsChecked = false;
            CheckBoxEnergyConsumerEntity.IsChecked = false;
            CheckBoxEnergySourceEntity.IsChecked = false;

        }

        //Get clicked coordinates
        private void GMapp_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
           double lat = GMapp.FromLocalToLatLng(e.X, e.Y).Lat; 
           double lon = GMapp.FromLocalToLatLng(e.X, e.Y).Lng; 

        }

        private void DrawACLines(object sender, RoutedEventArgs e)
        {
            GMapOverlay polyOverlay = new GMapOverlay("polygons");
            List<PointLatLng> points = new List<PointLatLng>();
            points.Add(new PointLatLng(45.22867333882806, 19.895433916774838));
            points.Add(new PointLatLng(45.230883919189736, 19.813390496256019));
            points.Add(new PointLatLng(45.327444442984572, 19.87210070186676));

            GMapRoute routes = new GMapRoute(points, "mypolygon")
            {
                Stroke = new System.Drawing.Pen(System.Drawing.Color.Red, 1)
            };
            polyOverlay.Routes.Add(routes);
            GMapp.Overlays.Add(polyOverlay);
        }

    }
}
