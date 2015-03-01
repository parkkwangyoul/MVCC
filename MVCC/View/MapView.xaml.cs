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
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;

using MVCC.ViewModel;
using MVCC.Model;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

using System.ComponentModel;
using System.Runtime.InteropServices;
using MVCC.Utill;

namespace MVCC.View
{
    /// <summary>
    /// MapView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MapView : UserControl
    {
        private Capture webcam; //캠 영상 받을 변수 
        Image<Bgr, Byte> obstacle_image; //캠에서 받은 원본(장애물 이미지를 위한)
        System.Drawing.Rectangle[] tracking_rect; //트래킹한 결과를 그리는 네모박스
        bool obstacle_check = false; //트래킹과 장애물검사랑 동기화 위해
        bool image_is_changed = true; //영상을 비교했을때 차이가 날경우 (초기화를 true하는이유는 차가 놓여진상태에서 시작하면 바로 탬플릿 매칭을 수행해야되기때문)

        // MapViewModel 가져옴
        private MapViewModel mapViewModel;

        // Globals Class
        private Globals globals = Globals.Instance;

        private BluetoothAndPathPlanning bluetoothAndPathPlanning;
        List<UGV> ugvList = new List<UGV>();

        private PathFinder pathFinder;

        string[] rotation = { "1", "1", "1", "1" };
        string[] prev_rotation = { "1", "1", "1", "1" };

        #region 카메라, thread start
        /**
        * 카메라를 켬
        * */
        private void CamOn(object sender, RoutedEventArgs e)
        {
            // 카메라 없을때, 테스트용        
            //MockCameraOn();
            // 카메라 연결했을때
            CameraOnAndDetectThings();
        }

        #region TestMock
        private void MockCameraOn()
        {

            for (int i = 0; i < 4; i++)
            {
                UGV ugv = new UGV("A" + i, 50, 50, 50 + 50 * i, 50 + 50 * i, "Green");

                ugvList.Add(ugv);
            }

            mapViewModel.AddUGV(ugvList);

            //test_thread();
        }

        private void test_thread()
        {
            //색 트레킹 쓰레드
            BackgroundWorker thread = new BackgroundWorker();
            thread.DoWork += test_redirect;
            thread.RunWorkerAsync();
        }

        private void test_redirect(object sender, DoWorkEventArgs e)
        {
            bool check = false;
            while (true)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (check)
                    {
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                        {
                            ugvList[i].X -= 1;
                            ugvList[i].Y -= 1;


                            //refreshView();
                        }));
                    }
                    else
                    {
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                        {
                            ugvList[i].X += 1;
                            ugvList[i].Y += 1;


                            //refreshView();
                        }));
                    }
                }

                check = !check;

                Thread.Sleep(30);

                //Console.WriteLine("child");
            }
        }

        #endregion TestMock

        private void CameraOnAndDetectThings()
        {
            if (webcam == null)
            {
                webcam = new Capture(0); //cam 설정
                thread_start(); //thread 시작
            }
        }

        private void thread_start()
        {
            //색 트레킹 쓰레드
            BackgroundWorker thread = new BackgroundWorker();
            thread.DoWork += colorTracking;
            thread.RunWorkerAsync();

            //장애물 감지 쓰레드
            thread = new BackgroundWorker();
            thread.DoWork += obstacleDection;
            thread.RunWorkerAsync();
        }
        #endregion 카메라 and thread start

        #region color 트레킹
        //색상 트레킹
        private void colorTracking(object sender, DoWorkEventArgs e)
        {
            ColorTracking colorTracking = new ColorTracking(); //트래킹클래스선언

            Image<Bgr, Byte> img1 = new Image<Bgr, Byte>("testtest7.jpg"); // 템플릿 매칭할 사진     
            Image<Gray, Byte> img1_gray = img1.Convert<Gray, Byte>().PyrDown().PyrUp();
            Image<Bgr, Byte> matchColorCheck = null;
            Image<Gray, float> matchResImage = null;
            int totalPicxel = img1.Width * img1.Height; //탬플릿이미지의 총 픽셀수(어느정도 픽셀의 기준을 잡기 위해)
            globals.TemplateWidth = img1.Width + 30;
            globals.TemplateHeight = img1.Height + 30;
            List<UGV> ugvList = new List<UGV>();

            while (true)
            {
                using (Image<Bgr, Byte> frame = webcam.QueryFrame().Flip(Emgu.CV.CvEnum.FLIP.HORIZONTAL).Flip(Emgu.CV.CvEnum.FLIP.VERTICAL)) //webcam에서 영상 받음
                {
                    frame.ROI = new System.Drawing.Rectangle(globals.rect_x, globals.rect_y, globals.rect_width, globals.rect_height); // 정한 범위를 ROI로 설정                 
                    obstacle_image = frame.Clone(); //원본 복사

                    if (image_is_changed == true && colorTracking.get_color_count() != 4) //시작할때 바로 들어고, 변화가 감지됬을때 들어가서 탬플릿 매칭 수행
                    {
                        image_is_changed = false; // 변화감지되면 true해서 들어옴
                        matchResImage = frame.Convert<Gray, Byte>().PyrDown().PyrUp().MatchTemplate(img1_gray, Emgu.CV.CvEnum.TM_TYPE.CV_TM_CCOEFF_NORMED); //템플릿 매칭 중간 결과 저장
                        matchColorCheck = frame.Clone(); //매치된 칼라가 저장될 변수

                        float[, ,] matches = matchResImage.Data;
                        for (int x = 0; x < matches.GetLength(1); x++)
                        {
                            for (int y = 0; y < matches.GetLength(0); y++)
                            {
                                double matchScore = matches[y, x, 0];

                                if (matchScore >= 0.85)
                                {
                                    colorTracking.colorCheck(matchColorCheck, totalPicxel, x, y, globals.TemplateWidth, globals.TemplateHeight); //어떤 색인지 체크                        
                                    y += img1.Height; //x축 다음 y축(세로)이 변화기 때문에 속도를 높이기 위해 검출된 y좌표 + 이미지 사이즈 함.                             
                                }
                            }
                        }

                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                        {
                            ugvList = colorTracking.get_ugv();
                            mapViewModel.AddUGV(ugvList);
                            refreshView();
                        }));
                    }

                    //색상 트래킹
                    tracking_rect = colorTracking.tracking_start(frame);

                    //영상에 트레킹 결과 내보내기
                    for (int i = 0; i < 4; i++)
                    {
                        //AddUGV(i.ToString(), tracking_rect[i].X, tracking_rect[i].Y);
                        if (tracking_rect[i].Width != 0 && tracking_rect[i].Height != 0)
                        {
                            //Console.WriteLine("index = " + i + " direction = " + globals.direction[i]);
                            Dictionary<string, State> AllUGVStateMap = new Dictionary<string, State>();

                            for (int m = 0; m < mapViewModel.MVCCItemStateList.Count; m++)
                            {
                                State tempState = mapViewModel.MVCCItemStateList[m];
                                AllUGVStateMap.Add(tempState.ugv.Id, tempState);
                            }

                            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                            {
                                for (int j = 0; j < mapViewModel.MVCCItemList.Count; j++)
                                {
                                    if (!(mapViewModel.MVCCItemList[j] is UGV))
                                        continue;

                                    UGV ugv = mapViewModel.MVCCItemList[j] as UGV;

                                    if (ugv.Id.Equals("A" + i))
                                    {
                                        ugv.X = tracking_rect[i].X + 30;
                                        ugv.Y = tracking_rect[i].Y + 30;

                                        State tempUGVState = AllUGVStateMap[ugv.Id];

                                        globals.UGVStopCommandLock.EnterWriteLock();


                                        if (ugv.PathList.Count != 0 && tempUGVState.IsPause == false)
                                        {
                                            //Console.WriteLine("ugv.Id = " + ugv.Id + " ugv.PathList.Count = " + ugv.PathList.Count + " tempUGVState.IsPause = " + tempUGVState.IsPause);
                                            KeyValuePair<int, int> temp = new KeyValuePair<int, int>();

                                            temp = ugv.PathList[ugv.PathList.Count - 1];


                                            if (Math.Abs(ugv.X - temp.Key) < 15 && Math.Abs(ugv.Y - temp.Value) < 15)
                                            //if(Math.Abs(ugv.X - temp.Key) < 10 && Math.Abs(ugv.Y - temp.Value) < 10)
                                            {
                                                for (int p = mapViewModel.MVCCUGVPathList.Count - 1; p >= 0; p--)
                                                {
                                                    UGVPath tempPath = mapViewModel.MVCCUGVPathList[p] as UGVPath;

                                                    if (tempPath.Id.Equals(ugv.Id))
                                                    {
                                                        mapViewModel.MVCCUGVPathList.Remove(tempPath);

                                                        ugv.PathList.RemoveAt(ugv.PathList.Count - 1);
                                                        //Console.WriteLine("UGV.Id = " + ugv.Id + "  하나씩 제거");
                                                        refreshViewPath();
                                                        break;
                                                    }

                                                }
                                            }

                                            #region 방향 계산

                                            int first_x = ugv.PathList[ugv.PathList.Count - 1].Key / 15;
                                            int first_y = ugv.PathList[ugv.PathList.Count - 1].Value / 15;


                                            int addX = 0;
                                            int addY = 0;

                                            if (tempUGVState.BeforeX == -1)
                                                tempUGVState.BeforeX = tempUGVState.CurrentPointX;

                                            if (tempUGVState.BeforeX % 15 == 0 && tempUGVState.CurrentPointX - tempUGVState.BeforeX == -1)
                                            {
                                                addX = 1;
                                                //Console.WriteLine("tempUGVState.CurrentPointX : " + tempUGVState.CurrentPointX + "\ntempUGVState.BeforeX : " + tempUGVState.BeforeX);
                                            }
                                            else
                                                tempUGVState.BeforeX = tempUGVState.CurrentPointX;

                                            if (tempUGVState.BeforeY == -1)
                                                tempUGVState.BeforeY = tempUGVState.CurrentPointY;

                                            if (tempUGVState.BeforeY % 15 == 0 && tempUGVState.CurrentPointY - tempUGVState.BeforeY == -1)
                                            {
                                                addY = 1;
                                                //Console.WriteLine("tempUGVState.CurrentPointY : " + tempUGVState.CurrentPointY + "\ntempUGVState.BeforeY : " + tempUGVState.BeforeY);
                                            }
                                            else
                                                tempUGVState.BeforeY = tempUGVState.CurrentPointY;


                                            int start_x = ((tempUGVState.CurrentPointX + addX) / 15);
                                            int start_y = ((tempUGVState.CurrentPointY + addY) / 15);

                                            int direction_x = ((tempUGVState.CurrentPointX) / 15);
                                            int direction_y = ((tempUGVState.CurrentPointY) / 15);

                                            if (globals.direction[i] == 0)
                                            {
                                                direction_y = direction_y - 1;
                                            }
                                            else if (globals.direction[i] == 1)
                                            {
                                                direction_x = direction_x + 1;
                                                direction_y = direction_y - 1;
                                            }
                                            else if (globals.direction[i] == 2)
                                            {
                                                direction_x = direction_x + 1;
                                            }
                                            else if (globals.direction[i] == 3)
                                            {
                                                direction_x = direction_x + 1;
                                                direction_y = direction_y + 1;
                                            }
                                            else if (globals.direction[i] == 4)
                                            {
                                                direction_y = direction_y + 1;
                                            }
                                            else if (globals.direction[i] == 5)
                                            {
                                                direction_x = direction_x - 1;
                                                direction_y = direction_y + 1;
                                            }
                                            else if (globals.direction[i] == 6)
                                            {
                                                direction_x = direction_x - 1;
                                            }
                                            else if (globals.direction[i] == 7)
                                            {
                                                direction_x = direction_x - 1;
                                                direction_y = direction_y + 1;
                                            }

                                            if ((first_x - start_x == 0) && (first_y - start_y == -1))
                                                globals.angle[i] = 0;
                                            else if ((first_x - start_x == 1) && (first_y - start_y == -1))
                                                globals.angle[i] = 1;
                                            else if ((first_x - start_x == 1) && (first_y - start_y == 0))
                                                globals.angle[i] = 2;
                                            else if ((first_x - start_x == 1) && (first_y - start_y == 1))
                                                globals.angle[i] = 3;
                                            else if ((first_x - start_x == 0) && (first_y - start_y == 1))
                                                globals.angle[i] = 4;
                                            else if ((first_x - start_x == -1) && (first_y - start_y == 1))
                                                globals.angle[i] = 5;
                                            else if ((first_x - start_x == -1) && (first_y - start_y == 0))
                                                globals.angle[i] = 6;
                                            else if ((first_x - start_x == -1) && (first_y - start_y == -1))
                                                globals.angle[i] = 7;


                                            if (globals.direction[i] != -1)
                                            {
                                                if ((globals.angle[i] - globals.direction[i] == 0))
                                                {
                                                    rotation[i] = "0";
                                                    prev_rotation[i] = "7";
                                                }
                                                else if ((globals.angle[i] - globals.direction[i] == 1))
                                                {
                                                    rotation[i] = "7";
                                                    prev_rotation[i] = rotation[i];
                                                }
                                                else if ((globals.angle[i] - globals.direction[i] == 2) || (globals.angle[i] - globals.direction[i] == -6))
                                                {
                                                    rotation[i] = "7";
                                                    prev_rotation[i] = rotation[i];
                                                }
                                                else if ((globals.angle[i] - globals.direction[i] == 3) || (globals.angle[i] - globals.direction[i] == -5))
                                                {
                                                    rotation[i] = "7";
                                                    prev_rotation[i] = rotation[i];
                                                }
                                                else if ((globals.angle[i] - globals.direction[i] == 4) || (globals.angle[i] - globals.direction[i] == -4))
                                                {
                                                    rotation[i] = "7";
                                                    prev_rotation[i] = rotation[i];
                                                }
                                                else if ((globals.angle[i] - globals.direction[i] == 5) || (globals.angle[i] - globals.direction[i] == -3))
                                                {
                                                    rotation[i] = "1";
                                                    prev_rotation[i] = rotation[i];
                                                }
                                                else if ((globals.angle[i] - globals.direction[i] == 6) || (globals.angle[i] - globals.direction[i] == -2))
                                                {
                                                    rotation[i] = "1";
                                                    prev_rotation[i] = rotation[i];
                                                }
                                                else if ((globals.angle[i] - globals.direction[i] == 7) || (globals.angle[i] - globals.direction[i] == -1))
                                                {
                                                    rotation[i] = "1";
                                                    prev_rotation[i] = rotation[i];
                                                }
                                                else if ((globals.angle[i] - globals.direction[i] == -7) && (globals.angle[i] - globals.direction[i] == 0))
                                                {
                                                    rotation[i] = "1";
                                                    prev_rotation[i] = rotation[i];
                                                }
                                            }
                                            else
                                            {
                                                rotation[i] = prev_rotation[i];
                                            }


                                            if (globals.direction[i] == globals.angle[i])
                                            {
                                                if (globals.SerialPortList[i].IsOpen)
                                                {
                                                    tempUGVState = AllUGVStateMap[ugv.Id];
                                                    ugv.Command = "0";
                                                    bluetoothAndPathPlanning.connect(ugv, tempUGVState);
                                                }
                                            }
                                            else
                                            {
                                                if (globals.SerialPortList[i].IsOpen)
                                                {
                                                    tempUGVState = AllUGVStateMap[ugv.Id];
                                                    ugv.Command = rotation[i];
                                                    bluetoothAndPathPlanning.connect(ugv, tempUGVState);
                                                }
                                            }

                                            globals.mapObstacleLock.EnterWriteLock();

                                            //UGV가 도착 했을떼 
                                            if (ugv.PathList.Count == 1)
                                            {
                                                RemoveEndPoint(ugv, tempUGVState);

                                                ugv.PathList.Clear();

                                                if (globals.SerialPortList[i].IsOpen)
                                                {
                                                    tempUGVState = AllUGVStateMap[ugv.Id];
                                                    ugv.Command = "s";
                                                    bluetoothAndPathPlanning.connect(ugv, tempUGVState);

                                                    tempUGVState.BeforeX = -1;
                                                    tempUGVState.BeforeY = -1;

                                                    tempUGVState.IsDriving = false;

                                                    RemoveEndPointInGroup(tempUGVState.ugv, tempUGVState); //그룹용 도착점을 장애물도착맵에서 지움

                                                    Console.WriteLine("ugv.Id = " + ugv.Id + " 도착 정지!");
                                                }

                                            }
                                            globals.mapObstacleLock.ExitWriteLock();

                                            #endregion 방향 계산

                                        }

                                        globals.UGVStopCommandLock.ExitWriteLock();
                                        break;
                                    }
                                }
                                refreshView();
                            }));
                        }
                        else
                        {
                            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                            {
                                mapViewModel.RemoveUGV("A" + i);
                                refreshView();
                            }));
                        }
                    }


                    //색상 트레킹중에 하나가 사라졌는지..(test임!! 나중엔.. 이걸로 말고 장애물 변화를 해야함. 밑에 image_is_changed는 장애물변화될떄!!!!)
                    for (int i = 0; i < 4; i++)
                    {
                        if (colorTracking.change_chk(i) == true)
                        {
                            image_is_changed = true;
                            colorTracking.change_chk_reset(i);
                        }
                    }

                    obstacle_check = true; //장애물이미지와 싱크 맞추기 위해 설정
                }
            }
        }
        #endregion color 트레킹

        #region obstacle 검출
        //장애물 검출
        private void obstacleDection(object sender, DoWorkEventArgs e)
        {
            ObstacleDetection obstacleDetection = new ObstacleDetection();
            globals.Map_obstacle = new int[globals.rect_height / globals.y_grid, globals.rect_width / globals.x_grid]; //Map의 장애물의 정보 
            globals.onlyObstacle = new int[globals.rect_height / globals.y_grid, globals.rect_width / globals.x_grid]; //only 건물 장애물 정보
            globals.pre_onlyObstacle = new int[globals.rect_height / globals.y_grid, globals.rect_width / globals.x_grid]; //onlyObstacle의 이전 장애물의 정보 
            globals.EndPointMap = new int[globals.rect_height / globals.y_grid, globals.rect_width / globals.x_grid]; //UGV 차량의 도착 정보 저장 
            globals.obstacleInCollision = new int[globals.rect_height / globals.y_grid, globals.rect_width / globals.x_grid]; //충돌 위기 일때 저장하는 장애물 정보

            int blob_count = 0, pre_blob_count = 0; //blob count의 변화감지를 위해      
            bool frist_change_check = false;
            List<Building> building_List = new List<Building>();

            while (true)
            {
                if (obstacle_check == true) //frame의 추적 영상 처리가 끝나고 처리
                {
                    globals.mapObstacleLock.EnterWriteLock(); //critical section start

                    Array.Clear(globals.Map_obstacle, 0, globals.rect_height / globals.y_grid * globals.rect_width / globals.x_grid);
                    blob_count = obstacleDetection.detectBlob(obstacle_image, globals.Map_obstacle, tracking_rect); //장애물 검출

                    for (int i = 0; i < globals.rect_width / globals.x_grid; i++)
                        for (int j = 0; j < globals.rect_height / globals.y_grid; j++)
                            if (globals.Map_obstacle[j, i] == '*')
                                globals.onlyObstacle[j, i] = globals.Map_obstacle[j, i];

                    /*
                    Console.WriteLine("====================================================");

                    for (int j = 0; j < globals.rect_height / globals.y_grid; j++)
                    {
                        for (int i = 0; i < globals.rect_width / globals.x_grid; i++)
                            Console.Write("{0, 3} ", globals.Map_obstacle[j, i]);

                        Console.WriteLine();
                    }
                    Console.WriteLine("====================================================");

                    Console.WriteLine();

                    */
                    globals.mapObstacleLock.ExitWriteLock(); //critical section end


                    if (frist_change_check == true) //제일 처음 변화감지는 건너 뜀
                    {

                        #region 차량 끼리 충돌

                        globals.evasionInfoLock.EnterWriteLock();

                        /*
                        //차량끼리의 충돌이 되었을때
                        if (globals.UGVsConflictInofo.Count != 0)
                        {
                            Console.WriteLine("차량 끼리 충돌 !!!");

                            Dictionary<string, State> AllUGVStateMap = new Dictionary<string, State>();

                            for (int m = 0; m < mapViewModel.MVCCItemStateList.Count; m++)
                            {
                                State tempState = mapViewModel.MVCCItemStateList[m];
                                AllUGVStateMap.Add(tempState.ugv.Id, tempState);
                            }

                            foreach (var evsionTempList in globals.UGVsConflictInofo)
                            {
                                // 두개 의 차량에 대해 정지 메시지 전송
                                if (globals.SerialPortList[evsionTempList.Key].IsOpen)
                                {
                                    State tempUGVState = AllUGVStateMap["A" + evsionTempList.Key];
                                    tempUGVState.ugv.Command = "s";

                                    if (tempUGVState.IsDriving == true)
                                    {
                                        bluetoothAndPathPlanning.connect(tempUGVState.ugv, tempUGVState);
                                        tempUGVState.IsPause = false;
                                        tempUGVState.IsDriving = false;

                                        globals.sortInfo.ugv = tempUGVState.ugv;
                                        globals.sortInfo.UGV_Id = tempUGVState.ugv.Id;

                                        globals.sortInfoList.Remove(globals.sortInfo);

                                        tempUGVState.ugv.PathList.Clear();                                       
                                        RemoveEndPoint(tempUGVState.ugv);


                                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                                        {
                                            removeAllUGVPath(tempUGVState.ugv);
                                        }));


                                        Console.WriteLine(evsionTempList.Key + " 차량에게 정지 신호 보냄");

                                    }
                                }
                                if (globals.SerialPortList[evsionTempList.Value].IsOpen)
                                {
                                    State tempUGVState = AllUGVStateMap["A" + evsionTempList.Value];
                                    tempUGVState.ugv.Command = "s";

                                    if (tempUGVState.IsDriving == true)
                                    {
                                        bluetoothAndPathPlanning.connect(tempUGVState.ugv, tempUGVState);
                                        tempUGVState.IsPause = false;
                                        tempUGVState.IsDriving = false;

                                        globals.sortInfo.ugv = tempUGVState.ugv;
                                        globals.sortInfo.UGV_Id = tempUGVState.ugv.Id;

                                        globals.sortInfoList.Remove(globals.sortInfo);

                                        tempUGVState.ugv.PathList.Clear();
                                        RemoveEndPoint(tempUGVState.ugv);

                                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                                        {
                                            removeAllUGVPath(tempUGVState.ugv);
                                        }));
                                        Console.WriteLine(evsionTempList.Value + " 차량에게 정지 신호 보냄");

                                    }
                                }
                            }
                        }
                        */
                        #endregion 차량 끼리 충돌

                        #region 충돌 위기에서 풀렸을때 출발 신호 전송

                        //만약 현재 충돌 정보가 이전 정보에 없을때 일시 정지된 차량에게 출발 신호 보냄
                        if (globals.pre_evasionInfo.Count != 0)
                        {

                            //prev_evasionInfo 대한 정보를 복사 
                            List<KeyValuePair<int, int>> remove_evasionInfo = new List<KeyValuePair<int, int>>();

                            foreach (var list in globals.pre_evasionInfo)
                            {
                                KeyValuePair<int, int> temp = new KeyValuePair<int, int>(list.Key, list.Value);
                                remove_evasionInfo.Add(temp);
                            }

                            Dictionary<string, State> AllUGVStateMap = new Dictionary<string, State>();

                            for (int m = 0; m < mapViewModel.MVCCItemStateList.Count; m++)
                            {
                                State tempState = mapViewModel.MVCCItemStateList[m];
                                AllUGVStateMap.Add(tempState.ugv.Id, tempState);
                            }

                            //pre_evasionInfo 와 현재 evasionInfo 정보의 차이를 구함(없어진걸 지우기 일시정지 풀기 위해)                          
                            for (int k = globals.pre_evasionInfo.Count - 1; k >= 0; k--)
                            {
                                KeyValuePair<int, int> remove_evasion = new KeyValuePair<int, int>();
                                remove_evasion = remove_evasionInfo[k];

                                foreach (var evsionTempList in globals.evasionInfo)
                                {
                                    if (AllUGVStateMap["A" + evsionTempList.Key].IsDriving == true && AllUGVStateMap["A" + evsionTempList.Value].IsDriving == true)
                                    {
                                        if (evsionTempList.Key == remove_evasion.Key && evsionTempList.Value == remove_evasion.Value)
                                        {
                                            //Console.WriteLine("evsionTempList.Key = " + evsionTempList.Key + " remove_evasion.Key = " + remove_evasion.Key + " evsionTempList.Value = " + evsionTempList.Value + " remove_evasion.Value = " + remove_evasion.Value);
                                            remove_evasionInfo.Remove(remove_evasion);
                                            break;
                                        }
                                        else if (evsionTempList.Key == remove_evasion.Value && evsionTempList.Value == remove_evasion.Key)
                                        {
                                            //Console.WriteLine("evsionTempList.Key = " + evsionTempList.Key + " remove_evasion.Value = " + remove_evasion.Value + " evsionTempList.Value = " + evsionTempList.Value + " remove_evasion.Key = " + remove_evasion.Key);
                                            remove_evasionInfo.Remove(remove_evasion);
                                            break;
                                        }
                                    }
                                }

                            }

                            //pre_evasionInfo에서 빠진 충돌은 일시 중지 상태 풀음
                            foreach (var remove_evasion in remove_evasionInfo)
                            {
                                //충돌 위기에서 풀린 차량에게 시작하라고 보냄, 일시중지 상태 해제
                                State tempUGVState = AllUGVStateMap["A" + remove_evasion.Key];
                                State tempUGVState2 = AllUGVStateMap["A" + remove_evasion.Value];

                                if (tempUGVState.IsPause == true)
                                {
                                    if (globals.SerialPortList[remove_evasion.Key].IsOpen)
                                    {
                                        //tempUGVState.ugv.Command = "d";
                                        //bluetoothAndPathPlanning.connect(tempUGVState.ugv, tempUGVState);
                                        tempUGVState.IsPause = false;
                                        tempUGVState2.IsFindPath = false;
                                        Console.WriteLine(remove_evasion.Key + " 차량에게 출발 신호 보냄");
                                    }

                                }

                                if (tempUGVState2.IsPause == true)
                                {
                                    if (globals.SerialPortList[remove_evasion.Value].IsOpen)
                                    {
                                        //tempUGVState.ugv.Command = "d";
                                        //bluetoothAndPathPlanning.connect(tempUGVState.ugv, tempUGVState);
                                        tempUGVState2.IsPause = false;
                                        tempUGVState.IsFindPath = false;
                                        Console.WriteLine(remove_evasion.Value + " 차량에게 출발 신호 보냄");
                                    }
                                }
                            }
                        }
                        #endregion 충돌 위기에서 풀렸을때 출발 신호 전송

                        #region 차량 끼리 충돌 위기

                        //차량들끼리 충돌 위기가 있을때
                        if (globals.evasionInfo.Count != 0)
                        {

                            Dictionary<string, State> AllUGVStateMap = new Dictionary<string, State>();

                            for (int m = 0; m < mapViewModel.MVCCItemStateList.Count; m++)
                            {
                                State tempState = mapViewModel.MVCCItemStateList[m];
                                AllUGVStateMap.Add(tempState.ugv.Id, tempState);
                            }

                            //충돌 위기 차량에게 일시정지 메세지 보냄
                            foreach (var evsionTempList in globals.evasionInfo)
                            {
                                int n = evsionTempList.Key;
                                int m = evsionTempList.Value;

                                foreach (var sortTempList in globals.sortInfoList)
                                {
                                    int index;
                                    int.TryParse(sortTempList.UGV_Id[1].ToString(), out index);

                                    if (AllUGVStateMap["A" + n].IsDriving == true && AllUGVStateMap["A" + m].IsDriving == true)
                                    {
                                        if (index == n)
                                        {
                                            //m에게 일시 정지 보내기
                                            if (globals.SerialPortList[m].IsOpen)
                                            {
                                                State tempUGVState = AllUGVStateMap["A" + m];
                                                State tempUGVState2 = AllUGVStateMap["A" + n];

                                                tempUGVState.ugv.Command = "s";


                                                bluetoothAndPathPlanning.connect(tempUGVState.ugv, tempUGVState);
                                                tempUGVState.IsPause = true;

                                                //Console.WriteLine(m + " 차량에게 일시 정지 신호 보냄");


                                                if (tempUGVState2.IsFindPath == false)
                                                {
                                                    RemoveEndPoint(tempUGVState2.ugv, tempUGVState2);

                                                    tempUGVState2.ugv.PathList.Clear();

                                                    globals.bluetoothConnectLock.EnterWriteLock();
                                                    pathFinder.init();
                                                    globals.bluetoothConnectLock.ExitWriteLock();

                                                    globals.mapObstacleLock.EnterWriteLock();

                                                    int[,] copy_map = new int[globals.rect_height / globals.y_grid, globals.rect_width / globals.x_grid];
                                                    copy_map = (int[,])globals.Map_obstacle.Clone();

                                                    for (int x = 0; x < globals.rect_width / globals.x_grid; x++)
                                                    {
                                                        for (int y = 0; y < globals.rect_height / globals.y_grid; y++)
                                                        {
                                                            if (globals.Map_obstacle[y, x] == m + 1 && AllUGVStateMap["A" + m].ugv.IsBelongToGroup && !AllUGVStateMap["A" + m].ugv.GroupName.Equals(AllUGVStateMap["A" + n].ugv.GroupName))
                                                                globals.Map_obstacle[y, x] = '*';

                                                            if (globals.Map_obstacle[y, x] == n + 1)
                                                            {
                                                                globals.Map_obstacle[y, x] = 0;
                                                            }

                                                            foreach (var temp in AllUGVStateMap)
                                                            {
                                                                int id;
                                                                int.TryParse(temp.Key[1].ToString(), out id);

                                                                if (id == m || id == n)
                                                                    continue;

                                                                if (temp.Value.IsDriving == false && globals.Map_obstacle[y, x] == int.Parse(temp.Value.ugv.Id[1].ToString()) + 1)
                                                                {
                                                                    if (temp.Value.ugv.PathList.Count == 0)
                                                                        globals.Map_obstacle[y, x] = 0;
                                                                    else
                                                                        globals.Map_obstacle[y, x] = '*';
                                                                }
                                                                else if (temp.Value.IsDriving && globals.Map_obstacle[y, x] == int.Parse(temp.Value.ugv.Id[1].ToString()) + 1)
                                                                {
                                                                    globals.Map_obstacle[y, x] = 0;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    globals.mapObstacleLock.ExitWriteLock(); //critical section end

                                                    globals.bluetoothConnectLock.EnterWriteLock();
                                                    if (pathFinder.find_path(tempUGVState2.ugv, tempUGVState2) == true)
                                                    {
                                                        tempUGVState2.IsFindPath = true;

                                                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                                                        {
                                                            AddMVCCUGVPathList(tempUGVState2.ugv);
                                                            tempUGVState2.IsDriving = true;
                                                            refreshViewPath();
                                                        }));

                                                        /*
                                                        Console.WriteLine("====================================================");

                                                        for (int j = 0; j < globals.rect_height / globals.y_grid; j++)
                                                        {
                                                            for (int i = 0; i < globals.rect_width / globals.x_grid; i++)
                                                                Console.Write("{0, 3} ", globals.Map_obstacle[j, i]);

                                                            Console.WriteLine();
                                                        }
                                                        Console.WriteLine("====================================================");

                                                        Console.WriteLine();
                                                        */


                                                        globals.mapObstacleLock.EnterWriteLock(); //critical section end

                                                        globals.Map_obstacle = (int[,])copy_map.Clone();

                                                        globals.mapObstacleLock.ExitWriteLock(); //critical section end

                                                    }
                                                    else
                                                    {
                                                        //만약 길이 없어졌을때 일어나는 버그 있을 수도 있으니 생각하기 =====================================================================================
                                                        tempUGVState2.IsDriving = false;
                                                    }

                                                    globals.bluetoothConnectLock.ExitWriteLock();
                                                }

                                                break;
                                            }
                                        }
                                        else if (index == m)
                                        {
                                            //n에게 일시 정지 보내기
                                            if (globals.SerialPortList[n].IsOpen)
                                            {
                                                State tempUGVState = AllUGVStateMap["A" + n];
                                                State tempUGVState2 = AllUGVStateMap["A" + m];

                                                tempUGVState.ugv.Command = "s";

                                                bluetoothAndPathPlanning.connect(tempUGVState.ugv, tempUGVState);
                                                tempUGVState.IsPause = true;

                                                //Console.WriteLine(n + " 차량에게 일시 정지 신호 보냄");

                                                if (tempUGVState2.IsFindPath == false)
                                                {
                                                    RemoveEndPoint(tempUGVState2.ugv, tempUGVState2);

                                                    tempUGVState2.ugv.PathList.Clear();

                                                    globals.bluetoothConnectLock.EnterWriteLock();
                                                    pathFinder.init();
                                                    globals.bluetoothConnectLock.ExitWriteLock();

                                                    globals.mapObstacleLock.EnterWriteLock();

                                                    int[,] copy_map = new int[globals.rect_height / globals.y_grid, globals.rect_width / globals.x_grid];
                                                    copy_map = (int[,])globals.Map_obstacle.Clone();

                                                    for (int x = 0; x < globals.rect_width / globals.x_grid; x++)
                                                    {
                                                        for (int y = 0; y < globals.rect_height / globals.y_grid; y++)
                                                        {
                                                            if (globals.Map_obstacle[y, x] == n + 1 && AllUGVStateMap["A" + n].ugv.IsBelongToGroup && !AllUGVStateMap["A" + n].ugv.GroupName.Equals(AllUGVStateMap["A" + m].ugv.GroupName))
                                                                globals.Map_obstacle[y, x] = '*';

                                                            if (globals.Map_obstacle[y, x] == m + 1)
                                                            {
                                                                globals.Map_obstacle[y, x] = 0;
                                                            }

                                                            foreach (var temp in AllUGVStateMap)
                                                            {
                                                                int id;
                                                                int.TryParse(temp.Key[1].ToString(), out id);

                                                                if (id == n || id == m)
                                                                    continue;

                                                                if (temp.Value.IsDriving == false && globals.Map_obstacle[y, x] == int.Parse(temp.Value.ugv.Id[1].ToString()) + 1)
                                                                {
                                                                    if (temp.Value.ugv.PathList.Count == 0)
                                                                        globals.Map_obstacle[y, x] = 0;
                                                                    else
                                                                        globals.Map_obstacle[y, x] = '*';
                                                                }
                                                                else if (temp.Value.IsDriving && globals.Map_obstacle[y, x] == int.Parse(temp.Value.ugv.Id[1].ToString()) + 1)
                                                                {
                                                                    globals.Map_obstacle[y, x] = 0;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    globals.mapObstacleLock.ExitWriteLock(); //critical section end

                                                    globals.bluetoothConnectLock.EnterWriteLock();
                                                    if (pathFinder.find_path(tempUGVState2.ugv, tempUGVState2) == true)
                                                    {
                                                        tempUGVState2.IsFindPath = true;

                                                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                                                        {
                                                            AddMVCCUGVPathList(tempUGVState2.ugv);
                                                            tempUGVState2.IsDriving = true;
                                                            refreshViewPath();
                                                        }));
                                                        /*
                                                        Console.WriteLine("====================================================");

                                                        for (int j = 0; j < globals.rect_height / globals.y_grid; j++)
                                                        {
                                                            for (int i = 0; i < globals.rect_width / globals.x_grid; i++)
                                                                Console.Write("{0, 3} ", globals.Map_obstacle[j, i]);

                                                            Console.WriteLine();
                                                        }
                                                        Console.WriteLine("====================================================");

                                                        Console.WriteLine();
                                                        */
                                                        globals.mapObstacleLock.EnterWriteLock();

                                                        globals.Map_obstacle = (int[,])copy_map.Clone();

                                                        globals.mapObstacleLock.ExitWriteLock();

                                                    }
                                                    else
                                                    {
                                                        //만약 길이 없어졌을때 일어나는 버그 있을 수도 있으니 생각하기 =====================================================================================
                                                        tempUGVState2.IsDriving = false;
                                                    }

                                                    globals.bluetoothConnectLock.ExitWriteLock();
                                                }
                                                break;
                                            }
                                        }

                                    }
                                }
                            }
                        }
                        globals.evasionInfoLock.ExitWriteLock();

                        #endregion 차량 끼리 충돌 위기



                        #region 차량과 장애물 충돌

                        //충돌한 차량에게 정지 신호를 보냄
                        if (globals.UGVandObstacleCollisionInofo.Count != 0)
                        {
                            Dictionary<string, State> AllUGVStateMap = new Dictionary<string, State>();

                            for (int i = 0; i < mapViewModel.MVCCItemStateList.Count; i++)
                            {
                                State tempState = mapViewModel.MVCCItemStateList[i];
                                AllUGVStateMap.Add(tempState.ugv.Id, tempState);
                            }


                            for (int i = globals.UGVandObstacleCollisionInofo.Count - 1; i >= 0; i--)
                            {
                                foreach (var ugv_state in AllUGVStateMap)
                                {
                                    int index;
                                    int.TryParse(ugv_state.Value.ugv.Id[1].ToString(), out index);

                                    if (globals.UGVandObstacleCollisionInofo[i] == index && ugv_state.Value.IsDriving == true)
                                    {
                                        globals.UGVandObstacleCollisionInofo.Remove(index);

                                        Console.WriteLine("ugv.Id = " + ugv_state.Value.ugv.Id + " 이 장애물과 충돌하여 정지 신호 보냄");

                                        RemoveEndPoint(ugv_state.Value.ugv, ugv_state.Value);
                                        ugv_state.Value.ugv.PathList.Clear();

                                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                                       {
                                           removeAllUGVPath(ugv_state.Value.ugv);
                                       }));

                                        ugv_state.Value.ugv.Command = "s";
                                        bluetoothAndPathPlanning.connect(ugv_state.Value.ugv, ugv_state.Value);

                                        break;
                                    }
                                }
                            }
                        }

                        #endregion 차량과 장애물 충돌

                        #region 장애물 갯수 변화 및 움직임 감지

                        if (pre_blob_count != blob_count) //이전 blob과 현재 blob의 카운터가 다르면 Map에 장애물 수 생김 
                        {
                            Console.WriteLine("Map의 장애물 수 변화 !!! pre_blob_count = " + pre_blob_count + " blob_count = " + blob_count);
                            image_is_changed = true; //Map변화가 감지 됬으니 탬플릿 매칭 시작


                            bool[] stop_check = new bool[4];

                            Dictionary<string, State> AllUGVStateMap = new Dictionary<string, State>();

                            for (int i = 0; i < mapViewModel.MVCCItemStateList.Count; i++)
                            {
                                State tempState = mapViewModel.MVCCItemStateList[i];
                                AllUGVStateMap.Add(tempState.ugv.Id, tempState);
                            }

                            //차량 전체에 정지신호
                            for (int i = 0; i < mapViewModel.MVCCItemList.Count; i++)
                            {
                                if (!(mapViewModel.MVCCItemList[i] is UGV))
                                    continue;

                                UGV tempUGV = mapViewModel.MVCCItemList[i] as UGV;

                                if (!AllUGVStateMap.ContainsKey(tempUGV.Id))
                                    continue;

                                State tempUGVState = AllUGVStateMap[tempUGV.Id];


                                //if (tempUGV.MovementCommandList.Count != 0)
                                //if (!(tempUGVState.EndPointX == -1 && tempUGVState.EndPointY == -1))
                                if (tempUGVState.IsDriving == true)
                                {
                                    Console.WriteLine("장애물수 변화 되어 ugv.Id = " + tempUGV.Id + " 에 s 보내서 일시 중지 시킴");

                                    int index;
                                    int.TryParse(tempUGV.Id[1].ToString(), out index);

                                    stop_check[index] = true;

                                    tempUGVState = AllUGVStateMap[tempUGV.Id];
                                    tempUGV.Command = "s";

                                    bluetoothAndPathPlanning.connect(tempUGV, tempUGVState);
                                }
                            }

                            //전체차량 다시 길 찾고 보냄
                            for (int i = 0; i < mapViewModel.MVCCItemList.Count; i++)
                            {
                                if (!(mapViewModel.MVCCItemList[i] is UGV))
                                    continue;

                                UGV tempUGV = mapViewModel.MVCCItemList[i] as UGV;

                                int index;
                                int.TryParse(tempUGV.Id[1].ToString(), out index);

                                if (stop_check[index] == true)
                                {
                                    State tempUGVState = AllUGVStateMap[tempUGV.Id];
                                    tempUGV.Command = "d";

                                    RemoveEndPoint(tempUGV, tempUGVState);

                                    globals.bluetoothConnectLock.EnterWriteLock();
                                    pathFinder.init();

                                    if (pathFinder.find_path(tempUGV, tempUGVState) == true)
                                    {
                                        tempUGVState.IsPause = false;

                                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                                        {
                                            AddMVCCUGVPathList(tempUGV);

                                            refreshViewPath();
                                            Console.WriteLine("장애물수 변화 되어 일시 중지 되었던 ugv.Id = " + tempUGV.Id + "를 다시 길찾기 시작");
                                        }));
                                        if (tempUGV.PathList.Count != 0)
                                            bluetoothAndPathPlanning.connect(tempUGV, tempUGVState);
                                    }
                                    else
                                    {
                                        RemoveEndPoint(tempUGV, tempUGVState);
                                        tempUGV.PathList.Clear();
                                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                                        {
                                            removeAllUGVPath(tempUGV);
                                        }));
                                        tempUGVState.IsDriving = false;
                                        Console.WriteLine("장애물수 변화 되어 일시 중지 되었던 ugv.Id = " + tempUGV.Id + "가 길이 없어서 중지함");
                                    }
                                    globals.bluetoothConnectLock.ExitWriteLock();
                                }
                            }


                        }
                        else
                        {
                            //장애물이 옮겨짐을 검사. 옮겨지고 있어도 차량은 정지 해야함
                            int moving_check_count = 0;

                            globals.mapObstacleLock.EnterReadLock();
                            
                            for (int i = 0; i < globals.rect_width / globals.x_grid; i++)
                                for (int j = 0; j < globals.rect_height / globals.y_grid; j++)
                                    if (globals.onlyObstacle[j, i] == '*' || globals.pre_onlyObstacle[j, i] == '*')
                                        if (!(globals.onlyObstacle[j, i] == '*' && globals.pre_onlyObstacle[j, i] == '*'))
                                            moving_check_count++;
                            
                            /*
                            if (moving_check_count != 0)
                            {
                                Console.WriteLine("====================================================");
                                Console.WriteLine("moving_check_count = " + moving_check_count);
                                Console.WriteLine("globals.onlyObstacl");

                                for (int j = 0; j < globals.rect_height / globals.y_grid; j++)
                                {
                                    for (int i = 0; i < globals.rect_width / globals.x_grid; i++)
                                        Console.Write("{0, 3} ", globals.onlyObstacle[j, i]);

                                    Console.WriteLine();
                                }
                                Console.WriteLine();
                                Console.WriteLine("globals.pre_onlyObstacle");

                                for (int j = 0; j < globals.rect_height / globals.y_grid; j++)
                                {
                                    for (int i = 0; i < globals.rect_width / globals.x_grid; i++)
                                        Console.Write("{0, 3} ", globals.pre_onlyObstacle[j, i]);

                                    Console.WriteLine();
                                }
                                Console.WriteLine("====================================================");
                                Console.WriteLine();
                            }              
                            */

                            globals.mapObstacleLock.ExitReadLock();

                            if (moving_check_count >= 4) //배열이 몇개 이상 차이날 경우 장애물이 옮겨지고 있음
                            {
                                Console.WriteLine("====================================================");                  
                                Console.WriteLine("장애물 옮기는 중! moving_check_count = " + moving_check_count);

                                bool[] stop_check = new bool[4];

                                Dictionary<string, State> AllUGVStateMap = new Dictionary<string, State>();

                                for (int i = 0; i < mapViewModel.MVCCItemStateList.Count; i++)
                                {
                                    State tempState = mapViewModel.MVCCItemStateList[i];
                                    AllUGVStateMap.Add(tempState.ugv.Id, tempState);
                                }

                                  
                                 //차량 전체에 정지신호
                            for (int i = 0; i < mapViewModel.MVCCItemList.Count; i++)
                            {
                                if (!(mapViewModel.MVCCItemList[i] is UGV))
                                    continue;

                                UGV tempUGV = mapViewModel.MVCCItemList[i] as UGV;

                                if (!AllUGVStateMap.ContainsKey(tempUGV.Id))
                                    continue;

                                State tempUGVState = AllUGVStateMap[tempUGV.Id];


                                //if (tempUGV.MovementCommandList.Count != 0)
                                //if (!(tempUGVState.EndPointX == -1 && tempUGVState.EndPointY == -1))
                                if (tempUGVState.IsDriving == true)
                                {
                                    Console.WriteLine("장애물이 움직여서 ugv.Id = " + tempUGV.Id + " 에 s 보내서 일시 중지 시킴");

                                    int index;
                                    int.TryParse(tempUGV.Id[1].ToString(), out index);

                                    stop_check[index] = true;

                                    tempUGVState = AllUGVStateMap[tempUGV.Id];
                                    tempUGV.Command = "s";

                                    bluetoothAndPathPlanning.connect(tempUGV, tempUGVState);
                                }
                            }

                            //전체차량 다시 길 찾고 보냄
                            for (int i = 0; i < mapViewModel.MVCCItemList.Count; i++)
                            {
                                if (!(mapViewModel.MVCCItemList[i] is UGV))
                                    continue;

                                UGV tempUGV = mapViewModel.MVCCItemList[i] as UGV;

                                int index;
                                int.TryParse(tempUGV.Id[1].ToString(), out index);

                                if (stop_check[index] == true)
                                {
                                    State tempUGVState = AllUGVStateMap[tempUGV.Id];
                                    tempUGV.Command = "d";

                                    RemoveEndPoint(tempUGV, tempUGVState);

                                    globals.bluetoothConnectLock.EnterWriteLock();
                                    pathFinder.init();

                                    if (pathFinder.find_path(tempUGV, tempUGVState) == true)
                                    {
                                        tempUGVState.IsPause = false;

                                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                                        {
                                            AddMVCCUGVPathList(tempUGV);

                                            refreshViewPath();
                                            Console.WriteLine("장애물이 움직여서 일시 중지 되었던 ugv.Id = " + tempUGV.Id + "를 다시 길찾기 시작");
                                        }));
                                        if (tempUGV.PathList.Count != 0)
                                            bluetoothAndPathPlanning.connect(tempUGV, tempUGVState);
                                    }
                                    else
                                    {
                                        RemoveEndPoint(tempUGV, tempUGVState);
                                        tempUGV.PathList.Clear();
                                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                                        {
                                            removeAllUGVPath(tempUGV);
                                        }));
                                        tempUGVState.IsDriving = false;
                                        Console.WriteLine("장애물이 움직여서 일시 중지 되었던 ugv.Id = " + tempUGV.Id + "가 길이 없어서 중지함");
                                    }
                                    globals.bluetoothConnectLock.ExitWriteLock();
                                }

                            }
                                Console.WriteLine("====================================================");
                            }

                        }
                        

                        #endregion 장애물 갯수 변화 및 움직임 감지
                    }
                    else
                        frist_change_check = true;


                    #region 이전 정보 저장 및 초기화 부분

                    pre_blob_count = blob_count; //현재 blob_count를 이전 blob_count에 저장
                    globals.pre_onlyObstacle = (int[,])globals.onlyObstacle.Clone(); //비교를 위해 이전 Map정보 설정

                    globals.pre_evasionInfo.Clear();

                    foreach (var list in globals.evasionInfo)
                    {
                        KeyValuePair<int, int> temp = new KeyValuePair<int, int>(list.Key, list.Value);
                        globals.pre_evasionInfo.Add(temp);
                    }

                    globals.evasionInfo.Clear();

                    #endregion 이전 정보 저장 및 초기화 부분

                    #region 장애물 GUI 업데이트

                    //GUI 장애물 업데이트 
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                    {
                        building_List = obstacleDetection.get_building();
                        mapViewModel.AddBuilding(building_List);


                        for (int i = 0; i < building_List.Count; i++) //DisapperCheck 가 false경우 추적에서 사라진거므로 list에 제거
                        {
                            Building remov_tmp = building_List[i];

                            if (remov_tmp.DisapperCheck == false)
                            {
                                building_List.Remove(remov_tmp);
                                mapViewModel.RemoveBuilding(remov_tmp.Id);
                            }
                        }

                        for (int i = 0; i < mapViewModel.MVCCItemList.Count; i++)
                        {

                            if (!(mapViewModel.MVCCItemList[i] is Building))
                                continue;

                            Building building = mapViewModel.MVCCItemList[i] as Building;

                            if (building.Id.Equals(building.Id))
                            {
                                foreach (Building temp_building in building_List) //list의 정보를 다 갱신
                                {
                                    if (temp_building.Id.Equals(building.Id))
                                    {
                                        building.X = temp_building.X;
                                        building.Y = temp_building.Y;
                                        building.Width = temp_building.Width;
                                        building.Height = temp_building.Height;
                                        building.DisapperCheck = true;
                                        break;
                                    }
                                }
                            }
                        }

                        refreshView();
                    }));

                    #endregion 장애물 GUI 업데이트
                }
            }
        }
        #endregion obstacle 검출

        //path_count 우선 순위 정하기
        public void UGV_priority_sort(Dictionary<string, UGV> GroupMap, Dictionary<string, State> GroupStateMap)
        {
            globals.sortInfoList.Clear();

            //정렬을 위해 list에 넣음
            foreach (var key in GroupMap.Keys)
            {
                globals.sortInfo.UGV_Id = key;
                globals.sortInfo.ugv = GroupMap[key];

                globals.sortInfoList.Add(globals.sortInfo);
            }


            globals.sortInfoList = globals.sortInfoList.OrderBy(o => o.ugv.PathList.Count).ToList(); //path_count를 오름차순 정렬

            Console.Write("ugv 순위 = " );
            foreach (var list in globals.sortInfoList)
            {
                Console.Write( list.UGV_Id + " ");
            }
            Console.WriteLine();

            //map에 도착 자리 배치
            foreach (var list in globals.sortInfoList)
                mapEndCoordinateArrange(GroupMap[list.UGV_Id], GroupStateMap[list.UGV_Id]);

        }

        //UGV 도착 좌표 배치
        public void mapEndCoordinateArrange(UGV ugv, State state)
        {
            //globals.mapObstacleLock.EnterReadLock(); //critical section start
            globals.endPointMapLock.EnterReadLock();

            //도착지점이 @로 미리 되어있으면 path에서 5칸 뒤로 가서 도착 지점으로 만듬
            int startX, startY, endX, endY;
            bool endPointCheck = true;

            startX = ugv.PathList[0].Key / 15 - 2;
            startY = ugv.PathList[0].Value / 15 - 2;

            endX = ugv.PathList[0].Key / 15 + 2;
            endY = ugv.PathList[0].Value / 15 + 2;

            //범위 초과일 경우 설정
            if (startX < 0)
            {
                endX -= startX;
                startX = 0;
            }
            if (startY < 0)
            {
                endY -= startY;
                startY = 0;
            }

            if (endX > globals.rect_width / globals.x_grid)
            {
                startX -= (endX - globals.rect_width / globals.x_grid);
                endX = globals.rect_width;
            }
            if (endY > globals.rect_height)
            {
                startY -= (endY - globals.rect_height / globals.y_grid);
                endY = globals.rect_height;
            }
            endPointCheck = true;

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    if (globals.EndPointMap[y, x] == '@')
                    {
                        endPointCheck = false;
                        break;
                    }
                }

                if (endPointCheck == false)
                    break;
            }



            if (endPointCheck == false)
            {
                while (true)
                {


                    for (int p = 0; p < mapViewModel.MVCCUGVPathList.Count; )
                    {
                        UGVPath tempPath = mapViewModel.MVCCUGVPathList[p] as UGVPath;

                        if (tempPath.Id.Equals(ugv.Id))
                        {
                            mapViewModel.MVCCUGVPathList.Remove(tempPath);

                            ugv.PathList.RemoveAt(0);
                            //Console.WriteLine("도착지점으로 부터 하나씩 제거");

                            startX = ugv.PathList[0].Key / 15 - 2;
                            startY = ugv.PathList[0].Value / 15 - 2;

                            endX = ugv.PathList[0].Key / 15 + 2;
                            endY = ugv.PathList[0].Value / 15 + 2;

                            //범위 초과일 경우 설정
                            if (startX < 0)
                            {
                                endX -= startX;
                                startX = 0;
                            }
                            if (startY < 0)
                            {
                                endY -= startY;
                                startY = 0;
                            }

                            if (endX > globals.rect_width / globals.x_grid)
                            {
                                startX -= (endX - globals.rect_width / globals.x_grid);
                                endX = globals.rect_width;
                            }
                            if (endY > globals.rect_height)
                            {
                                startY -= (endY - globals.rect_height / globals.y_grid);
                                endY = globals.rect_height;
                            }
                            endPointCheck = true;
                       
                            for (int x = startX; x <= endX; x++)
                            {
                                for (int y = startY; y <= endY; y++)
                                {
                                    //Console.WriteLine("X : " + x + " Y : " + y);
                                    //Console.WriteLine("globals.EndPointMap[y, x] : " + globals.EndPointMap[y, x]);
                                    if (globals.EndPointMap[y, x] == '@' || globals.EndPointMap[y, x] == 1)
                                    {
                                        endPointCheck = false;
                                        break;
                                    }
                                }

                                if (endPointCheck == false)
                                    break;
                            }
                        }
                        else
                        {
                            p++;
                            //Console.WriteLine("// 오지???");
                        }

                        if (endPointCheck == true)
                            break;
                    }

                    if (endPointCheck == true)
                        break;
                }
            }
            globals.endPointMapLock.ExitReadLock();


            startX = ugv.PathList[0].Key / 15 - 2;
            startY = ugv.PathList[0].Value / 15 - 2;

            endX = ugv.PathList[0].Key / 15 + 2;
            endY = ugv.PathList[0].Value / 15 + 2;

            state.EndPointX = ugv.PathList[0].Key / 15;
            state.EndPointY = ugv.PathList[0].Value / 15;

            //범위 초과일 경우 설정
            if (startX < 0)
            {
                endX -= startX;
                startX = 0;
            }
            if (startY < 0)
            {
                endY -= startY;
                startY = 0;
            }

            if (endX > globals.rect_width / globals.x_grid)
            {
                startX -= (endX - globals.rect_width / globals.x_grid);
                endX = globals.rect_width;
            }
            if (endY > globals.rect_height)
            {
                startY -= (endY - globals.rect_height / globals.y_grid);
                endY = globals.rect_height;
            }

            globals.endPointMapLock.EnterWriteLock();

            for (int x = startX; x <= endX; x++)
                for (int y = startY; y <= endY; y++)
                    globals.EndPointMap[y, x] = '@'; //장애물은 @ 설정  



            /*
            for (int j = 0; j < globals.rect_height / globals.y_grid; j++)
            {
                for (int i = 0; i < globals.rect_width / globals.x_grid; i++)
                    Console.Write("{0, 3} ", globals.EndPointMap[j, i]);

                Console.WriteLine();
            }

            Console.WriteLine();
            */

            globals.endPointMapLock.ExitWriteLock();
            //globals.mapObstacleLock.ExitReadLock(); //critical section end
        }


        public MapView()
        {
            InitializeComponent();

            mapViewModel = DataContext as MapViewModel;

            (FindResource("MVCCItemSrc") as CollectionViewSource).Source = mapViewModel.MVCCItemList;

            (FindResource("UGVStateSrc") as CollectionViewSource).Source = mapViewModel.MVCCItemStateList;

            (FindResource("UGVGroupSrc") as CollectionViewSource).Source = mapViewModel.MVCCGroupList;

            (FindResource("UGVPathSrc") as CollectionViewSource).Source = mapViewModel.MVCCUGVPathList;

            bluetoothAndPathPlanning = new BluetoothAndPathPlanning();

            for (int i = 15; i <= globals.rect_width; i += 15)
            {
                Line line = new Line();

                line.X1 = i;
                line.Y1 = 0;
                line.X2 = i;
                line.Y2 = globals.rect_height;

                line.Stroke = Brushes.Gray;

                MapItemControlWrapGrid.Children.Add(line);
            }

            for (int i = 15; i <= globals.rect_height; i += 15)
            {
                Line line = new Line();

                Panel.SetZIndex(line, 0);

                line.X1 = 0;
                line.Y1 = i;
                line.X2 = globals.rect_width;
                line.Y2 = i;

                line.Stroke = Brushes.Gray;

                MapItemControlWrapGrid.Children.Add(line);
            }

            pathFinder = new PathFinder();
        }

        // UGV에게 이동 명령을 내림
        private void MoveUGV(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);

            // 클릭한 위치를 현재 좌표계 값으로 변경
            int endPointX = ((int)Math.Round(p.X) - 185) / 15;
            int endPointY = ((int)Math.Round(p.Y) - 160) / 15;

            // 개인용
            UGV individualUGV = null;
            State individualUGVState = null;

            // 그룹용
            Dictionary<string, UGV> GroupMap = new Dictionary<string, UGV>();
            Dictionary<string, State> GroupStateMap = new Dictionary<string, State>();
            string groupName = null;

            //모드 검사용
            string mode = "N";

            // UGV 
            for (int i = 0; i < mapViewModel.MVCCItemList.Count; i++)
            {
                if (!(mapViewModel.MVCCItemList[i] is UGV))
                    continue;

                UGV tempUGV = mapViewModel.MVCCItemList[i] as UGV;

                // 개인이 선택된 것인지 검사
                if (tempUGV.IsClicked && !tempUGV.IsGroupClicked)
                {
                    individualUGV = tempUGV;

                    mode = "I";

                    break;
                }
                else if (tempUGV.IsGroupClicked)
                {
                    if (groupName == null || groupName.Equals(tempUGV.GroupName))
                    {
                        GroupMap.Add(tempUGV.Id, tempUGV);
                    }

                    mode = "G";
                }
            }

            //State
            for (int i = 0; i < mapViewModel.MVCCItemStateList.Count; i++)
            {
                State tempState = mapViewModel.MVCCItemStateList[i];

                if (mode.Equals("I"))
                {
                    if (tempState.ugv.Id.Equals(individualUGV.Id))
                    {
                        individualUGVState = tempState;
                    }
                }
                else if (mode.Equals("G"))
                {
                    if (GroupMap.ContainsKey(tempState.ugv.Id))
                    {
                        GroupStateMap.Add(tempState.ugv.Id, tempState);
                    }
                }
            }

            // 개인
            if (mode.Equals("I"))
            {

                int index;
                int.TryParse(individualUGV.Id[1].ToString(), out index);

                //정지된 차량을 장애물로 인식
                globals.mapObstacleLock.EnterWriteLock(); //critical section start

                Dictionary<string, State> AllUGVStateMap = new Dictionary<string, State>();

                for (int i = 0; i < mapViewModel.MVCCItemStateList.Count; i++)
                {
                    State tempState = mapViewModel.MVCCItemStateList[i];
                    AllUGVStateMap.Add(tempState.ugv.Id, tempState);
                }

                for (int i = 0; i < globals.rect_width / globals.x_grid; i++)
                {
                    for (int j = 0; j < globals.rect_height / globals.y_grid; j++)
                    {
                        for (int k = 0; k < mapViewModel.MVCCItemList.Count; k++)
                        {
                            if (!(mapViewModel.MVCCItemList[k] is UGV))
                                continue;

                            UGV tempUGV = mapViewModel.MVCCItemList[k] as UGV;

                            if (!AllUGVStateMap.ContainsKey(tempUGV.Id))
                                continue;

                            if (globals.Map_obstacle[j, i] != 0 && globals.Map_obstacle[j, i] != index + 1)
                            {
                                if (globals.Map_obstacle[j, i] != '*')
                                {
                                    string id = "A" + (globals.Map_obstacle[j, i] - 1);
                                    if (!AllUGVStateMap[id].IsDriving)
                                        globals.Map_obstacle[j, i] = '*';
                                }
                                //Console.WriteLine("tempUGVState.IsDriving = " + tempUGVState.IsDriving + " tempUGV.Id = " + tempUGV.Id);

                            }
                        }
                    }
                }

                globals.mapObstacleLock.ExitWriteLock(); //critical section end


                individualUGVState.EndPointX = endPointX;
                individualUGVState.EndPointY = endPointY;

                individualUGV.Command = "d";

                RemoveEndPoint(individualUGV, individualUGVState);

                individualUGV.PathList.Clear();

                globals.bluetoothConnectLock.EnterWriteLock();
                pathFinder.init();

                if (pathFinder.find_path(individualUGV, individualUGVState) == true)
                {
                    individualUGVState.IsPause = false;

                    AddMVCCUGVPathList(individualUGV);

                    //여기서 도착 지점 배치 함수
                    mapEndCoordinateArrange(individualUGV, individualUGVState);

                    refreshViewPath();

                    bluetoothAndPathPlanning.connect(individualUGV, individualUGVState);
                }
                else
                {
                    removeAllUGVPath(individualUGV);
                }
                globals.bluetoothConnectLock.ExitWriteLock();
            }
            else if (mode.Equals("G"))
            {
                List<int> index_list = new List<int>();

                //그룹 UGV를 인덱스를 다 비교 하기 위해
                foreach (var key in GroupMap.Keys)
                {
                    UGV tempUGV = GroupMap[key];
                    int index;
                    int.TryParse(tempUGV.Id[1].ToString(), out index);
                    index_list.Add(index);
                }

                //그룹으로 지정된 차량 빼고 정지된 차량을 장애물로 인식
                globals.mapObstacleLock.EnterWriteLock(); //critical section start

                for (int i = 0; i < globals.rect_width / globals.x_grid; i++)
                {
                    for (int j = 0; j < globals.rect_height / globals.y_grid; j++)
                    {
                        bool index_check = true;

                        for (int k = 0; k < index_list.Count; k++)
                        {
                            if (globals.Map_obstacle[j, i] != 0)
                            {
                                if (globals.Map_obstacle[j, i] == index_list.ElementAt(k) + 1)
                                {
                                    index_check = true;
                                    break;
                                }
                                else if (globals.Map_obstacle[j, i] != index_list.ElementAt(k) + 1 || GroupStateMap["A" + index_list.ElementAt(k)].IsDriving == false)
                                {
                                    if (GroupStateMap["A" + index_list.ElementAt(k)].IsDriving == true)
                                    {
                                        index_check = true;
                                        break;
                                    }

                                    index_check = false;
                                }
                            }
                            else if (globals.Map_obstacle[j, i] == 0)
                            {
                                break;
                            }
                        }

                        if (index_check == false)
                        {
                            globals.Map_obstacle[j, i] = '*';
                        }
                    }
                }

                globals.mapObstacleLock.ExitWriteLock(); //critical section end


                List<string> temp_list = new List<string>();

                foreach (var key in GroupMap.Keys)
                {
                    UGV tempUGV = GroupMap[key];

                    State tempState = GroupStateMap[key];


                    tempState.EndPointX = endPointX;
                    tempState.EndPointY = endPointY;

                    tempUGV.Command = "d";

                    RemoveEndPoint(tempUGV, tempState);

                    tempUGV.PathList.Clear();

                    globals.bluetoothConnectLock.EnterWriteLock();
                    pathFinder.init();

                    if (pathFinder.find_path(tempUGV, tempState) == true)
                    {
                        tempState.IsPause = false;
                        AddMVCCUGVPathList(tempUGV);
                    }
                    else
                    {
                        tempState.IsDriving = false;
                    }
                    globals.bluetoothConnectLock.ExitWriteLock();

                    if (tempUGV.PathList.Count == 0)
                        temp_list.Add(key);
                }

                //길 없는 것을 그룹에서 빼기 위해 
                foreach (var remov_key in temp_list)
                {
                    removeAllUGVPath(GroupMap[remov_key]);
                    GroupMap.Remove(remov_key);
                }

                //여기서 도착 지점 배치 함수
                UGV_priority_sort(GroupMap, GroupStateMap);


                foreach (var key in GroupMap.Keys)
                {
                    UGV tempUGV = GroupMap[key];
                    State tempState = GroupStateMap[key];

                    // if(tempState.IsDriving == true)

                    bluetoothAndPathPlanning.connect(tempUGV, tempState);

                }

                refreshViewPath();
            }

        }

        // UGV를 선택하는 모드
        private void SelectUGV(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Main");
            IInputElement clickedElement = Mouse.DirectlyOver;

            // 그룹모드로 선택할때
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (clickedElement is System.Windows.Shapes.Ellipse)
                {
                    System.Windows.Shapes.Ellipse ellipse = clickedElement as System.Windows.Shapes.Ellipse;

                    Grid grid = ellipse.Parent as Grid;
                    string id = (grid.Children[0] as TextBlock).Text;

                    UGV ugv = new UGV();

                    for (int i = 0; i < mapViewModel.MVCCItemList.Count; i++)
                    {
                        if (!(mapViewModel.MVCCItemList[i] is UGV))
                            continue;

                        UGV tempUGV = mapViewModel.MVCCItemList[i] as UGV;
                        if (tempUGV.Id.Equals(id))
                            ugv = tempUGV;

                        // 그룹 선택을 할때, 하나하나 선택된것들을 모두 해제한다.
                        if (tempUGV.IsClicked)
                        {
                            cancelSelectUGV(tempUGV);
                        }
                    }

                    if (!ugv.IsBelongToGroup)
                    {
                        // 그룹 선택이 안된 것
                        if (!ugv.IsClickedReadyBelongToGroup)
                        {
                            ugv.IsClickedReadyBelongToGroup = true;

                            selectUGVAndStateChangeLayout(ugv, "Blue", id);

                            mapViewModel.MVCCTempList.Add(ugv);
                        }
                        else
                        {
                            ugv.IsClickedReadyBelongToGroup = false;

                            cancelSelectUGV(ugv);

                            RemoveSelectedUGVInGroupTempList(ugv);
                        }

                        // 그룹 대기열에 없는 UGV중에 이미 그룹에 속한 UGV들의 Layout을 해제                       
                        for (int i = 0; i < mapViewModel.MVCCItemList.Count; i++)
                        {
                            if (!(mapViewModel.MVCCItemList[i] is UGV))
                                continue;

                            UGV tempUGV = mapViewModel.MVCCItemList[i] as UGV;

                            if (!tempUGV.Id.Equals(ugv.Id) && tempUGV.IsBelongToGroup)
                            {
                                cancelSelectUGV(tempUGV);
                            }
                        }
                    }

                    refreshView();
                }
                else
                {
                    cancelSelectUGV();

                    refreshView();
                }
            }

            // 그룹이 선택된 상태에서 Alt를 누르고 부대선택되지 않은 UGV를 선택하면, 그 그룹에 추가된다.
            else if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                Group group = findClickedGroup();

                if (group != null)
                {
                    if (clickedElement is System.Windows.Shapes.Ellipse)
                    {
                        System.Windows.Shapes.Ellipse ellipse = clickedElement as System.Windows.Shapes.Ellipse;

                        Grid grid = ellipse.Parent as Grid;
                        string id = (grid.Children[0] as TextBlock).Text;

                        UGV ugv = new UGV();

                        for (int i = 0; i < mapViewModel.MVCCItemList.Count; i++)
                        {
                            if (!(mapViewModel.MVCCItemList[i] is UGV))
                                continue;

                            UGV tempUGV = mapViewModel.MVCCItemList[i] as UGV;
                            if (tempUGV.Id.Equals(id))
                            {
                                ugv = tempUGV;
                            }
                        }

                        if (!ugv.IsBelongToGroup)
                        {

                            ugv.GroupName = group.Name;
                            ugv.IsBelongToGroup = true;

                            ugv.IsGroupClicked = true;

                            group.MemberList.Add(ugv);

                            selectUGVAndStateChangeLayout(ugv, group.StateBorderBrush, ugv.Id);
                        }
                        else if (ugv.GroupName.Equals(group.Name))
                        {
                            ugv.GroupName = null;
                            ugv.IsBelongToGroup = false;

                            group.MemberList.Remove(ugv);

                            if (group.MemberList.Count == 0)
                            {
                                mapViewModel.MVCCGroupList.Remove(group);
                            }

                            cancelSelectUGV(ugv);
                        }
                        else
                        {
                            MessageBox.Show("이미 다른 그룹에 속한 UGV 입니다.");
                        }
                    }
                }

                refreshView();
            }

            // 하나하나 선택할때
            else
            {
                //Console.WriteLine("Red Circle");
                if (clickedElement is System.Windows.Shapes.Ellipse)
                {
                    System.Windows.Shapes.Ellipse ellipse = clickedElement as System.Windows.Shapes.Ellipse;

                    Grid grid = ellipse.Parent as Grid;
                    string id = (grid.Children[0] as TextBlock).Text;

                    UGV ugv = new UGV();

                    // 선택한 UGV를 찾아서 나머지 선택을 해제
                    for (int i = 0; i < mapViewModel.MVCCItemList.Count; i++)
                    {
                        if (!(mapViewModel.MVCCItemList[i] is UGV))
                            continue;

                        UGV tempUGV = mapViewModel.MVCCItemList[i] as UGV;

                        if (!tempUGV.Id.Equals(id))
                        {
                            cancelSelectUGV(tempUGV);
                        }
                        else
                        {
                            ugv = tempUGV;
                        }
                    }

                    /**
                     * UGV가 그룹에 속해 있으면, 그룹이 선택되고, 그룹전체의 UGV가 선택됨
                     * UGV가 그룹에 속해 있지 않으면, 선택한 UGV만 선택되고 그 UGV의 State만 선택됨
                     * */
                    if (!ugv.IsBelongToGroup)
                    {
                        ugv.IsClicked = true;
                        selectUGVAndStateChangeLayout(ugv, "Red", id);
                    }
                    else
                    {
                        Group selectGroup = new Group();

                        ugv.IsClicked = true;

                        for (int i = 0; i < mapViewModel.MVCCGroupList.Count; i++)
                        {
                            Group tempGroup = mapViewModel.MVCCGroupList[i];

                            if (tempGroup.MemberList.Contains(ugv))
                            {
                                selectGroup = tempGroup;
                            }
                        }

                        for (int i = 0; i < selectGroup.MemberList.Count; i++)
                        {
                            UGV tempUGV = selectGroup.MemberList[i];

                            selectUGVAndStateChangeLayout(tempUGV, selectGroup.StateBorderBrush, tempUGV.Id);

                            tempUGV.IsGroupClicked = true;
                        }
                    }

                    refreshView();
                }
                else
                {
                    cancelSelectUGV();

                    refreshView();
                }
            }
        }

        private void AddMVCCUGVPathList(UGV ugv)
        {
            removeAllUGVPath(ugv);

            List<KeyValuePair<int, int>> pathList = ugv.PathList;

            for (int i = 0; i < pathList.Count; i++)
            {
                if (i == 0)
                    continue;

                KeyValuePair<int, int> beforePathTemp = pathList[i - 1];
                KeyValuePair<int, int> currentPathTemp = pathList[i];

                int startX = beforePathTemp.Key;
                int startY = beforePathTemp.Value;

                int endX = currentPathTemp.Key;
                int endY = currentPathTemp.Value;

                mapViewModel.MVCCUGVPathList.Add(new UGVPath(ugv.Id, startX, startY, endX, endY, ugv.UGVColor));
            }
        }

        // 현재 UGV의 UGV Path 전체를 지우는 기능
        private void removeAllUGVPath(UGV ugv)
        {
            globals.UGVStopCommandLock.EnterWriteLock();

            //if (ugv.PathList.Count != 0)
            //{
            List<KeyValuePair<int, int>> pathList = ugv.PathList;

            for (int i = mapViewModel.MVCCUGVPathList.Count - 1; i >= 0; i--)
            {
                UGVPath tempUGVPath = mapViewModel.MVCCUGVPathList[i] as UGVPath;

                if (tempUGVPath.Id.Equals(ugv.Id))
                {
                    mapViewModel.MVCCUGVPathList.Remove(tempUGVPath);
                }
            }

            refreshViewPath();
            //}

            globals.UGVStopCommandLock.ExitWriteLock();
        }

        //길찾기를 하고 난 뒤 다른 지점을 찍었을 때 도착Map에서 지우기
        private void RemoveEndPoint(UGV ugv, State UGVstate)
        {
            //도착 지점 좌표 지우기
            int startX, startY, endX, endY;
            if (ugv.PathList.Count != 0)
            {
                startX = ugv.PathList[0].Key / 15 - 2;
                startY = ugv.PathList[0].Value / 15 - 2;

                endX = ugv.PathList[0].Key / 15 + 2;
                endY = ugv.PathList[0].Value / 15 + 2;

                //범위 초과일 경우 설정
                if (startX < 0)
                {
                    endX -= startX;
                    startX = 0;
                }
                if (startY < 0)
                {
                    endY -= startY;
                    startY = 0;
                }

                if (endX > globals.rect_width / globals.x_grid)
                {
                    startX -= (endX - globals.rect_width / globals.x_grid);
                    endX = globals.rect_width;
                }
                if (endY > globals.rect_height)
                {
                    startY -= (endY - globals.rect_height / globals.y_grid);
                    endY = globals.rect_height;
                }

                globals.endPointMapLock.EnterWriteLock();

                for (int x = startX; x <= endX; x++)
                    for (int y = startY; y <= endY; y++)
                        globals.EndPointMap[y, x] = 0; //장애물은  설정  

                globals.endPointMapLock.ExitWriteLock();

            }
        }

        //그룹 길찾기 하고 난 뒤 도착지점 지우기
        private void RemoveEndPointInGroup(UGV ugv, State UGVstate)
        {
            globals.evasionInfoLock.EnterWriteLock();

            //도착 지점 좌표 지우기
            int startX, startY, endX, endY;
            if (ugv.PathList.Count != 0)
            {
                startX = UGVstate.EndPointX / 15 - 2;
                startY = UGVstate.EndPointY / 15 - 2;

                endX = UGVstate.EndPointX / 15 + 2;
                endY = UGVstate.EndPointY / 15 + 2;

                //범위 초과일 경우 설정
                if (startX < 0)
                {
                    endX -= startX;
                    startX = 0;
                }
                if (startY < 0)
                {
                    endY -= startY;
                    startY = 0;
                }

                if (endX > globals.rect_width / globals.x_grid)
                {
                    startX -= (endX - globals.rect_width / globals.x_grid);
                    endX = globals.rect_width;
                }
                if (endY > globals.rect_height)
                {
                    startY -= (endY - globals.rect_height / globals.y_grid);
                    endY = globals.rect_height;
                }

                globals.evasionInfoLock.EnterWriteLock();

                for (int x = startX; x <= endX; x++)
                    for (int y = startY; y <= endY; y++)
                        globals.obstacleInCollision[y, x] = 0; //장애물은  설정            
            }
            globals.evasionInfoLock.ExitWriteLock();

        }

        private void MakeGroup(object sender, KeyEventArgs e)
        {

            if (Keyboard.Modifiers == ModifierKeys.Shift || Keyboard.Modifiers == ModifierKeys.Alt)
                return;

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (mapViewModel.MVCCTempList.Count > 0)
                {
                    int groupNum = findGroupNum(e.Key);

                    if (groupNum != 0)
                        MakeGroup(groupNum);
                }
                else
                {
                    MessageBox.Show("그룹 대기열에 포함된 UGV가 없습니다.");
                }
            }

            // S 누르면 정지              
            else if (e.Key == Key.S)
            {
                // 정지시킬 UGV
                UGV stopUGV = null;
                // 정지시킬 UGV의 상태값
                State stopUGVState = null;

                for (int i = 0; i < mapViewModel.MVCCItemStateList.Count; i++)
                {
                    State tempState = mapViewModel.MVCCItemStateList[i];
                    UGV tempUGV = tempState.ugv;
                    if (tempUGV.IsClicked && tempState.IsDriving)
                    {
                        tempUGV.Command = "s";

                        stopUGV = tempUGV;
                        stopUGVState = tempState;

                        stopUGVState.IsDriving = false;

                        if (stopUGV.IsGroupClicked)
                            stopUGV.IsClicked = false;

                        bluetoothAndPathPlanning.connect(tempUGV, tempState);

                        removeAllUGVPath(stopUGV);
                        RemoveEndPoint(tempUGV, stopUGVState);
                        tempUGV.PathList.Clear();
                        //Console.WriteLine(" tempUGV.PathList.count = " + tempUGV.PathList.Count);
                        break;
                    }
                }

                if (stopUGV == null && stopUGVState == null)
                {
                    Dictionary<string, State> clickedUGVStateMap = new Dictionary<string, State>();

                    for (int i = 0; i < mapViewModel.MVCCItemStateList.Count; i++)
                    {
                        State tempState = mapViewModel.MVCCItemStateList[i];
                        UGV tempUGV = tempState.ugv;
                        if (tempUGV.IsGroupClicked)
                        {
                            clickedUGVStateMap.Add(tempUGV.Id, tempState);

                            stopUGV = tempUGV;
                        }
                    }

                    Group clickedGroup = new Group();

                    for (int i = 0; i < mapViewModel.MVCCGroupList.Count; i++)
                    {
                        Group tempGroup = mapViewModel.MVCCGroupList[i];
                        if (tempGroup.MemberList.Contains(stopUGV))
                        {
                            clickedGroup = tempGroup;
                            break;
                        }
                    }

                    for (int i = 0; i < clickedGroup.MemberList.Count; i++)
                    {
                        UGV tempUGV = clickedGroup.MemberList[i];

                        if (clickedUGVStateMap[tempUGV.Id].IsDriving)
                        {
                            tempUGV.Command = "s";

                            clickedUGVStateMap[tempUGV.Id].IsDriving = false;

                            bluetoothAndPathPlanning.connect(tempUGV, clickedUGVStateMap[tempUGV.Id]);

                            removeAllUGVPath(tempUGV);
                            RemoveEndPoint(tempUGV, clickedUGVStateMap[tempUGV.Id]);
                            tempUGV.PathList.Clear();
                            //Console.WriteLine(" tempUGV.PathList.count = " + tempUGV.PathList.Count);
                        }
                    }
                }

            }

            // 해당 그룹 번호를 누르면 해당그룹이 선택됨
            else
            {
                int groupNum = findGroupNum(e.Key);

                for (int i = 0; i < mapViewModel.MVCCGroupList.Count; i++)
                {
                    Group tempGroup = mapViewModel.MVCCGroupList[i];

                    if (tempGroup.Name.Equals("G" + groupNum))
                    {
                        cancelSelectUGV();
                        for (int j = 0; j < tempGroup.MemberList.Count; j++)
                        {
                            UGV tempUGV = tempGroup.MemberList[j];
                            selectUGVAndStateChangeLayout(tempUGV, tempGroup.StateBorderBrush, tempUGV.Id);
                            tempUGV.IsGroupClicked = true;
                        }
                    }
                    else
                    {
                        if (Keyboard.Modifiers != ModifierKeys.Control
                            && Keyboard.Modifiers != ModifierKeys.Alt
                            && Keyboard.Modifiers != ModifierKeys.Shift)
                        {
                            for (int j = 0; j < tempGroup.MemberList.Count; j++)
                            {
                                UGV tempUGV = tempGroup.MemberList[j];
                                cancelSelectUGV(tempUGV);
                            }
                        }
                    }
                }
                //내일오면 선택부분 오류 수정
                refreshView();
            }
        }

        private void MakeGroup(int groupNum)
        {
            string GroupName = "G" + groupNum;

            // 이미 존재하는 그룹인지 검사
            bool isExisted = false;
            for (int i = 0; i < mapViewModel.MVCCGroupList.Count; i++)
            {
                Group tempGroup = mapViewModel.MVCCGroupList[i];

                if (tempGroup.Name.Equals(GroupName))
                {
                    isExisted = true;
                }
            }

            if (!isExisted)
            {
                Group group = new Group(GroupName);

                group.StateBorderBrush = getGroupColor(groupNum);

                // 선택됬던 UGV들을 그룹에 포함시킴
                for (int i = 0; i < mapViewModel.MVCCTempList.Count; i++)
                {
                    UGV tempUGV = mapViewModel.MVCCTempList[i];
                    tempUGV.UGVStroke = group.StateBorderBrush;
                    tempUGV.IsClickedReadyBelongToGroup = false;

                    tempUGV.IsBelongToGroup = true;
                    tempUGV.IsGroupClicked = true;
                    tempUGV.GroupName = GroupName;

                    group.MemberList.Add(tempUGV);
                }

                for (int i = 0; i < group.MemberList.Count; i++)
                {
                    UGV tempUGV = group.MemberList[i];

                    for (int j = 0; j < mapViewModel.MVCCItemStateList.Count; j++)
                    {
                        State tempState = mapViewModel.MVCCItemStateList[j];

                        if (tempUGV.Id.Equals(tempState.ugv.Id))
                        {
                            tempState.StateBorderBrush = group.StateBorderBrush;
                        }
                    }
                }

                mapViewModel.MVCCGroupList.Add(group);

                mapViewModel.MVCCTempList.Clear();

                refreshView();
            }
            else
            {
                MessageBox.Show("이미 " + GroupName + "이 존재합니다.");
            }
        }

        // 선택한 UGV의 Layout을 변경해주는 기능
        private void selectUGVAndStateChangeLayout(UGV ugv, string color, string id)
        {
            ugv.UGVStrokeThickness = 2;
            ugv.UGVStroke = color;

            for (int i = 0; i < mapViewModel.MVCCItemStateList.Count; i++)
            {
                State tempState = mapViewModel.MVCCItemStateList[i];

                if (tempState.ugv.Id.Equals(id))
                {
                    tempState.StateBorderBrush = color;
                }
            }
        }

        // 특정 UGV의 선택을 해제하는 기능
        private void cancelSelectUGV(UGV ugv)
        {
            ugv.IsClicked = false;
            ugv.IsClickedReadyBelongToGroup = false;
            ugv.IsGroupClicked = false;

            ugv.UGVStroke = "Transparent";

            for (int j = 0; j < mapViewModel.MVCCItemStateList.Count; j++)
            {
                State tempState = mapViewModel.MVCCItemStateList[j];

                if (tempState.ugv.Id.Equals(ugv.Id))
                {
                    tempState.StateBorderBrush = "#78C8FF";
                }
            }

        }

        // UGV 전체의 선택을 해제하는 기능
        private void cancelSelectUGV()
        {
            // UGV가 아닌 다른곳을 클릭했을경우 선택이 해제된다.
            for (int i = 0; i < mapViewModel.MVCCItemList.Count; i++)
            {
                if (!(mapViewModel.MVCCItemList[i] is UGV))
                    continue;

                UGV tempUGV = mapViewModel.MVCCItemList[i] as UGV;

                tempUGV.IsClicked = false;
                tempUGV.IsClickedReadyBelongToGroup = false;
                tempUGV.IsGroupClicked = false;

                tempUGV.UGVStroke = "Transparent";
            }

            for (int i = 0; i < mapViewModel.MVCCItemStateList.Count; i++)
            {
                State tempState = mapViewModel.MVCCItemStateList[i];

                tempState.StateBorderBrush = "#78C8FF";
            }

            // 그룹에 포함되지 않았지만, 대기중인 UGV전체를 해제한다.
            mapViewModel.MVCCTempList.Clear();
        }

        // UGV가 그룹 대기열에 들어갔을때, 그것을 다시 선택할때, UGV가 해제됨.
        private void RemoveSelectedUGVInGroupTempList(UGV ugv)
        {
            // 그룹에 포함되지 않았지만, 대기중이 UGV들 중에 선택한 UGV가 해제된다.
            for (int i = 0; i < mapViewModel.MVCCTempList.Count; i++)
            {
                UGV tempUGV = mapViewModel.MVCCTempList[i];

                if (tempUGV.Id.Equals(ugv.Id))
                {
                    mapViewModel.MVCCTempList.Remove(tempUGV);
                }
            }
        }

        private void refreshView()
        {
            // View에 반영
            (FindResource("MVCCItemSrc") as CollectionViewSource).View.Refresh();

            (FindResource("UGVStateSrc") as CollectionViewSource).View.Refresh();

            (FindResource("UGVGroupSrc") as CollectionViewSource).View.Refresh();


        }

        private void refreshViewPath()
        {
            refreshView();
            (FindResource("UGVPathSrc") as CollectionViewSource).View.Refresh();
        }

        // 숫자키에 대응되는 그룹의 번호
        private int findGroupNum(Key key)
        {
            switch (key)
            {
                case Key.D1: return 1;
                case Key.D2: return 2;
                case Key.D3: return 3;
                case Key.D4: return 4;
                default: return 0;
            }
        }

        private Group findClickedGroup()
        {
            for (int i = 0; i < mapViewModel.MVCCGroupList.Count; i++)
            {
                Group tempGroup = mapViewModel.MVCCGroupList[i];

                for (int j = 0; j < tempGroup.MemberList.Count; j++)
                {
                    UGV tempUGV = tempGroup.MemberList[j];
                    if (tempUGV.IsBelongToGroup && tempUGV.IsGroupClicked)
                    {
                        return tempGroup;
                    }
                }
            }

            return null;
        }

        private string getGroupColor(int groupNum)
        {
            switch (groupNum)
            {
                case (int)GroupColor.Orange:
                    return GroupColor.Orange.ToString();
                case (int)GroupColor.Pink:
                    return GroupColor.Pink.ToString();
                case (int)GroupColor.White:
                    return GroupColor.White.ToString();
                case (int)GroupColor.Yellow:
                    return GroupColor.Yellow.ToString();

                default:
                    return "Green";
            }
        }
    }
}
