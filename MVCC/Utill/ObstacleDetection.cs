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
        int blob_indenti_count; //blob된 장애물의 고유번호를 위해 
        int[] obstacle_color_count = new int[4]; //color의 count를 세기 위해 [0]purple [1] black [2] yellow [3]

        Globals globals = Globals.Instance; //globalsal 변수를 위해
        List<Building> building_list = new List<Building>(); //blob된 장애물 정보 저장
       
        //영상에서 차량을 제외 한 후 blob을 검출하고 색상을 통해 장애물 판별
        public int detectBlob(Image<Bgr, Byte> blob_image, int[,] Map_obstacle, Rectangle[] tracking_rect)
        {
            int blob_count = 0;

            Image<Bgr, Byte> _blobsImg = blob_image.Clone();
            Image<Gray, Byte> graySoft = _blobsImg.Convert<Gray, Byte>().PyrDown().PyrUp();
            Image<Gray, Byte> gray = graySoft.SmoothGaussian(3);
            gray = gray.AddWeighted(graySoft, 1.3, -0.5, 0);
            Image<Gray, Byte> bin = gray.ThresholdBinary(new Gray(90), new Gray(255));

            Gray cannyThreshold = new Gray(149);
            Gray cannyThresholdLinking = new Gray(149);
            Gray circleAccumulatorThreshold = new Gray(1000);
            Image<Gray, Byte> greyThreshImg = bin.Canny(cannyThreshold.Intensity, cannyThresholdLinking.Intensity);

            CvBlobs resultingImgBlobs = new CvBlobs();
            CvBlobDetector bDetect = new CvBlobDetector();
            bDetect.Detect(greyThreshImg, resultingImgBlobs);

            Image<Bgr, Byte> temp_img = greyThreshImg.Convert<Bgr, Byte>();

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

                            Map_obstacle[t_y, t_x] = 2; // 잡힌 차량은 Map에 2라고 표시
                        }
                    }
                }
            }

            int[] temp_color_count = new int[4]; //[0]purple [1] black [2] yellow [3]
            temp_color_count = (int[])obstacle_color_count.Clone();
            int temp_blob_count = building_list.Count;

            List<Building> tmp_list = new List<Building>(); //몇개 있는지 확인후에 제거 하면서 검사 하기 위해
            for (int i = 0; i < building_list.Count; i++)
                tmp_list.Add(new Building(building_list[i].Id, building_list[i].Width, building_list[i].Height, building_list[i].X, building_list[i].Y, building_list[i].BuildingColor, building_list[i].DisapperCheck));

            //blob 검출
            foreach (CvBlob targetBlob in resultingImgBlobs.Values)
            {
                if (targetBlob.Area > 100 && targetBlob.Area < 700)
                {
                    string color_str;
                    int color_index = -1;
                    //검출된 색이 장애물인지
                    if ((color_str = obstacle_colorCheck(blob_image, targetBlob.Area, targetBlob.BoundingBox.X, targetBlob.BoundingBox.Y, targetBlob.BoundingBox.Width, targetBlob.BoundingBox.Height)) == "null")
                    {
                        for (int x = targetBlob.BoundingBox.X; x < targetBlob.BoundingBox.X + targetBlob.BoundingBox.Width; x++)
                            for (int y = targetBlob.BoundingBox.Y; y < targetBlob.BoundingBox.Y + targetBlob.BoundingBox.Height; y++)
                                temp_img[y, x] = new Bgr(0, 0, 0);
                        continue; //장애물색상이 아니면 검정으로 색칠
                    }

                    for (int x = targetBlob.BoundingBox.X; x < targetBlob.BoundingBox.X + targetBlob.BoundingBox.Width; x++)
                        for (int y = targetBlob.BoundingBox.Y; y < targetBlob.BoundingBox.Y + targetBlob.BoundingBox.Height; y++)
                            temp_img[y, x] = new Bgr(255, 255, 255); //검출된 부분은 다 하얀색으로 색칠

                    if (color_str == "purple")
                        color_index = 0;
                    else if (color_str == "black")
                        color_index = 1;
                    else if (color_str == "yellow")
                        color_index = 2;

                    if (temp_color_count[color_index] == 0) //검출된 색의 color_count가 0 일땐 list에 추가함
                    {
                        building_list.Add(new Building("B" + blob_indenti_count++, (double)targetBlob.BoundingBox.Width, (double)targetBlob.BoundingBox.Height, targetBlob.BoundingBox.X, targetBlob.BoundingBox.Y, color_str, true));
                        obstacle_color_count[color_index]++; //obstacle_color_count 증가
                    }
                    else if (temp_color_count[color_index] != 0) //color_count가 0이 아니면 list에 있으니 정보 갱신만 함
                    {
                        for (int i = 0; i < tmp_list.Count; i++)
                        {
                            Building remov_tmp = tmp_list[i];

                            if (remov_tmp.BuildingColor == color_str)
                            {
                                tmp_list.Remove(remov_tmp); //tmp_list에서 하나 삭제
                                temp_color_count[color_index]--; // temp_color_count 하나 감소 

                                foreach (Building building in building_list) //building_list의 정보 갱신
                                {
                                    if (building.BuildingColor == color_str && building.DisapperCheck == false) //building.DisapperCheck가 false인 경우 정보 갱신
                                    {
                                        building.X = targetBlob.BoundingBox.X;
                                        building.Y = targetBlob.BoundingBox.Y;
                                        building.Width = targetBlob.BoundingBox.Width;
                                        building.Height = targetBlob.BoundingBox.Height;
                                        building.DisapperCheck = true; //갱신했으면 building.DisapperChecf를 true로 
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }

                    blob_count++;
                }
                else //범위를 벗어난 크기는 검정으로 색칠            
                {
                    for (int x = targetBlob.BoundingBox.X; x < targetBlob.BoundingBox.X + targetBlob.BoundingBox.Width; x++)
                        for (int y = targetBlob.BoundingBox.Y; y < targetBlob.BoundingBox.Y + targetBlob.BoundingBox.Height; y++)
                            temp_img[y, x] = new Bgr(0, 0, 0);
                }
            }

            drow_bloded_Grid(Map_obstacle, temp_img); //Map 배열에 장애물 표시

            return blob_count;
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

        //장애물 정보를 Map에 표시
        public void drow_bloded_Grid(int[,] Map_obstacle, Image<Bgr, Byte> count_img)
        {
            bool grid_check = false;
          
            for (int x = 0; x < globals.rect_width; x++)
            {
                for (int y = 0; y < globals.rect_height; y++)
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

                                    Map_obstacle[t_y, t_x] = 1; // 장애물은 Map에 1로 표시
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

        //building_list 정보 넘겨주고 building_list 정보 갱신함
        public List<Building> get_building()
        {
            List<Building> tmp_list = new List<Building>(); //building_list 넘겨주기 위한 tmp_list 만듬

            for (int i = 0; i < building_list.Count; i++) //list 복사
                tmp_list.Add(new Building(building_list[i].Id, building_list[i].Width, building_list[i].Height, building_list[i].X, building_list[i].Y, building_list[i].BuildingColor, building_list[i].DisapperCheck));

            for (int i = 0; i < building_list.Count; i++)
            {
                Building remov_tmp = building_list[i];

                if (remov_tmp.DisapperCheck == false) //false인건 색이 사라졌단 소리 이므로 building_list에 제거 후 obstacle_color_count를 감소
                {
                    building_list.Remove(remov_tmp);
                    if (remov_tmp.BuildingColor == "purple")
                        obstacle_color_count[0]--;
                    else if (remov_tmp.BuildingColor == "black")
                        obstacle_color_count[1]--;
                    else if (remov_tmp.BuildingColor == "yellow")
                        obstacle_color_count[2]--;
                }
                else
                    remov_tmp.DisapperCheck = false; //true였다면 false로 바꿔줌 
            }

            return tmp_list;
        }
    }
}
