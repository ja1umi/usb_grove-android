using Android.OS;
using Android.Util;
using System.Timers;
using MCP2221A;
using Android.App;
using Android.Widget;
//using CommunityToolkit.Maui.Alerts;
//using CommunityToolkit.Maui.Core;

namespace AndroidApp3
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private const string TAG = "[MainActivity]";
        USB2GROVE? u2g;
        TextView textView1, textView2, textView3;
        System.Timers.Timer timer1 = new System.Timers.Timer();
        Handler myHandler = new Handler();
        byte[] data = new byte[2]; // 2-byte data
        const byte adr = 0x48; // LM75B
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            //
            //CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            textView1 = FindViewById<TextView>(Resource.Id.textView1);
            textView2 = FindViewById<TextView>(Resource.Id.textView2);
            textView3 = FindViewById<TextView>(Resource.Id.textView3);
            //u2g = new USB2GROVE(this);
            u2g = USB2GROVE.create(this);
            if (u2g is null)
            {
                var toast = Toast.MakeText(this, "No MCP2221A detected", ToastLength.Long);
                toast.Show();
            }
            //u2g.i2cWriteReg(adr, 0x2C, 0x0B);
            //u2g.i2cWriteReg(adr, 0x31, 0x09);
            //u2g.i2cWriteReg(adr, 0x2D, 0x08);

            timer1.Enabled = true;
            timer1.AutoReset = true;
            timer1.Interval = 1000; // 1000ms
            timer1.Elapsed += new ElapsedEventHandler(OnTimerEvent);
            timer1.Start();
        }
        void OnTimerEvent(object source, ElapsedEventArgs e)
        {
            //data = u2g.i2cReadBlock(adr, 0x32, 6);
            data = u2g?.i2cReadBlock(adr, 0, 2);
            short tmp = (short) ((data[0] << 8 | data[1]) >> 5);
            //Log.Info(TAG, "tmp=" + tmp);
            //Log.Info(TAG, "data[0]=" + data[0]);
            //Log.Info(TAG, "data[1]=" + data[1]);
            if (tmp > 1027)
            {
                tmp -= 2048;
            }
            float c = tmp * 0.125f;
            float f = c * 1.8f + 32;
            float k = c + 273.15f;
            //float y = (short)(data[3] << 8 | data[2]) * 0.004f;
            //float z = (short)(data[5] << 8 | data[4]) * 0.004f;

            myHandler.Post(() =>
            {
                textView1.Text = c.ToString("F3") + " degree C";
                textView2.Text = f.ToString("F3") + " degree F";
                textView3.Text = k.ToString("F3") + " Kelvin";
            });
        }
    }
}