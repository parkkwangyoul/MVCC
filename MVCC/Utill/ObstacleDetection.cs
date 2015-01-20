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
        int[] obstacle_color_count = new int[4]; //[0]purple [1] black [2] yellow [3]
        
        /*
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
        */

        public object[] detectBlob(Image<Bgr, Byte> blob_image, int[,] Map_obstacle, object[] blob_info, Rectangle[] tracking_rect)
        {
            int blob_count = 0;

            Object[] blob_indenti = new Object[6]; // [0]indetifier [2]color [3] width [4] height [5] x [6] y  

            Image<Bgr, Byte> _blobsImg = blob_image.Clone();
            Image<Gray, Byte> greyImg = _blobsImg.Convert<Gray, Byte>().PyrDown().PyrUp();

            Image<Gray, Byte> graySoft = _blobsImg.Convert<Gray, Byte>().PyrDown().PyrUp();
            Image<Gray, Byte> gray = graySoft.SmoothGaussian(3);
            gray = gray.AddWeighted(graySoft, 1.2, -0.5, 0);
            Image<Gray, Byte> bin = gray.ThresholdBinary(new Gray(90), new Gray(255));

            Gray cannyThreshold = new Gray(149);
            Gray cannyThresholdLinking = new Gray(149);
            Gray circleAccumulatorThreshold = new Gray(1000);

            blob_info[1] = bin.Canny(cannyThreshold.Intensity, cannyThresholdLinking.Intensity);
         
            CvBlobs resultingImgBlobs = new CvBlobs();
            CvBlobDetector bDetect = new CvBlobDetector();
            bDetect.Detect((Image<Gray, Byte>)blob_info[1], resultingImgBlobs);

            Image<Bgr, Byte> temp_img = ((Image<Gray, Byte>)blob_info[1]).Convert<Bgr, Byte>();

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

                if (pos_x + tracking_rect[i].Width > globals.rect_width)
                    pos_x = globals.rect_width - tracking_rect[i].Width;
                if (pos_y + tracking_rect[i].Height > globals.rect_height)
                    pos_y = globals.rect_height - tracking_rect[i].Height;

                for (int x = pos_x; x < pos_x + tracking_rect[i].Width; x++)
                {
                    for (int y = pos_y; y < pos_y + tracking_rect[i].Height; y++)
                    {
                        temp_img[y, x] = new Bgr(0, 0, 0);
                        if (x % globals.x_grid == 0 && y % globals.y_grid == 0)
                        {
                            int t_x = x;
                            int t_y = y;

                            if (t_x != 0)
                                t_x = x / globals.x_grid;

                            if (t_y != 0)
                                t_y = y / globals.y_grid;

                            Map_obstacle[t_y, t_x] = 2;
                        }
                    }
                }
            }
        
            int[] temp_color_count = new int[4]; //[0]purple [1] black [2] yellow [3]
            temp_color_count = (int[])obstacle_color_count.Clone();
            int temp_blob_count = building_list.Count;

            List<Building> tmp = new List<Building>(); //몇개 있는지 확인후에 제거 하면서 검사 하기 위해
            for (int i = 0; i < building_list.Count; i++)
                tmp.Add(new Building(building_list[i].Id, building_list[i].Width, building_list[i].Height, building_list[i].X, building_list[i].Y, building_list[i].BuildingColor, building_list[i].DisapperCheck));

            foreach (CvBlob targetBlob in resultingImgBlobs.Values)
            {
                if (targetBlob.Area > 100 && targetBlob.Area < 700)
                {
                    string temp;
                    bool is_check = false;

                    if ((temp = obstacle_colorCheck(blob_image, targetBlob.Area, targetBlob.BoundingBox.X, targetBlob.BoundingBox.Y, targetBlob.BoundingBox.Width, targetBlob.BoundingBox.Height)) == "null")
                    {
                        for (int x = targetBlob.BoundingBox.X; x < targetBlob.BoundingBox.X + targetBlob.BoundingBox.Width; x++)
                            for (int y = targetBlob.BoundingBox.Y; y < targetBlob.BoundingBox.Y + targetBlob.BoundingBox.Height; y++)
                                temp_img[y, x] = new Bgr(0, 0, 0);
                        continue;
                    }

                    for (int x = targetBlob.BoundingBox.X; x < targetBlob.BoundingBox.X + targetBlob.BoundingBox.Width; x++)
                        for (int y = targetBlob.BoundingBox.Y; y < targetBlob.BoundingBox.Y + targetBlob.BoundingBox.Height; y++)
                            temp_img[y, x] = new Bgr(255, 255, 255);
                    
                    if (temp == "purple")
                    {
                        if (temp_color_count[0] == 0)
                        {
                            building_list.Add(new Building("B" + blob_indenti_count++, (double)targetBlob.BoundingBox.Width, (double)targetBlob.BoundingBox.Height, targetBlob.BoundingBox.X, targetBlob.BoundingBox.Y, temp, true));
                            obstacle_color_count[0]++;
                        }
                        else if (temp_color_count[0] != 0)
                        {
                            for (int i = 0; i < tmp.Count; i++)
                            {
                                Building remov_tmp = tmp[i];
                                if (remov_tmp.BuildingColor == temp)
                                {
                                    tmp.Remove(remov_tmp);
                                    temp_color_count[0]--;

                                    foreach (Building building in building_list)
                                    {
                                        if (building.BuildingColor == temp && building.DisapperCheck == false)
                                        {
                                            building.X = targetBlob.BoundingBox.X;
                                            building.Y = targetBlob.BoundingBox.Y;
                                            building.Width = targetBlob.BoundingBox.Width;
                                            building.Height = targetBlob.BoundingBox.Height;
                                            building.DisapperCheck = true;
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }                         
                        }
                    }
                    else if (temp == "black")
                    {
                        if (temp_color_count[1] == 0)
                        {
                            building_list.Add(new Building("B" + blob_indenti_count++, (double)targetBlob.BoundingBox.Width, (double)targetBlob.BoundingBox.Height, targetBlob.BoundingBox.X, targetBlob.BoundingBox.Y, temp, true));
                            obstacle_color_count[1]++;
                        }
                        else if (temp_color_count[1] != 0)
                        {
                            for (int i = 0; i < tmp.Count; i++)
                            {
                                Building remov_tmp = tmp[i];
                                if (remov_tmp.BuildingColor == temp)
                                {
                                    tmp.Remove(remov_tmp);
                                    temp_color_count[1]--;

                                    foreach (Building building in building_list)
                                    {
                                        if (building.BuildingColor == temp && building.DisapperCheck == false)
                                        {
                                            building.X = targetBlob.BoundingBox.X;
                                            building.Y = targetBlob.BoundingBox.Y;
                                            building.Width = targetBlob.BoundingBox.Width;
                                            building.Height = targetBlob.BoundingBox.Height;
                                            building.DisapperCheck = true;
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }                         
                        }
                    }
                    else if (temp == "yellow")
                    {
                        if (temp_color_count[2] == 0)
                        {
                            building_list.Add(new Building("B" + blob_indenti_count++, (double)targetBlob.BoundingBox.Width, (double)targetBlob.BoundingBox.Height, targetBlob.BoundingBox.X, targetBlob.BoundingBox.Y, temp, true));
                            obstacle_color_count[2]++;
                        }
                        else if(temp_color_count[2] != 0)
                        {
                            for (int i = 0; i < tmp.Count; i++)
                            {
                                Building remov_tmp = tmp[i];
                                if (remov_tmp.BuildingColor == temp)
                                {
                                    tmp.Remove(remov_tmp);
                                    temp_color_count[2]--;

                                    foreach (Building building in building_list)
                                    {
                                        if (building.BuildingColor == temp && building.DisapperCheck == false)
                                        {
                                            building.X = targetBlob.BoundingBox.X;
                                            building.Y = targetBlob.BoundingBox.Y;
                                            building.Width = targetBlob.BoundingBox.Width;
                                            building.Height = targetBlob.BoundingBox.Height;
                                            building.DisapperCheck = true;
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }                      
                        }
                    }

                    blob_count++;
                 }
                else            
                {
                    for (int x = targetBlob.BoundingBox.X; x < targetBlob.BoundingBox.X + targetBlob.BoundingBox.Width; x++)
                        for (int y = targetBlob.BoundingBox.Y; y < targetBlob.BoundingBox.Y + targetBlob.BoundingBox.Height; y++)
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
            List<Building> tmp = new List<Building>();

            for (int i = 0; i < building_list.Count; i++ )
                tmp.Add(new Building(building_list[i].Id, building_list[i].Width, building_list[i].Height, building_list[i].X, building_list[i].Y, building_list[i].BuildingColor, building_list[i].DisapperCheck));
            
            //Console.WriteLine("building List : " + building_list.Count);

            /*
            for (int i = 0; i < building_list.Count; i++)
            {
                Console.WriteLine("building_list[" + i + "].DisapperCheck = " + building_list[i].DisapperCheck);
            }
            */
            for (int i = 0; i < building_list.Count; i++)
            {
                Building remov_tmp = building_list[i];
                if (remov_tmp.DisapperCheck == false)
                {
                    building_list.Remove(remov_tmp);
                    if(remov_tmp.BuildingColor == "purple")
                        obstacle_color_count[0]--;
                    else if (remov_tmp.BuildingColor == "black")
                        obstacle_color_count[1]--;
                    else if (remov_tmp.BuildingColor == "yellow")
                        obstacle_color_count[2]--;
                }
                else
                    remov_tmp.DisapperCheck = false;
            }

            //for (int i = 0; i < building_list.Count; i++)
            //{
                //Console.WriteLine("building_list[" + i + "].DisapperCheck = " + building_list[i].DisapperCheck);
                //Console.WriteLine("tmp[" + i + "].DisapperCheck = " + tmp[i].DisapperCheck);     
           // }

            return tmp;
        }

        //감지된 blob들 중 장애물 색상 검색
        public string obstacle_colorCheck(Image<Bgr, Byte> image, int totalPicxel, int x, int y, int width, int height)
        {
            if (obstacle_YccColorCheck(image, totalPicxel, x, y, width, height, 0, 95, 136, 255, 184, 173) == 1) //보라 8섹션
                return "purple";
            else if (obstacle_YccColorCheck(image, totalPicxel, x, y, width, height, 0, 122, 106, 65, 140, 141) == 1) //검정 8섹션
                return "black";
            else if (obstacle_YccColorCheck(image, totalPicxel, x, y, width, height, 0, 120, 13, 255, 168, 103) == 1) //노랑 8섹션
                return "yellow";
            else
                return "null";
        }

        //장애물 색상이 맞는지 판별
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

                        if (x > pos_x / 5 + x && y > pos_y / 5 + y) //좌표의 1/5 넘으면 없는걸로
                            return -1;
                    }
                }
            }

            return -1;
        }

        public void drow_bloded_Grid(Image<Bgr, Byte> img, int[,] Map_obstacle, object[] blob_info)
        {
            bool grid_check = false;
            Image<Bgr, Byte> count_img = ((Image<Gray, Byte>)blob_info[1]).Convert<Bgr, Byte>();

            
            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {      
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



        /*
        //그리드 그림 (그냥 canny만 했을때)
        public int drowGrid(Image<Gray, Byte> canny_img, Image<Bgr, Byte> img, int[,] Map_obstacle)
        {
            
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
            

            return obstacle_grid_fill(canny_img, img, Map_obstacle);
        }
        */

        /*
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
        */

        /*
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
        */ 
    }
}
