﻿#pragma checksum "..\..\..\View\GISUserControl.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "0A19CA99489DAB86166F91AF54E7307B8151C06F2B8C51CDA1B3E8B4DC818D35"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Maps.MapControl.WPF;
using Microsoft.Xaml.Behaviors;
using Microsoft.Xaml.Behaviors.Core;
using Microsoft.Xaml.Behaviors.Input;
using Microsoft.Xaml.Behaviors.Layout;
using Microsoft.Xaml.Behaviors.Media;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace UI.View {
    
    
    /// <summary>
    /// GISUserControl
    /// </summary>
    public partial class GISUserControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 12 "..\..\..\View\GISUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Microsoft.Maps.MapControl.WPF.Map Map;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\..\View\GISUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox EnergySource;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\..\View\GISUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox SolarPanel;
        
        #line default
        #line hidden
        
        
        #line 46 "..\..\..\View\GISUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox WindTurbine;
        
        #line default
        #line hidden
        
        
        #line 55 "..\..\..\View\GISUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox EnergyConsumer;
        
        #line default
        #line hidden
        
        
        #line 64 "..\..\..\View\GISUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox DERBlue;
        
        #line default
        #line hidden
        
        
        #line 73 "..\..\..\View\GISUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox DERGreen;
        
        #line default
        #line hidden
        
        
        #line 82 "..\..\..\View\GISUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox DERRed;
        
        #line default
        #line hidden
        
        
        #line 91 "..\..\..\View\GISUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox LineBlue;
        
        #line default
        #line hidden
        
        
        #line 100 "..\..\..\View\GISUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox LineGreen;
        
        #line default
        #line hidden
        
        
        #line 111 "..\..\..\View\GISUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox LineRed;
        
        #line default
        #line hidden
        
        
        #line 124 "..\..\..\View\GISUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox GISTextBlock;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/UI;component/view/gisusercontrol.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\View\GISUserControl.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.Map = ((Microsoft.Maps.MapControl.WPF.Map)(target));
            return;
            case 2:
            this.EnergySource = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 3:
            this.SolarPanel = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 4:
            this.WindTurbine = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 5:
            this.EnergyConsumer = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 6:
            this.DERBlue = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 7:
            this.DERGreen = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 8:
            this.DERRed = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 9:
            this.LineBlue = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 10:
            this.LineGreen = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 11:
            this.LineRed = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 12:
            this.GISTextBlock = ((System.Windows.Controls.TextBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

