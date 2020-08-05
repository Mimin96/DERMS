using System;
using System.Collections.Generic;
using System.IO;
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
using FTN.Common;
using FTN.Services.NetworkModelService.TestClient;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<ModelCode> izabraniProperty = new List<ModelCode>();
        Dictionary<ModelCode, List<ModelCode>> listeReferenci = new Dictionary<ModelCode, List<ModelCode>>();
        public MainWindow()
        {
            InitializeComponent();
            InitializeCB();
        }

        private void InitializeCB()
        {
            Dictionary<long, ModelCode> l = Program.t.GetAllGids();
            l = l.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            cb1.ItemsSource = l;
            cb3.ItemsSource = l;

            List<ModelCode> tipovi = new List<ModelCode>() { ModelCode.BREAKER, ModelCode.ACLINESEGMENT, ModelCode.TERMINAL, ModelCode.ANALOG, ModelCode.DISCRETE, ModelCode.SUBGEOGRAPHICALREGION, ModelCode.GEOGRAPHICALREGION, ModelCode.SUBSTATION, ModelCode.ENERGYSOURCE, ModelCode.ENERGYCONSUMER, ModelCode.CONNECTIVITYNODE, ModelCode.SYNCHRONOUSMACHINE };
            cb2.ItemsSource = tipovi;

            cb12.IsEnabled = false;
            cb22.IsEnabled = false;
            cb32.IsEnabled = false;
            cb33.IsEnabled = false;
            cb34.IsEnabled = false;
            cb33.Text = "";

            InitializeListuRef();
        }

        private void InitializeListuRef()
        {
            List<ModelCode> temp = new List<ModelCode>();

            //temp.Add(ModelCode.CURVE_CURVEDATAS);
            //listeReferenci.Add(ModelCode.CURVE, temp);
            //temp = new List<ModelCode>();

            //temp.Add(ModelCode.PSR_OUTSCHES);
            //listeReferenci.Add(ModelCode.PROTECTED_SWITCH, temp);
            //temp = new List<ModelCode>();

            //temp.Add(ModelCode.IRREGULARINTSHCEDULE_TIME_POINTS);          
            //listeReferenci.Add(ModelCode.OUTAGESHCEDULE, temp);
            //temp = new List<ModelCode>();

            //temp.Add(ModelCode.REGULARINTSCHEDULE_TIMEPOINTS);
            //listeReferenci.Add(ModelCode.REGULARINTSCHEDULE, temp);
            //temp = new List<ModelCode>();

            //listeReferenci.Add(ModelCode.CURVEDATA, temp);
            //temp = new List<ModelCode>();

            //listeReferenci.Add(ModelCode.IRREGULARTIMEPOINT, temp);
            //temp = new List<ModelCode>();

            //listeReferenci.Add(ModelCode.REGULARTIMEPOINT, temp);
            //temp = new List<ModelCode>();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            KeyValuePair<long, ModelCode> kvp = (KeyValuePair<long, ModelCode>)cb1.SelectedValue;

            cb12.ItemsSource = null;
            cb12.ItemsSource = Program.t.GetAllProperty(kvp.Value);
            cb12.IsEnabled = true;
            lb1.ItemsSource = null;
            izabraniProperty = new List<ModelCode>();

            tb1.Text = kvp.Key.ToString("x");
        }

        private void Cb12_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb12.SelectedValue != null)
            {
                if (!izabraniProperty.Contains((ModelCode)cb12.SelectedValue))
                {
                    izabraniProperty.Add((ModelCode)cb12.SelectedValue);
                }
            }

            lb1.ItemsSource = null;
            lb1.ItemsSource = izabraniProperty;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (cb1.SelectedIndex == -1)
            {
                tb.Text = "Error";
            }
            else
            {
                KeyValuePair<long, ModelCode> kvp = (KeyValuePair<long, ModelCode>)cb1.SelectedValue;
                if (izabraniProperty.Count == 0)
                {
                    izabraniProperty = Program.t.GetAllProperty(kvp.Value);
                    Program.t.GetValues(kvp.Key, izabraniProperty);
                }
                else
                {
                    Program.t.GetValues(kvp.Key, izabraniProperty);
                }

                tb.Text = File.ReadAllText(Config.Instance.ResultDirecotry + "\\GetValues_Results.xml");
            }

            izabraniProperty = new List<ModelCode>();
            cb1.SelectedValue = null;
            cb12.ItemsSource = null;
            lb1.ItemsSource = null;
            cb12.IsEnabled = false;
            tb1.Text = "";
        }

        private void Cb2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cb22.ItemsSource = null;
            cb22.ItemsSource = Program.t.GetAllProperty((ModelCode)cb2.SelectedValue);
            cb22.IsEnabled = true;
            lb2.ItemsSource = null;
            izabraniProperty = new List<ModelCode>();
        }

        private void Cb22_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb22.SelectedValue != null)
            {
                if (!izabraniProperty.Contains((ModelCode)cb22.SelectedValue))
                {
                    izabraniProperty.Add((ModelCode)cb22.SelectedValue);
                }
            }

            lb2.ItemsSource = null;
            lb2.ItemsSource = izabraniProperty;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (cb2.SelectedIndex == -1)
            {
                tb.Text = "Error";
            }
            else
            {
                ModelCode mc = (ModelCode)cb2.SelectedValue;
                if (izabraniProperty.Count == 0)
                {
                    izabraniProperty = Program.t.GetAllProperty(mc);
                    Program.t.GetExtentValues((ModelCode)cb2.SelectedValue, izabraniProperty);
                }
                else
                {
                    Program.t.GetExtentValues((ModelCode)cb2.SelectedValue, izabraniProperty);
                }

                tb.Text = File.ReadAllText(Config.Instance.ResultDirecotry + "\\GetExtentValues_Results.xml");
            }

            izabraniProperty = new List<ModelCode>();
            cb2.SelectedValue = null;
            cb22.ItemsSource = null;
            lb2.ItemsSource = null;
            cb22.IsEnabled = false;
        }

        private void Cb3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            KeyValuePair<long, ModelCode> kvp = (KeyValuePair<long, ModelCode>)cb3.SelectedValue;
            cb32.ItemsSource = null;
            cb32.IsEnabled = true;

            cb34.ItemsSource = null;
            cb34.IsEnabled = false;

            cb32.ItemsSource = listeReferenci[kvp.Value];
            cb33.Text = "";

            lb3.ItemsSource = null;
            izabraniProperty = new List<ModelCode>();

            tb3.Text = kvp.Key.ToString("x");
        }

        private void Cb32_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb32.SelectedValue != null)
            {
                ModelCode mc = (ModelCode)cb32.SelectedValue;
                cb33.Text = "";

                switch (mc)
                {
                    //case ModelCode.PSR_OUTSCHES:
                    //    mc = ModelCode.OUTAGESHCEDULE;
                    //    break;
                    //case ModelCode.IRREGULARINTSHCEDULE_TIME_POINTS:
                    //    mc = ModelCode.IRREGULARTIMEPOINT;
                    //    break;
                    //case ModelCode.CURVE_CURVEDATAS:
                    //    mc = ModelCode.CURVEDATA;
                    //    break;
                    //case ModelCode.REGULARINTSCHEDULE_TIMEPOINTS:
                    //    mc = ModelCode.REGULARTIMEPOINT;
                    //    break;
                }
                cb33.Text = mc.ToString();
                cb34.IsEnabled = true;
                cb34.ItemsSource = Program.t.GetAllProperty(mc);
            }
        }

        private void Cb34_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb34.SelectedValue != null)
            {
                if (!izabraniProperty.Contains((ModelCode)cb34.SelectedValue))
                {
                    izabraniProperty.Add((ModelCode)cb34.SelectedValue);
                }
            }

            lb3.ItemsSource = null;
            lb3.ItemsSource = izabraniProperty;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (cb3.SelectedIndex == -1 || cb32.SelectedIndex == -1)
            {
                tb.Text = "Error";
            }
            else
            {
                KeyValuePair<long, ModelCode> kvp = (KeyValuePair<long, ModelCode>)cb3.SelectedValue;

                Association a = new Association();
                a.PropertyId = (ModelCode)cb32.SelectedValue;
                ModelCode mc;
                ModelCodeHelper.GetModelCodeFromString(cb33.Text, out mc);
                a.Type = mc;

                if (izabraniProperty.Count == 0)
                {
                    izabraniProperty = Program.t.GetAllProperty(mc);
                    Program.t.GetRelatedValues(kvp.Key, a, izabraniProperty);
                }
                else
                {
                    Program.t.GetRelatedValues(kvp.Key, a, izabraniProperty);
                }

                tb.Text = File.ReadAllText(Config.Instance.ResultDirecotry + "\\GetRelatedValues_Results.xml");
            }

            izabraniProperty = new List<ModelCode>();
            cb3.SelectedValue = null;
            cb32.ItemsSource = null;
            lb3.ItemsSource = null;
            cb32.IsEnabled = false;
            tb3.Text = "";
        }
    }
}
