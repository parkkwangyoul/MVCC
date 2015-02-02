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
        string[] colorStr = { "blue", "green", "orange", "red" };

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
                    if (i == 0) //blue
                       YccColorCheck(i, 0, 41, 151, 255, 119, 240); //8섹션
                    else if (i == 1) //green
                         YccColorCheck(i, 0, 75, 100, 186, 123, 143); //8섹션
                    else if (i == 2) //orange
                         YccColorCheck(i, 0, 155, 0, 255, 218, 106); //8섹션
                    else //red
                        YccColorCheck(i, 0, 159, 102, 196, 240, 124); //8섹션
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

                        if (totalPicxel / 10 <= pixCount) //일정 픽섹 이상시 색상배열 변경후 종료
                        {
                            color[index] = true;
                            change_check[index] = false;
                            color_ROI[index].X = x;
                            color_ROI[index].Y = y;
                            ugvList.Add(new UGV("A" + index, glo.TemplateWidth, glo.TemplateHeight, x, y, colorStr[index]));
                            return;
                        }
                    }
                }
            }
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
                        color_traking(i, 0, 41, 151, 255, 119, 240, iamge, rect); //8섹션
                    else if (i == 1)
                         color_traking(i, 0, 75, 100, 186, 123, 143, iamge, rect); //8섹션
                    else if (i == 2)
                         color_traking(i, 0, 155, 0, 255, 218, 106, iamge, rect); //8섹션
                    else
                         color_traking(i, 0, 159, 102, 196, 240, 124, iamge, rect); //8섹션
                }// ([0]blue [1] green [2]orange [3]red)
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
            if (pixCount >= 10) //개수가 0이 아닐때 ROI 변경해줌
            {
                //사라진것을 판별하기 위해.. 원랜 마이너스값으로 좌표가 계산되어 일부러 음수좌표는 0으로 만들고 사라졌을때 좌표를 -1로 만듬
                int tmp_x = x_p / pixCount - glo.TemplateWidth / 2;
                int tmp_y = y_p / pixCount - glo.TemplateHeight / 2;

                int tmp_width = glo.TemplateWidth;
                int tmp_height = glo.TemplateHeight;

                if (tmp_x < 0)
                {
                    tmp_width += tmp_x;
                    tmp_x = 0;
                }
                if (tmp_y < 0)
                {
                    tmp_height += tmp_y;
                    tmp_y = 0;
                }

                rect[index] = new Rectangle(tmp_x, tmp_y, tmp_width, tmp_height); //사각형의 왼쪽 위의 좌표
                color_ROI[index].X = x_p / pixCount - glo.TemplateWidth / 2;
                color_ROI[index].Y = y_p / pixCount - glo.TemplateHeight / 2;
            }
            else
            {
                rect[index] = new Rectangle(0, 0, 0, 0);  //사라졌을때 좌표를 -1로 만듬
                color[index] = false;
                change_check[index] = true;
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
    }
}
