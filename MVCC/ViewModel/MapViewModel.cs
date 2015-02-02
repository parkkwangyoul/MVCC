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
        private ObservableCollection<ModelBase> _MVCCItemList;


        public ObservableCollection<ModelBase> MVCCItemList
        {
            get
            {
                if (_MVCCItemList == null)
                    _MVCCItemList = new ObservableCollection<ModelBase>();

                return _MVCCItemList;
            }

            set
            {
                Set<ObservableCollection<ModelBase>>(ref _MVCCItemList, value);
            }
        }
        #endregion MVCCItemList

        #region MVCCItemStateList

        private ObservableCollection<State> _MVCCItemStateList;

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
                Set<List<UGV>>(ref _MVCCTempList, value);
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

        #region UGVCommand
   
        /* *
         * UGV를 넣는 코드가 들어가면 된다. (영상인식으로 들어온 UGV)
         * */
        
        public void AddUGV(List<UGV> ugvList)
        {
            foreach (UGV ugv in ugvList)
            {
                bool addUGVFlag = false;

                for(int i = 0 ; i < MVCCItemList.Count; i ++)
                {
                    if(!(MVCCItemList[i] is UGV))
                        continue;

                    UGV existUGV = MVCCItemList[i] as UGV;

                    if(existUGV.Id.Equals(ugv.Id)){
                        addUGVFlag = true;
                        break;
                    }
                }

                if (!addUGVFlag)
                {
                    MVCCItemList.Add(ugv);
                    MVCCItemStateList.Add(new State(ugv));
                }
                
                if (ugvList.Count == MVCCItemList.Count)
                    break;
            }
        }

        public void RemoveUGV(string ugvId)
        {
            UGV removeUGV = new UGV();

            for (int i = 0; i < MVCCItemList.Count; i++)
            {
                if (!(MVCCItemList[i] is UGV))
                    continue;

                UGV tempUGV = MVCCItemList[i] as UGV;

                if (tempUGV.Id.Equals(ugvId))
                {
                    removeUGV = tempUGV;

                    MVCCItemList.Remove(removeUGV);
                    break;
                }
            }

            // 없어진 UGV 상태 제거
            foreach (State state in MVCCItemStateList)
            {
                if (state.ugv.Equals(removeUGV))
                {
                    MVCCItemStateList.Remove(state);
                    break;
                }
            }

            foreach (Group group in MVCCGroupList)
            {
                foreach (UGV tempUGV in group.MemberList)
                {
                    if (tempUGV.Equals(removeUGV))
                    {
                        MVCCGroupList.Remove(group);
                        break;
                    }
                }
            }
        }

        #endregion UGVCommand

        #region AddBuildingCommand
        public void AddBuilding(List<Building> buildingList)
        {
            foreach (Building building in buildingList)
            {
                bool addBuildingFlag = false;

                foreach (ModelBase existBuilding in MVCCItemList)
                {
                    if (existBuilding is UGV)
                        continue;

                    Building ExistBuilding = existBuilding as Building;

                    if (ExistBuilding.Id.Equals(building.Id))
                    {
                        addBuildingFlag = true;
                        break;
                    }
                }

                if (!addBuildingFlag)
                {
                    MVCCItemList.Add(building);
                }

                if (buildingList.Count == MVCCItemList.Count)
                    break;
            }
        }

        //chu 추가
        public void RemoveBuilding(string ugvId)
        {
            Building removeUGV = new Building();

            for (int i = 0; i < MVCCItemList.Count; i++)
            {
                if (!(MVCCItemList[i] is Building))
                    continue;

                Building tempBuilding = MVCCItemList[i] as Building;

                if (tempBuilding.Id.Equals(ugvId))
                {
                    removeUGV = tempBuilding;

                    MVCCItemList.Remove(removeUGV);
                    break;
                }
            }
        }
        #endregion AddBuildingCommand
    }
}