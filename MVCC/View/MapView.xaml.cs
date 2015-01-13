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

namespace MVCC.View
{
    /// <summary>
    /// MapView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MapView : UserControl
    {

        // MapViewModel 가져옴
        private MapViewModel mapViewModel;

        // Globals Class
        private Globals globals = Globals.Instance;

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
                if (clickedElement is Ellipse)
                {
                    Ellipse ellipse = clickedElement as Ellipse;

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
                    if (clickedElement is Ellipse)
                    {
                        Ellipse ellipse = clickedElement as Ellipse;

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
                if (clickedElement is Ellipse)
                {
                    Ellipse ellipse = clickedElement as Ellipse;

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
