using System;

using UnityEngine;

using JetBrains.Annotations;

namespace NativeStore
{
    public class ReceiverAnswerFromAppStore : MonoBehaviour
    {
        public event Action OnReceiveCallback = delegate {};
        public event Action OnSkShow = delegate {};
        public event Action OnSkHide = delegate {};
        
        // Calls from NativeAppstore.mm
        [UsedImplicitly] 
        public void AppStoreClosed(string message)
        {
            OnReceiveCallback?.Invoke();
        }
        
        [UsedImplicitly] 
        public void SkShowStart(string message)
        {
            OnSkShow?.Invoke();
        }
        
        [UsedImplicitly] 
        public void SkDismissFinish(string message)
        {
            OnSkHide?.Invoke();
        }
    }
}