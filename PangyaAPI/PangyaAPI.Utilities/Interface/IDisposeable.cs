using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.Utilities.Interface
{
    public interface IDisposeable : IDisposable
    {
        bool Disposed { get; set; }
    }
}
