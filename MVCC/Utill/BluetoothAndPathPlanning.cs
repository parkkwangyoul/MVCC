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

            bluetoothConnect();

            /*
            //bluetooth 연결쓰레드
            BackgroundWorker thread = new BackgroundWorker();
            thread.DoWork += bluetoothConnect;
            thread.RunWorkerAsync();
             * */
        }

        private void bluetoothConnect()
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

                        //serialport.WriteLine((8 - (globals.angle_direction[index] - globals.direction[index])).ToString());

                        int first_x = globals.first_point_x[index];
                        int first_y = globals.first_point_y[index];

                        int start_x = ((state.CurrentPointX) / 15);
                        int start_y = ((state.CurrentPointY) / 15);

                        int direction_x = ((state.CurrentPointX) / 15);
                        int direction_y = ((state.CurrentPointY) / 15);

                        Console.WriteLine("first_x {0}", globals.first_point_x[index]);
                        Console.WriteLine("first_y {0}", globals.first_point_y[index]);

                        Console.WriteLine("start_point_x {0}", start_x);
                        Console.WriteLine("start_point_y {0}", start_y);

                        Console.WriteLine("Direction Value : {0}", globals.direction[index]);

                        if (globals.direction[index] == 0)
                        {
                            direction_y = direction_y - 1;
                        }
                        else if (globals.direction[index] == 1)
                        {
                            direction_x = direction_x + 1;
                            direction_y = direction_y - 1;
                        }
                        else if (globals.direction[index] == 2)
                        {
                            direction_x = direction_x + 1;
                        }
                        else if (globals.direction[index] == 3)
                        {
                            direction_x = direction_x + 1;
                            direction_y = direction_y + 1;
                        }
                        else if (globals.direction[index] == 4)
                        {
                            direction_y = direction_y + 1;
                        }
                        else if (globals.direction[index] == 5)
                        {
                            direction_x = direction_x - 1;
                            direction_y = direction_y + 1;
                        }
                        else if (globals.direction[index] == 6)
                        {
                            direction_x = direction_x - 1;
                        }
                        else if (globals.direction[index] == 7)
                        {
                            direction_x = direction_x - 1;
                            direction_y = direction_y + 1;
                        }

                        Console.WriteLine("direction_x {0}", direction_x);
                        Console.WriteLine("direction_y {0}", direction_y);

                        if ((first_x - start_x == 0) && (first_y - start_y == -1))
                        {
                            globals.angle[index] = 0;
                        }
                        else if ((first_x - start_x == 1) && (first_y - start_y == -1))
                        {
                            globals.angle[index] = 1;
                        }
                        else if ((first_x - start_x == 1) && (first_y - start_y == 0))
                        {
                            globals.angle[index] = 2;
                        }
                        else if ((first_x - start_x == 1) && (first_y - start_y == 1))
                        {
                            globals.angle[index] = 3;
                        }
                        else if ((first_x - start_x == 0) && (first_y - start_y == 1))
                        {
                            globals.angle[index] = 4;
                        }
                        else if ((first_x - start_x == -1) && (first_y - start_y == 1))
                        {
                            globals.angle[index] = 5;
                        }
                        else if ((first_x - start_x == -1) && (first_y - start_y == 0))
                        {
                            globals.angle[index] = 6;
                        }
                        else if ((first_x - start_x == -1) && (first_y - start_y == -1))
                        {
                            globals.angle[index] = 7;
                        }

                        Console.WriteLine("Turn Value : {0}", (globals.angle[index] - globals.direction[index]).ToString() );

                        Console.WriteLine("ugv.Id = " + ugv.Id + " direction[index] = " + globals.direction[index]);

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
