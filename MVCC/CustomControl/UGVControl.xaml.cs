﻿using System;
using System.Collections.Generic;
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

namespace MVCC.CustomControl
{
    /// <summary>
    /// UGVControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UGVControl : UserControl
    {
        public UGVControl()
        {
            InitializeComponent();
        }

        private void ellipse_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
           // Canvas.SetLeft(ellipse, Canvas.GetLeft(ellipse) + e.DeltaManipulation.Translation.X);
            //Canvas.SetTop(ellipse, Canvas.GetTop(ellipse) + e.DeltaManipulation.Translation.Y);
        }
    }
}