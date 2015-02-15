using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCC.Model
{
    class UGVPath : ModelBase
    {
        public UGVPath(string id, int startX, int startY, int endX, int endY, string stroke)
        {
            this.id = id;
            this.startX = startX;
            this.startY = startY;
            this.endX = endX;
            this.endY = endY;
            this.stroke = stroke;
        }
        
        // Path가 필요한 해당 UGV Id
        private string id;
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        // 출발 X 좌표
        private int startX;
        public int StartX
        {
            get { return startX; }
            set { startX = value; }
        }

        // 출발 Y 좌표
        private int startY;
        public int StartY
        {
            get { return startY; }
            set { startY = value; }
        }

        // 도착 X 좌표
        private int endX;
        public int EndX
        {
            get { return endX; }
            set { endX = value; }
        }

        // 도착 Y 좌표
        private int endY;
        public int EndY
        {
            get { return endY; }
            set { endY = value; }
        }

        // Path Color
        private string stroke = "Gray";
        public string Stroke
        {
            get { return stroke; }
            set { stroke = value; }
        }

        public override string ToString(){ return "UGVPath"; }
    }
}
