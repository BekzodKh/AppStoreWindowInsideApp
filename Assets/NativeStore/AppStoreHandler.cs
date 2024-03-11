using System;
using Cysharp.Threading.Tasks;

using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
using UnityEngine.iOS;
#endif

namespace NativeStore
{
    public class AppStoreHandler : IDisposable
    {
        public event Action OnOpenAppStore = delegate {  };
        public event Action OnCloseAppStore = delegate {  };

        public bool OverlayIsActive => _skCompletionSource != null;
        
        // GameObject which will be receive call from mm scripts, should named like this const string value
        private const string ReceiverName = "AppStoreHandler";
        
        private static ReceiverAnswerFromAppStore _receiverAnswerFromAppStore;
        
        private readonly TimeSpan _delay = TimeSpan.FromSeconds(0.1f);
        
        private bool _rotateScreen;
        private UniTaskCompletionSource _appStoreCompletionSource;
        private UniTaskCompletionSource _skCompletionSource;

        public AppStoreHandler()
        {
            if (_receiverAnswerFromAppStore == null)
            {
                // if you forgot to create necessary object in scene
                CreateReceiverObject();
            }
            
            _receiverAnswerFromAppStore.OnReceiveCallback += AppStoreClosed;
            _receiverAnswerFromAppStore.OnSkShow += SkShowStart;
            _receiverAnswerFromAppStore.OnSkHide += SkDismissFinish;
        }

        public bool CanShowStoreOverlay()
        {
#if UNITY_IOS && !UNITY_EDITOR
            var version = Version.Parse(Device.systemVersion);

            return version.Major >= 14;
#else
            return false;
#endif
        }
        
        public async UniTask ShowStoreOverlay(string appId, string campaignToken = "", string providerToken = "")
        {
#if UNITY_IOS && !UNITY_EDITOR
            ShowAppStoreOverlay(appId, campaignToken, providerToken);
            
            _skCompletionSource = new UniTaskCompletionSource();

            await _skCompletionSource.Task;

            _skCompletionSource = null;
#endif
        }
        
        public async UniTask DismissStoreOverlay()
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (OverlayIsActive)
            {
                _skCompletionSource.TrySetResult();
                
                DismissAppStoreOverlay();
            }
#endif
        }

        public async UniTask OpenAppInStore(long appId, string campaignToken, string providerToken)
        {          
            Debug.Log($"[{nameof(AppStoreHandler)}] App id: {appId.ToString()}\nCampaign Token: {campaignToken}\nProvider Token: {providerToken}");
            
#if UNITY_IOS && !UNITY_EDITOR
            _appStoreCompletionSource = new UniTaskCompletionSource();

            OnOpenAppStore?.Invoke();

            OpenAppStore(appId, campaignToken, providerToken);

            await _appStoreCompletionSource.Task;
#endif
        }
        
        private void CreateReceiverObject()
        {
            var receiverObject = new GameObject(ReceiverName);
                
            _receiverAnswerFromAppStore = receiverObject.AddComponent<ReceiverAnswerFromAppStore>();
                
            Object.DontDestroyOnLoad(receiverObject);
        }
        
        void IDisposable.Dispose()
        {
            _receiverAnswerFromAppStore.OnReceiveCallback -= AppStoreClosed;
            _receiverAnswerFromAppStore.OnSkShow -= SkShowStart;
            _receiverAnswerFromAppStore.OnSkHide -= SkDismissFinish;
        }
        
        private void AppStoreClosed()
        {
            AppStoreClosedAsync().Forget();
        }

        private async UniTaskVoid AppStoreClosedAsync()
        {
            if (_rotateScreen)
            {
                Screen.orientation = ScreenOrientation.AutoRotation;
                
                await UniTask.Delay(_delay);
            }
            
            OnCloseAppStore?.Invoke();

            _appStoreCompletionSource?.TrySetResult();
            
            Debug.Log($"[{nameof(AppStoreHandler)}] Appstore closed.");
        }
        
        private void SkShowStart()
        {
            Debug.Log($"[{nameof(AppStoreHandler)}] Appstore SKShowStart");
        }

        private void SkDismissFinish()
        {
            Debug.Log($"[{nameof(AppStoreHandler)}] Appstore SKDismissFinish");
#if UNITY_IOS && !UNITY_EDITOR
            _skCompletionSource?.TrySetResult();
#endif
        }
                
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void OpenAppStore(long appID, string campaignToken, string providerToken);
        
        [DllImport("__Internal")]
        private static extern void ShowAppStoreOverlay(string appID, string campaignToken, string providerToken);
        
        [DllImport("__Internal")]
        private static extern void DismissAppStoreOverlay();
#endif
    }
}
