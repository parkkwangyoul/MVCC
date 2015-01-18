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

        //템플릿 매칭 너비
        private int _TemplateWidth = 50;
        public int TemplateWidth
        {
            get { return _TemplateWidth; }
            set
            {
                _TemplateWidth = value;
            }
        }

        //템플릿 매칭 높이
        private int _TemplateHeight = 50;
        public int TemplateHeight
        {
            get { return _TemplateHeight; }
            set
            {
                _TemplateHeight = value;
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
