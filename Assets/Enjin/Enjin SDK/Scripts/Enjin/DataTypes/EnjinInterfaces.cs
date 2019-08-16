using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnjinSDK
{
    public interface IErrorHandle
    {
        void SetErrorDetails(int code, string description);
    }

    public interface IEnjinIdentity
    {

    }
}