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

namespace MVCC.CustomControl
{
    /// <summary>
    /// BluetoothControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BluetoothControl : UserControl
    {
        Globals globals = Globals.Instance;

        List<string> ComportList = new List<string>();

        List<int> BaudrateList = new List<int>();

        List<int> DataBitList = new List<int>();

        List<int> StopBitList = new List<int>();

        public BluetoothControl()
        {
            InitializeComponent();
            
            init();

            COMportComboBox.ItemsSource = ComportList;

            BaudrateComboBox.ItemsSource = BaudrateList;

            DataBitComboBox.ItemsSource = DataBitList;

            StopBitComboBox.ItemsSource = StopBitList;         
        }

        private void setCOMPort(object sender, SelectionChangedEventArgs e)
        {
            if (globals.UGVSettingDictionary.FirstOrDefault(x => x.Key.Equals(SettingId.Text)).Value == null)
            {
                globals.UGVSettingDictionary.Add(SettingId.Text, new UGV(SettingId.Text));
            }

            globals.UGVSettingDictionary[SettingId.Text].ComPort = COMportComboBox.SelectedItem.ToString();

            //MessageBox.Show(globals.UGVSettingDictionary[SettingId.Text].Id + " : " + globals.UGVSettingDictionary[SettingId.Text].ComPort);
        }
        private void setBaudrate(object sender, SelectionChangedEventArgs e)
        {
            if (globals.UGVSettingDictionary.FirstOrDefault(x => x.Key.Equals(SettingId.Text)).Value == null)
            {
                globals.UGVSettingDictionary.Add(SettingId.Text, new UGV(SettingId.Text));
            }

            int value;
            int.TryParse(BaudrateComboBox.SelectedItem.ToString(), out value);

            globals.UGVSettingDictionary[SettingId.Text].Baudrate = value;

            //MessageBox.Show(globals.UGVSettingDictionary[SettingId.Text].Id + " : " + globals.UGVSettingDictionary[SettingId.Text].ComPort);
        }
        private void setDatabit(object sender, SelectionChangedEventArgs e)
        {
            if (globals.UGVSettingDictionary.FirstOrDefault(x => x.Key.Equals(SettingId.Text)).Value == null)
            {
                globals.UGVSettingDictionary.Add(SettingId.Text, new UGV(SettingId.Text));
            }

            int value;
            int.TryParse(DataBitComboBox.SelectedItem.ToString(), out value);

            globals.UGVSettingDictionary[SettingId.Text].Databit = value;

            //MessageBox.Show(globals.UGVSettingDictionary[SettingId.Text].Id + " : " + globals.UGVSettingDictionary[SettingId.Text].ComPort);
        }
        private void setStopbit(object sender, SelectionChangedEventArgs e)
        {
            if (globals.UGVSettingDictionary.FirstOrDefault(x => x.Key.Equals(SettingId.Text)).Value == null)
            {
                globals.UGVSettingDictionary.Add(SettingId.Text, new UGV(SettingId.Text));
            }

            int value;
            int.TryParse(StopBitComboBox.SelectedItem.ToString(), out value);

            globals.UGVSettingDictionary[SettingId.Text].Stopbit = value;

            //MessageBox.Show(globals.UGVSettingDictionary[SettingId.Text].Id + " : " + globals.UGVSettingDictionary[SettingId.Text].ComPort);
        }

        public void init()
        {
            #region ComportList init

            for (int i = 0; i < 10; i++)
            {
                ComportList.Add("COM" + (i + 1));
            }

            #endregion ComportList init

            #region BaudrateList init

            initBaudrate();

            #endregion BaudrateList init

            #region DataBitList init

            DataBitList.Add(8);
            DataBitList.Add(9);

            #endregion DataBitList init

            #region StopBitList init

            StopBitList.Add(0);
            StopBitList.Add(1);

            #endregion StopBitList init
        }

        public void initBaudrate()
        {
            BaudrateList.Add(75);
            BaudrateList.Add(110);
            BaudrateList.Add(134);
            BaudrateList.Add(150);
            BaudrateList.Add(300);
            BaudrateList.Add(600);
            BaudrateList.Add(1200);
            BaudrateList.Add(1800);
            BaudrateList.Add(2400);
            BaudrateList.Add(4800);
            BaudrateList.Add(7200);
            BaudrateList.Add(9600);
            BaudrateList.Add(14400);
            BaudrateList.Add(19200);
            BaudrateList.Add(38900);
            BaudrateList.Add(57600);
            BaudrateList.Add(115200);
            BaudrateList.Add(128000);
        }

    }
}
