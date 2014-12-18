using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCC.Model
{
    public class State : ModelBase
    {
        public State(UGV ugv)
        {
            this.ugv = ugv;
        }

        private UGV _ugv;
        public UGV ugv
        {
            get { return _ugv; }
            set
            {
                _ugv = value;
            }

        }

        public override string ToString()
        {
            return "State";
        }
    }
}
