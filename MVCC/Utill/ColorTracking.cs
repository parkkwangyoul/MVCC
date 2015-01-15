using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

using MVCC.Model;

namespace MVCC.Utill
{
    class ColorTracking
    {
        bool[] color = new bool[4]; //색상 여부 ([0]blue [1] green [2]pink [3]red)
        Point[] color_ROI = new Point[4]; //색상추적에 대한 이동 ROI
        bool[] change_check = new bool[4]; // 이미지 변환 됫는지 체크.. 지금은 그냥 블루색깔 없어졌는지 test;

        Image<Bgr, Byte> colorCheckImage; //칼라 체크할 이미지 변수
        int totalPicxel, pos_x, pos_y, img_width, img_height; //탬플릿매칭으로 넘어온 정보
        Globals glo = Globals.Instance; //gloal 변수를 위해

        List<UGV> ugvList = new List<UGV>();

        //어떤 색인지 알아내기 위해
        public void colorCheck(Image<Bgr, Byte> iamge, int totalPicxel, int x, int y, int width, int height)
        {
            //이미지에 대한 정보들 복사
            colorCheckImage = iamge;
            this.totalPicxel = totalPicxel;
            pos_x = x; pos_y = y;
            img_width = width; img_height = height;

            //색상 알아내기. 있는 색은 검사 안함
            for (int i = 0; i < 4; i++)
            {
                if (color[i] == false)
                {
                    //42, 133, 0, 169, 179, 97 yellow
                    if (i == 0) //blue
                        YccColorCheck(i, 0, 51, 141, 255, 240, 240);
                    else if (i == 1) //green
                        //YccColorCheck(i, 0, 0, 0, 194, 103, 154); //가까울떄
                        YccColorCheck(i, 66, 66, 61, 114, 124, 140); //라온제나
                    else if (i == 2) //pink
                        //YccColorCheck(i, 41, 81, 134, 205, 240, 162);
                        //YccColorCheck(i, 0, 133, 20, 255, 160, 97); //yellow
                        //YccColorCheck(i, 0, 130, 0, 255, 223, 102); //orange 가까울때
                        YccColorCheck(i, 65, 166, 74, 255, 240, 129); //orange 라온제나
                    else //red
                        //YccColorCheck(i, 0, 136, 100, 110, 221, 129); //가까울때
                        YccColorCheck(i, 0, 149, 93, 100, 221, 142); //멀때
                    //YccColorCheck(i, 0, 133, 20, 255, 160, 97); //yellow
                }
            }
        }

        //Ycc 색상 모델로 색상 정보 추출함
        public void YccColorCheck(int index, int min1, int min2, int min3, int max1, int max2, int max3)
        {
            int pixCount = 0;

            Image<Ycc, Byte> YCrCbFrame = colorCheckImage.Convert<Ycc, Byte>(); //YCrCb 변환
            Image<Gray, byte> colorSetting = new Image<Gray, byte>(YCrCbFrame.Width, YCrCbFrame.Height); //Ycc범위로 뽑아낸 것을 gray로 바꿔서 수축팽창 하기 위해

            Ycc YCrCb_min = new Ycc(min1, min2, min3);
            Ycc YCrCb_max = new Ycc(max1, max2, max3);   //blue 색 범위

            colorSetting = YCrCbFrame.InRange((Ycc)YCrCb_min, (Ycc)YCrCb_max); //색 범위 설정

            StructuringElementEx rect_12 = new StructuringElementEx(12, 12, 6, 6, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_RECT);
            CvInvoke.cvErode(colorSetting, colorSetting, rect_12, 1);
            StructuringElementEx rect_6 = new StructuringElementEx(6, 6, 3, 3, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_RECT);
            CvInvoke.cvDilate(colorSetting, colorSetting, rect_6, 2); //수축 팽창

            Image<Bgr, Byte> colorCount = colorSetting.Convert<Bgr, Byte>(); //픽셀수 세기 위해


            //이미지가 범위를 벗어날경우 처리
            if (pos_x < 0)
                pos_x = 0;
            if (pos_y < 0)
                pos_y = 0;

            if (pos_x + img_width > colorCheckImage.Width)
                pos_x = colorCheckImage.Width - img_width;
            if (pos_y + img_height > colorCheckImage.Height)
                pos_y = colorCheckImage.Height - img_height;


            for (int x = pos_x; x < pos_x + img_width; x++)
            {
                for (int y = pos_y; y < pos_y + img_height; y++)
                {
                    if (!colorCount[y, x].Equals(new Bgr(0, 0, 0)))
                    {
                        pixCount++;

                        if (totalPicxel / 5 <= pixCount) //일정 픽섹 이상시 색상배열 변경후 종료
                        //if (pixCount > 1000)
                        {
                            color[index] = true;
                            change_check[index] = false;
                            color_ROI[index].X = x;
                            color_ROI[index].Y = y;
                            ugvList.Add(new UGV("A" + index, glo.TemplateWidth, glo.TemplateHeight, x, y));
                            return;
                        }
                    }
                }
            }

            /*
            //이걸로 하면 안됨  왜안되지?? 범위를 지정하고 한건데.. 근데 신기하게 위에 코드는 됨.
           
            //이미지가 범위를 벗어날경우 처리
            if (pos_x < 0)
                pos_x = 0;
            if (pos_y < 0)
                pos_y = 0;

            if (pos_x + img_width > colorCheckImage.Width)
                pos_x = colorCheckImage.Width - img_width;
            if (pos_y + img_height > colorCheckImage.Height)
                pos_y = colorCheckImage.Height - img_height;

            for (int x = pos_x; x < pos_x + img_width; x++)
            {
                for (int y = pos_y; y < pos_y + img_height; y++)
                {
                    if (!colorCount[y, x].Equals(new Bgr(0, 0, 0)))
                    {
                        pixCount++;

                        //if (totalPicxel / 5 <= pixCount) //일정 픽섹 이상시 색상배열 변경후 종료
                        if (pixCount > 3000)
                        {
                            color[index] = true;
                            change_check[index] = false;
                            color_ROI[index].X = x;
                            color_ROI[index].Y = y;
                            return;
                        }
                    }
                }
            }    
            */
        }

        //색상 트레킹 시작
        public Rectangle[] tracking_start(Image<Bgr, Byte> iamge)
        {
            Rectangle[] rect = new Rectangle[4]; //tracking 결과 반환 네모 배열

            for (int i = 0; i < 4; i++)
            {
                if (color[i] == true) //있는 색상만 트레킹
                {
                    if (i == 0)
                        color_traking(i, 0, 51, 151, 105, 124, 194, iamge, rect);
                    else if (i == 1)
                        //color_traking(i, 0, 0, 0, 194, 103, 154, iamge, rect); //가까울때
                        color_traking(i, 66, 66, 61, 114, 124, 140, iamge, rect); //라온제나
                    else if (i == 2)
                        //color_traking(i, 41, 81, 134, 205, 240, 162, iamge, rect); //pink
                        //color_traking(i, 0, 133, 20, 255, 160, 97, iamge, rect); //yellow
                        // color_traking(i, 0, 130, 0, 255, 223, 102, iamge, rect); //orange 가까울때
                        color_traking(i, 65, 166, 74, 255, 240, 129, iamge, rect); //orange 라온제나

                    else
                        //color_traking(i, 0, 136, 100, 110, 221, 129, iamge, rect);
                        color_traking(i, 0, 149, 93, 100, 221, 142, iamge, rect); //멀때

                }// ([0]blue [1] green [2]pink [3]red)
            }

            return rect;
        }

        //각각의 색상을 트레킹해서 rect을 만들어줌
        public void color_traking(int index, int min1, int min2, int min3, int max1, int max2, int max3, Image<Bgr, Byte> iamge, Rectangle[] rect)
        {
            int pixCount = 0;

            Image<Ycc, Byte> YCrCbFrame = iamge.Convert<Ycc, Byte>(); //YCrCb 변환

            Image<Gray, byte> colorSetting = new Image<Gray, byte>(YCrCbFrame.Width, YCrCbFrame.Height);

            Ycc YCrCb_min = new Ycc(min1, min2, min3);
            Ycc YCrCb_max = new Ycc(max1, max2, max3);   //blue 색 범위

            colorSetting = YCrCbFrame.InRange((Ycc)YCrCb_min, (Ycc)YCrCb_max); //색 범위 설정

            StructuringElementEx rect_12 = new StructuringElementEx(12, 12, 6, 6, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_RECT);
            CvInvoke.cvErode(colorSetting, colorSetting, rect_12, 1);
            StructuringElementEx rect_6 = new StructuringElementEx(6, 6, 3, 3, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_RECT);
            CvInvoke.cvDilate(colorSetting, colorSetting, rect_6, 2); //수축 팽창

            Image<Bgr, Byte> colorCount = colorSetting.Convert<Bgr, Byte>(); //픽셀수 세기 위해
            int x_p = 0;
            int y_p = 0; //x, y 픽셀수 저장

            //이미지가 범위를 벗어날경우 처리
            if (color_ROI[index].X < 0)
                color_ROI[index].X = 0;
            if (color_ROI[index].Y < 0)
                color_ROI[index].Y = 0;

            if (color_ROI[index].X + img_width > iamge.Width)
                color_ROI[index].X = iamge.Width - img_width;
            if (color_ROI[index].Y + img_height > iamge.Height)
                color_ROI[index].Y = iamge.Height - img_height;

            //픽셀수 셈
            for (int x = color_ROI[index].X; x < color_ROI[index].X + img_width; x++)
            {
                for (int y = color_ROI[index].Y; y < color_ROI[index].Y + img_height; y++)
                {
                    if (!colorCount[y, x].Equals(new Bgr(0, 0, 0)))
                    {
                        pixCount++;
                        x_p += x;
                        y_p += y;
                    }
                }
            }

            //픽셀 개수에 따라
            if (pixCount != 0) //개수가 0이 아닐때 ROI 변경해줌
            {
                rect[index] = new Rectangle(x_p / pixCount - glo.TemplateWidth / 2, y_p / pixCount - glo.TemplateHeight / 2, glo.TemplateWidth, glo.TemplateHeight); //사각형의 왼쪽 위의 좌표
                color_ROI[index].X = x_p / pixCount - glo.TemplateWidth / 2;
                color_ROI[index].Y = y_p / pixCount - glo.TemplateHeight / 2;
            }
            else if (pixCount <= 100)//픽셀이 0이면 사라졌단 소리이므로 추적에서 제외
            {
                //rect[index] = new Rectangle(-1, -1, 0, 0); //test중
                color[index] = false;
                change_check[index] = true;

                foreach (UGV ugv in ugvList)
                {
                    if (ugv.Id.Equals("A" + index))
                    {
                        ugvList.Remove(ugv);
                        break;
                    }
                }
            }
        }

        public bool change_chk(int index)
        {
            return change_check[index];
        }

        public void change_chk_reset(int index)
        {
            change_check[index] = false;
        }

        public List<UGV> get_ugv()
        {
            return ugvList;
        }

        public void clean_hand(Image<Bgr, Byte> img, int[,] arr)
        {
            Image<Ycc, Byte> YCrCbFrame = img.Convert<Ycc, Byte>(); //YCrCb 변환
            Image<Gray, byte> colorSetting = new Image<Gray, byte>(YCrCbFrame.Width, YCrCbFrame.Height); //Ycc범위로 뽑아낸 것을 gray로 바꿔서 수축팽창 하기 위해

            Ycc YCrCb_min = new Ycc(0, 131, 90);
            Ycc YCrCb_max = new Ycc(197, 164, 143);   //살색 범위
            //Ycc YCrCb_min = new Ycc(0, 132, 69);
            //Ycc YCrCb_max = new Ycc(174, 194, 151);   //살색 범위
            colorSetting = YCrCbFrame.InRange((Ycc)YCrCb_min, (Ycc)YCrCb_max); //색 범위 설정

            StructuringElementEx rect_12 = new StructuringElementEx(12, 12, 6, 6, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_RECT);
            CvInvoke.cvErode(colorSetting, colorSetting, rect_12, 1);
            StructuringElementEx rect_6 = new StructuringElementEx(6, 6, 3, 3, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_RECT);
            CvInvoke.cvDilate(colorSetting, colorSetting, rect_6, 2); //수축 팽창

            Image<Bgr, Byte> colorCount = colorSetting.Convert<Bgr, Byte>(); //픽셀수 세기 위해

            for (int x = 0; x < img.Width; x++)
                for (int y = 0; y < img.Height; y++)
                    if (!colorCount[y, x].Equals(new Bgr(0, 0, 0)))
                        arr[y, x] = 1;
        }
    }
}
