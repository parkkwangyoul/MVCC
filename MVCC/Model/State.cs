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

        // 주행 상태를 나타냄
        private bool isDriving = false;
        public bool IsDriving
        {
            get { return isDriving; }
            set
            {
                isDriving = value;
            }
        }

        // 현재 지나고 있는 좌표
        private int[,] currentPoint;
        public int[,] CurrentPoint
        {
            get { return currentPoint; }
            set
            {
                currentPoint = value;
            }
        }

        // 지정된 도착 X좌표를 나타냄
        private int endPointX;
        public int EndPointX
        {
            get { return endPointX; }
            set
            {
                endPointX = value;
            }
        }

        // 지정된 도착 Y좌표를 나타냄
        private int endPointY;
        public int EndPointY
        {
            get { return endPointY; }
            set
            {
                endPointY = value;
            }
        }

        // 블루투수 상태
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
