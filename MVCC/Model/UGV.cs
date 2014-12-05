using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCC.Model
{
    public class UGV
    {
        public UGV(string id, double width, double height, double x, double y)
        {
            this.id = id;
            this.width = width;
            this.height = height;
            this.x = x;
            this.y = y;
        }

        /**
         * return 각 UGV의 고유한 ID
         * */
        private string id
        {
            get;
            set;
        }

        /**
         * return UGV의 Width
         * */
        public double width
        {
            get;
            set;
        }

        /**
         * return UGV Height
         * */
        public double height
        {
            get;
            set;
        }

        /**
         * return UGV 중심 x 좌표
         **/
        public double x
        {
            get;
            set;
        }

        /**
         *  return UGV 중심 y 좌표
         **/
        public double y
        {
            get;
            set;
        }

        /**
         * return 해당 UGV의 정보 
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
