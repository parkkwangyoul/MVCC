using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCC.Model
{

    public class ModelBase
    {
    }

    public class MVCCItem : ModelBase
    {
        public MVCCItem(){ }
        
        public MVCCItem(double width, double height, double x, double y)
        {
            this.Width = width;
            this.Height = height;
            this.X = x;
            this.y = y;
        }

        /**
         * return 각 Item의 고유한 ID
         * */

        private string id;
        public string Id
        {
            get { return id; }
            set
            {
                id = value;
            }

        }

        /**
         * return Item의 Width
         * */
        private double width;
        public double Width
        {
            get { return width; }
            set
            {
                width = value;
            }
        }

        /**
         * return Item의 Height
         * */
        private double height;
        public double Height
        {
            get { return height; }
            set
            {
                height = value;
            }
        }

        /**
         * return Item의 중심 x 좌표
         **/
        private double x;
        public double X
        {
            get { return x; }
            set
            {
                x = value;
            }
        }

        /**
         *  return Item의 중심 y 좌표
         **/
        private double y;
        public double Y
        {
            get { return y; }
            set
            {
                y = value;
            }
        }

        public override string ToString() { return "Item"; }
    }
}
