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

        public DataTemplate DataTemplateBuildingControl { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var dataItem = item as ModelBase;

            /* UGV인지 Building 인지 판별하여 DataTemplate를 반환한다. */
            switch (dataItem.ToString())
            {
                case "UGV" :
                    return DataTemplateUGVControl;

                case "Building" :
                    return DataTemplateBuildingControl;

                default:
                    return new DataTemplate();
            }
        }
    }
}
