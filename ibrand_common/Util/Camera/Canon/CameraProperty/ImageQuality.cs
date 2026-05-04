using System.Collections.Generic;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Internal.SDK;

namespace NomadLogic.Common.Util.Camera.Canon.CameraProperty
{
    public class ImageQuality : AbstractCameraProperty
    {
        public ImageQuality(EosCamera camera) : base(camera) { }

        /// <inheritdoc />
        protected override uint InitPropertyId()
        {
            return Edsdk.PropID_ImageQuality;
        }

        /// <inheritdoc />
        public override void InitGlobalProperties(Dictionary<uint, string> dic)
        {
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_LR, "RAW");

            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_LRLJF, "RAW + Large Fine Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_LRMJF, "RAW + Middle Fine Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_LRSJF, "RAW + Small Fine Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_LRLJN, "RAW + Large Normal Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_LRMJN, "RAW + Middle Normal Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_LRSJN, "RAW + Small Normal Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_LRS1JF, "RAW + Small1 Fine Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_LRS1JN, "RAW + Small1 Normal Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_LRS2JF, "RAW + Small2 Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_LRS3JF, "RAW + Small3 Jpeg");

            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_LRLJ, "RAW + Large Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_LRM1J, "RAW + Middle1 Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_LRM2J, "RAW + Middle2 Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_LRSJ, "RAW + Small Jpeg");

            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_MR, "Middle Raw(Small RAW1)");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_MRLJF, "Middle Raw(Small RAW1) + Large Fine Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_MRMJF, "Middle Raw(Small RAW1) + Middle Fine Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_MRSJF, "Middle Raw(Small RAW1) + Small Fine Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_MRLJN, "Middle Raw(Small RAW1) + Large Normal Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_MRMJN, "Middle Raw(Small RAW1) + Middle Normal Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_MRSJN, "Middle Raw(Small RAW1) + Small Normal Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_MRS1JF, "Middle RAW + Small1 Fine Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_MRS1JN, "Middle RAW + Small1 Normal Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_MRS2JF, "Middle RAW + Small2 Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_MRS3JF, "Middle RAW + Small3 Jpeg");

            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_MRLJ, "Middle Raw + Large Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_MRM1J, "Middle Raw + Middle1 Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_MRM2J, "Middle Raw + Middle2 Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_MRSJ, "Middle Raw + Small Jpeg");

            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_SR, "Small RAW(Small RAW2)");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_SRLJF, "Small RAW(Small RAW2) + Large Fine Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_SRMJF, "Small RAW(Small RAW2) + Middle Fine Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_SRSJF, "Small RAW(Small RAW2) + Small Fine Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_SRLJN, "Small RAW(Small RAW2) + Large Normal Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_SRMJN, "Small RAW(Small RAW2) + Middle Normal Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_SRSJN, "Small RAW(Small RAW2) + Small Normal Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_SRS1JF, "Small RAW + Small1 Fine Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_SRS1JN, "Small RAW + Small1 Normal Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_SRS2JF, "Small RAW + Small2 Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_SRS3JF, "Small RAW + Small3 Jpeg");

            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_SRLJ, "Small RAW + Large Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_SRM1J, "Small RAW + Middle1 Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_SRM2J, "Small RAW + Middle2 Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_SRSJ, "Small RAW + Small Jpeg");

            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_LJF, "Large Fine Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_LJN, "Large Normal Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_MJF, "Middle Fine Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_MJN, "Middle Normal Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_SJF, "Small Fine Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_SJN, "Small Normal Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_S1JF, "Small1 Fine Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_S1JN, "Small1 Normal Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_S2JF, "Small2 Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_S3JF, "Small3 Jpeg");

            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_LJ, "Large Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_M1J, "Middle1 Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_M2J, "Middle2 Jpeg");
            dic.Add((uint)Edsdk.ImageQuality.EdsImageQuality_SJ, "Small Jpeg");

        }
    }
}
