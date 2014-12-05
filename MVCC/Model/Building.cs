using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCC.Model
{
    public class Building
    {
        public Building(string id, double width, double height, double x, double y)
        {
            this.id = id;
            this.width = width;
            this.height = height;
            this.x = x;
            this.y = y;
        }

        /**
         * return Building ID
         * */
        private string id
        {
            get;
            set;
        }

        /**
         * return Building의 Width
         * */
        public double width
        {
            get;
            set;
        }

        /**
         * return Building Height
         * */
        public double height
        {
            get;
            set;
        }

        /**
         * return Building 좌측 상단 x좌표
         * */
        public double x
        {
            get;
            set;
        }

        /**
         * return Building 좌측 상단 y좌표
         * */
        public double y
        {
            get;
            set;
        }

        /**
         * Building 의 정보
         * */
        public void toString
        {
            get
            {
                Console.WriteLine("id : " + id + "\n");
                Console.WriteLine("width : " + width + "\n");
                Console.WriteLine("height : " + height + "\n");
                Console.WriteLine("x : " + x + "\n");
                Console.WriteLine("y : " + y + "\n");
            }
        }
    }
}
