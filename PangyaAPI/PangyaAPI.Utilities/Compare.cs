using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.Utilities
{
    public class Compare
    {
        public static T IfCompare<T>(bool expression, T trueValue, T falseValue)
        {
            if (expression)
            {
                return trueValue;
            }
            else
            {
                return falseValue;
            }
        }
    }
}
