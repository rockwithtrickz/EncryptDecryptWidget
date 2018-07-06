using Android.App;
using Android.OS;
using Android.Content;
using Android.Support.V7.App;

namespace EncryptDecryptWidget
{
    [Activity(MainLauncher = true, Theme = "@android:style/Theme.DeviceDefault.Light.NoActionBar")]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
           
           // SetContentView(Resource.Layout.Main);

           
        

            StartService(new Intent(this, typeof(EncryptDecryptWidgetService)));

            Finish();


        }
    }
}

