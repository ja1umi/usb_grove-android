using Android.OS;
using Android.Util;
using System.Timers;
//using System;
using MCP2221A;
using ArduinoTone;
using ArduinoUtil;
using Android.App;
using Android.Widget;

namespace AndroidApp2
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private const string TAG = "[MainActivity]";
        private const double MINVAL = 0;
        private const double MAXVAL = 1023;
        private const double MINHZ = 300;
        private const double MAXHZ = 2000;
        private const double THDVAL = 5;
        private const byte GP2 = 2;

        USB2GROVE? u2g;
        TextView textView1;
        ArduinoToneGenerator audioTrack1;
        double currentVal;
        System.Timers.Timer timer1 = new System.Timers.Timer();
        Handler myHandler = new Handler();
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            //
            //CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            textView1 = FindViewById<TextView>(Resource.Id.textView1);
            audioTrack1 = new ArduinoToneGenerator();
            currentVal = 0;
            //u2g = new USB2GROVE(this);
            u2g = USB2GROVE.create(this);
            if (u2g is null)
            {
                var toast = Toast.MakeText(this, "No MCP2221A detected", ToastLength.Long);
                toast.Show();
            }
            u2g?.pinMode4([USB2GROVE.INPUT, USB2GROVE.INPUT, USB2GROVE.ADC, USB2GROVE.INPUT]);

            timer1.Enabled = true;
            timer1.AutoReset = true;
            timer1.Interval = 250; // A conversion occurs every 250 milliseconds
            timer1.Elapsed += new ElapsedEventHandler(OnTimerEvent);
            timer1.Start();
        }
        void OnTimerEvent(object source, ElapsedEventArgs e)
        {
            myHandler.Post(() =>
            {
                double val = u2g?.analogRead(GP2) ?? MINVAL;
                Log.Info(TAG, "adc=" + val);
                textView1.Text = "ADC = " + val.ToString("####");
                if (Math.Abs(val - currentVal) >= THDVAL)
                {
                    double freqHz = ArduinoCompat.map(val, MINVAL, MAXVAL, MINHZ, MAXHZ);
                    Log.Info(TAG, "freqency=" + freqHz);
                    currentVal = freqHz;
                    audioTrack1.tone(freqHz);
                }
            });
        }
//
//        private double arduinoMap(double x, double in_min, double in_max, double out_min, double out_max)
//        {
//            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
//        }
//
//        private T arduinoConstrain<T>(T val, T min, T max) where T : IComparable<T>
//        {
//            if (val.CompareTo(min) < 0)
//            {
//                return min;
//            }
//            else if (val.CompareTo(max) > 0)
//            {
//                return max;
//            }
//            return val;
//        }
     }
}