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

            Image<Gray, Byte> graySoft = blob_image.Clone().Convert<Gray, Byte>().PyrDown().PyrUp();
            Image<Gray, Byte> gray = graySoft.SmoothGaussian(3);
            //gray = gray.AddWeighted(graySoft, 1.3, -0.6, 0);
            //Image<Gray, Byte> bin = gray.ThresholdBinary(new Gray(70), new Gray(255));

            //gray = gray.AddWeighted(graySoft, 0.95, -0.1, 0);
            //Image<Gray, Byte> bin = gray.ThresholdBinary(new Gray(85), new Gray(255));

            gray = gray.AddWeighted(graySoft, 1.3, -0.6, 0);
            Image<Gray, Byte> bin = gray.ThresholdBinary(new Gray(60), new Gray(255));

            Gray cannyThreshold = new Gray(149);
            Gray cannyThresholdLinking = new Gray(149);
            Image<Gray, Byte> greyThreshImg = bin.Canny(cannyThreshold.Intensity, cannyThresholdLinking.Intensity);

            CvBlobs resultingImgBlobs = new CvBlobs();
            CvBlobDetector bDetect = new CvBlobDetector();
            bDetect.Detect(greyThreshImg, resultingImgBlobs);

            Image<Bgr, Byte> temp_img = greyThreshImg.Convert<Bgr, Byte>();

            //영상에서 차량 범위를 빼고 ROI만들기
            for (int i = 0; i < 4; i++)
            {
                if (tracking_rect[i].Width != 0 && tracking_rect[i].Height != 0)
                {
                    int pos_x = tracking_rect[i].X - globals.x_grid;
                    int pos_y = tracking_rect[i].Y - globals.y_grid;

                    //이미지가 범위를 벗어날경우 처리
                    if (pos_x < 0)
                        pos_x = 0;
                    if (pos_y < 0)
                        pos_y = 0;

                    if (pos_x + tracking_rect[i].Width > globals.rect_width)
                        pos_x = globals.rect_width - tracking_rect[i].Width;
                    if (pos_y + tracking_rect[i].Height > globals.rect_height)
                        pos_y = globals.rect_height - tracking_rect[i].Height;

                    for (int x = pos_x; x < pos_x + tracking_rect[i].Width + globals.x_grid; x++)
                    {
                        for (int y = pos_y; y < pos_y + tracking_rect[i].Height + globals.y_grid; y++)
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

                                Map_obstacle[t_y, t_x] = i + 1; // 잡힌 차량은 Map에 2라고 표시
                            }
                        }
                    }
                    
                    //차량 vs 차량 충돌 검사
                    for (int j = 0; j < 4; j++)
                    {
                        int leftA, leftB;
                        int rightA, rightB;
                        int topA, topB;
                        int bottomA, bottomB;

                        if (i != j)
                        {
                            if (!(tracking_rect[j].Width == 0 && tracking_rect[j].Height == 0))
                            {
                                int add_size = 23;
                                
                                leftA = tracking_rect[i].X - add_size;
                                rightA = tracking_rect[i].X + tracking_rect[i].Width + add_size;
                                topA = tracking_rect[i].Y - add_size;
                                bottomA = tracking_rect[i].Y + tracking_rect[i].Height + add_size;

                                leftB = tracking_rect[j].X - add_size;
                                rightB = tracking_rect[j].X + tracking_rect[j].Width + add_size;
                                topB = tracking_rect[j].Y - add_size;
                                bottomB = tracking_rect[j].Y + tracking_rect[j].Height + add_size;

                                if (bottomA < topB) continue; //아래
                                if (topA > bottomB) continue; //위
                                if (rightA < leftB) continue; //오른쪽
                                if (leftA > rightB) continue; //왼쪽

                                int boarder_size = 18;


                                globals.evasionInfoLock.EnterWriteLock();

                                
                                if (bottomA - topB <= boarder_size || bottomB - topA <= boarder_size || rightA - leftB <= boarder_size || rightB - leftA <= boarder_size)
                                {
                                    Console.WriteLine(i + " 차량과 " + j + " 차량이 충돌 위기");

                                    KeyValuePair<int, int> temp = new KeyValuePair<int, int>(i, j);

                                    bool isEmpty = true; 
                                    foreach(var evsionTempList in globals.evasionInfo)
                                    {
                                        if(evsionTempList.Key == i && evsionTempList.Value == j)
                                        {
                                            isEmpty = false;
                                            break;
                                        }
                                        else if (evsionTempList.Key == j && evsionTempList.Value == i)
                                        {
                                            isEmpty = false;
                                            break;
                                        }
                                    }

                                    if (isEmpty == false)
                                        globals.evasionInfo.Add(temp);

                                }
                                 else
                                {
                                    Console.WriteLine(i + " 차량과 " + j + " 차량이 충돌함\n");

                                    KeyValuePair<int, int> temp = new KeyValuePair<int, int>(i, j);

                                     bool isEmpty = true;
                                     foreach (var evsionTempList in globals.UGVsConflictInofo)
                                    {
                                        if(evsionTempList.Key == i && evsionTempList.Value == j)
                                        {
                                            isEmpty = false;
                                            break;
                                        }
                                        else if (evsionTempList.Key == j && evsionTempList.Value == i)
                                        {
                                            isEmpty = false;
                                            break;
                                        }
                                    }

                                    if (isEmpty == false)
                                        globals.UGVsConflictInofo.Add(temp);                                                               
                                }

                                globals.evasionInfoLock.ExitWriteLock();
                            }
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

                    /*
                    // 장애물 범위 지정안했을때 
                    int temp_x, temp_y, temp_width, temp_height;

                    temp_x = targetBlob.BoundingBox.X;
                    temp_y = targetBlob.BoundingBox.Y;

                    temp_width = targetBlob.BoundingBox.Width;
                    temp_height = targetBlob.BoundingBox.Height;
                    */
                    
                    //장애물의 충돌 검사를 위해 범위 설정
                    int xx = 10, yy = 10;
                    int temp_x, temp_y, temp_width, temp_height;

                    temp_x = targetBlob.BoundingBox.X - xx;
                    temp_width = targetBlob.BoundingBox.Width + xx * 2;
                    if (temp_x < 0)
                    {
                        temp_x = 0;
                        temp_width = targetBlob.BoundingBox.Width + xx;
                    }

                    temp_y = targetBlob.BoundingBox.Y - yy;
                    temp_height = targetBlob.BoundingBox.Height + yy * 2;
                    if (temp_y < 0)
                    {
                        temp_y = 0;
                        temp_height = targetBlob.BoundingBox.Height + yy;

                    }
                    

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
                    //else if (color_str == "yellow")
                    //    color_index = 2;

                    if (temp_color_count[color_index] == 0) //검출된 색의 color_count가 0 일땐 list에 추가함
                    {
                        building_list.Add(new Building("B" + blob_indenti_count++, (double)targetBlob.BoundingBox.Width, (double)targetBlob.BoundingBox.Height, targetBlob.BoundingBox.X, targetBlob.BoundingBox.Y, color_str, true));
                        obstacle_color_count[color_index]++; //obstacle_color_count 증가
                        //Console.WriteLine("blob_indenti_count = " + blob_indenti_count + " color_str = " + color_str + " x  = " + targetBlob.BoundingBox.X + " y = " + targetBlob.BoundingBox.Y);
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
                    
                    //장애물 vs 차량 충돌 검사
                    for (int i = 0; i < 4; i++)
                    {
                        int leftA, leftB;
                        int rightA, rightB;
                        int topA, topB;
                        int bottomA, bottomB;

                        if (!(tracking_rect[i].Width == 0 && tracking_rect[i].Height == 0))
                        {
                            leftA = tracking_rect[i].X;
                            rightA = tracking_rect[i].X + tracking_rect[i].Width;
                            topA = tracking_rect[i].Y;
                            bottomA = tracking_rect[i].Y + tracking_rect[i].Height;

                            leftB = temp_x;
                            rightB = temp_x + temp_width;
                            topB = temp_y;
                            bottomB = temp_y + temp_height;

                            /*
                            if (bottomA < topB)
                            {
                                //if (topB - bottomA <= 2)
                                //    Console.WriteLine(i + " 번쨰 장애물과 충돌함 (아래) 아직 떨어져있을때 \n");

                                continue; //아래
                            }

                            if (topA > bottomB)
                            {
                                //if (topA - bottomB <= 2)
                                //    Console.WriteLine(i + " 번쨰 장애물과 충돌함 (위) 아직 떨어져있을때\n");

                                continue; //위
                            }
                            if (rightA < leftB)
                            {
                                //if (leftB - rightA <= 2)
                                //    Console.WriteLine(i + " 번쨰 장애물과 충돌함 (오른쪽) 아직 떨어져있을때\n");

                                continue; //오른쪽
                            }
                            if (leftA > rightB)
                            {
                                //if (leftA - rightB <= 2)
                                //    Console.WriteLine(i + " 번쨰 장애물과 충돌함 (왼쪽) 아직 떨어져있을때\n");

                                continue; //왼쪽
                            }
                            */
                            if (bottomA < topB) continue; //아래
                            if (topA > bottomB) continue; //위
                            if (rightA < leftB) continue; //오른쪽
                            if (leftA > rightB) continue; //왼쪽


                            if (bottomA - topB <= 7)
                                Console.WriteLine(i + " 번쨰 장애물과 충돌함 (아래) 차이 = " + (bottomA - topB) + "\n");
                            else if (bottomB - topA <= 7)
                                Console.WriteLine(i + " 번쨰 장애물과 충돌함 (위) 차이 = " + (bottomB - topA) + "\n");
                            else if (rightA - leftB <= 7)
                                Console.WriteLine(i + " 번쨰 장애물과 충돌함 (오른쪽) 차이 = " + (rightA - leftB) + "\n");
                            else if (rightB - leftA <= 7)
                                Console.WriteLine(i + " 번쨰 장애물과 충돌함 (왼쪽) 차이 = " + (rightB - leftA) + "\n");
                                                                          
                            /*
                            if (bottomA < topB) continue; //아래
                            if (topA > bottomB) continue; //위
                            if (rightA < leftB) continue; //오른쪽
                            if (leftA > rightB) continue; //왼쪽

                            
                            if (bottomA - topB <= yy - 4 || bottomB - topA <= yy - 4 || rightA - leftB <= xx - 4 || rightB - leftA <= xx - 4)
                                Console.WriteLine(i + " 번쨰 장애물과 충돌위기\n");
                            else
                                Console.WriteLine(i + " 번쨰 장애물과 충돌함\n");
                            //[0]blue [1] green [2]orange [3]red
                             */
                        }
                    }          
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
            //else if (obstacle_YccColorCheck(image, totalPicxel, x, y, width, height, 0, 120, 13, 255, 168, 103) == 1) //노랑 8섹션
            //    return "yellow";
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

                                    Map_obstacle[t_y, t_x] = '*'; // 장애물은 Map에 1로 표시
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
                    //else if (remov_tmp.BuildingColor == "yellow")
                    //    obstacle_color_count[2]--;
                }
                else
                    remov_tmp.DisapperCheck = false; //true였다면 false로 바꿔줌 
            }

            return tmp_list;
        }
    }
}
