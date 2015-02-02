using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCC.Model
{
    public class Building : MVCCItem
    {
        public Building() : base() { }

        public Building(string id, double width, double height, double x, double y, string buildingColor, bool disapperCheck) : base(id, width, height, x, y) { this.BuildingColor = buildingColor; this.DisapperCheck = disapperCheck; }        
        
        // 각 Building별 색깔이 다름
        private string buildingColor = "#78C8FF";
        public string BuildingColor
        {
            get { return buildingColor; }
            set
            {
                buildingColor = value;
            }
        }

        private bool disapperCheck;
        public bool DisapperCheck
        {
            get { return disapperCheck; }
            set
            {
                disapperCheck = value;
            }
        }


        public override string ToString() { return "Building"; }
    }
}
