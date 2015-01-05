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
        public UGV(string id, double width, double height, double x, double y) : base(id, width, height, x, y) { }        

        // 클릭 했을때 상태
        private bool isClicked = false;
        public bool IsClicked
        {
            get { return isClicked; }
            set { isClicked = value; }
        }

        private bool isBluetoothConnected;
        public bool IsBluetoothConnected
        {
            get { return isBluetoothConnected; }
            set
            {
                isBluetoothConnected = value;
            }
        }

        private bool isBelongToGroup = false;
        public bool IsBelongToGroup
        {
            get { return isBelongToGroup; }
            set
            {
                isBelongToGroup = value;
            }
        }

        private int groupNum;
        public int GroupNum
        {
            get { return groupNum; }
            set
            {
                groupNum = value;
            }
        }

        // 선택했을때, 점선으로 바뀜
        private int uGVStrokeThickness = 0;
        public int UGVStrokeThickness
        {
            get { return uGVStrokeThickness; }
            set
            {
                uGVStrokeThickness = value;
            }
        }

        private string uGVStroke = "#78C8FF";
        public string UGVStroke
        {
            get { return uGVStroke; }
            set
            {
                uGVStroke = value;
            }
        }

        // 각 UGV별 색깔이 다름
        private string uGVColor = "#78C8FF";
        public string UGVColor
        {
            get { return uGVColor; }
            set
            {
                uGVColor = value;
            }
        }

        #region Bluetooth Property
        /**
         * Bluetooth 관련 속성들 
         **/

        private int baudrate;
        public int Baudrate
        {
            get { return baudrate; }
            set
            {
                baudrate = value;
            }
        }

        private int databit;
        public int Databit
        {
            get { return databit; }
            set
            {
                databit = value;
            }
        }

        private string portName;
        public string PortName
        {
            get { return portName; }
            set
            {
                portName = value;
            }
        }

        #endregion Bluetoot Property

        public override string ToString() { return "UGV"; }
    }
}
