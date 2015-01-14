using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCC.Utill
{
    public class GlobalClass
    {
        private static readonly GlobalClass instance = new GlobalClass();

        private GlobalClass() { }

        public static GlobalClass Instance
        {
            get
            {
                return instance;
            }
        }

        //템플릿 매칭 너비
        private int _TemplateWidth = 120;
        public int TemplateWidth
        {
            get { return _TemplateWidth; }
            set
            {
                _TemplateWidth = value;
            }
        }

        //템플릿 매칭 높이
        private int _TemplateHeight = 120;
        public int TemplateHeight
        {
            get { return _TemplateHeight; }
            set
            {
                _TemplateHeight = value;
            }
        }
    }
}
