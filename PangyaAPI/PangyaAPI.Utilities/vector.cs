using System.Collections.Generic;

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
