using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.IO;
using System.Security.Cryptography;

namespace EncryptDecryptWidget
{
    [Service]
    internal class EncryptDecryptWidgetService : Service, View.IOnTouchListener
    {

        private bool expanded;
        private IWindowManager _windowManager;
        private WindowManagerLayoutParams _layoutParams;
        private View floatingView, expandedView;
        private Button decryptMessage, encryptMessage;
        private ImageView close, icon;
        private ClipboardManager _clipboard;
        private int _initialX, _initialY;
        private float _initialTouchX, _initialTouchY;
        private readonly string encryptionKey = "YourPasswordKey";


        public override void OnCreate()
        {
            base.OnCreate();

            floatingView = LayoutInflater.From(this).Inflate(Resource.Layout.expandedLayout, null);
            expandedView = floatingView.FindViewById(Resource.Id.flyout);
            
            icon = floatingView.FindViewById<ImageView>(Resource.Id.icon);
            close = floatingView.FindViewById<ImageView>(Resource.Id.close);
            encryptMessage = floatingView.FindViewById<Button>(Resource.Id.encryptMessage);
            decryptMessage = floatingView.FindViewById<Button>(Resource.Id.decryptMessage);

            _clipboard = (ClipboardManager)GetSystemService(ClipboardService);


            SetTouchListener();


           

            _layoutParams = new WindowManagerLayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent,
                WindowManagerTypes.Phone,
                WindowManagerFlags.NotFocusable,
                Format.Translucent)
            {
                Gravity = GravityFlags.Center | GravityFlags.Left
            };


             
            _windowManager = GetSystemService(WindowService).JavaCast<IWindowManager>();
            _windowManager.AddView(floatingView, _layoutParams);

          

            encryptMessage.Click += delegate {

                expandedView.Visibility = ViewStates.Gone;

                var msg = ClipData.NewPlainText("text", PasswordEncrypt(_clipboard.PrimaryClip.GetItemAt(0).Text, encryptionKey)); //get and encrypt string from clipboard
                _clipboard.PrimaryClip = msg;

                Toast.MakeText(this, "Encrypted : )", ToastLength.Short).Show();
                
            };

            decryptMessage.Click += delegate {

                try
                {
                    expandedView.Visibility = ViewStates.Gone;

                    Toast.MakeText(this, PasswordDecrypt(_clipboard.PrimaryClip.GetItemAt(0).Text, encryptionKey), ToastLength.Long).Show();
                   
                }

                catch
                {
                    Toast.MakeText(this, "Error : (", ToastLength.Long).Show();
                };  //wrong encryption format

            };

            close.Click += delegate {

                StopService(new Intent(this, typeof(EncryptDecryptWidgetService)));
            };
        }


     


        private void SetTouchListener()
        {
            var mainContainer = floatingView.FindViewById<RelativeLayout>(Resource.Id.root);
            mainContainer.SetOnTouchListener(this);
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (floatingView != null)
            {
                _windowManager.RemoveView(floatingView);
            }
            //   StopService(new Intent(this, typeof(EncryptDecryptWidgetService)));  you can stop service after destroy
        }


        public static string PasswordEncrypt(string inText, string key)
        {
            var bytesBuff = System.Text.Encoding.Unicode.GetBytes(inText);
            using (var aes = Aes.Create())
            {
                var crypto = new Rfc2898DeriveBytes(key,
                new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                aes.Key = crypto.GetBytes(32);
                aes.IV = crypto.GetBytes(16);
                using (var mStream = new MemoryStream())
                {
                    using (var cStream = new CryptoStream(mStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cStream.Write(bytesBuff, 0, bytesBuff.Length);
                        cStream.Close();
                    }

                    inText = Convert.ToBase64String(mStream.ToArray());
                }
            }

            return inText;
        }



        public static string PasswordDecrypt(string cryptTxt, string key)
        {
            cryptTxt = cryptTxt.Replace(" ", "+");
            var bytesBuff = Convert.FromBase64String(cryptTxt);
            using (var aes = Aes.Create())
            {
                var crypto = new Rfc2898DeriveBytes(key,
                new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                aes.Key = crypto.GetBytes(32);
                aes.IV = crypto.GetBytes(16);
                using (var mStream = new MemoryStream())
                {
                    using (var cStream = new CryptoStream(mStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cStream.Write(bytesBuff, 0, bytesBuff.Length);
                        cStream.Close();
                    }

                    cryptTxt = System.Text.Encoding.Unicode.GetString(mStream.ToArray());
                }
            }

            return cryptTxt;
        }



        public bool OnTouch(View view, MotionEvent motion)
        {
            switch (motion.Action)
            {
                case MotionEventActions.Down:


                    //initial position
                    _initialX = _layoutParams.X;
                    _initialY = _layoutParams.Y;

                    //touch point
                    _initialTouchX = motion.RawX;
                    _initialTouchY = motion.RawY;
                 
                    return true;


                case MotionEventActions.Up:
                    {
                        if (expanded == false)
                        {
                            expandedView.Visibility = ViewStates.Gone;
                            expanded = true;
                        }
                        else
                        {
                            expandedView.Visibility = ViewStates.Visible;
                            expanded = false;
                        }
                    }

                    return true;

                case MotionEventActions.Move:

                    //calculate the X and Y coordinates of the view.
                    _layoutParams.X = _initialX + (int)(motion.RawX - _initialTouchX);
                    _layoutParams.Y = _initialY + (int)(motion.RawY - _initialTouchY);


                    expandedView.Visibility = ViewStates.Gone;
                  
                    _windowManager.UpdateViewLayout(floatingView, _layoutParams);
                    return true;
            }

            return false;
        }
    }
}