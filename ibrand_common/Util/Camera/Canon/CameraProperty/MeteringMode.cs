using System.Collections.Generic;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Internal.SDK;

namespace NomadLogic.Common.Util.Camera.Canon.CameraProperty
{
    public class MeteringMode : AbstractCameraProperty
    {
        public MeteringMode(EosCamera camera) : base(camera) { }

        /// <inheritdoc />
        protected override uint InitPropertyId()
        {
            return Edsdk.PropID_MeteringMode;
        }

        /// <inheritdoc />
        public override void InitGlobalProperties(Dictionary<uint, string> dic)
        {
            dic.Add( 1,"Spot");
	        dic.Add( 3,"Evaluative");
	        dic.Add( 4,"Partial");
	        dic.Add( 5,"Center-Weighted Average");
	        dic.Add( 0xffffffff,"unknown");
        }
    }
}
