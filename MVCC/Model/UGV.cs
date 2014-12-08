using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCC.Model
{
    public class UGV : MVCCItem
    {
        public UGV() : base() { }
        public UGV(double width, double height, double x, double y) : base(width, height, x, y) { }
        
        public bool isBluetoothConnect()
        {
            return false;
        }

        public override string ToString() { return "UGV"; }
    }
}
