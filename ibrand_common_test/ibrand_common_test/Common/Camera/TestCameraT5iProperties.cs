using System;
using System.Collections.Generic;
using Canon.Eos.Framework.Internal.SDK;
using NomadLogic.Common.Util.Camera.Canon;
using NomadLogic.Common.Util.Camera.Canon.CameraProperty;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable LocalizableElement

namespace NomadLogic.Test.Common.Camera
{
    public class TestCameraT5IProperties : TestBaseCanonCamera
    {
        [Test]
        public void BruteForce_T5I_properties()
        {
            var c = GetViableCanonCameraController();
            var cam = GetFirstCameraReference(c, "T5i");

            SkipNotAllCamsAvailiable(cam);
            
            // Extract main properties
            var propList = cam.GetType().GetProperties();
            foreach (var pi in propList)
            {
                Console.WriteLine(@"{0}={1}", pi.Name, pi.GetValue(cam, null));
            }

            var listPropId = new List<Tuple<uint, string>>
            {
                new Tuple<uint, string>(Edsdk.PropID_AEMode, "AEMode"),
                new Tuple<uint, string>(Edsdk.PropID_Av, "Av"),
                new Tuple<uint, string>(Edsdk.PropID_ExposureCompensation, "Exposure Comp"),
                new Tuple<uint, string>(Edsdk.PropID_ImageQuality, "Image Quality"),
                new Tuple<uint, string>(Edsdk.PropID_ISOSpeed, "Iso"),
                new Tuple<uint, string>(Edsdk.PropID_MeteringMode, "Metering Mode"),
                new Tuple<uint, string>(Edsdk.PropID_Tv, "Tv"),
                new Tuple<uint, string>(Edsdk.PropID_WhiteBalance, "White Balance")
            };

            Console.WriteLine(@"Listing properties values");
            foreach (var t in listPropId)
            {
                var desData = cam.GetPropertyDescription(t.Item1);
                var intData = cam.GetPropertyIntegerData(t.Item1);
                var strData = cam.GetPropertyStringData(t.Item1);
                
                Console.WriteLine(t.Item2);
                Console.WriteLine('\t' + $"Desc={desData.PropDesc}, Int=0x{intData:X}, Str={strData}");
            }
        }

        [Test]
        public void TestT5IProperties_AllProperties()
        {
            var c = GetViableCanonCameraController();
            var cam = GetFirstCameraReference(c, "T5i");

            SkipNotAllCamsAvailiable(cam);

            var propList = new List<AbstractCameraProperty>
            {
                new Iso(cam),
                new AeMode(cam),
                new Av(cam),
                new ExposureComp(cam),
                new ImageQuality(cam),
                new MeteringMode(cam),
                new Tv(cam),
                new WhiteBalance(cam)
            }.AsReadOnly();

            TestAllCameraGroupSettings(cam, propList);
        }

        [Test]
        public void TestT5IProperties_ManagerGroup_MixedCameras()
        {
            var c = GetViableCanonCameraController();
            var camT3i = GetFirstCameraReference(c, "T3i");
            var camT5i = GetFirstCameraReference(c, "T5i");

            SkipNotAllCamsAvailiable(camT3i, camT5i);

            var manager = new CameraSettingsManagerCanon();
            manager.UpdateSettingsForCamera(camT3i);
            manager.UpdateSettingsForCamera(camT5i);
            
            var g3 = manager.GetPropertiesGroup(camT3i);
            var g5 = manager.GetPropertiesGroup(camT5i);
            
            TestAllCameraGroupSettings(camT3i, g3.GetAllAbstractProperties());
            TestAllCameraGroupSettings(camT5i, g5.GetAllAbstractProperties());
            
        }

        [Test]
        public void TestT5IProperties_ManagerGroup()
        {
            var c = GetViableCanonCameraController();
            var cam = GetFirstCameraReference(c);

            Assert.That(cam.ProductName.Contains("T5i"));

            var manager = new CameraSettingsManagerCanon();
            manager.UpdateSettingsForCamera(cam);
            
            var group = manager.GetPropertiesGroup(cam.SerialNumber);
            
            TestAllCameraGroupSettings(cam, group.GetAllAbstractProperties());
        }
        
        
    }
}