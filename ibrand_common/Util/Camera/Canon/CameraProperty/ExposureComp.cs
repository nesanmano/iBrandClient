using System.Collections.Generic;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Internal.SDK;

namespace NomadLogic.Common.Util.Camera.Canon.CameraProperty
{
    public class ExposureComp : AbstractCameraProperty
    {
        public ExposureComp(EosCamera camera) : base(camera) { }

        /// <inheritdoc />
        protected override uint InitPropertyId()
        {
            return Edsdk.PropID_ExposureCompensation;
        }

        /// <inheritdoc />
        public override void InitGlobalProperties(Dictionary<uint, string> dic)
        {
            dic.Add( 0x18,"+3");
	        dic.Add( 0x15,"+2 2/3");
	        dic.Add( 0x14,"+2 1/2");
	        dic.Add( 0x13,"+2 1/3");
	        dic.Add( 0x10,"+2");
	        dic.Add( 0x0d,"+1 2/3");
	        dic.Add( 0x0c,"+1 1/2");
	        dic.Add( 0x0b,"+1 1/3");
	        dic.Add( 0x08,"+1");
	        dic.Add( 0x05,"+2/3");
	        dic.Add( 0x04,"+1/2");
	        dic.Add( 0x03,"+1/3");
	        dic.Add( 0x00,"0");
	        dic.Add( 0xfd,"-1/3");
	        dic.Add( 0xfc,"-1/2");
	        dic.Add( 0xfb,"-2/3");
	        dic.Add( 0xf8,"-1");
	        dic.Add( 0xf5,"-1 1/3");
	        dic.Add( 0xf4,"-1 1/2");
	        dic.Add( 0xf3,"-1 2/3");
	        dic.Add( 0xf0,"-2");
	        dic.Add( 0xed,"-2 1/3");
	        dic.Add( 0xec,"-2 1/2");
	        dic.Add( 0xeb,"-2 2/3");
	        dic.Add( 0xe8,"-3");
	        dic.Add( 0xffffffff,"unknown");
        }
    }
}
