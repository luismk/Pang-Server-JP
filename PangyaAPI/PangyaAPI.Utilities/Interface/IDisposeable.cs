using System;

namespace PangyaAPI.Utilities.Interface
{
    public interface IDisposeable : IDisposable
    {
        bool Disposed { get; set; }
    }
}
