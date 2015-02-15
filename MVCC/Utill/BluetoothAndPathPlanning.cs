using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.IO.Ports;

using MVCC.Model;

namespace MVCC.Utill
{
    class BluetoothAndPathPlanning
    {
        private UGV ugv;
        private State state;

        private Globals globals = Globals.Instance;
                
        public void connect(UGV ugv, State state)
        {
            this.ugv = ugv;
            this.state = state;
   
            //bluetooth 연결쓰레드
            BackgroundWorker thread = new BackgroundWorker();
            thread.DoWork += bluetoothConnect;
            thread.RunWorkerAsync();
        }

        private void bluetoothConnect(object sender, DoWorkEventArgs e)
        {
            string write_data = ugv.Command;
  
            int index;
            int.TryParse(ugv.Id[1].ToString(), out index);

            SerialPort serialport = new SerialPort();

            UGV settingUGV = globals.UGVSettingDictionary[convertId(ugv.Id)];

            serialport.PortName = settingUGV.ComPort;
            serialport.BaudRate = settingUGV.Baudrate;
            serialport.DataBits = settingUGV.Databit;
            serialport.StopBits = getStopBit(settingUGV.Stopbit);
            serialport.ReadTimeout = 200;
            serialport.WriteTimeout = 200;

            serialport.Open();
          
            if (serialport.IsOpen)
            {
                ugv.IsBluetoothConnected = true;
                state.BluetoothOnOff = true;

                if (write_data.Equals("f"))
                {
                    serialport.WriteLine((write_data[0]).ToString());

                    try
                    {
                        #region Transmit_Movement_Command

                        serialport.WriteLine(globals.direction[index].ToString());

                        for (int i = 0; i < ugv.MovementCommandList.Count; i++)
                        {
                            serialport.WriteLine(ugv.MovementCommandList[i]);
                        }
                        serialport.WriteLine("e");

                        Console.WriteLine("TX Complete");
                        #endregion
                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine("TimeOutException");

                        Console.Write("Buffer : ");
                        Console.WriteLine(serialport.ReadExisting());
                    }

                    disConnect(serialport);
                }

                else if (write_data == "g")
                {
                    try
                    {

                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine("TimeOutException");

                        Console.Write("Buffer : ");
                        Console.WriteLine(serialport.ReadExisting());
                    }
                }
                else if (write_data == "i")
                {

                }

                else if (write_data == "q")
                {
                    disConnect(serialport);
                }
                Console.Out.Flush();
            }
        }

        private void disConnect(SerialPort serialport)
        {
            serialport.Close();
            ugv.IsBluetoothConnected = false;
            state.BluetoothOnOff = false;
        }

        private StopBits getStopBit(int bit)
        {
            if (bit == 0)
                return StopBits.None;
            else if (bit == 1)
                return StopBits.One;

            return StopBits.One;
        }

        private string convertId(string id)
        {
            switch (id)
            {
                case "A0":
                    return "Vehicle 0";
                case "A1":
                    return "Vehicle 1";
                case "A2":
                    return "Vehicle 2";
                case "A3":
                    return "Vehicle 3";

                default:
                    return "Vehicle 0";
            }
        }
    }
}
