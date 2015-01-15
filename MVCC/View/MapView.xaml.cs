﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;

using MVCC.ViewModel;
using MVCC.Model;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

using System.ComponentModel;
using System.Runtime.InteropServices;
using MVCC.Utill;

namespace MVCC.View
{
    /// <summary>
    /// MapView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MapView : UserControl
    {
        private Capture webcam; //캠 영상 받을 변수 
        Image<Bgr, Byte> obstacle_image; //캠에서 받은 원본(장애물 이미지를 위한)
        System.Drawing.Rectangle[] tracking_rect; //트래킹한 결과를 그리는 네모박스
        int[,] hand_image_arr = new int[480, 640]; //손검출한 좌표의정보
        bool obstacle_check = false; //트래킹과 장애물검사랑 동기화 위해

        // MapViewModel 가져옴
        private MapViewModel mapViewModel;

        // Globals Class
        private Globals globals = Globals.Instance;

        /**
      * 카메라를 켬
      * */
        private void CamOn(object sender, RoutedEventArgs e)
        {
            webcam = new Capture(1); //cam 설정
            thread_start(); //thread 시작
        }

        private void thread_start()
        {
            //색 트레킹 쓰레드
            BackgroundWorker thread = new BackgroundWorker();
            thread.DoWork += colorTracking;
            thread.RunWorkerAsync();

            //장애물 감지 쓰레드
            thread = new BackgroundWorker();
            thread.DoWork += obstacleDection;
            thread.RunWorkerAsync();
        }

        private void colorTracking(object sender, DoWorkEventArgs e)
        {
            bool image_is_changed = true; //영상을 비교했을때 차이가 날경우 (초기화를 true하는이유는 차가 놓여진상태에서 시작하면 바로 탬플릿 매칭을 수행해야되기때문)
            ColorTracking colorTracking = new ColorTracking(); //트래킹클래스선언

            Image<Bgr, Byte> img1 = new Image<Bgr, Byte>("testtest7.jpg"); // 템플릿 매칭할 사진     
            Image<Bgr, Byte> matchColorCheck = null;
            Image<Gray, float> matchResImage = null;
            int totalPicxel = img1.Width * img1.Height; //탬플릿이미지의 총 픽셀수(어느정도 픽셀의 기준을 잡기 위해)
            globals.TemplateWidth = img1.Width + 30;
            globals.TemplateHeight = img1.Height + 30;
            List<UGV> ugvList = new List<UGV>();

            while (true)
            {
                using (Image<Bgr, Byte> frame = webcam.QueryFrame().Flip(Emgu.CV.CvEnum.FLIP.HORIZONTAL)) //webcam에서 영상 받음
                {
                    obstacle_image = frame.Clone(); //원본 복사

                    if (image_is_changed == true) //시작할때 바로 들어고, 변화가 감지됬을때 들어가서 탬플릿 매칭 수행
                    {
                        //image_is_changed = false; // 변화변수 초기화 (나중에 변화감지함수를 구현하면 풀도록)
                        Image<Gray, Byte> img1_gray = img1.Convert<Gray, Byte>().PyrDown().PyrUp();
                        matchResImage = frame.Convert<Gray, Byte>().PyrDown().PyrUp().MatchTemplate(img1_gray, Emgu.CV.CvEnum.TM_TYPE.CV_TM_CCOEFF_NORMED); //템플릿 매칭 중간 결과 저장
                        matchColorCheck = frame.Clone(); //매치된 칼라가 저장될 변수

                        float[, ,] matches = matchResImage.Data;
                        for (int x = 0; x < matches.GetLength(1); x++)
                        {
                            for (int y = 0; y < matches.GetLength(0); y++)
                            {
                                double matchScore = matches[y, x, 0];

                                if (matchScore > 0.87)
                                {
                                    colorTracking.colorCheck(matchColorCheck, totalPicxel, x, y, globals.TemplateWidth, globals.TemplateHeight); //어떤 색인지 체크                        
                                    image_is_changed = false; //지금은 test라 여기다해놈.
                                    //변화감지 함수 구현하면 이거 지우고 암에껄로 해야함(왠지 이거 바꾸는것은 장애물변화랑 같이 해야될듯)                              
                                    y += img1.Height; //x축 다음 y축(세로)이 변화기 때문에 속도를 높이기 위해 검출된 y좌표 + 이미지 사이즈 함.                             
                                }
                            }
                        }

                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                        {
                            ugvList = colorTracking.get_ugv();
                            mapViewModel.AddUGV(ugvList);
                            refreshView();                
                        }));          
                    }

                    //(frame); //이건 트래킹되는 색상을 표시하기 위한 테스트 함수(블루)                  

                    //색상 트래킹
                    tracking_rect = colorTracking.tracking_start(frame);

                    //영상에 트레킹 결과 내보내기
                    for (int i = 0; i < 4; i++)
                    {                       
                        //AddUGV(i.ToString(), tracking_rect[i].X, tracking_rect[i].Y);
                        if (tracking_rect[i].X != 0 && tracking_rect[i].Y != 0)
                        {
                            //frame.Draw(tracking_rect[i], new Bgr(255, 255, 255), 3);
                            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                            {
                                foreach (UGV ugv in mapViewModel.MVCCItemList)
                                {
                                    if (ugv.Id.Equals("A" + i))
                                    {
                                        ugv.X = tracking_rect[i].X;
                                        ugv.Y = tracking_rect[i].Y;
                                        break;
                                    }
                                }
                                
                                refreshView();
                            }));   
                        }
                        else
                        {

                            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                            {
                                mapViewModel.RemoveUGV("A" + i);
                                refreshView();
                            }));   
                        }
                    }

                    //색상 트레킹중에 하나가 사라졌는지..(test임!! 나중엔.. 이걸로 말고 장애물 변화를 해야함. 밑에 image_is_changed는 장애물변화될떄!!!!)
                    for (int i = 0; i < 4; i++)
                    {
                        if (colorTracking.change_chk(i) == true)
                        {
                            image_is_changed = true;
                            colorTracking.change_chk_reset(i);
                        }
                    }

                    //손 색 지우기
                    //colorTracking.clean_hand(frame, hand_image_arr);

                    System.Drawing.Rectangle map_rect = new System.Drawing.Rectangle(frame.Width / 8, frame.Height / 6, frame.Width / 8 * 6, frame.Height / 6 * 4);
                    frame.Draw(map_rect, new Bgr(0, 255, 0), 3);

                    //System.Drawing.Rectangle map_outside_rect = new System.Drawing.Rectangle(frame.Width / 8 - 30, frame.Height / 6 - 30, frame.Width / 8 * 6 + 30 * 2, frame.Height / 6 * 4 + 30 * 2);
                    //frame.Draw(map_outside_rect, new Bgr(0, 255, 255), 3);

                    /*
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                    {
                        InputImage4.Source = frame.ToBitmapSource(); //원본 영상 + 트래킹결과                          
                    }));
                    */


                    obstacle_check = true; //장애물이미지와 싱크 맞추기 위해 설정
                }
            }
        }

        private void obstacleDection(object sender, DoWorkEventArgs e)
        {
            ObstacleDetection obstacleDetection = new ObstacleDetection();

            Image<Gray, Byte> cannyRes; //트래킹돈 부분을 제외한 원본 이미지
            Image<Bgr, Byte> gridImage; //장애물을 표시하는 이미지(배열에도 저장함)
            Image<Gray, Byte> pre_image = null; //이전 이미지 저장
            Image<Gray, Byte> dst_image = null; //차영상의 대한 결과 저장

            int[,] Map_obstacle = new int[48, 60]; //Map의 장애물의 정보 
            int frame_count = 0; //frame 카운터를 샘(차영상에서 지연을 주기 위해)

            while (true)
            {
                if (obstacle_check == true) //frame의 추적 영상 처리가 끝나고 처리
                {
                    cannyRes = obstacleDetection.cannyEdge(obstacle_image, tracking_rect, hand_image_arr); //외곽선 땀
                    gridImage = obstacleDetection.drowGrid(cannyRes, obstacle_image.Clone(), Map_obstacle, 20, 20); //Map 정보 만듬

                    frame_count++; //프레임수 셈

                    if (frame_count == 5) //5프레임 마다 변화 검사
                    {
                        /*
                        if (obstacleDetection.sub_image(cannyRes, pre_image, dst_image) == 1)
                        {
                        }
                        */
                        dst_image = obstacleDetection.sub_image(cannyRes, pre_image, dst_image); //차영상 구함
                        frame_count = 0;
                        /*
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                        {
                            inputImage3.Source = dst_image.ToBitmapSource(); //차영상 결과                  
                        }));
                         * */
                    }

                    pre_image = cannyRes.Clone(); //차영상을 위한 이전프레임 설정
                    /*
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                    {
                        inputImage2.Source = cannyRes.ToBitmapSource(); //canny 결과               
                        inputImage6.Source = gridImage.ToBitmapSource(); //grid 그림결과                
                    }));
                    */
                    obstacle_check = false;
                }
            }

        }







        public MapView()
        {
            InitializeComponent();

            mapViewModel = DataContext as MapViewModel;

            (FindResource("UGVItemSrc") as CollectionViewSource).Source = mapViewModel.MVCCItemList;

            (FindResource("UGVStateSrc") as CollectionViewSource).Source = mapViewModel.MVCCItemStateList;

            (FindResource("UGVGroupSrc") as CollectionViewSource).Source = mapViewModel.MVCCGroupList;            
        }

        // UGV를 선택하는 모드
        private void SelectUGV(object sender, MouseButtonEventArgs e)
        {
            IInputElement clickedElement = Mouse.DirectlyOver;

            // 그룹모드로 선택할때
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (clickedElement is System.Windows.Shapes.Ellipse)
                {
                    System.Windows.Shapes.Ellipse ellipse = clickedElement as System.Windows.Shapes.Ellipse;

                    Grid grid = ellipse.Parent as Grid;
                    string id = (grid.Children[0] as TextBlock).Text;

                    UGV ugv = new UGV();

                    for (int i = 0; i < mapViewModel.MVCCItemList.Count; i++)
                    {
                        UGV tempUGV = mapViewModel.MVCCItemList[i];
                        if (tempUGV.Id.Equals(id))
                            ugv = tempUGV;

                        // 그룹 선택을 할때, 하나하나 선택된것들을 모두 해제한다.
                        if (tempUGV.IsClicked)
                        {
                            cancelSelectUGV(tempUGV);
                        }
                    }

                    if (!ugv.IsBelongToGroup)
                    {
                        // 그룹 선택이 안된 것
                        if (!ugv.IsClickedReadyBelongToGroup)
                        {                            
                            ugv.IsClickedReadyBelongToGroup = true;

                            selectUGVAndStateChangeLayout(ugv, "Blue", id);

                            mapViewModel.MVCCTempList.Add(ugv);
                        }
                        else
                        {
                            ugv.IsClickedReadyBelongToGroup = false;

                            cancelSelectUGV(ugv);

                            RemoveSelectedUGVInGroupTempList(ugv);
                        }

                        // 그룹 대기열에 없는 UGV중에 이미 그룹에 속한 UGV들의 Layout을 해제                       
                        for (int i = 0; i < mapViewModel.MVCCItemList.Count; i++)
                        {
                            UGV tempUGV = mapViewModel.MVCCItemList[i];

                            if (!tempUGV.Id.Equals(ugv.Id) && tempUGV.IsBelongToGroup)
                            {
                                cancelSelectUGV(tempUGV);
                            }
                        }
                    }

                    refreshView();
                }
                else
                {
                    cancelSelectUGV();

                    refreshView();
                }
            }

            // 그룹이 선택된 상태에서 Alt를 누르고 부대선택되지 않은 UGV를 선택하면, 그 그룹에 추가된다.
            else if (Keyboard.Modifiers == ModifierKeys.Alt) 
            {
                Group group = findClickedGroup();

                if (group != null)
                {
                    if (clickedElement is System.Windows.Shapes.Ellipse)
                    {
                        System.Windows.Shapes.Ellipse ellipse = clickedElement as System.Windows.Shapes.Ellipse;

                        Grid grid = ellipse.Parent as Grid;
                        string id = (grid.Children[0] as TextBlock).Text;

                        UGV ugv = new UGV();
                                                
                        for (int i = 0; i < mapViewModel.MVCCItemList.Count; i++)
                        {
                            UGV tempUGV = mapViewModel.MVCCItemList[i];
                            if (tempUGV.Id.Equals(id))
                            {
                                ugv = tempUGV;
                            }
                        }

                        if (!ugv.IsBelongToGroup)
                        {

                            ugv.GroupName = group.Name;
                            ugv.IsBelongToGroup = true;

                            group.MemberList.Add(ugv);

                            selectUGVAndStateChangeLayout(ugv, group.StateBorderBrush, ugv.Id);
                        }
                        else
                        {
                            ugv.GroupName = null;
                            ugv.IsBelongToGroup = false;

                            group.MemberList.Remove(ugv);

                            cancelSelectUGV(ugv);
                        }
                    }
                }

                refreshView();
            }

            // 하나하나 선택할때
            else
            {
                if (clickedElement is System.Windows.Shapes.Ellipse)
                {
                    System.Windows.Shapes.Ellipse ellipse = clickedElement as System.Windows.Shapes.Ellipse;

                    Grid grid = ellipse.Parent as Grid;
                    string id = (grid.Children[0] as TextBlock).Text;

                    UGV ugv = new UGV();

                    // 선택한 UGV를 찾아서 나머지 선택을 해제
                    for (int i = 0; i < mapViewModel.MVCCItemList.Count; i++)
                    {
                        UGV tempUGV = mapViewModel.MVCCItemList[i];
                        if (!tempUGV.Id.Equals(id))
                        {
                            cancelSelectUGV(tempUGV);
                        }
                        else
                        {
                            ugv = tempUGV;
                        }
                    }

                    /**
                     * UGV가 그룹에 속해 있으면, 그룹이 선택되고, 그룹전체의 UGV가 선택됨
                     * UGV가 그룹에 속해 있지 않으면, 선택한 UGV만 선택되고 그 UGV의 State만 선택됨
                     * */
                    if (!ugv.IsBelongToGroup)
                    {
                        ugv.IsClicked = true;
                        selectUGVAndStateChangeLayout(ugv, "Red", id);
                    }
                    else
                    {
                        Group selectGroup = new Group();

                        for (int i = 0; i < mapViewModel.MVCCGroupList.Count; i++)
                        {
                            Group tempGroup = mapViewModel.MVCCGroupList[i];

                            if (tempGroup.MemberList.Contains(ugv))
                            {
                                selectGroup = tempGroup;
                            }
                        }

                        for (int i = 0; i < selectGroup.MemberList.Count; i++)
                        {
                            UGV tempUGV = selectGroup.MemberList[i];

                            selectUGVAndStateChangeLayout(tempUGV, selectGroup.StateBorderBrush, tempUGV.Id);

                            tempUGV.IsGroupClicked = true;
                        }
                    }

                    refreshView();
                }
                else
                {
                    cancelSelectUGV();

                    refreshView();
                }
            }
        }        

        private void MakeGroup(object sender, KeyEventArgs e)
        {

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (mapViewModel.MVCCTempList.Count > 0)
                {
                    int groupNum = findGroupNum(e.Key);

                    if (groupNum != 0)
                        MakeGroup(groupNum);
                }
                else
                {
                    MessageBox.Show("그룹 대기열에 포함된 UGV가 없습니다.");
                }
            }
            
            // 해당 그룹 번호를 누르면 해당그룹이 선택됨.
            else
            {
                int groupNum = findGroupNum(e.Key);

                for (int i = 0; i < mapViewModel.MVCCGroupList.Count; i++)
                {
                    Group tempGroup = mapViewModel.MVCCGroupList[i];

                    if (tempGroup.Name.Equals("G" + groupNum))
                    {
                        for (int j = 0; j < tempGroup.MemberList.Count; j++)
                        {
                            UGV tempUGV = tempGroup.MemberList[j];
                            selectUGVAndStateChangeLayout(tempUGV, tempGroup.StateBorderBrush, tempUGV.Id);
                            tempUGV.IsGroupClicked = true;
                        }
                    }
                    else
                    {
                        if (Keyboard.Modifiers != ModifierKeys.Control
                            && Keyboard.Modifiers != ModifierKeys.Alt
                            && Keyboard.Modifiers != ModifierKeys.Shift)
                        {
                            for (int j = 0; j < tempGroup.MemberList.Count; j++)
                            {
                                UGV tempUGV = tempGroup.MemberList[j];
                                cancelSelectUGV(tempUGV);
                            }
                        }
                    }
                }
                //내일오면 선택부분 오류 수정
                refreshView();
            }
        }

        private void MakeGroup(int groupNum)
        {
            string GroupName = "G" + groupNum;

            // 이미 존재하는 그룹인지 검사
            bool isExisted = false;
            for (int i = 0; i < mapViewModel.MVCCGroupList.Count; i++)
            {
                Group tempGroup = mapViewModel.MVCCGroupList[i];

                if (tempGroup.Name.Equals(GroupName))
                {
                    isExisted = true;
                }
            }

            if (!isExisted)
            {
                Group group = new Group(GroupName);

                group.StateBorderBrush = getGroupColor(groupNum);

                // 선택됬던 UGV들을 그룹에 포함시킴
                for (int i = 0; i < mapViewModel.MVCCTempList.Count; i++)
                {
                    UGV tempUGV = mapViewModel.MVCCTempList[i];
                    tempUGV.UGVStroke = group.StateBorderBrush;
                    tempUGV.IsClickedReadyBelongToGroup = false;

                    tempUGV.IsBelongToGroup = true;
                    tempUGV.GroupName = GroupName;

                    group.MemberList.Add(tempUGV);
                }

                for (int i = 0; i < group.MemberList.Count; i++)
                {
                    UGV tempUGV = group.MemberList[i];

                    for (int j = 0; j < mapViewModel.MVCCItemStateList.Count; j++)
                    {
                        State tempState = mapViewModel.MVCCItemStateList[j];

                        if (tempUGV.Id.Equals(tempState.ugv.Id))
                        {
                            tempState.StateBorderBrush = group.StateBorderBrush;
                        }
                    }
                }
                
                mapViewModel.MVCCGroupList.Add(group);

                mapViewModel.MVCCTempList.Clear();

                refreshView();
            }
            else
            {
                MessageBox.Show("이미 " + GroupName + "이 존재합니다.");
            }
        }

        // 선택한 UGV의 Layout을 변경해주는 기능
        private void selectUGVAndStateChangeLayout(UGV ugv, string color, string id)
        {
            ugv.UGVStrokeThickness = 2;
            ugv.UGVStroke = color;
            
            for (int i = 0; i < mapViewModel.MVCCItemStateList.Count; i++)
            {
                State tempState = mapViewModel.MVCCItemStateList[i];

                if (tempState.ugv.Id.Equals(id))
                {
                    tempState.StateBorderBrush = color;
                }
            }
        }

        // 특정 UGV의 선택을 해제하는 기능
        private void cancelSelectUGV(UGV ugv)
        {
            ugv.UGVStrokeThickness = 0;
            ugv.IsClicked = false;
            ugv.IsClickedReadyBelongToGroup = false;
            ugv.IsGroupClicked = false;

            for (int j = 0; j < mapViewModel.MVCCItemStateList.Count; j++)
            {
                State tempState = mapViewModel.MVCCItemStateList[j];

                if (tempState.ugv.Id.Equals(ugv.Id))
                {
                    tempState.StateBorderBrush = "#78C8FF";
                }
            }

        }

        // UGV 전체의 선택을 해제하는 기능
        private void cancelSelectUGV()
        {
            // UGV가 아닌 다른곳을 클릭했을경우 선택이 해제된다.
            for (int i = 0; i < mapViewModel.MVCCItemList.Count; i++)
            {
                UGV tempUGV = mapViewModel.MVCCItemList[i];

                tempUGV.UGVStrokeThickness = 0;
                tempUGV.IsClicked = false;
                tempUGV.IsClickedReadyBelongToGroup = false;
                tempUGV.IsGroupClicked = false;
            }

            for (int i = 0; i < mapViewModel.MVCCItemStateList.Count; i++)
            {
                State tempState = mapViewModel.MVCCItemStateList[i];

                tempState.StateBorderBrush = "#78C8FF";
            }

            // 그룹에 포함되지 않았지만, 대기중인 UGV전체를 해제한다.
            mapViewModel.MVCCTempList.Clear();
        }

        // UGV가 그룹 대기열에 들어갔을때, 그것을 다시 선택할때, UGV가 해제됨.
        private void RemoveSelectedUGVInGroupTempList(UGV ugv)
        {
            // 그룹에 포함되지 않았지만, 대기중이 UGV들 중에 선택한 UGV가 해제된다.
            for (int i = 0; i < mapViewModel.MVCCTempList.Count; i++)
            {
                UGV tempUGV = mapViewModel.MVCCTempList[i];

                if (tempUGV.Id.Equals(ugv.Id))
                {
                    mapViewModel.MVCCTempList.Remove(tempUGV);
                }
            }
        }

        private void refreshView()
        {
            // View에 반영
            (FindResource("UGVItemSrc") as CollectionViewSource).View.Refresh();

            (FindResource("UGVStateSrc") as CollectionViewSource).View.Refresh();

            (FindResource("UGVGroupSrc") as CollectionViewSource).View.Refresh();
        }

        // 숫자키에 대응되는 그룹의 번호
        private int findGroupNum(Key key)
        {
            switch (key)
            {
                case Key.D1: return 1;
                case Key.D2: return 2;
                case Key.D3: return 3;
                case Key.D4: return 4;
                default: return 0;
            }
        }

        private Group findClickedGroup()
        {
            for (int i = 0; i < mapViewModel.MVCCGroupList.Count; i++)
            {
                Group tempGroup = mapViewModel.MVCCGroupList[i];

                for (int j = 0; j < tempGroup.MemberList.Count; j++)
                {
                    UGV tempUGV = tempGroup.MemberList[j];
                    if (tempUGV.IsBelongToGroup && tempUGV.IsGroupClicked)
                    {
                        return tempGroup;
                    }
                }
            }

            return null;
        }

        private string getGroupColor(int groupNum)
        {
            switch (groupNum)
            {
                case (int)GroupColor.Orange:
                    return GroupColor.Orange.ToString();
                case (int)GroupColor.Pink:
                    return GroupColor.Pink.ToString();
                case (int)GroupColor.White:
                    return GroupColor.White.ToString();
                case (int)GroupColor.Yellow:
                    return GroupColor.Yellow.ToString();      

                default:
                    return "Green";
            }
        }
    }
}
