using UnityEngine;
using System.Collections;

namespace System.Runtime.InteropServices
{
    public interface INameable
    {
        void SetName(string name);
        string GetName();
    }
}
