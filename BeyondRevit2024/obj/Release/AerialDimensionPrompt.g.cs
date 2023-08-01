﻿#pragma checksum "AerialDimensionPrompt.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "3F3B45B45F3BCDCB36B065D93ABD757A076EE3A722620396C8ECB35B6FF0865E"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using BeyondRevit.UI;
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


namespace BeyondRevit.UI {
    
    
    /// <summary>
    /// AerialDimensionPrompt
    /// </summary>
    public partial class AerialDimensionPrompt : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        /// <summary>
        /// dimension_box Name Field
        /// </summary>
        
        #line 36 "AerialDimensionPrompt.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public System.Windows.Controls.TextBox dimension_box;
        
        #line default
        #line hidden
        
        /// <summary>
        /// pixel_box Name Field
        /// </summary>
        
        #line 55 "AerialDimensionPrompt.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public System.Windows.Controls.TextBox pixel_box;
        
        #line default
        #line hidden
        
        /// <summary>
        /// X_Coordinate Name Field
        /// </summary>
        
        #line 69 "AerialDimensionPrompt.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public System.Windows.Controls.TextBox X_Coordinate;
        
        #line default
        #line hidden
        
        /// <summary>
        /// Y_Coordinate Name Field
        /// </summary>
        
        #line 83 "AerialDimensionPrompt.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public System.Windows.Controls.TextBox Y_Coordinate;
        
        #line default
        #line hidden
        
        
        #line 99 "AerialDimensionPrompt.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox CheckBox3D;
        
        #line default
        #line hidden
        
        
        #line 100 "AerialDimensionPrompt.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox CheckBoxRaster;
        
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
            System.Uri resourceLocater = new System.Uri("/BeyondRevit2024;component/ui/aerialdimension/aerialdimensionprompt.xaml", System.UriKind.Relative);
            
            #line 1 "AerialDimensionPrompt.xaml"
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
            
            #line 35 "AerialDimensionPrompt.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Button_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.dimension_box = ((System.Windows.Controls.TextBox)(target));
            
            #line 45 "AerialDimensionPrompt.xaml"
            this.dimension_box.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.textbox_TextChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 54 "AerialDimensionPrompt.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.PickPointButton_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.pixel_box = ((System.Windows.Controls.TextBox)(target));
            
            #line 64 "AerialDimensionPrompt.xaml"
            this.pixel_box.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.textbox_TextChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.X_Coordinate = ((System.Windows.Controls.TextBox)(target));
            
            #line 78 "AerialDimensionPrompt.xaml"
            this.X_Coordinate.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.textbox_TextChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.Y_Coordinate = ((System.Windows.Controls.TextBox)(target));
            return;
            case 7:
            this.CheckBox3D = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 8:
            this.CheckBoxRaster = ((System.Windows.Controls.CheckBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
