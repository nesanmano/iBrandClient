using System.Collections.Generic;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Internal.SDK;

namespace NomadLogic.Common.Util.Camera.Canon.CameraProperty
{
    public class Iso : AbstractCameraProperty
    {
        /// <summary>
        /// Constructeur principal.
        /// </summary>
        /// <param name="camera"></param>
        public Iso(EosCamera camera) : base(camera) { }

        /// <inheritdoc />
        protected override uint InitPropertyId()
        {
            return Edsdk.PropID_ISOSpeed;
        }

        /// <inheritdoc />
        public override void InitGlobalProperties(Dictionary<uint, string> dic)
        {
            dic.Add(0x00,"Auto");
	        dic.Add(0x28,"6");
	        dic.Add(0x30,"12");
	        dic.Add(0x38,"25");
	        dic.Add(0x40,"50");
	        dic.Add(0x48,"100");
	        dic.Add(0x4b,"125");
	        dic.Add(0x4d,"160");
	        dic.Add(0x50,"200");
	        dic.Add(0x53,"250");
	        dic.Add(0x55,"320");
	        dic.Add(0x58,"400");
	        dic.Add(0x5b,"500");
	        dic.Add(0x5d,"640");
	        dic.Add(0x60,"800");
	        dic.Add(0x63,"1000");
	        dic.Add(0x65,"1250");
	        dic.Add(0x68,"1600");
	        dic.Add(0x70,"3200");
	        dic.Add(0x78,"6400");
	        dic.Add(0x80,"12800");
	        dic.Add(0x88,"25600");
	        dic.Add(0x90,"51200");
	        dic.Add(0x98,"102400");
	        dic.Add(0xffffffff,"unknown");
        }
    }
}
