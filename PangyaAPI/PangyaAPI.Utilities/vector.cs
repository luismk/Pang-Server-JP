using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.Utilities
{
    public class vector<T> : List<T>
    {
        public vector()
        {
        }

        public vector(T t) { Add(t); } 
    }
}
