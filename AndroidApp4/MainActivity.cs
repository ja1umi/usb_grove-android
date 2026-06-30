using MCP2221A;

namespace AndroidApp4
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private const string TAG = "[MainActivity]";
        private const byte GP3 = 3;
        TextView textView1;
        SeekBar seekBar1;
        USB2GROVE? u2g;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            textView1 = FindViewById<TextView>(Resource.Id.textView1);
            seekBar1 = FindViewById<SeekBar>(Resource.Id.seekBar1);
            seekBar1.Max = 31;
            seekBar1.Min = 0;
            seekBar1.Progress = 15;
            u2g = USB2GROVE.create(this);
            if (u2g is null)
            {
                var toast = Toast.MakeText(this, "No MCP2221A detected", ToastLength.Long);
                toast.Show();
            }
            u2g?.pinMode4([USB2GROVE.INPUT, USB2GROVE.INPUT, USB2GROVE.INPUT, USB2GROVE.DAC]);
            u2g?.analogWrite(GP3, (byte)seekBar1.Progress);
            seekBar1.ProgressChanged += (sender, e) =>
            {
                textView1.Text = e.Progress.ToString("##");
                u2g?.analogWrite(GP3, (byte)e.Progress);
            };
        }
    }
}