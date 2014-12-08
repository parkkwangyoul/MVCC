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

        public Building(double width, double height, double x, double y) : base(width, height, x, y) { }

        public override string ToString() { return "Building"; }
    }
}
