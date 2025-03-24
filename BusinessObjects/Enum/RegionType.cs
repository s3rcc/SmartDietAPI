using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.FixedData
{
    [Flags]
    public enum RegionType
    {
        None = 0,
        NViet = 1,
        TViet = 2,
        BViet = 4


        #region Old
        //American,
        //Italian,
        //Chinese,
        //Japanese,
        //Mexican,
        //Indian,
        //Thai,
        //Mediterranean,
        //French,
        //Korean,
        //Vietnamese,
        //MiddleEastern,
        //Greek,
        //Spanish,
        //Brazilian,
        #endregion


    }
}
