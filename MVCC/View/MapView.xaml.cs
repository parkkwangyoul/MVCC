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
        }

        private void SelectUGV(object sender, MouseButtonEventArgs e)
        {
            IInputElement clickedElement = Mouse.DirectlyOver;

            if (clickedElement is Ellipse)
            {
                //rectangle was clicked...do your thing here:
                Ellipse ellipse = clickedElement as Ellipse;


                // GroupMode일때 클릭
                if (globals.IsGroupMode)
                {
                    if (ellipse.StrokeThickness == 0)
                        ellipse.StrokeThickness = 5;
                    else
                        ellipse.StrokeThickness = 0;
                }
                else
                {
                    Grid grid = ellipse.Parent as Grid;
                    string id = (grid.Children[1] as TextBlock).Text;

                    UGV ugv = new UGV();

                    // 선택한 UGV를 찾아서 나머지 선택을 해제
                    for (int i = 0; i < mapViewModel.MVCCItemList.Count; i ++ )
                    {
                        UGV tempUGV = mapViewModel.MVCCItemList[i];
                        if (!tempUGV.Id.Equals(id))
                        {
                            tempUGV.UGVStrokeThickness = 0;
                            tempUGV.IsClicked = false;
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
                        ugv.UGVStrokeThickness = 2;
                        ugv.UGVStroke = "Red";
                        ugv.IsClicked = true;
                    }
                    else
                    {

                    }

                    for (int i = 0; i < mapViewModel.MVCCItemStateList.Count; i++)
                    {
                        State tempState = mapViewModel.MVCCItemStateList[i];

                        if(tempState.ugv.Id.Equals(id)){
                            tempState.StateBorderBrush = "Red";
                        } else {
                            tempState.StateBorderBrush = "#78C8FF";
                        }
                    }

                    // View에 반영
                    (FindResource("UGVItemSrc") as CollectionViewSource).View.Refresh();

                    (FindResource("UGVStateSrc") as CollectionViewSource).View.Refresh();
                }
            }
            else
            {

                // UGV가 아닌 다른곳을 클릭했을경우 선택이 해제된다.
                for (int i = 0; i < mapViewModel.MVCCItemList.Count; i++)
                {
                    UGV tempUGV = mapViewModel.MVCCItemList[i];
                    
                    tempUGV.UGVStrokeThickness = 0;
                    tempUGV.IsClicked = false;
                }

                for (int i = 0; i < mapViewModel.MVCCItemStateList.Count; i++)
                {
                    State tempState = mapViewModel.MVCCItemStateList[i];

                    tempState.StateBorderBrush = "#78C8FF";
                }

                // View에 반영
                (FindResource("UGVItemSrc") as CollectionViewSource).View.Refresh();

                (FindResource("UGVStateSrc") as CollectionViewSource).View.Refresh();
            }
        }

        private void EndPointUGV(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition((IInputElement)sender);
             // GroupMode일때 클릭
            if (globals.IsGroupMode)
            {
            }
            else
            {
                // 선택한 UGV를 찾아서 나머지 선택을 해제
                for (int i = 0; i < mapViewModel.MVCCItemList.Count; i++)
                {
                    UGV tempUGV = mapViewModel.MVCCItemList[i];

                    if (tempUGV.IsClicked)
                    {
                        tempUGV.X = p.X - tempUGV.Width / 2;
                        tempUGV.Y = p.Y - tempUGV.Height / 2;


                        // View에 반영
                        (FindResource("UGVItemSrc") as CollectionViewSource).View.Refresh();

                        (FindResource("UGVStateSrc") as CollectionViewSource).View.Refresh();
                    }
                }
                    

            }
        }
    }
}
