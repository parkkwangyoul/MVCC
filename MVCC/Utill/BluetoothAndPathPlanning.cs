using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

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

            bluetoothConnect(ugv);

            bluetoothCommand(ugv, state);

            /*
            //bluetooth 연결쓰레드
            BackgroundWorker thread = new BackgroundWorker();
            thread.DoWork += bluetoothConnect;
            thread.RunWorkerAsync();
             * */
        }

        private void bluetoothConnect(UGV ugv)
        {
            int index = 0;
            int.TryParse(ugv.Id[1].ToString(), out index);

            if (!globals.UGVSettingDictionary.ContainsKey(convertId(ugv.Id)))
            {
                MessageBox.Show("블루투스 설정이 되어있지 않습니다.");
                state.IsDriving = true;

                return;
            }

            UGV settingUGV = globals.UGVSettingDictionary[convertId(ugv.Id)];
            try
            {
                if (!globals.SerialPortList[index].IsOpen)
                {

                    globals.SerialPortList[index].Close();
                    globals.SerialPortList[index].PortName = settingUGV.ComPort;
                    globals.SerialPortList[index].BaudRate = settingUGV.Baudrate;
                    globals.SerialPortList[index].DataBits = settingUGV.Databit;
                    globals.SerialPortList[index].StopBits = getStopBit(settingUGV.Stopbit);
                    globals.SerialPortList[index].ReadTimeout = 500;
                    globals.SerialPortList[index].WriteTimeout = 500;

                    globals.SerialPortList[index].Open(); 
                }

            }
            catch (System.IO.IOException e)
            {
                MessageBox.Show("블루투스 COMPort 설정을 다시해주세요.");
                return;
            }          
        }

        private void bluetoothCommand(UGV ugv, State state)
        {
            string write_data = ugv.Command;

            int index;
            int.TryParse(ugv.Id[1].ToString(), out index);

            if (globals.SerialPortList[index].IsOpen)
            {
                Console.Out.Flush();
               
                if (write_data.Equals("d"))
                {
                    globals.SerialPortList[index].WriteLine((write_data[0]).ToString());
                    //Console.WriteLine("index = " + index + " ugv command : " + write_data);

                    state.IsDriving = true;

                    try
                    {

                        #region Transmit_Movement_Command
                        /*
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
                        */
                        #endregion

                        //Console.WriteLine("direction_x {0}", direction_x);
                        ///Console.WriteLine("direction_y {0}", direction_y);

                        #region Angle Calculation
                        /*
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
                        */
                        #endregion
                        
                        
                        #region Send Angle
                        /*
                        if ((globals.angle[index] - globals.direction[index] == 0))
                        {
                            globals.SerialPortList[index].WriteLine("0");
                            Console.WriteLine("0");
                        }
                        else if ((globals.angle[index] - globals.direction[index] == 1))
                        {
                            globals.SerialPortList[index].WriteLine("1");
                            Console.WriteLine("1");
                        }
                        else if ((globals.angle[index] - globals.direction[index] == 2) || (globals.angle[index] - globals.direction[index] == -6))
                        {
                            globals.SerialPortList[index].WriteLine("2");
                            Console.WriteLine("2");
                        }
                        else if ((globals.angle[index] - globals.direction[index] == 3) || (globals.angle[index] - globals.direction[index] == -5))
                        {
                            globals.SerialPortList[index].WriteLine("3");
                            Console.WriteLine("3");
                        }
                        else if ((globals.angle[index] - globals.direction[index] == 4) || (globals.angle[index] - globals.direction[index] == -4))
                        {
                            globals.SerialPortList[index].WriteLine("4");
                            Console.WriteLine("4");
                        }
                        else if ((globals.angle[index] - globals.direction[index] == 5) || (globals.angle[index] - globals.direction[index] == -3))
                        {
                            globals.SerialPortList[index].WriteLine("5");
                            Console.WriteLine("5");
                        }
                        else if ((globals.angle[index] - globals.direction[index] == 6) || (globals.angle[index] - globals.direction[index] == -2))
                        {
                            globals.SerialPortList[index].WriteLine("6");
                            Console.WriteLine("6");
                        }
                        else if ((globals.angle[index] - globals.direction[index] == 7) || (globals.angle[index] - globals.direction[index] == -1))
                        {
                            globals.SerialPortList[index].WriteLine("7");
                            Console.WriteLine("7");
                        }
                        else if ((globals.angle[index] - globals.direction[index] == -7) && (globals.angle[index] - globals.direction[index] == 0))
                        {
                            globals.SerialPortList[index].WriteLine("8");
                            Console.WriteLine("8");
                        }
                        */
                        #endregion
                        

                        //Console.WriteLine("ugv.Id = " + ugv.Id + " direction[index] = " + globals.direction[index]);
                        
                        #region Transmit Movement Commands
                        /*
                        for (int i = 0; i < ugv.MovementCommandList.Count; i++)
                        {
                            globals.SerialPortList[index].WriteLine(ugv.MovementCommandList[i][0].ToString());
                        }
                        globals.SerialPortList[index].WriteLine("e");

                        Console.WriteLine("TX Complete");
                        */
                        #endregion
                                       
                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine("TimeOutException");

                        Console.Write("Buffer : ");
                        Console.WriteLine(globals.SerialPortList[index].ReadExisting());
                    }

                    //disConnect(serialport);
                }

                else if (write_data.Equals("q"))
                {
                    globals.SerialPortList[index].WriteLine((write_data[0]).ToString());
                }

                else
                {
                    globals.SerialPortList[index].WriteLine((write_data[0]).ToString());
                }
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
