using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.Linq;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MVCC.Model;

namespace MVCC.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MapViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MapViewModel class.
        /// </summary>
        public MapViewModel()
        {
            
        }

        private Globals globals = Globals.Instance;

        #region MVCCItemList

        /* *
         * MVCC Building, UGV 객체를 보관하는 MVCCItemList
         * */
        private ObservableCollection<UGV> _MVCCItemList;
        private ObservableCollection<State> _MVCCItemStateList;             


        public ObservableCollection<UGV> MVCCItemList
        {
            get
            {
                if (_MVCCItemList == null)
                    _MVCCItemList = new ObservableCollection<UGV>();

                return _MVCCItemList;
            }

            set
            {
                Set<ObservableCollection<UGV>>(ref _MVCCItemList, value); 
            }
        }
        #endregion MVCCItemList

        #region MVCCItemStateList 

        public ObservableCollection<State> MVCCItemStateList
        {
            get
            {
                if (_MVCCItemStateList == null)
                    _MVCCItemStateList = new ObservableCollection<State>();

                return _MVCCItemStateList;
            }

            set
            {
                Set<ObservableCollection<State>>(ref _MVCCItemStateList, value);
            }
        }
        #endregion MVCCItemStateList

        #region MVCCTempList
        private List<UGV> _MVCCTempList;
        public List<UGV> MVCCTempList
        {
            get 
            {
                if (_MVCCTempList == null)
                {
                    _MVCCTempList = new List<UGV>();
                }

                return _MVCCTempList;
            }
            set
            {
                Set <List<UGV>>(ref _MVCCTempList, value);
            }
        }
        #endregion MVCCTempList

        #region MVCCGroupList
        /**
         *  MVCC UGV 그룹 리스트
         * */
        private ObservableCollection<Group> _MVCCGroupList;
        public ObservableCollection<Group> MVCCGroupList
        {
            get
            {
                if (_MVCCGroupList == null)
                    _MVCCGroupList = new ObservableCollection<Group>();

                return _MVCCGroupList;
            }
            set
            {
                Set<ObservableCollection<Group>>(ref _MVCCGroupList, value);
            }
        }
        #endregion MVCCGroupList

        //private List<Point> mainTouchPoint = new List<Point>();

        #region AddUGVCommand

        private RelayCommand _AddUGVCommand;
        /* *
         * UGV를 추가
         * */
        public RelayCommand AddUGVCommand
        {
            get
            {
                return _AddUGVCommand
                    ?? (_AddUGVCommand = new RelayCommand(AddUGV));
            }
        }

        /* *
         * UGV를 넣는 코드가 들어가면 된다. (영상인식으로 들어온 UGV)
         * */

        private void AddUGV()
        {
            // Test용
            for (int i = 0; i < 3; i++)
            {
                string id = "A" + i;
                UGV ugv = new UGV(id, 50, 50, 0 + i * 100, 0 + i * 100);

                MVCCItemList.Add(ugv);
                MVCCItemStateList.Add(new State(ugv));                            
            }
        }
        #endregion AddUGVCommand

        #region AddBuildingCommand

        private RelayCommand _AddBuildingCommand;

        #endregion AddBuildingCommand

    }
}