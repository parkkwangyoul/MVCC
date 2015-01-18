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
    public class SettingViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the SettingViewModel class.
        /// </summary>
        /// 

        private Globals globals = Globals.Instance;

        public SettingViewModel()
        {
            for (int i = 0; i < 4; i++)
            {
                globals.UGVSettingDictionary.Add("A" + i, new UGV());
            }
        }
    }
}