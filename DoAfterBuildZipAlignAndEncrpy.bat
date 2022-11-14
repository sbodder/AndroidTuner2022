rem drop this bat file into the archive folder and it will do the needfull before you upload to google play store
if exist com.full.Tunetracker.apk (
    zipalign -f -v  4  "com.full.Tunetracker.apk" "com.full.Tunetracker_ZA.apk"
	apksigner sign --ks "C:\Users\Shane\AppData\Local\Xamarin\Mono for Android\Keystore\ShanesKey\ShanesKey.keystore" --ks-key-alias shaneskey "com.full.Tunetracker_ZA.apk"
	
) else (
    zipalign -f -v  4  "com.music.Tunetracker.apk" "com.music.Tunetracker_ZA.apk"
	apksigner sign --ks "C:\Users\Shane\AppData\Local\Xamarin\Mono for Android\Keystore\ShanesKey\ShanesKey.keystore" --ks-key-alias shaneskey "com.music.Tunetracker_ZA.apk"
)

