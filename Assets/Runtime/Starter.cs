using UnityEngine;

using NativeStore;

namespace DefaultNamespace
{
    // Just example Starter class, in your project you can use for example any DI.
    public class Starter : MonoBehaviour
    {
        private AppStoreHandler _appStoreHandler;
        
        private void Start()
        {
            _appStoreHandler = new AppStoreHandler();
        }
    }
}