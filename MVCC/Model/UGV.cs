﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCC.Model
{
    public class UGV : MVCCItem
    {
        // 빈 생성자
        public UGV() : base() { }

        // UGV Setting 상태를 저장하기 위한 임시 생성자
        public UGV(string id) { this.Id = id; }

        // UGV를 만들기 위한 생성자
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

        // 그룹에 포함되기 전에 선택된 UGV를 의미
        private bool isClickedReadyBelongToGroup = false;
        public bool IsClickedReadyBelongToGroup
        {
            get { return isClickedReadyBelongToGroup; }
            set
            {
                isClickedReadyBelongToGroup = value;
            }
        }

        // 그룹에 포함 되어있는지 아닌지
        private bool isBelongToGroup = false;
        public bool IsBelongToGroup
        {
            get { return isBelongToGroup; }
            set
            {
                isBelongToGroup = value;
            }
        }

        // 그룹에 포함되어 있다면, 그 그룹의 이름
        private string groupName;
        public string GroupName
        {
            get { return groupName; }
            set
            {
                groupName = value;
            }
        }

        // 그룹에 속해있는 UGV가 선택 되었을때, 그룹 전체가 선택되며, 이 속성이 true로 바뀜.
        private bool isGroupClicked = false;
        public bool IsGroupClicked
        {
            get { return isGroupClicked; }
            set
            {
                isGroupClicked = value;
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

        private string comPort;
        public string ComPort
        {
            get { return comPort; }
            set { comPort = value; }
        }

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
