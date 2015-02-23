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
        Dictionary<string, UGV> UGVMap = new Dictionary<string, UGV>();
        Dictionary<string, State> StateMap = new Dictionary<string, State>();

        private Globals globals = Globals.Instance;

        /**
         * UGV별 시리얼포트
         **/
        private SerialPort serialPortA0;
        private SerialPort serialPortA1;
        private SerialPort serialPortA2;
        private SerialPort serialPortA3;
                
        public void connect(UGV ugv, State state)
        {
            if (UGVMap[ugv.Id] == null)
            {
                UGVMap.Add(ugv.Id, ugv);
            }

            if (StateMap[state.ugv.Id] == null)
            {
                StateMap.Add(state.ugv.Id, state);
            }

            if (!getSerialPort(ugv).IsOpen)
            {
                List<ModelBase> modelParam = new List<ModelBase>();

                modelParam.Add(ugv);
                modelParam.Add(state);

                //bluetooth 연결쓰레드
                BackgroundWorker thread = new BackgroundWorker();
                thread.DoWork += bluetoothConnect;
                thread.RunWorkerAsync(modelParam);
            }

            bluetoothCommand(ugv, state);
        }

        private void bluetoothConnect(object sender, DoWorkEventArgs e)
        {
            List<ModelBase> param = (List<ModelBase>)e.Argument;

            UGV ugv = param[0] as UGV;
            State state = param[1] as State;

            SerialPort serialport = getSerialPort(ugv);

            UGV settingUGV = globals.UGVSettingDictionary[convertId(ugv.Id)];

            serialport.PortName = settingUGV.ComPort;
            serialport.BaudRate = settingUGV.Baudrate;
            serialport.DataBits = settingUGV.Databit;
            serialport.StopBits = getStopBit(settingUGV.Stopbit);
            serialport.ReadTimeout = 200;
            serialport.WriteTimeout = 200;

            Console.WriteLine("serialport.isopen : " + serialport.IsOpen);

            if(!serialport.IsOpen){ serialport.Open(); }
            
            while (serialport.IsOpen)
            {
                Console.WriteLine("serialport.isopen : " + serialport.IsOpen);
                
                //ugv.IsBluetoothConnected = true;
                //state.BluetoothOnOff = true;

                Console.Out.Flush();
            }
        }

        private void bluetoothCommand(UGV ugv, State state)
        {
            SerialPort serialport = getSerialPort(ugv);

            string write_data = ugv.Command;
            Console.WriteLine("ugv command : " + write_data);

            int index;
            int.TryParse(ugv.Id[1].ToString(), out index);

            if (write_data.Equals("f"))
            {
                serialport.WriteLine((write_data[0]).ToString());

                try
                {
                    #region Transmit_Movement_Command

                    int first_x = globals.first_point_x[index];
                    int first_y = globals.first_point_y[index];

                    int start_x = ((state.CurrentPointX) / 15);
                    int start_y = ((state.CurrentPointY) / 15);

                    int direction_x = ((state.CurrentPointX) / 15);
                    int direction_y = ((state.CurrentPointY) / 15);

                    //Console.WriteLine("first_x {0}", globals.first_point_x[index]);
                    //Console.WriteLine("first_y {0}", globals.first_point_y[index]);

                    //Console.WriteLine("start_point_x {0}", start_x);
                    //Console.WriteLine("start_point_y {0}", start_y);

                    //Console.WriteLine("Direction Value : {0}", globals.direction[index]);

                    #region Direction Calculation

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

                    #endregion

                    //Console.WriteLine("direction_x {0}", direction_x);
                    //Console.WriteLine("direction_y {0}", direction_y);

                    #region Angle Calculation

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
                    #endregion


                    #region Send Angle

                    if ((globals.angle[index] - globals.direction[index] == 0))
                    {
                        serialport.WriteLine("0");
                    }
                    else if ((globals.angle[index] - globals.direction[index] == 1))
                    {
                        serialport.WriteLine("1");
                    }
                    else if ((globals.angle[index] - globals.direction[index] == 2) || (globals.angle[index] - globals.direction[index] == -6))
                    {
                        serialport.WriteLine("2");
                    }
                    else if ((globals.angle[index] - globals.direction[index] == 3) || (globals.angle[index] - globals.direction[index] == -5))
                    {
                        serialport.WriteLine("3");
                    }
                    else if ((globals.angle[index] - globals.direction[index] == 4) || (globals.angle[index] - globals.direction[index] == -4))
                    {
                        serialport.WriteLine("4");
                    }
                    else if ((globals.angle[index] - globals.direction[index] == 5) || (globals.angle[index] - globals.direction[index] == -3))
                    {
                        serialport.WriteLine("5");
                    }
                    else if ((globals.angle[index] - globals.direction[index] == 6) || (globals.angle[index] - globals.direction[index] == -2))
                    {
                        serialport.WriteLine("6");
                    }
                    else if ((globals.angle[index] - globals.direction[index] == 7) || (globals.angle[index] - globals.direction[index] == -1))
                    {
                        serialport.WriteLine("7");
                    }
                    else if ((globals.angle[index] - globals.direction[index] == -7) && (globals.angle[index] - globals.direction[index] == 0))
                    {
                        serialport.WriteLine("8");
                    }
                    #endregion

                    //Console.WriteLine("ugv.Id = " + ugv.Id + " direction[index] = " + globals.direction[index]);

                    #region Transmit Movement Commands

                    for (int i = 0; i < ugv.MovementCommandList.Count; i++)
                    {
                        serialport.WriteLine(ugv.MovementCommandList[i][0].ToString());
                    }
                    serialport.WriteLine("e");

                    Console.WriteLine("TX Complete");

                    disConnect(serialport);

                    #endregion

                    #endregion
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("TimeOutException");

                    Console.Write("Buffer : ");
                    Console.WriteLine(serialport.ReadExisting());
                }

                //disConnect(serialport);
            }

            else if (write_data.Equals("q"))
            {
                serialport.WriteLine((write_data[0]).ToString());

                //disConnect(serialport);
            }
        }

        private SerialPort getSerialPort(UGV ugv)
        {
            switch (ugv.Id)
            {
                case "A0":
                    if (serialPortA0 == null)
                        serialPortA0 = new SerialPort();

                    return serialPortA0;
                case "A1":
                    if (serialPortA1 == null)
                        serialPortA1 = new SerialPort();

                    return serialPortA1;
                case "A2":
                    if (serialPortA2 == null)
                        serialPortA2 = new SerialPort();

                    return serialPortA2;
                case "A3":
                    if (serialPortA3 == null)
                        serialPortA3 = new SerialPort();

                    return serialPortA3;
                default:

                    return new SerialPort();
            }
        }

        private void disConnect(SerialPort serialport)
        {
            serialport.Close();
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
