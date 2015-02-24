using System;
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

using MVCC.Model;
using MVCC.Utill;

namespace MVCC.CustomControl
{
    /// <summary>
    /// StateControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class StateControl : UserControl
    {
        private State state;

        private BluetoothAndPathPlanning bluetoothAndPathPlanning = new BluetoothAndPathPlanning();

        public StateControl()
        {
            InitializeComponent();
        }

        private void StopUGV(object sender, MouseButtonEventArgs e)
        {
            state = DataContext as State;

            if (state.IsDriving)
            {
                state.ugv.Command = "q";

                bluetoothAndPathPlanning.connect(state.ugv, state);
            }
        }
    }
}
