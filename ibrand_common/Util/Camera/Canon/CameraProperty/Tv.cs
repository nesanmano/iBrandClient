using System.Collections.Generic;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Internal.SDK;

namespace NomadLogic.Common.Util.Camera.Canon.CameraProperty
{
    public class Tv : AbstractCameraProperty
    {
        public Tv(EosCamera camera) : base(camera) {}

        /// <inheritdoc />
        protected override uint InitPropertyId()
        {
            return Edsdk.PropID_Tv;
        }

        /// <inheritdoc />
        public override void InitGlobalProperties(Dictionary<uint, string> dic)
        {
            dic.Add(0x0c,"Bulb");
	        dic.Add( 0x10,"30h");
	        dic.Add( 0x13,"25h");
	        dic.Add( 0x14,"20h");
	        dic.Add( 0x15,"20h");
	        dic.Add( 0x18,"15h");
	        dic.Add( 0x1B,"13h");
	        dic.Add( 0x1C,"10h");
	        dic.Add( 0x1D,"10h");
	        dic.Add( 0x20,"8h");
	        dic.Add( 0x23,"6h");
	        dic.Add( 0x24,"6h");
	        dic.Add( 0x25,"5h");
	        dic.Add( 0x28,"4h");
	        dic.Add( 0x2B,"3h2");
	        dic.Add( 0x2C,"3h");
	        dic.Add( 0x2D,"2h5");
	        dic.Add( 0x30,"2h");
	        dic.Add( 0x33,"1h6");
	        dic.Add( 0x34,"1h5");
	        dic.Add( 0x35,"1h3");
	        dic.Add( 0x38,"1h");
	        dic.Add( 0x3B,"0h8");
	        dic.Add( 0x3C,"0h7");
	        dic.Add( 0x3D,"0h6");
	        dic.Add( 0x40,"0h5");
	        dic.Add( 0x43,"0h4");
	        dic.Add( 0x44,"0h3");
	        dic.Add( 0x45,"0h3");
	        dic.Add( 0x48,"4");
	        dic.Add( 0x4B,"5");
	        dic.Add( 0x4C,"6");
	        dic.Add( 0x4D,"6");
	        dic.Add( 0x50,"8");
	        dic.Add( 0x53,"10");
	        dic.Add( 0x54,"10");
	        dic.Add( 0x55,"13");
	        dic.Add( 0x58,"15");
	        dic.Add( 0x5B,"20");
	        dic.Add( 0x5C,"20");
	        dic.Add( 0x5D,"25");
	        dic.Add( 0x60,"30");
	        dic.Add( 0x63,"40");
	        dic.Add( 0x64,"45");
	        dic.Add( 0x65,"50");
	        dic.Add( 0x68,"60");
	        dic.Add( 0x6B,"80");
	        dic.Add( 0x6C,"90");
	        dic.Add( 0x6D,"100");
	        dic.Add( 0x70,"125");
	        dic.Add( 0x73,"160");
	        dic.Add( 0x74,"180");
	        dic.Add( 0x75,"200");
	        dic.Add( 0x78,"250");
	        dic.Add( 0x7B,"320");
	        dic.Add( 0x7C,"350");
	        dic.Add( 0x7D,"400");
	        dic.Add( 0x80,"500");
	        dic.Add( 0x83,"640");
	        dic.Add( 0x84,"750");
	        dic.Add( 0x85,"800");
	        dic.Add( 0x88,"1000");
	        dic.Add( 0x8B,"1250");
	        dic.Add( 0x8C,"1500");
	        dic.Add( 0x8D,"1600");
	        dic.Add( 0x90,"2000");
	        dic.Add( 0x93,"2500");
	        dic.Add( 0x94,"3000");
	        dic.Add( 0x95,"3200");
	        dic.Add( 0x98,"4000");
	        dic.Add( 0x9B,"5000");
	        dic.Add( 0x9C,"6000");
	        dic.Add( 0x9D,"6400");
	        dic.Add( 0xA0,"8000");
	        dic.Add( 0xffffffff,"unknown");
        }
    }
}
