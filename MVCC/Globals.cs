using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;
using System.Threading;
using MVCC.Model;
using System.IO.Ports;

namespace MVCC
{
    public class Globals
    {
        public int[,] Map_obstacle; //모든 맵의 정보
        public int[,] onlyObstacle; //건물 장애물만의 정보
        public int[,] pre_onlyObstacle; //onlyPbstacle의 이전 정보
        public int[,] EndPointMap; //도착지점 정보
        public int[,] obstacleInCollision; //충돌 위기 일때 장애물 정보

        public char[,] UGVsCollisionPath; //Ugv들의 충돌 path 범위정보 

        public struct UGV_Info
        {
            public string UGV_Id;
            public UGV ugv;
        }

        public UGV_Info sortInfo; // Group 차량 우선 순위 정보
        public List<UGV_Info> sortInfoList = new List<UGV_Info>(); // Group 차량 우선 순위 정보 리스트

        public List<State> individualsortInfo = new List<State>(); // 개인 차량 우선 순위 정보 리스트
        
       
        public List<KeyValuePair<int, int>> evasionInfo = new List<KeyValuePair<int,int>>();
        public List<KeyValuePair<int, int>> pre_evasionInfo = new List<KeyValuePair<int,int>>(); //차량들 끼리의 충돌 위기 정보

        public List<KeyValuePair<int, int>> UGVsConflictInofo = new List<KeyValuePair<int, int>>(); //차량들 끼리의 충돌한 정보
        public List<int> UGVandObstacleCollisionInofo = new List<int>(); //차량과 장애물 충돌한 정보


        public int[] MovementCommandCount = new int[4];
        public bool[] direction_check = new bool[4];

        public int[] directionForceCountX = { 10, 10, 10, 10 };
        public bool[] directionForceCheckX = new bool[4];
        public int[] directionForceCountY = { 10, 10, 10, 10 };
        public bool[] directionForceCheckY = new bool[4];


        public string[] rotate = new string[4];
        public int[] angle = new int[4];
        public int[] direction = new int[4];
        public int[] first_point_x = new int[4];
        public int[] first_point_y = new int[4];

        public ReaderWriterLockSlim mapObstacleLock = new ReaderWriterLockSlim();
        public ReaderWriterLockSlim bluetoothLock = new ReaderWriterLockSlim();
        public ReaderWriterLockSlim UGVStopCommandLock = new ReaderWriterLockSlim();
        public ReaderWriterLockSlim endPointMapLock = new ReaderWriterLockSlim();
        public ReaderWriterLockSlim evasionInfoLock = new ReaderWriterLockSlim(); //obstacleInCollision Map을 위한
        public ReaderWriterLockSlim bluetoothConnectLock = new ReaderWriterLockSlim(); //pathFinder의 Lock을 위한
        public ReaderWriterLockSlim UGVsCollisionPathLock = new ReaderWriterLockSlim(); //충돌 path 구하는 곳의 lock을 위한
        
        
        private static Globals _Instance;
        public static Globals Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new Globals();
                return _Instance;
            }
        }

        private Globals()
        {
        }

        private UGV nowSelectedUGV;

        public UGV NowSelectedUGV
        {
            get
            {
                if (nowSelectedUGV == null)
                {
                    nowSelectedUGV = new UGV();              
                }

                return nowSelectedUGV;
            }

            set
            {
                nowSelectedUGV = value;
            }
        }

        //영상 너비
        private int _ImageWidth = 640;

        public int ImageWidth
        {
            get { return _ImageWidth; }
            set
            {
                _ImageWidth = value;
            }
        }

        //영상 높이
        private int _ImageHeight = 480;
        public int ImageHeight
        {
            get { return _ImageHeight; }
            set
            {
                _ImageHeight = value;
            }
        }

        //템플릿 매칭 너비
        private int _TemplateWidth;
        public int TemplateWidth
        {
            get { return _TemplateWidth; }
            set
            {
                _TemplateWidth = value;
            }
        }

        //템플릿 매칭 높이
        private int _TemplateHeight;
        public int TemplateHeight
        {
            get { return _TemplateHeight; }
            set
            {
                _TemplateHeight = value;
            }
        }

        //x축 gird 간격
        private int _x_grid = 15;
        public int x_grid
        {
            get { return _x_grid; }
            set
            {
                _x_grid = value;
            }
        }

        //y축 gird 간격
        private int _y_grid = 15;
        public int y_grid
        {
            get { return _y_grid; }
            set
            {
                _y_grid = value;
            }
        }

        //경계 사각형 시작 x좌표
        private int _rect_x = 20;

        public int rect_x
        {
            get { return _rect_x; }
            set
            {
                _rect_x = value;
            }
        }

        //경계 사각형 시작 y좌표
        private int _rect_y = 120;
        public int rect_y
        {
            get { return _rect_y; }
            set
            {
                _rect_y = value;
            }
        }

        //경계 사각형 너비
        private int _rect_width = 540;

        public int rect_width
        {
            get { return _rect_width; }
            set
            {
                _rect_width = value;
            }
        }

        //경계 사각형 높이
        private int _rect_height = 360;
        public int rect_height
        {
            get { return _rect_height; }
            set
            {
                _rect_height = value;
            }
        }

        // UGV별로 블루투스 설정을 저장하는 Dictionary
        private Dictionary<string, UGV> _UGVSettingDictionary;
        public Dictionary<string, UGV> UGVSettingDictionary
        {
            get 
            {
                if (_UGVSettingDictionary == null)
                    _UGVSettingDictionary = new Dictionary<string, UGV>();

                return _UGVSettingDictionary;
            }
        }

        // 블루투스 통신하는 시리얼포트
        private List<SerialPort> serialPortList = new List<SerialPort>();
        public  List<SerialPort> SerialPortList
        {
            get
            {
                if (serialPortList.Count != 4)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        serialPortList.Add(new SerialPort());
                    }
                }

                return serialPortList;
            }
        }
    }
}

