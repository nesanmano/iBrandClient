using System.Collections.Generic;
using NomadLogic.Common.Serveur;

namespace NomadLogic.Test.iBrand.ProjectDownload.Mocks
{
    public class ServerExpectedResult
    {

        public static List<ResourceFileToDownload> JujuExpectedResult()
        {
            var r1 = new ResourceFileToDownload()
            {
                Src = "t1-2017-01-android-ui.xml",
                ServerModifiedTime = 1525894014107,
                FileSize = 38860
            };
            
            var r2 = new ResourceFileToDownload()
            {
                Src = "1-Tap to start.jpg",
                ServerModifiedTime = 1525894418851,
                FileSize = 226406
            };
            
            var r3 = new ResourceFileToDownload()
            {
                Src = "2-Waiver - Rules and regulations.png",
                ServerModifiedTime = 1525894422675,
                FileSize = 12
            };
            
            var r4 = new ResourceFileToDownload()
            {
                Src = "2-Waiver - Rules and regulations_FR.png",
                ServerModifiedTime = 1525894434121,
                FileSize = 13
            };

            var r5 = new ResourceFileToDownload()
            {
                Src = "2-Waiver_Bottom_banner.png",
                ServerModifiedTime = 1525894438813,
                FileSize = 25
            };

            var r6 = new ResourceFileToDownload()
            {
                Src = "2-Waiver_top_banner.png",
                ServerModifiedTime = 1525894442070,
                FileSize = 26
            };

            var r7 = new ResourceFileToDownload()
            {
                Src = "2-Waiver_top_banner_FR.png",
                ServerModifiedTime = 1525894445266,
                FileSize = 30
            };

            var r8 = new ResourceFileToDownload()
            {
                Src = "3-Terms and conditions.jpg",
                ServerModifiedTime = 1525894447886,
                FileSize = 543
            };

            var r9 = new ResourceFileToDownload()
            {
                Src = "3-Terms and conditions_FR.jpg",
                ServerModifiedTime = 1525894450721,
                FileSize = 459
            };

            var r10 = new ResourceFileToDownload()
            {
                Src = "4-Scan_Top_banner_ENG.png",
                ServerModifiedTime = 1525894453484,
                FileSize = 312
            };

            var r11 = new ResourceFileToDownload()
            {
                Src = "4-Scan_Top_banner_FR.png",
                ServerModifiedTime = 1525894455996,
                FileSize = 308
            };

            var r12 = new ResourceFileToDownload()
            {
                Src = "5-Thank you.jpg",
                ServerModifiedTime = 1525894458654,
                FileSize = 211
            };
            

            var r13 = new ResourceFileToDownload()
            {
                Src = "5-Thank you_FR.jpg",
                ServerModifiedTime = 1525894461838,
                FileSize = 198
            };

            var r14 = new ResourceFileToDownload()
            {
                Src = "Background.jpg",
                ServerModifiedTime = 1525894469142,
                FileSize = 140
            };

            var r15 = new ResourceFileToDownload()
            {
                Src = "BigSpin_Watermark.png",
                ServerModifiedTime = 1525894472375,
                FileSize = 28
            };

            var r16 = new ResourceFileToDownload()
            {
                Src = "Bouton_suivant.png",
                ServerModifiedTime = 1525894477218,
                FileSize = 6
            };

            var r17 = new ResourceFileToDownload()
            {
                Src = "Bouton_suivant_ET_précédent.png",
                ServerModifiedTime = 1525894481739,
                FileSize = 8
            };

            var r18 = new ResourceFileToDownload()
            {
                Src = "database.xml",
                ServerModifiedTime = 1525894491148,
                FileSize = 255
            };

            var r19 = new ResourceFileToDownload()
            {
                Src = "facebook.json",
                ServerModifiedTime = 1525894500458,
                FileSize = 0
            };

            var r20 = new ResourceFileToDownload()
            {
                Src = "next_AND_previous_button.png",
                ServerModifiedTime = 1525894503436,
                FileSize = 8
            };

            var r21 = new ResourceFileToDownload()
            {
                Src = "next_button.png",
                ServerModifiedTime = 1525894509063,
                FileSize = 5
            };

            var r22 = new ResourceFileToDownload()
            {
                Src = "subscription.xml",
                ServerModifiedTime = 1525894514177,
                FileSize = 0
            };

            var r23 = new ResourceFileToDownload()
            {
                Src = "t1-2017-01-android.xml",
                ServerModifiedTime = 1536864239462,
                FileSize = 5
            };

            var r24 = new ResourceFileToDownload()
            {
                Src = "email_fr_dl.xml",
                ServerModifiedTime = 1525894891164,
                FileSize = 22
            };

            var r25 = new ResourceFileToDownload()
            {
                Src = "sms_button_inactive.png",
                ServerModifiedTime = 1542147203197,
                FileSize = 26
            };

            var listRes = new List<ResourceFileToDownload>() { r1,r2,r3,r4,r5,r6,r7,r8,r9,r10,r11,r12,r13,r14,r15,r16,r17,r18,r19,r20,r21,r22,r23,r24,r25};

            return listRes;
        }



        public static List<ResourceFileToDownload> LegacyExpectedResult()
        {
            var r1 = new ResourceFileToDownload()
            {
                Src = "\\config.txt",
                ServerModifiedTime = 1547830063980,
                FileSize = 130
            };
            
            var r2 = new ResourceFileToDownload()
            {
                Src = "\\ffmpeg_sa.txt",
                ServerModifiedTime = 1538080805571,
                FileSize = 232
            };
            
            var r3 = new ResourceFileToDownload()
            {
                Src = "\\gifImages\\001.png",
                ServerModifiedTime = 1538080811490,
                FileSize = 1710852
            };
            
            var r4 = new ResourceFileToDownload()
            {
                Src = "\\gifImages\\002.png",
                ServerModifiedTime = 1538080810403,
                FileSize = 1657066
            };

            var r5 = new ResourceFileToDownload()
            {
                Src = "\\gifImages\\003.png",
                ServerModifiedTime = 1538080809360,
                FileSize = 1524485
            };

            var r6 = new ResourceFileToDownload()
            {
                Src = "\\gifImages\\004.png",
                ServerModifiedTime = 1538080808381,
                FileSize = 1395306
            };

            var r7 = new ResourceFileToDownload()
            {
                Src = "\\gifImages\\005.png",
                ServerModifiedTime = 1538080814118,
                FileSize = 1271425
            };

            var r8 = new ResourceFileToDownload()
            {
                Src = "\\overlay.png",
                ServerModifiedTime = 1538080807171,
                FileSize = 1710852
            };

            var r9 = new ResourceFileToDownload()
            {
                Src = "\\photobooth_ui\\2. Sharing options.jpg",
                ServerModifiedTime = 1538080916168,
                FileSize = 459
            };

            var r10 = new ResourceFileToDownload()
            {
                Src = "\\photobooth_ui\\2. Sharing.jpg",
                ServerModifiedTime = 1538080916376,
                FileSize = 78216
            };

            var r12 = new ResourceFileToDownload()
            {
                Src = "\\photobooth_ui\\3. Email.jpg",
                ServerModifiedTime = 1538080916220,
                FileSize = 47635
            };
            

            var r13 = new ResourceFileToDownload()
            {
                Src = "\\photobooth_ui\\4. SMS.jpg",
                ServerModifiedTime = 1538080916080,
                FileSize = 49709
            };

         

            var listRes = new List<ResourceFileToDownload>() { r1,r2,r3,r4,r5,r6,r7,r8,r9,r10,r12,r13};

            return listRes;
        }



    }
}
