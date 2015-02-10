using System;
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
        bool obstacle_check = false; //트래킹과 장애물검사랑 동기화 위해
        bool image_is_changed = true; //영상을 비교했을때 차이가 날경우 (초기화를 true하는이유는 차가 놓여진상태에서 시작하면 바로 탬플릿 매칭을 수행해야되기때문)

        // MapViewModel 가져옴
        private MapViewModel mapViewModel;

        // Globals Class
        private Globals globals = Globals.Instance;

        private BluetoothAndPathPlanning bluetoothAndPathPlanning;
        List<UGV> ugvList = new List<UGV>();

        #region 카메라, thread start
        /**
      * 카메라를 켬
      * */
        private void CamOn(object sender, RoutedEventArgs e)
        {
            // 카메라 없을때, 테스트용        
            MockCameraOn();
            // 카메라 연결했을때
            //CameraOnAndDetectThings();
        }

        #region TestMock
        private void MockCameraOn()
        {

            for (int i = 0; i < 4; i++)
            {
                UGV ugv = new UGV("A" + i, 50, 50, 50 + 50 * i, 50 + 50 * i, "Green");

                ugvList.Add(ugv);
            }
            
            mapViewModel.AddUGV(ugvList);

            test_thread();
        }

        private void test_thread()
        {
            //색 트레킹 쓰레드
            BackgroundWorker thread = new BackgroundWorker();
            thread.DoWork += test_redirect;
            thread.RunWorkerAsync();
        }

        private void test_redirect(object sender, DoWorkEventArgs e)
        {
            bool check = false;
            while (true)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (check)
                    {
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                        {
                        ugvList[i].X -= 1;
                        ugvList[i].Y -= 1;


                        refreshView();
                        }));
                    }
                    else
                    {
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                        {
                        ugvList[i].X += 1;
                        ugvList[i].Y += 1;


                        refreshView();
                        }));
                    }
                }

                check = !check;

                Thread.Sleep(30);

                Console.WriteLine("child");
            }
        }

        #endregion TestMock

        private void CameraOnAndDetectThings()
        {            
            webcam = new Capture(0); //cam 설정
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
        #endregion 카메라 and thread start

        #region color 트레킹
        //색상 트레킹
        private void colorTracking(object sender, DoWorkEventArgs e)
        {
            ColorTracking colorTracking = new ColorTracking(); //트래킹클래스선언

            Image<Bgr, Byte> img1 = new Image<Bgr, Byte>("testtest7.jpg"); // 템플릿 매칭할 사진     
            Image<Gray, Byte> img1_gray = img1.Convert<Gray, Byte>().PyrDown().PyrUp();
            Image<Bgr, Byte> matchColorCheck = null;
            Image<Gray, float> matchResImage = null;
            int totalPicxel = img1.Width * img1.Height; //탬플릿이미지의 총 픽셀수(어느정도 픽셀의 기준을 잡기 위해)
            globals.TemplateWidth = img1.Width + 30;
            globals.TemplateHeight = img1.Height + 30;
            List<UGV> ugvList = new List<UGV>();

            while (true)
            {
                using (Image<Bgr, Byte> frame = webcam.QueryFrame()) //webcam에서 영상 받음
                {
                    frame.ROI = new System.Drawing.Rectangle(0, 0, globals.rect_width, globals.rect_height); // 정한 범위를 ROI로 설정                 
                    obstacle_image = frame.Clone(); //원본 복사

                    if (image_is_changed == true) //시작할때 바로 들어고, 변화가 감지됬을때 들어가서 탬플릿 매칭 수행
                    {
                        image_is_changed = false; // 변화감지되면 true해서 들어옴
                        matchResImage = frame.Convert<Gray, Byte>().PyrDown().PyrUp().MatchTemplate(img1_gray, Emgu.CV.CvEnum.TM_TYPE.CV_TM_CCOEFF_NORMED); //템플릿 매칭 중간 결과 저장
                        matchColorCheck = frame.Clone(); //매치된 칼라가 저장될 변수

                        float[, ,] matches = matchResImage.Data;
                        for (int x = 0; x < matches.GetLength(1); x++)
                        {
                            for (int y = 0; y < matches.GetLength(0); y++)
                            {
                                double matchScore = matches[y, x, 0];

                                if (matchScore > 0.7)
                                {
                                    colorTracking.colorCheck(matchColorCheck, totalPicxel, x, y, globals.TemplateWidth, globals.TemplateHeight); //어떤 색인지 체크                        
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
                        if (tracking_rect[i].Width != 0 && tracking_rect[i].Height != 0)
                        {
                            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                            {
                                for (int j = 0; j < mapViewModel.MVCCItemList.Count; j++)
                                {
                                    if (!(mapViewModel.MVCCItemList[j] is UGV))
                                        continue;

                                    UGV ugv = mapViewModel.MVCCItemList[j] as UGV;

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
                    
                    obstacle_check = true; //장애물이미지와 싱크 맞추기 위해 설정
                }
            }
        }
        #endregion color 트레킹

        #region obstacle 검출
        //장애물 검출
        private void obstacleDection(object sender, DoWorkEventArgs e)
        {
            ObstacleDetection obstacleDetection = new ObstacleDetection();
            globals.Map_obstacle = new int[globals.rect_height / globals.y_grid, globals.rect_width / globals.x_grid]; //Map의 장애물의 정보 
            globals.pre_Map_obstacle = new int[globals.rect_height / globals.y_grid, globals.rect_width / globals.x_grid]; //이전 Map의 장애물의 정보 
         
           int blob_count = 0, pre_blob_count = 0; //blob count의 변화감지를 위해      
            bool frist_change_check = false;
            List<Building> building_List = new List<Building>();

            while (true)
            {
                if (obstacle_check == true) //frame의 추적 영상 처리가 끝나고 처리
                {
                    blob_count = obstacleDetection.detectBlob(obstacle_image, globals.Map_obstacle, tracking_rect); //장애물 검출

                    if (frist_change_check == true) //제일 처음 변화감지는 건너 뜀
                    {
                        if (pre_blob_count != blob_count) //이전 blob과 현재 blob의 카운터가 다르면 Map에 장애물 수 생김 
                        {
                            Console.WriteLine("Map의 장애물 수 변화 !!! pre_blob_count = " + pre_blob_count + " blob_count = " + blob_count);
                            image_is_changed = true; //Map변화가 감지 됬으니 탬플릿 매칭 시작
                        }

                        //장애물이 옮겨짐을 검사. 옮겨지고 있어도 차량은 정지 해야함
                        int moving_check_count = 0;

                        for (int i = 0; i < globals.rect_width / globals.x_grid; i++)
                            for (int j = 0; j < globals.rect_height / globals.y_grid; j++)
                                if (globals.Map_obstacle[j, i] != 2 || globals.pre_Map_obstacle[j, i] != 2)
                                    if (globals.Map_obstacle[j, i] != globals.pre_Map_obstacle[j, i])
                                        moving_check_count++;

                        if (moving_check_count >= 5) //배열이 5개 이상 차이날 경우 장애물이 옮겨지고 있음
                            Console.WriteLine("장애물 옮기는 중!");
                    }
                    else
                        frist_change_check = true;

                    pre_blob_count = blob_count; //현재 blob_count를 이전 blob_count에 저장
                    globals.pre_Map_obstacle = (int[,])globals.Map_obstacle.Clone(); //비교를 위해 이전 Map정보 설정
                    //Array.Clear(globals.Map_obstacle, 0, globals.rect_height / globals.y_grid * globals.rect_width / globals.x_grid);


                    globals.mutex = true;
 
                    if (!globals.mutex)
                    {
                        Console.WriteLine("너가 문제냐");
                        for (int i = 0; i < globals.rect_width / globals.x_grid; i++)
                            for (int j = 0; j < globals.rect_height / globals.y_grid; j++)
                                globals.Map_obstacle[j, i] = 0;
                    }

                    globals.mutex = false;
                   
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                    {
                        building_List = obstacleDetection.get_building();
                        mapViewModel.AddBuilding(building_List);

                       
                        for (int i = 0; i < building_List.Count; i++) //DisapperCheck 가 false경우 추적에서 사라진거므로 list에 제거
                        {
                            Building remov_tmp = building_List[i];
                
                            if (remov_tmp.DisapperCheck == false)
                            {
                                building_List.Remove(remov_tmp);
                                mapViewModel.RemoveBuilding(remov_tmp.Id);
                            }
                        }

                        for (int i = 0; i < mapViewModel.MVCCItemList.Count; i++)
                        {

                            if (!(mapViewModel.MVCCItemList[i] is Building))
                                continue;

                            Building building = mapViewModel.MVCCItemList[i] as Building;

                            if (building.Id.Equals(building.Id))
                            {
                                foreach (Building temp_building in building_List) //list의 정보를 다 갱신
                                {
                                    if (temp_building.Id.Equals(building.Id))
                                    {
                                        building.X = temp_building.X;
                                        building.Y = temp_building.Y;
                                        building.Width = temp_building.Width;
                                        building.Height = temp_building.Height;
                                        building.DisapperCheck = true;
                                        break;
                                    }
                                }
                            }
                        }
                        refreshView();
                    }));
                }
            }
        }
        #endregion obstacle 검출


        public MapView()
        {
            InitializeComponent();

            mapViewModel = DataContext as MapViewModel;

            (FindResource("MVCCItemSrc") as CollectionViewSource).Source = mapViewModel.MVCCItemList;

            (FindResource("UGVStateSrc") as CollectionViewSource).Source = mapViewModel.MVCCItemStateList;

            (FindResource("UGVGroupSrc") as CollectionViewSource).Source = mapViewModel.MVCCGroupList;

            bluetoothAndPathPlanning = new BluetoothAndPathPlanning();           
        }

        // UGV에게 이동 명령을 내림
        private void MoveUGV(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);
            
            int endPointX = ((int)Math.Round(p.X) - 155) / 15;
            int endPointY = ((int)Math.Round(p.Y) - 160) / 15;

            // 개인용
            UGV individualUGV = null;
            State individualUGVState = null;

            // 그룹용
            List<UGV> GroupList = new List<UGV>();
            string groupName = "";
            
            //모드 검사용
            string mode = "N";

            // UGV 
            for (int i = 0; i < mapViewModel.MVCCItemList.Count; i++)
            {
                if (!(mapViewModel.MVCCItemList[i] is UGV))
                    continue;

                UGV tempUGV = mapViewModel.MVCCItemList[i] as UGV;

                // 개인이 선택된 것인지 검사
                if (tempUGV.IsClicked)
                {
                    individualUGV = tempUGV;

                    mode = "I";

                    break;
                }
                else if (tempUGV.IsGroupClicked)
                {
                    if (groupName == null || groupName.Equals(tempUGV.GroupName))
                    {
                        GroupList.Add(tempUGV);
                    }

                    mode = "G";
                }
            }

            //State
            for(int i = 0 ; i < mapViewModel.MVCCItemStateList.Count; i ++){
                State tempState = mapViewModel.MVCCItemStateList[i];

                if(mode.Equals("I")){
                    if(tempState.ugv.Id.Equals(individualUGV.Id)){
                        individualUGVState = tempState;
                    }
                }
            }

            // 개인
            if (mode.Equals("I"))
            {
                individualUGVState.EndPointX = endPointX;
                individualUGVState.EndPointY = endPointY;

                individualUGV.Command = "f";

                bluetoothAndPathPlanning.connect(individualUGV, individualUGVState);

                refreshView();
            }


        }

        // UGV를 선택하는 모드
        private void SelectUGV(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Main");
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
                        if (!(mapViewModel.MVCCItemList[i] is UGV))
                            continue;

                        UGV tempUGV = mapViewModel.MVCCItemList[i] as UGV;
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
                            if (!(mapViewModel.MVCCItemList[i] is UGV))
                                continue;

                            UGV tempUGV = mapViewModel.MVCCItemList[i] as UGV;

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
                            if (!(mapViewModel.MVCCItemList[i] is UGV))
                                continue;

                            UGV tempUGV = mapViewModel.MVCCItemList[i] as UGV;
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
                        else if (ugv.GroupName.Equals(group.Name))
                        {
                            ugv.GroupName = null;
                            ugv.IsBelongToGroup = false;

                            group.MemberList.Remove(ugv);

                            cancelSelectUGV(ugv);
                        }
                        else
                        {
                            MessageBox.Show("이미 다른 그룹에 속한 UGV 입니다.");
                        }
                    }
                }

                refreshView();
            }

            // 하나하나 선택할때
            else
            {
                //Console.WriteLine("Red Circle");
                if (clickedElement is System.Windows.Shapes.Ellipse)
                {
                    System.Windows.Shapes.Ellipse ellipse = clickedElement as System.Windows.Shapes.Ellipse;

                    Grid grid = ellipse.Parent as Grid;
                    string id = (grid.Children[0] as TextBlock).Text;

                    UGV ugv = new UGV();

                    // 선택한 UGV를 찾아서 나머지 선택을 해제
                    for (int i = 0; i < mapViewModel.MVCCItemList.Count; i++)
                    {
                        if (!(mapViewModel.MVCCItemList[i] is UGV))
                            continue;

                        UGV tempUGV = mapViewModel.MVCCItemList[i] as UGV;

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

            if (Keyboard.Modifiers == ModifierKeys.Shift || Keyboard.Modifiers == ModifierKeys.Alt)
                return;

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
                if (!(mapViewModel.MVCCItemList[i] is UGV))
                    continue;

                UGV tempUGV = mapViewModel.MVCCItemList[i] as UGV;

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
            (FindResource("MVCCItemSrc") as CollectionViewSource).View.Refresh();

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
