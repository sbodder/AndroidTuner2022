using Android.Content;
using Android.Views;
using System;
using TTtuner_2022_2;

internal class Android12PlusPermissionClickHandler : Java.Lang.Object, View.IOnClickListener
{
    private SplashScreenActivity splashActivity;
    public Android12PlusPermissionClickHandler()
    {
    }
    public Android12PlusPermissionClickHandler(SplashScreenActivity splashActivity)
    {
        this.splashActivity = splashActivity;
    }
    public void OnClick(View v)
    {
        // throw new System.NotImplementedException();ACTION_MANAGE_APP_ALL_FILES_ACCESS_PERMISSION
        try
        {
            splashActivity.StartActivity(new Intent(
global::Android.Provider.Settings.ActionManageAllFilesAccessPermission,
global::Android.Net.Uri.Parse("package:" + global::Android.App.Application.Context.PackageName)));

        }
        catch (Exception ex)
        {
            Intent intent = new Intent();
            intent.SetAction(global::Android.Provider.Settings.ActionManageAllFilesAccessPermission);
            splashActivity.StartActivity(intent);
        }
    }
}