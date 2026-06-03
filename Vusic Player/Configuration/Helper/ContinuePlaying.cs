using System;
using Vusic_Player.Configuration.ClassModels;

namespace Vusic_Player.Configuration.Helper
{
    public class ContinuePlaying
    {
        public static VideoProgress? videoProgressMain;
        public static event Action? InvokeList;
        public static void InvokeCall()
        {
            InvokeList?.Invoke();
        }
    }
}
