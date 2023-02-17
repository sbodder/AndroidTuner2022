using System;

using Android.App;
using global::Android.Content;

namespace TTtuner_2022_2.Common
{
    class SharedPreferences
    {
        private ISharedPreferences _prefMan;
        internal SharedPreferences()
        {
            _prefMan = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
        }

        internal string StoreDataFolderUri( string uri)
        {

            ISharedPreferencesEditor editor = _prefMan.Edit();
            editor.PutString("data_folder_uri", uri.ToString());
            // editor.Commit();    // applies changes synchronously on older APIs
            editor.Apply();        // applies changes asynchronously on newer APIs

            return uri.ToString();

        }

        internal string GetDataFolderUri()
        {
            
            return _prefMan.GetString("data_folder_uri", String.Empty);  

        }

    }

}