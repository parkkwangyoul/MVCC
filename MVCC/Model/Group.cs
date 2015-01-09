using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCC.Model
{
    public class Group
    {
        public Group() { }

        public Group(string name)
        {
            this.name = name;
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string stateBorderBrush = "Green";
        public string StateBorderBrush
        {
            get { return stateBorderBrush; }
            set { stateBorderBrush = value; }
        }

        private List<UGV> memberList;

        public List<UGV> MemberList{
            get 
            {
                if (memberList == null)
                {
                    memberList = new List<UGV>();
                }
                return memberList; 
            }
        }

        public override string ToString()
        {
            return "Group";
        }
    }
}
