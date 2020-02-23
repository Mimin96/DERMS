using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Resources;
using Microsoft.Maps.MapControl.WPF;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Documents;
using DERMSCommon.UIModel;
using DERMSCommon;

namespace UI.ViewModel
{
    public class GISUserControlViewModel : BindableBase
    {
        #region Variables
        private TextBlock _gisTextBlock;
        private Map _map;
        #endregion

        public GISUserControlViewModel(Map map, TextBlock gisTextBlock)
        {
            _map = map;
            _gisTextBlock = gisTextBlock;
        }

        #region Properties
        public TreeNode<NodeData> Tree { get; set; }
        #endregion

        #region Public Methods
        public void GetCoordinatesOnMouseClick(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition((UIElement)sender);
            Location pinLocation = _map.ViewportPointToLocation(mousePosition);

            // unutar Name propertija staviti gid od elementa kak obi posle mogao da se pronadje i pin i linija

            // pushpin
            Pushpin pushpin = new Pushpin();
            pushpin.Location = pinLocation;
            pushpin.ToolTip = "Hello there";
            pushpin.Cursor = Cursors.Hand;
            pushpin.Template = (ControlTemplate)Application.Current.Resources["WindTurbineTemplate"];

            //polygon
            MapPolygon polygon = new MapPolygon();
            polygon.ToolTip = "Polygon hello";
            polygon.Stroke = new SolidColorBrush(Colors.Green);
            polygon.StrokeThickness = 2;
            polygon.Opacity = 0.9;
            polygon.Cursor = Cursors.Hand;
            polygon.Locations = new LocationCollection() {
        new Location(pinLocation.Latitude, pinLocation.Longitude),
        new Location(pinLocation.Latitude - 0.3, pinLocation.Longitude + 0.4),};

            // Adds to the map.
            _map.Children.Add(pushpin);
            _map.Children.Add(polygon);
        }
        public void OnMouseClick(object sender, MouseButtonEventArgs e)
        {
            // ovde getujemo element na koji je kliknuto pogledati u Name da se vidi koji mu je  gid
            UIElement iElement;
            UIElementCollection ee = ((Map)sender).Children;
            
            foreach(UIElement uIElement in ee)
            {
                if (uIElement.IsMouseOver)
                {
                    iElement = uIElement;
                }
            }

            //dobaviti informacije o kliknutom elementu i ispisati ih u odredjenom formatu
            _gisTextBlock.Text = String.Empty;
            _gisTextBlock.Inlines.Add(new Bold(new Run("Hello")));
        }
        #endregion
    }
}
