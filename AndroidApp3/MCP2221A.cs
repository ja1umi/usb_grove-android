//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
using Android.Content;
//using Android.Graphics;
using Android.Hardware.Usb;
using Android.Util;
//using static Android.Renderscripts.Sampler;

namespace MCP2221A
{
    public class USB2GROVE : Activity
    {
        public static readonly byte HIGH = 1;
        public static readonly byte LOW = 0;
        public static readonly byte INPUT = 0;
        public static readonly byte OUTPUT = 1;
        public static readonly byte ADC = 2;
        public static readonly byte DAC = 3;

        private UsbDeviceConnection myusbconnection;
        private UsbInterface myusbinterface;
        private UsbEndpoint myendpointout, myendpointin;
        //private UsbDevice myusbdevice;
        private const int myVendorId = 0x04D8, myProductId = 0x00DD;
        private const string TAG = "[MCP2221A]";
        private USB2GROVE(Context mycontext, UsbDevice myusbdevice)
        {
            var myusbmanager = (UsbManager)mycontext.GetSystemService(Context.UsbService);
            //var myusbdevicelist = myusbmanager.DeviceList;

            ////UsbDevice myusbdevice = null;
            //myusbdevice = null;
            //foreach (var n in myusbdevicelist.Keys)
            //{
            //    var tmpdevice = myusbdevicelist[n];
            //    if (tmpdevice.VendorId == myVendorId && tmpdevice.ProductId == myProductId)
            //        myusbdevice = tmpdevice;
            //}
            ////Log.Info(TAG, "Device=" + myusbdevice);
            //
            string ACTION_USB_PERMISSION = "com.android.example.USB_PERMISSION";

            Log.Info(TAG, "Before permissionintent");
            var mypermissionintent =
                //    PendingIntent.GetBroadcast(mycontext, 0, new Intent(ACTION_USB_PERMISSION), 0);
                PendingIntent.GetBroadcast(mycontext, 0, new Intent(ACTION_USB_PERMISSION), PendingIntentFlags.Immutable);

            //Log.Info(TAG, "Permissionintent=" + mypermissionintent);

            myusbmanager.RequestPermission(myusbdevice, mypermissionintent);

            var myhaspermission = myusbmanager.HasPermission(myusbdevice);
            myusbconnection = myusbmanager.OpenDevice(myusbdevice);

            myusbinterface = myusbdevice.GetInterface(2);
            myendpointout = myusbinterface.GetEndpoint(1);
            myendpointin = myusbinterface.GetEndpoint(0);
            var myclaim = myusbconnection.ClaimInterface(myusbinterface, true);
            //Log.Info(TAG, "Claim="+myclaim);
            pinMode4([
                INPUT,
                INPUT,
                INPUT,
                INPUT
            ]);
        }

        public static USB2GROVE? create(Context mycontext)
        {
            var myusbmanager = (UsbManager)mycontext.GetSystemService(Context.UsbService);
            var myusbdevicelist = myusbmanager.DeviceList;

            UsbDevice? myusbdevice = null;
            foreach (var n in myusbdevicelist.Keys)
            {
                var tmpdevice = myusbdevicelist[n];
                if (tmpdevice.VendorId == myVendorId && tmpdevice.ProductId == myProductId)
                    myusbdevice = tmpdevice;
            }
            if (myusbdevice is null)
            {
                return (null);
            }
            else
            {
                return (new USB2GROVE(mycontext, myusbdevice));
            }
        }

        //GPIO(4ビット)の一括設定(設定値は配列で指定)
        //(0=DIGITAL INPUT,1=DIGITAL OUTPUT,2=ANALOG INOUT)
        public void pinMode4(byte[] mode)
        {
            byte[] buffer = new byte[64];
            for (int ic = 0; ic < 64; ic++)
                buffer[ic] = 0;
            buffer[0] = 0x60;
            buffer[7] = 0x80;
            for (int ic = 0; ic < 4; ic++)
            {
                switch (mode[ic])
                {
                    case 0: //INPUT
                        buffer[8 + ic] = 0b00001000;
                        break;
                    case 1: //OUTPUT
                        buffer[8 + ic] = 0b00000000;
                        break;
                    case 2: //ADC
                        buffer[8 + ic] = 0b00000010;
                        break;
                    case 3: //DAC
                        buffer[8 + ic] = 0b00000011;
                        break;
                    default:
                        buffer[8 + ic] = 0b00010000;
                        break;
                }
            }
            SendData(buffer);
        }
        //GPIOのデジタル書き込み(ピン番号は0～3、値は0か1が有効)
        public void digitalWrite(byte pin, byte value)
        {
            byte[] buffer = new byte[64];
            for (int ic = 0; ic < 64; ic++)
                buffer[ic] = 0;
            buffer[0] = 0x50;
            buffer[2 + pin * 4 + 0] = 1; //Change Value
            buffer[2 + pin * 4 + 1] = value;
            SendData(buffer);
        }

        //GPIOのデジタル読み込み(ピン番号は0～3が有効,戻り値は0か1)
        public byte digitalRead(byte pin)
        {
            byte[] buffer = new byte[64];
            for (int ic = 0; ic < 64; ic++)
                buffer[ic] = 0;
            buffer[0] = 0x51;
            byte[] result = new byte[64];
            result = SendData(buffer);
            return result[2 + pin * 2];
        }
        //GPIOのアナログ読み込み(ピン番号は1～3が有効,戻り値は0～1023)
        public int analogRead(byte pin)
        {
            byte[] buffer = new byte[64];
            for (int ic = 0; ic < 64; ic++)
                buffer[ic] = 0;
            buffer[0] = 0x10;

            byte[] result = new byte[64];
            int analog;
            result = SendData(buffer);
            analog = result[48 + pin * 2 + 0] + result[48 + pin * 2 + 1] * 256;
            return analog;
        }

        //GPIOのアナログ書き込み（ピン番号は2または3,値は0～31が有効）
        //アナログ読み出し（ADC）と違い、異なるピンに異なる値は設定できない
        //（同じ値が出力される）
        public void analogWrite(byte pin, byte value)
        {
            if (pin == 2 || pin == 3)
            {
                analogWrite(value);
            }
        }

        public void analogWrite(byte value)
        {
            byte[] buffer = new byte[64];
            for (int ic = 0; ic < 64; ic++)
                buffer[ic] = 0;
            buffer[0] = 0x60;
            buffer[4] = (byte)((value & 0x1F) | 0x80);
            SendData(buffer);
        }

        //I2Cの1バイト書き込み
        public void i2cWriteByte(byte adr, byte val)
        {
            byte[] rbuffer = new byte[1];
            rbuffer[0] = val;
            i2cWriteBlock(adr, rbuffer, 1);
        }
        public void i2cWriteReg(byte adr, byte reg, byte val)
        {
            byte[] buffer = new byte[64];
            for (int ic = 0; ic < 64; ic++)
                buffer[ic] = 0;
            buffer[0] = 0x90; //I2C Write Data
            buffer[1] = 2; //書き込むデータのバイト数(下位バイト)
            buffer[2] = 0; //同(上位バイト)
            buffer[3] = (byte)(adr << 1); //I2Cアドレス(7ビット仕様にするためシフト)
            buffer[4] = reg;
            buffer[5] = val;
            SendData(buffer);
        }
        //I2Cの複数バイト書き込み
        public void i2cWriteBlock(byte adr, byte[] val, byte size)
        {
            byte[] buffer = new byte[64];
            for (int ic = 0; ic < 64; ic++)
                buffer[ic] = 0;
            buffer[0] = 0x90; //I2C Write Data
            buffer[1] = size; //書き込むデータのバイト数(下位バイト)
            buffer[2] = 0; //同(上位バイト)
            buffer[3] = (byte)(adr << 1); //I2Cアドレス(7ビット仕様にするためシフト)

            for (int ic = 0; ic < size; ic++)
                buffer[ic + 4] = val[ic];
            SendData(buffer);
        }

        //I2Cの1バイト読み込み
        public byte i2cReadByte(byte adr, byte reg)
        {
            byte[] rbuffer = new byte[1];
            rbuffer = i2cReadBlock(adr, reg, 1);
            return rbuffer[0];
        }
        //I2Cの複数バイト読み込み
        public byte[] i2cReadBlock(byte adr, byte reg, byte size)
        {
            i2cWriteByte(adr, reg);
            byte[] buffer = new byte[64];
            for (int ic = 0; ic < 64; ic++)
                buffer[ic] = 0;
            buffer[0] = 0x91; //I2C Read Data
            buffer[1] = size; //読み込むデータのバイト数(下位バイト)
            buffer[2] = 0; //同(上位バイト)
            buffer[3] = (byte)(adr << 1); //I2Cアドレス(7ビット仕様にするためシフト)
            SendData(buffer);

            byte[] result = new byte[64];
            for (int ic = 0; ic < 64; ic++)
                buffer[ic] = 0;
            buffer[0] = 0x40; //I2C Get Data
            result = SendData(buffer);

            byte[] rbuffer = new byte[64];
            for (int ic = 0; ic < size; ic++)
                rbuffer[ic] = result[ic + 4];
            return rbuffer;
        }

        //HIDバッファ書き込み(64バイト配列) 
        private byte[] SendData(byte[] buf)
        {
            int written;
            written = myusbconnection.BulkTransfer(myendpointout, buf, 64, 30);
            if (written != 64)
                return null;
            written = myusbconnection.BulkTransfer(myendpointin, buf, 64, 30);
            if (written != 64)
                return null;
            return buf;
        }
    }
}
