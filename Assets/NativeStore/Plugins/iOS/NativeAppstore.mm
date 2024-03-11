#import "NativeAppstore.h"

static NSString *const kAppStoreURLFormat = @"http://itunes.apple.com/app/id%ld%ld?mt=8";


@implementation NativeAppstore

+ (instancetype)sharedManager {
    static NativeAppstore *sharedInstance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedInstance = [[NativeAppstore alloc] init];
    });
    return sharedInstance;
}

- (void)openAppInStore:(long)appID campaignToken:(char *)campaignToken providerToken:(char *)providerToken {
    NSString *campaignTokenStr = [NSString stringWithUTF8String:campaignToken];
    NSString *providerTokenStr = [NSString stringWithUTF8String:providerToken];
    
    if ([SKStoreProductViewController class]) {
        NSDictionary *parameters = @{
            SKStoreProductParameterITunesItemIdentifier: @(appID),
            SKStoreProductParameterCampaignToken: campaignTokenStr,
            SKStoreProductParameterProviderToken: providerTokenStr
        };
        
        SKStoreProductViewController *productViewController = [[SKStoreProductViewController alloc] init];
        [productViewController loadProductWithParameters:parameters completionBlock:nil];
        [productViewController setDelegate:self];
        [UnityGetGLViewController() presentViewController:productViewController animated:YES completion:nil];
    } else {
        NSString *appStoreURL = [NSString stringWithFormat:kAppStoreURLFormat, appID];
        [[UIApplication sharedApplication] openURL:[NSURL URLWithString:appStoreURL]];
    }
}

- (void)productViewControllerDidFinish:(SKStoreProductViewController *)viewController {
    [viewController dismissViewControllerAnimated:YES completion:nil];
    UnitySendMessage("AppStoreHandler", "AppStoreClosed", "");
}

- (BOOL)prefersStatusBarHidden {
    return YES;
}

@end

extern "C" {
    void OpenAppStore(long appID, char *campaignToken, char *providerToken) {
        NSLog(@"NativeAppStore :: Open App %ld", appID);
        [[NativeAppstore sharedManager] openAppInStore:appID campaignToken:campaignToken providerToken:providerToken];
    }
}
