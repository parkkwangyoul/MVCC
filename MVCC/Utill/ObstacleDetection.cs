using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace MVCC.Utill
{
    class ObstacleDetection
    {
        int sub_check;
        int sub_count;
        Globals globals = Globals.Instance; //globalsal 변수를 위해

        //윤곽선 검출
        public Image<Gray, Byte> cannyEdge(Image<Bgr, Byte> img, Rectangle[] tracking_rect)
        {
            Image<Gray, Byte> canny = img.Convert<Gray, Byte>().PyrDown().PyrUp();
            CvInvoke.cvCanny(canny, canny, 65, 30, 3);

            Image<Bgr, Byte> sub_ROI_image = canny.Convert<Bgr, Byte>();

            //영상에서 차량 범위를 빼고 ROI만들기
            for (int i = 0; i < 4; i++)
            {
                int pos_x = tracking_rect[i].X;
                int pos_y = tracking_rect[i].Y;

                //이미지가 범위를 벗어날경우 처리
                if (pos_x < 0)
                    pos_x = 0;
                if (pos_y < 0)
                    pos_y = 0;

                if (pos_x + tracking_rect[i].Width > img.Width)
                    pos_x = img.Width - tracking_rect[i].Width;
                if (pos_y + tracking_rect[i].Height > img.Height)
                    pos_y = img.Height - tracking_rect[i].Height;

                for (int x = pos_x; x < pos_x + tracking_rect[i].Width; x++)
                    for (int y = pos_y; y < pos_y + tracking_rect[i].Height; y++)
                        sub_ROI_image[y, x] = new Bgr(0, 0, 0);
            }

            return sub_ROI_image.Convert<Gray, Byte>();
        }

        //그리드 그림
        public int drowGrid(Image<Gray, Byte> canny_img, Image<Bgr, Byte> img, int[,] Map_obstacle)
        {
            /*
            //가로 줄 (y는 세로 간격)
            for (int y = globals.y_grid; y < img.Height; y += globals.y_grid)
            {
                Point start = new Point(0, y);
                Point end = new Point(img.Width - 1, y);
                LineSegment2D line = new LineSegment2D(start, end);
                img.Draw(line, new Bgr(0, 255, 0), 1);
            }

            //세로 줄 (x는 가로 간격)
            for (int x = globals.x_grid; x < img.Width; x += globals.x_grid)
            {
                Point start = new Point(x, 0);
                Point end = new Point(x, img.Height - 1);
                LineSegment2D line = new LineSegment2D(start, end);
                img.Draw(line, new Bgr(0, 255, 0), 1);
            }
            */

            return obstacle_grid_fill(canny_img, img, Map_obstacle);

        }


        //장애물 있는곳 그리드 색칠하기
        public int obstacle_grid_fill(Image<Gray, Byte> canny_img, Image<Bgr, Byte> img, int[,] Map_obstacle)
        {
            Image<Bgr, Byte> colorCount = canny_img.Convert<Bgr, Byte>(); //픽셀수 세기 위해
            int count = 0;
            bool grid_check = false;

            for (int x = 0; x < canny_img.Width; x++)
            {
                for (int y = 0; y < canny_img.Height; y++)
                {
                    if (!colorCount[y, x].Equals(new Bgr(0, 0, 0)))
                    {
                        for (int pos_x = x - globals.x_grid; pos_x < x; pos_x++)
                        {
                            for (int pos_y = y - globals.y_grid; pos_y < y; pos_y++)
                            {
                                if (pos_x % globals.x_grid == 0 && pos_y % globals.y_grid == 0)
                                {
                                    int t_x = pos_x;
                                    int t_y = pos_y;

                                    if (pos_x < 0)
                                        t_x = 0;
                                    else
                                        t_x = pos_x / globals.x_grid;

                                    if (pos_y < 0)
                                        t_y = 0;
                                    else
                                        t_y = pos_y / globals.y_grid;

                                    Map_obstacle[t_y, t_x] = 1;
                                    Rectangle rect = new Rectangle(pos_x, pos_y, globals.x_grid, globals.y_grid);
                                    img.Draw(rect, new Bgr(0, 0, 255), 1);
                                    grid_check = true;

                                    y = pos_y + globals.y_grid;
                                }
                                if (grid_check == true)
                                    break;
                            }
                            if (grid_check == true)
                            {
                                grid_check = false;
                                break;
                            }
                        }
                    }
                }
            }
            return count;
        }


        //영상 변화를 확인하는 함수
        public Image<Gray, Byte> sub_image(Image<Gray, Byte> cannyRes, Image<Gray, Byte> pre_image, Image<Gray, Byte> dst_image)
        {
            Image<Bgr, Byte> pix_img;
            int pix_count = 0;

            dst_image = cannyRes - pre_image;
            pix_img = dst_image.Convert<Bgr, Byte>();

            for (int x = 0; x < cannyRes.Width; x++)
            {
                for (int y = 0; y < cannyRes.Height; y++)
                {
                    if (!pix_img[y, x].Equals(new Bgr(0, 0, 0)))
                        pix_count++;
                }
            }
            sub_check++;

            if (sub_check > 1)
            {
                if (sub_count + 200 <= pix_count)
                    System.Console.WriteLine(" 장애물생김!!! pix_count = " + pix_count + " sub_count = " + sub_count);
                else if (sub_count - 200 >= pix_count)
                    System.Console.WriteLine(" 장애물없어짐!!! pix_count = " + pix_count + " sub_count = " + sub_count + "\n");

                if (sub_check >= 1000)
                    sub_check = 1;
            }

            sub_count = pix_count;

            //System.Windows.MessageBox.Show("pix_count = " + pix_count);
            //System.Console.WriteLine("pix_count = " + pix_count);

            return dst_image;

        }
    }

}
