using Android.App;
using Android.Widget;
using Android.OS;
using Java.IO;
using Android.Content;
using Android.Support.V7.App;
using Android;
using Android.Content.PM;
using Android.Support.V4.Content;
using System;
using Android.Support.V4.App;
using Android.Views;
using Android.Provider;
using Android.Runtime;
using Android.Graphics;
using Android.Graphics.Drawables;
using Leadtools.ImageProcessing;
using System.Drawing;
using System.Collections.Generic;
//using System.IO;

namespace XamarinCropImage
{
    [Activity(Label = "Report Generator", MainLauncher = true, Icon = "@drawable/icon",Theme ="@style/AppTheme")]
    public class MainActivity : AppCompatActivity
    {
        ImageView imageView;
        Android.Support.V7.Widget.Toolbar toolbar;
        File file;
        Android.Net.Uri uri;
        Intent CamIntent, GalIntent, CropIntent;
        const int RequestPermissionCode = 1;
        int threshold;
        int x;
        string a = "result";
        string report;
        Android.Graphics.Bitmap myBitmap;
        public static readonly int PickImageId = 1000;

        private Android.Graphics.Bitmap takeScreenShot(Activity activity)
        {
            View view = activity.Window.DecorView;
            view.DrawingCacheEnabled = true;
            view.BuildDrawingCache();
            Android.Graphics.Bitmap bitmap = view.DrawingCache;
            Rect rect = new Rect();
            activity.Window.DecorView.GetWindowVisibleDisplayFrame(rect);
            int statusBarHeight = rect.Top;
            int width = activity.WindowManager.DefaultDisplay.Width;
            int height = activity.WindowManager.DefaultDisplay.Height;
            Android.Graphics.Bitmap screenShotBitmap = Android.Graphics.Bitmap.CreateBitmap(bitmap, 0, statusBarHeight, width,
                 height - statusBarHeight);
            view.DestroyDrawingCache();
            imageView.SetImageBitmap(screenShotBitmap);
            return screenShotBitmap;
        }
        public static void store(Android.Graphics.Bitmap bm, String fileName)
        {
            //bm = BitmapFactory.DecodeStream(getAssets().open("result.jpg"));
            String dirPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/Report Generator";
            File dir = new File(dirPath);
            if (!dir.Exists())
                dir.Mkdirs();
            File file = new File(dirPath, "result.png");
            try
            {
                //string path = Path.Combine(dirPath, "newProdict.png");
                //FileOutputStream fs = new FileOutputStream(file);
                var fs = new System.IO.FileStream(dirPath, System.IO.FileMode.Open);
                bm.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 85, fs);
                fs.Flush();
                fs.Close();
            }
            catch (IOException e)
            {
                e.PrintStackTrace();
            }
        }

        /*
        public void SaveImage(Android.Graphics.Bitmap image, string user)
        {

            var document = cbDB.CreateDocument();
            var properties = new Dictionary<string, object>()
    {
        {"type","image"},
        {"user",user},
        {"created",DateTime.Now.ToString()}
    };

            var rev = document.PutProperties(properties);
            var newRev = document.CurrentRevision.CreateRevision();

            using (System.IO.MemoryStream stream = new MemoryStream())
            {
                image.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg, 50, stream);
                byte[] imageData = stream.ToArray();
                newRev.SetAttachment("image", "image/jpg", stream);
                newRev.Save();
            }

            var retrieveDocument = cbDB.GetDocument(document.Id);
            foreach (var props in retrieveDocument.Properties)
            {
                Android.Util.Log.Info("--DOCUMENT WITH IMAGE EVENT--", "Document Properties: " + props.Key + " " + props.Value);
            }
        }*/
        private void processCamera_Click(object sender, EventArgs e)
        {

            myBitmap = toGrayscale(myBitmap);
            imageView.SetImageBitmap(myBitmap);
            processImage(myBitmap);
        }
        private void ButtonOnClick(object sender, EventArgs eventArgs)
        {
            Intent intent2 = new Intent();
            intent2.SetType("image/*");
            intent2.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(intent2, "Select Picture"), PickImageId);
        }

        private void save_Click(object sender, EventArgs eventArgs)
        {
            myBitmap = takeScreenShot(this);
            imageView.SetImageBitmap(myBitmap);
            store(myBitmap, a);
        }
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView (Resource.Layout.Main);
            imageView = FindViewById<ImageView>(Resource.Id.imageView);
            Button button2 = FindViewById<Button>(Resource.Id.button2);
            button2.Click += processCamera_Click;
            Button button4 = FindViewById<Button>(Resource.Id.button4);
            button4.Click += generateReport;
            Button button3 = FindViewById<Button>(Resource.Id.button3);
            //button3.Click += save_Click;
            toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "Camera / Galery ->";
            toolbar.SetTitleTextColor(Android.Graphics.Color.White);
            SetSupportActionBar(toolbar);
            button3.Click += delegate {
                var email = new Intent(Android.Content.Intent.ActionSend);
                email.PutExtra(Android.Content.Intent.ExtraEmail, new string[] {
                    "manas.agrawal990@gmail.com"
                });
                /*email.PutExtra(Android.Content.Intent.ExtraCc, new string[] {
                    "susairajs18@live.com"
                });*/
                email.PutExtra(Android.Content.Intent.ExtraSubject, "Lateral Flow Assay Report");
                email.PutExtra(Android.Content.Intent.ExtraText, report);
                email.SetType("message/rfc822");
                StartActivity(email);
            };
            imageView = FindViewById<ImageView>(Resource.Id.imageView);
            int permissionCheck = (int)ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera);
            if (permissionCheck == (int)Permission.Denied)
                RequestRuntimePermission();
                
        }
          
        private void RequestRuntimePermission()
        {
            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.Camera))
                Toast.MakeText(this, "CAMERA permission will allows us to access CAMERA app", ToastLength.Short).Show();
            else
                ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.Camera }, RequestPermissionCode);

        }
        public string getReport(int flag)
        {
            if (flag == 2)
            {
                report = "Control Line - Valid\nTest Line - Valid";
            }
            else if (flag == 1)
            {
                report = "Control Line - Valid\nTest Line - Invalid";
            }
            else
            {
                report = "Invalid Sample";
            }
            return report;
        }
        private void generateReport(object sender, EventArgs e)
        {
            getReport(x);
            TextView t1 = FindViewById<TextView>(Resource.Id.t1);
            t1.Text = report;
        }
        public void processImage(Android.Graphics.Bitmap myBitmap)
        {
            Android.Graphics.Bitmap b = myBitmap;
            int[] flag = new int[8];
             int w = b.Width;
            int h = b.Height;
            threshold = 150;
            imageView.SetImageBitmap(b);
            int[,] Pixeldata = new int[8, w];
            int count = 0;
            for (int i = 60; i <= 67; i++)
            {
                for (int j = 100; j < 300; j++ )
                {
                    Android.Graphics.Color pixelColor = new Android.Graphics.Color(b.GetPixel(j, i));
                    Pixeldata[count, j] = (int)pixelColor.R;
                    
                        if (pixelColor.R < threshold)
                        {
                            flag[count] = 1;
                            break;
                        }
                }
                for (int k = 340; k < 540; k++)
                {
                    Android.Graphics.Color pixelColor = new Android.Graphics.Color(b.GetPixel(k, i));
                    Pixeldata[count, k] = (int)pixelColor.R;
                    
                        if (flag[count] == 1 && pixelColor.R < threshold)
                        {
                            flag[count] = 2;
                            break;
                        }
                }
                count++;
            }

            int count2 = 0;
            int count1 = 0;
            int count0 = 0;
            for (int i=0;i<8;i++)
            {
                if(flag[i]==2)
                {
                    count2++;
                }
                else if(flag[i]==1)
                {
                    count1++;
                }
                else
                {
                    count0++;
                }
            }

            if (count2>=4)
            {
                x = 2;
            }
            else if (count1>=4)
            {
                x = 1;
            }
            else
            {
                x = 0;
            }
        }
        public Android.Graphics.Bitmap toGrayscale(Android.Graphics.Bitmap bmpOriginal)
        {
            int width, height;
            height = bmpOriginal.Height;
            width = bmpOriginal.Width;
            Android.Graphics.Bitmap bmpGrayscale = Android.Graphics.Bitmap.CreateBitmap(width, height, Android.Graphics.Bitmap.Config.Argb8888);
            Android.Graphics.Canvas c = new Android.Graphics.Canvas(bmpGrayscale);
            Android.Graphics.Paint paint = new Android.Graphics.Paint();
            Android.Graphics.ColorMatrix cm = new Android.Graphics.ColorMatrix();
            cm.SetSaturation(0);
            Android.Graphics.ColorMatrixColorFilter f = new Android.Graphics.ColorMatrixColorFilter(cm);
            paint.SetColorFilter(f);
            c.DrawBitmap(bmpOriginal, 0, 0, paint);
            return bmpGrayscale;
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.btn_camera)
                CameraOpen();
            else if (item.ItemId == Resource.Id.btn_gallery)
                GalleryOpen();
            return true;
        }
        private void GalleryOpen()
        {
            GalIntent = new Intent(Intent.ActionPick, MediaStore.Images.Media.ExternalContentUri);
            StartActivityForResult(Intent.CreateChooser(GalIntent, "Select image from Gallery"), 2);
        }
        private void CameraOpen()
        {
            CamIntent = new Intent(MediaStore.ActionImageCapture);
            File file = new File(Android.OS.Environment.ExternalStorageDirectory, "file_" + Guid.NewGuid().ToString() + ".jpg");
            uri = Android.Net.Uri.FromFile(file);
            CamIntent.PutExtra(MediaStore.ExtraOutput, uri);
            CamIntent.PutExtra("return-data", true);
            StartActivityForResult(CamIntent, 0);
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == 0 && resultCode == Result.Ok)
                CropImage();
            else if (requestCode == 2)
            {
                if (data != null)
                {
                    uri = data.Data;
                    CropImage();
                    imageView.SetImageURI(uri);
                    myBitmap = ((BitmapDrawable)imageView.Drawable).Bitmap;
                    imageView.SetImageBitmap(myBitmap);
                }
            }
            else if(requestCode == 1)
            {
                if(data != null)
                {
                    uri = data.Data;
                    //CropImage();
                    imageView.SetImageURI(uri);
                    myBitmap = ((BitmapDrawable)imageView.Drawable).Bitmap;
                }
            }
        }
        private void CropImage()
        {
           
                CropIntent = new Intent("com.android.camera.action.CROP");
                CropIntent.SetDataAndType(uri, "image/*");
            CropIntent.PutExtra("crop", "true");
                CropIntent.PutExtra("outputX", 745);
                CropIntent.PutExtra("outputY", 127);
                CropIntent.PutExtra("crop", "true");
                CropIntent.PutExtra("return-data", true);
                StartActivityForResult(CropIntent, 1);
            
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
           switch(requestCode)
            {
                case RequestPermissionCode:
                    {
                        if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                            Toast.MakeText(this, "Permission Granted", ToastLength.Short).Show();
                        else
                            Toast.MakeText(this, "Permission Canceled", ToastLength.Short).Show();
                    }
                    break;
            }
        }
    }
}

