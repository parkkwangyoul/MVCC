using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;
using System.Threading;
using MVCC.Model;

namespace MVCC
{
    public class Globals
    {
        public int[,] Map_obstacle;
        public int[,] pre_Map_obstacle;
        public int[] direction = new int[4];

        public bool mutex = false;

        public ReaderWriterLock rwl = new ReaderWriterLock();
        public int readerTimeouts = 0;
        public int writerTimeouts = 0;
        public int reads = 0;
        public int writes = 0;

        public ReaderWriterLockSlim theLock = new ReaderWriterLockSlim();

        //public Mutex mutex = new Mutex();

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

        //경계 사각형 너비
        private int _rect_width = 600;

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
    }
}

