using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Cvb;
using System.Drawing;
using MVCC.Model;

namespace MVCC.Utill
{
    class ObstacleDetection
    {
        int sub_check;
        int sub_count;
        int blob_indenti_count;
        Globals globals = Globals.Instance; //globalsal 변수를 위해
        List<Object> blob_indenti_list = new List<Object>();
        List<Building> building_list = new List<Building>();

        //윤곽선 검출, blob할 이미지 만들기
        public Image<Gray, Byte> cannyEdge(Image<Bgr, Byte> img, Rectangle[] tracking_rect, Image<Bgr, Byte> blob_image)
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
                    {
                        sub_ROI_image[y, x] = new Bgr(0, 0, 0);
                        blob_image[y, x] = new Bgr(0, 0, 0); //canny 할떄
                        blob_image[y, x] = new Bgr(255, 255, 255); //blob에서 하얀색으로 할때
                    }
            }

            return sub_ROI_image.Convert<Gray, Byte>();
        }

        public object[] detectBlob(Image<Bgr, Byte> blob_image, int[,] Map_obstacle, object[] blob_info)
        {
            int blob_count = 0;

            Object[] blob_indenti = new Object[6]; // [0]indetifier [2]color [3] width [4] height [5] x [6] y  
            
            //라온제나 좁은 곳간 test 범위 지정
            for (int x = 0; x < globals.ImageWidth; x++)
                for (int y = 0; y < globals.ImageHeight; y++)
                    if (!(x >= 221 && x < 220 + 200 && y >= 11 && y < 10 + 380))
                        blob_image[y, x] = new Bgr(255, 255, 255);

            Image<Bgr, Byte> _blobsImg = blob_image.Clone();
            Image<Gray, Byte> greyImg = _blobsImg.Convert<Gray, Byte>().PyrDown().PyrUp();
            //Image<Gray, Byte> greyThreshImg = greyImg.ThresholdBinaryInv(new Gray(87), new Gray(255));

            blob_info[1] = greyImg.ThresholdBinaryInv(new Gray(87), new Gray(255));

            CvBlobs resultingImgBlobs = new CvBlobs();
            CvBlobDetector bDetect = new CvBlobDetector();
            bDetect.Detect((Image<Gray, Byte>)blob_info[1], resultingImgBlobs);

            Image<Bgr, Byte> temp_img = ((Image<Gray, Byte>)blob_info[1]).Convert<Bgr, Byte>();

            foreach (CvBlob targetBlob in resultingImgBlobs.Values)
            {
                if (targetBlob.Area > 100 && targetBlob.Area < 1500)
                {           
                    string temp;
                    bool is_check = false;
                    if ((temp = obstacle_colorCheck(blob_image, targetBlob.Area, targetBlob.BoundingBox.X, targetBlob.BoundingBox.Y, targetBlob.BoundingBox.Width, targetBlob.BoundingBox.Height)) == "null")
                    {
                        for (int x = (int)targetBlob.BoundingBox.X; x < (int)targetBlob.BoundingBox.X + targetBlob.BoundingBox.Width; x++)
                            for (int y = (int)targetBlob.BoundingBox.Y; y < (int)targetBlob.BoundingBox.Y + targetBlob.BoundingBox.Height; y++)
                                temp_img[y, x] = new Bgr(0, 0, 0);
                        continue;
                    }

                    foreach (Building building in building_list)
                    {
                        if (targetBlob.BoundingBox.X - targetBlob.BoundingBox.Width / 2 < building.X && building.X < targetBlob.BoundingBox.X + targetBlob.BoundingBox.Width / 2
                            && targetBlob.BoundingBox.Y - targetBlob.BoundingBox.Height / 2 < building.Y && building.Y < targetBlob.BoundingBox.Y + targetBlob.BoundingBox.Height / 2
                            && building.BuildingColor == temp)
                        {
                            building.X = targetBlob.BoundingBox.X;
                            building.Y = targetBlob.BoundingBox.Y;

                            building.Width = targetBlob.BoundingBox.Width;
                            building.Height = targetBlob.BoundingBox.Height;
                            is_check = true;
                            break;
                        }
                    }

                    if (is_check == true)
                        continue;
                    /*
                    blob_indenti[0] = blob_indenti_count++;
                    blob_indenti[1] = temp;            
                    blob_indenti[2] = targetBlob.BoundingBox.Width;
                    blob_indenti[3] = targetBlob.BoundingBox.Height;
                    blob_indenti[5] = (int)targetBlob.BoundingBox.X;
                    blob_indenti[6] = (int)targetBlob.BoundingBox.Y;           
                    blob_indenti_list.Add(blob_indenti);
                    */
                    building_list.Add(new Building("B" + blob_indenti_count++, (double)targetBlob.BoundingBox.Width, (double)targetBlob.BoundingBox.Height, targetBlob.BoundingBox.X, targetBlob.BoundingBox.Y));

                   
                    blob_image.Draw(targetBlob.BoundingBox, new Bgr(0, 255, 0), 1);
                    blob_count++;
                }
                else
                //if(targetBlob.Area < 50)
                {
                    for (int x = (int)targetBlob.BoundingBox.X; x < (int)targetBlob.BoundingBox.X + targetBlob.BoundingBox.Width; x++)
                        for (int y = (int)targetBlob.BoundingBox.Y; y < (int)targetBlob.BoundingBox.Y + targetBlob.BoundingBox.Height; y++)
                            temp_img[y, x] = new Bgr(0, 0, 0);
                }
            }

            blob_info[1] = temp_img.Convert<Gray, Byte>();
            blob_info[0] = blob_count;
            blob_info[2] = blob_indenti_list;

            return blob_info;
        }

        public List<Building> get_building()
        {
            return building_list;
        }

        //보라 0 101 133 67 137 188
        public string obstacle_colorCheck(Image<Bgr, Byte> image, int totalPicxel, int x, int y, int width, int height)
        {
            if (obstacle_YccColorCheck(image, totalPicxel, x, y, width, height, 0, 101, 133, 67, 137, 188) == 1) //보라
                return "purple";
            else if (obstacle_YccColorCheck(image, totalPicxel, x, y, width, height, 0, 133, 20, 255, 160, 97) == 1) //노랑
                return "yellow";
            else
                return "null";
        }

        public int obstacle_YccColorCheck(Image<Bgr, Byte> iamge, int totalPicxel, int pos_x, int pos_y, int img_width, int img_height, int min1, int min2, int min3, int max1, int max2, int max3)
        {
            int pixCount = 0;

            Image<Ycc, Byte> YCrCbFrame = iamge.Convert<Ycc, Byte>(); //YCrCb 변환
            Image<Gray, byte> colorSetting = new Image<Gray, byte>(YCrCbFrame.Width, YCrCbFrame.Height); //Ycc범위로 뽑아낸 것을 gray로 바꿔서 수축팽창 하기 위해

            Ycc YCrCb_min = new Ycc(min1, min2, min3);
            Ycc YCrCb_max = new Ycc(max1, max2, max3);   //blue 색 범위

            colorSetting = YCrCbFrame.InRange((Ycc)YCrCb_min, (Ycc)YCrCb_max); //색 범위 설정

            StructuringElementEx rect_12 = new StructuringElementEx(12, 12, 6, 6, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_RECT);
            CvInvoke.cvErode(colorSetting, colorSetting, rect_12, 1);
            StructuringElementEx rect_6 = new StructuringElementEx(6, 6, 3, 3, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_RECT);
            CvInvoke.cvDilate(colorSetting, colorSetting, rect_6, 2); //수축 팽창

            Image<Bgr, Byte> colorCount = colorSetting.Convert<Bgr, Byte>(); //픽셀수 세기 위해

            for (int x = pos_x; x < pos_x + img_width; x++)
            {
                for (int y = pos_y; y < pos_y + img_height; y++)
                {
                    if (colorCount[y, x].Equals(new Bgr(255, 255, 255)))
                    {
                        pixCount++;
                        if (totalPicxel / 10 <= pixCount) //일정 픽섹 이상시 색상배열 변경후 종료
                            return 1;
                    }
                }
            }

            System.Console.WriteLine("totalPicxel = " + totalPicxel + "pixCount = " + pixCount + " pos_x = " + pos_x + " pos_y = " + pos_y);
            return -1;
        }

        //그리드 그림 (그냥 canny만 했을때)
        public int drowGrid(Image<Gray, Byte> canny_img, Image<Bgr, Byte> img, int[,] Map_obstacle)
        {
            /*
            //가로 줄 (y는 세로 간격)
            for (int y = glo.y_grid; y < img.Height; y += glo.y_grid)
            {
                Point start = new Point(0, y);
                Point end = new Point(img.Width - 1, y);
                LineSegment2D line = new LineSegment2D(start, end);
                img.Draw(line, new Bgr(0, 255, 0), 1);
            }

            //세로 줄 (x는 가로 간격)
            for (int x = glo.x_grid; x < img.Width; x += glo.x_grid)
            {
                Point start = new Point(x, 0);
                Point end = new Point(x, img.Height - 1);
                LineSegment2D line = new LineSegment2D(start, end);
                img.Draw(line, new Bgr(0, 255, 0), 1);
            }
            */

            return obstacle_grid_fill(canny_img, img, Map_obstacle);

        }

        public void drow_bloded_Grid(Image<Bgr, Byte> img, int[,] Map_obstacle, object[] blob_info)
        {
            bool grid_check = false;
            Image<Bgr, Byte> count_img = ((Image<Gray, Byte>)blob_info[1]).Convert<Bgr, Byte>();


            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {

                    /*
                              for (int x = 221; x < 220 + 200; x++)
                              {
                                  for (int y = 11; y < 10 + 380; y++)
                                  {
                                 */
                    if (count_img[y, x].Equals(new Bgr(255, 255, 255)))
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
