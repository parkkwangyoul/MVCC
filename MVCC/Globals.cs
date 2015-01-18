using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;

using MVCC.Model;

namespace MVCC
{
    public class Globals
    {
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
        private int _x_grid = 20;
        public int x_grid
        {
            get { return _x_grid; }
            set
            {
                _x_grid = value;
            }
        }

        //y축 gird 간격
        private int _y_grid = 20;
        public int y_grid
        {
            get { return _y_grid; }
            set
            {
                _y_grid = value;
            }
        }

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

