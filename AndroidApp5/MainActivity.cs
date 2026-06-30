using Android.OS;
using System.Timers;
using MCP2221A;
using Android.Graphics;

namespace AndroidApp5
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private const string TAG = "[MainActivity]";
        private const byte GP0 = 0;
        private const byte GP1 = 1;

        USB2GROVE? u2g;
        TextView mytextview0, mytextview1;
        System.Timers.Timer timer1 = new System.Timers.Timer();
        Handler myHandler = new Handler();

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            mytextview0 = FindViewById<TextView>(Resource.Id.textView0);
            mytextview0.Text = "SW0 OFF";
            mytextview1 = FindViewById<TextView>(Resource.Id.textView1);
            mytextview1.Text = "SW1 OFF";

            u2g = USB2GROVE.create(this);
            if (u2g is null)
            {
                var toast = Toast.MakeText(this, "No MCP2221A detected", ToastLength.Long);
                toast.Show();
            }
            u2g?.pinMode4([
                USB2GROVE.INPUT, 
                USB2GROVE.INPUT, 
                USB2GROVE.INPUT, 
                USB2GROVE.INPUT
             ]);
            timer1.Enabled = true;
            timer1.AutoReset = true;
            timer1.Interval = 50; // 50ms, for debouncing purpose
            timer1.Elapsed += new ElapsedEventHandler(OnTimerEvent);
            timer1.Start();
        }

        void OnTimerEvent(object source, ElapsedEventArgs e)
        {
            myHandler.Post(() =>
            {
                var sw0 = u2g?.digitalRead(GP0) ?? USB2GROVE.HIGH;
                var sw1 = u2g?.digitalRead(GP1) ?? USB2GROVE.HIGH;
                //byte sw0 = USB2GROVE.HIGH;
                //byte sw1 = USB2GROVE.HIGH;
                if (sw0 == USB2GROVE.LOW)
                {
                    mytextview0.Text = "SW0 ON";
                    mytextview0.SetBackgroundColor(Color.Black);
                    mytextview0.SetTextColor(Color.White);
                }
                else
                {
                    mytextview0.Text = "SW0 OFF";
                    mytextview0.SetBackgroundColor(Color.White);
                    mytextview0.SetTextColor(Color.Black);
                }

                if (sw1 == USB2GROVE.LOW)
                {
                    mytextview1.Text = "SW1 ON";
                    mytextview1.SetBackgroundColor(Color.Black);
                    mytextview1.SetTextColor(Color.White);
                }
                else
                {
                    mytextview1.Text = "SW1 OFF";
                    mytextview1.SetBackgroundColor(Color.White);
                    mytextview1.SetTextColor(Color.Black);
                }

            });
        }
        //        void Button0_Click(object sender, EventArgs e)
        //        {
        //            GP0 ^= 1;
        //            u2g.digitalWrite(0, GP0);
        //            mybutton0.SetTextColor(Color.Rgb(255 * GP0, 0, 0));
        //        }
        //        void Button1_Click(object sender, EventArgs e)
        //        {
        //            GP1 ^= 1;
        //            u2g.digitalWrite(1, GP1);
        //            mybutton1.SetTextColor(Color.Rgb(0, 255 * GP1, 0));
        //        }
    }
}