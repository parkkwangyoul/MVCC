using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCC.Model
{
    class UGVPath
    {
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
        private int EndX
        {
            get { return endX; }
            set { endX = value; }
        }

        // 도착 Y 좌표
        private int endY;
        private int EndY
        {
            get { return endY; }
            set { endY = value; }
        }

        private List<KeyValuePair<int, int>> pathList;
        public List<KeyValuePair<int, int>> PathList
        {
            get
            {
                if (startX == endX && startY == endY)
                    return null;
                else if (pathList == null)
                    return new List<KeyValuePair<int, int>>();
                else
                    return pathList;
            }

            set
            {
                pathList = value;
            }
        }
    }
}
