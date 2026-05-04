using System.Collections.Generic;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Internal.SDK;

namespace NomadLogic.Common.Util.Camera.Canon.CameraProperty
{
    public class Av : AbstractCameraProperty
    {
        public Av(EosCamera camera) : base(camera) { }

        /// <inheritdoc />
        protected override uint InitPropertyId()
        {
            return Edsdk.PropID_Av;
        }

        /// <inheritdoc />
        public override void InitGlobalProperties(Dictionary<uint, string> dic)
        {
            dic.Add( 0x00,"00");
	        dic.Add( 0x08,"1");
	        dic.Add( 0x0B,"1.1");
	        dic.Add( 0x0C,"1.2");
	        dic.Add( 0x0D,"1.2");
	        dic.Add( 0x10,"1.4");
	        dic.Add( 0x13,"1.6");
	        dic.Add( 0x14,"1.8");
	        dic.Add( 0x15,"1.8");
	        dic.Add( 0x18,"2");
	        dic.Add( 0x1B,"2.2");
	        dic.Add( 0x1C,"2.5");
	        dic.Add( 0x1D,"2.5");
	        dic.Add( 0x20,"2.8");
	        dic.Add( 0x23,"3.2");
	        dic.Add( 0x24,"3.5");
	        dic.Add( 0x25,"3.5");
	        dic.Add( 0x28,"4");
	        dic.Add( 0x2B,"4.5");
	        dic.Add( 0x2C,"4.5");
	        dic.Add( 0x2D,"5.0");
	        dic.Add( 0x30,"5.6");
	        dic.Add( 0x33,"6.3");
	        dic.Add( 0x34,"6.7");
	        dic.Add( 0x35,"7.1");
	        dic.Add( 0x38,"8");
	        dic.Add( 0x3B,"9");
	        dic.Add( 0x3C,"9.5");
	        dic.Add( 0x3D,"10");
	        dic.Add( 0x40,"11");
	        dic.Add( 0x43,"13");
	        dic.Add( 0x44,"13");
	        dic.Add( 0x45,"14");
	        dic.Add( 0x48,"16");
	        dic.Add( 0x4B,"18");
	        dic.Add( 0x4C,"19");
	        dic.Add( 0x4D,"20");
	        dic.Add( 0x50,"22");
	        dic.Add( 0x53,"25");
	        dic.Add( 0x54,"27");
	        dic.Add( 0x55,"29");
	        dic.Add( 0x58,"32");
	        dic.Add( 0x5B,"36");
	        dic.Add( 0x5C,"38");
	        dic.Add( 0x5D,"40");
	        dic.Add( 0x60,"45");
	        dic.Add( 0x63,"51");
	        dic.Add( 0x64,"54");
	        dic.Add( 0x65,"57");
	        dic.Add( 0x68,"64");
	        dic.Add( 0x6B,"72");
	        dic.Add( 0x6C,"76");
	        dic.Add( 0x6D,"80");
	        dic.Add( 0x70,"91");
	        dic.Add( 0xffffffff,"unknown");
        }
    }
}
