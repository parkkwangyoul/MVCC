using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private bool isGroupMode = false;
        public bool IsGroupMode
        {
            get { return isGroupMode; }
            set
            {
                isGroupMode = value;
            }
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
    }
}
