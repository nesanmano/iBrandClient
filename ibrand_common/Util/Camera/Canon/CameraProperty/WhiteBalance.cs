using System.Collections.Generic;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Internal.SDK;

namespace NomadLogic.Common.Util.Camera.Canon.CameraProperty
{
    public class WhiteBalance : AbstractCameraProperty
    {
        public WhiteBalance(EosCamera camera) : base(camera) { }

        /// <inheritdoc />
        protected override uint InitPropertyId()
        {
            return Edsdk.PropID_WhiteBalance;
        }

        /// <inheritdoc />
        public override void InitGlobalProperties(Dictionary<uint, string> dic)
        {
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_LR, "RAW");

            dic.Add(Edsdk.WhiteBalance_Auto, "Auto");
            dic.Add(Edsdk.WhiteBalance_Daylight, "Daylight");
            dic.Add(Edsdk.WhiteBalance_Cloudy, "Cloudy");
            dic.Add(Edsdk.WhiteBalance_Tangsten, "Tangsten");
            dic.Add(Edsdk.WhiteBalance_Fluorescent, "Fluorescent");
            dic.Add(Edsdk.WhiteBalance_Strobe, "Flash");
            dic.Add(Edsdk.WhiteBalance_WhitePaper, "White Paper");
            dic.Add(Edsdk.WhiteBalance_Shade, "Shade");
            dic.Add(Edsdk.WhiteBalance_ColorTemp, "Color Temp");
            dic.Add(Edsdk.WhiteBalance_PCSet1, "PCSet1");
            dic.Add(Edsdk.WhiteBalance_PCSet2, "PCSet2");
            dic.Add(Edsdk.WhiteBalance_PCSet3, "PCSet3");
            //_property_dictionary.Add((uint)Edsdk.WhiteBalance_Manual2, "Manual");
        }
    }
}
