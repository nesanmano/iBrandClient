using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Canon.Eos.Framework;
using NomadLogic.Common.Util.Camera;
using NomadLogic.Common.Util.Camera.Canon;
using NomadLogic.Common.Util.Camera.Canon.CameraProperty;
using NUnit.Framework;

namespace NomadLogic.Test.Common.Camera
{
    public abstract class TestBaseCanonCamera : TestBaseNunit 
    {
        protected void TestAllCameraGroupSettings(EosCamera cam, IReadOnlyCollection<AbstractCameraProperty> props)
        {
            foreach (var prop in props)
            {
                var supp = prop.SupportedNumericValues;

                foreach (var s in supp)
                {
                    cam.SetPropertyIntegerData(prop.PropertyId, s);

                    Assert.AreEqual(s, cam.GetPropertyIntegerData(prop.PropertyId), $"Set value was not equal. Cam={cam.ProductName} ({prop.GetType().Name})");
                }
            }
        }
        
        protected CameraControllerCanon GetViableCanonCameraController()
        {
            var controller = CameraControllerCanon.GetInstance();
            Assert.That(controller.Init(), "Cannot init Canon camera controller.");
            if (controller.NbrCameraReady() == 0) Assert.Ignore("No camera connected. Skipping test.");
            return controller;
        }

        protected EosCamera GetFirstCameraReference(CameraControllerCanon controller, string model = null)
        {
            EosCamera cam = null;
            using (var ite = controller.EosCameras)
            {
                while (ite.MoveNext())
                {
                    cam = ite.Current;

                    if (string.IsNullOrWhiteSpace(model))
                    {
                        break; // Prend la première caméra disponible
                    }
                    
                    if (cam?.ProductName.Contains(model) ?? false)
                    {
                        break; // Si la caméra contient le numéro de modèle.
                    }
                }
            }

            return cam;
        }

        protected void SkipNotAllCamsAvailiable(params EosCamera[] cams)
        {
            if (cams.Length == 0) Assert.Fail("Tests must use at least one camera.");
            
            if (cams.Any(c => c == null))
            {
                Assert.Ignore("Not all cameras are ready. Skipping tests.");
            }
        }
        
        protected MockMovieListener TakeSynchMovie(CameraControllerCanon ccc, TimeSpan movTime, TimeSpan maxTime)
        {
            var cts = new CancellationTokenSource();

            var listener = new MockMovieListener
            {
                OnUnregister = () => Assert.Fail("Camera Unregistered."),
                OnVideoRequestCancelled = () => Assert.Fail("Video Cancelled while it should not be."),
                OnVideoRequestCompleted = () => { cts.Cancel(); }
            };

            var request = new VideoRequestSingle
            {
                VideoListener = listener,
                CountDown = 2,
                UseLiveView = false,
                LiveViewListener = null,
                Duration = 4.5,
                CountDownEventListener = null
            };
            
            var t = Task.Run(() => { 
                Assert.That(ccc.StartVideo(request));
                return listener;
            }, cts.Token);

            t.Wait(maxTime);

            return t.Result;
        }
    }
}