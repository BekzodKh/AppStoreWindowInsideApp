#import <Foundation/Foundation.h>
#import <StoreKit/StoreKit.h>

@interface OverlayManager : NSObject <SKOverlayDelegate>

+ (instancetype)sharedManager;
- (void)showAppStoreOverlayWithAppStoreID:(NSString *)appStoreID campaignToken:(NSString *)campaignToken providerToken:(NSString *)providerToken;
- (void)dismissOverlay;

@end

@implementation OverlayManager

+ (instancetype)sharedManager {
    static OverlayManager *sharedInstance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedInstance = [[OverlayManager alloc] init];
    });
    return sharedInstance;
}

- (void)showAppStoreOverlayWithAppStoreID:(NSString *)appStoreID campaignToken:(NSString *)campaignToken providerToken:(NSString *)providerToken  {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (@available(iOS 14.0, *)) {
            SKOverlayAppConfiguration *config = [[SKOverlayAppConfiguration alloc] initWithAppIdentifier:appStoreID position:SKOverlayPositionBottom];
            config.campaignToken = campaignToken;
            config.providerToken = providerToken;
            
            SKOverlay *overlay = [[SKOverlay alloc] initWithConfiguration:config];
            overlay.delegate = self;
            [overlay presentInScene:[UIApplication sharedApplication].windows.firstObject.windowScene];
        }
    });
}

- (void)dismissOverlay {
    if (@available(iOS 14.0, *)) {
        [SKOverlay dismissOverlayInScene:[UIApplication sharedApplication].windows.firstObject.windowScene];
    }
}

- (void)storeOverlay:(SKOverlay *)overlay
willStartPresentation:(SKOverlayTransitionContext *)transitionContext
API_AVAILABLE(ios(14.0)){
    NSLog(@"OverlayManager :: willStartPresentation");
    UnitySendMessage("AppStoreHandler", "SkShowStart", "");
}

- (void)storeOverlay:(SKOverlay *)overlay
  didFinishDismissal:(SKOverlayTransitionContext *)transitionContext
API_AVAILABLE(ios(14.0)){
    NSLog(@"OverlayManager :: didFinishDismissal");
    UnitySendMessage("AppStoreHandler", "SkDismissFinish", "");
}

@end

extern "C" {
    void ShowAppStoreOverlay(const char *appStoreID, char *campaignToken, char *providerToken) {
        NSString *appStoreIDString = [NSString stringWithUTF8String:appStoreID];
        NSString *campaignTokenStr = [NSString stringWithUTF8String:campaignToken];
        NSString *providerTokenStr = [NSString stringWithUTF8String:providerToken];
        
        [[OverlayManager sharedManager] showAppStoreOverlayWithAppStoreID:appStoreIDString campaignToken:campaignTokenStr providerToken:providerTokenStr];
    }
    
    void DismissAppStoreOverlay() {
        NSLog(@"OverlayManager :: Dismiss App Store Overlay");
        [[OverlayManager sharedManager] dismissOverlay];
    }
}
