using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCC.Model
{
    public class State : ModelBase
    {
        public State(UGV ugv)
        {
            this.ugv = ugv;
        }

        private UGV _ugv;
        public UGV ugv
        {
            get { return _ugv; }
            set
            {
                _ugv = value;
            }

        }

        // 선택 됬을때, 색깔을 나타냄
        private string stateBorderBrush = "#78C8FF";
        public string StateBorderBrush
        {
            get { return stateBorderBrush; }
            set
            {
                stateBorderBrush = value;
            }
        }

        private bool bluetoothOnOff = false;
        public bool BluetoothOnOff
        {
            get { return bluetoothOnOff; }
            set
            {
                bluetoothOnOff = value;
            }
        }

        // 블루투스 상태 색깔을 나타냄
        //private string bluetoothPath = "/Resource/bluetooth_off.png";
        public string BluetoothPath
        {
            get
            {
                if (!bluetoothOnOff)
                    return "/Resource/bluetooth_off.png";
                else
                    return "/Resource/bluetooth_on.png";
            }
        }

        public override string ToString()
        {
            return "State";
        }
    }
}
