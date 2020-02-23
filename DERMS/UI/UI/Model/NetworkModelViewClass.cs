using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace UI.Model
{
    /// <summary>
    /// Used in NetworkModel User Control
    /// </summary>
    public class NetworkModelViewClass
    {
        public Brush BrushColor { get; set; }
        public PackIconKind Kind { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }

        public NetworkModelViewClass(Brush brush, PackIconKind packIconKind, string name, string info)
        {
            BrushColor = brush;
            Kind = packIconKind;
            Name = name;
            Info = info;
        }
    }
}
