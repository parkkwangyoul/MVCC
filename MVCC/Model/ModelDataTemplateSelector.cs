using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using MVCC.Model;


namespace MVCC.Model
{
    public class ModelDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DataTemplateUGVControl { get; set; }

        public DataTemplate DataTemplateUGVPathControl { get; set; }

        public DataTemplate DataTemplateBuildingControl { get; set; }

        public DataTemplate DataTemplateStateControl { get; set; }

        public DataTemplate DataTemplateGroupControl { get; set; }


        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var dataItem = item;

            /* UGV인지 Building 인지 판별하여 DataTemplate를 반환한다. */
            switch (dataItem.ToString())
            {
                case "UGV" :
                    return DataTemplateUGVControl;

                case "UGVPath":
                    return DataTemplateUGVPathControl;

                case "Building" :
                    return DataTemplateBuildingControl;

                case "State" :
                    return DataTemplateStateControl;

                case "Group":
                    return DataTemplateGroupControl;

                default:
                    return new DataTemplate();
            }
        }
    }
}
