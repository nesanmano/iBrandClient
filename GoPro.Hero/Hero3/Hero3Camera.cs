using System;
using System.Collections.Generic;
using GoPro.Hero.Commands;
using System.Linq;
using GoPro.Hero.Browser.FileSystem;
using GoPro.Hero.Browser.Media;
using System.Threading.Tasks;

namespace GoPro.Hero.Hero3
{
    public class Hero3Camera : Camera
    {
        public Hero3Camera(Bacpac bacpac) : base(bacpac)
        {
            //var filter = FilterGeneric.Create("GoPro.Hero.Hero3.Hero3FilterScheme.xml");
            //base.SetFilter(filter);
        }

        public Hero3Camera(string address) : base(Bacpac.Create(address))
        {
        }

        public Node Browse()
        {
            return FileSystem<AmbarellaBrowser>();
        }

        public Hero3Camera VideoResolution(VideoResolution resolution, bool nonBlocking = false)
        {
            return ExecuteMultiChoiceCommand<CommandCameraVideoResolution,VideoResolution>(resolution, nonBlocking);
        }

        public async Task<Hero3Camera> VideoResolutionAsync(VideoResolution resolution)
        {
            return await PrepareCommand<CommandCameraVideoResolution>().Select(resolution).ExecuteAsync() as Hero3Camera;
        }

        public async Task<VideoResolution> VideoResolutionAsync()
        {
            return (await ExtendedSettingsAsync()).VideoResolution;
        }

        public Hero3Camera VideoResolution(out VideoResolution resolution)
        {
            resolution = ExtendedSettings().VideoResolution;
            return this;
        }

        public MediaBrowser Contents()
        {
            return Browse<GoProMediaBrowser>();
        }

        public IEnumerable<VideoResolution> ValidVideoResolution()
        {
            var valid = PrepareCommand<CommandCameraVideoResolution>().ValidStates();
            return valid;
        }

        public Hero3Camera ValidVideoResolution(out IEnumerable<VideoResolution> validVideoResolution)
        {
            validVideoResolution = ValidVideoResolution();
            return this;
        }

        public Hero3Camera Orientation(Orientation orientation ,bool nonBlocking = false)
        {
            return ExecuteMultiChoiceCommand<CommandCameraOrientation, Orientation>(orientation, nonBlocking);
        }

        public async Task<Hero3Camera> OrientationAsync(Orientation orientation)
        {
            return await PrepareCommand<CommandCameraOrientation>().Select(orientation).ExecuteAsync() as Hero3Camera;
        }

        public Hero3Camera Orientation(out Orientation orientation)
        {
            orientation = ExtendedSettings().Orientation;
            return this;
        }

        public async Task<Orientation> OrientationAsync()
        {
            return (await ExtendedSettingsAsync()).Orientation;
        }

        public Hero3Camera TimeLapse(TimeLapse timeLapse,bool nonBlocking = false)
        {
            return ExecuteMultiChoiceCommand<CommandCameraTimeLapse, TimeLapse>(timeLapse, nonBlocking);
        }

        public async Task<Hero3Camera> TimeLapseAsync(TimeLapse timeLapse)
        {
            return await PrepareCommand<CommandCameraTimeLapse>().Select(timeLapse).ExecuteAsync() as Hero3Camera;
        }

        public Hero3Camera TimeLapse(out TimeLapse timeLapse)
        {
            timeLapse = ExtendedSettings().TimeLapse;
            return this;
        }

        public async Task<TimeLapse> TimeLapseAsync()
        {
            return (await ExtendedSettingsAsync()).TimeLapse;
        }

        public Hero3Camera BeepSound(BeepSound beepSound , bool nonBlocking = false)
        {
            return ExecuteMultiChoiceCommand<CommandCameraBeepSound, BeepSound>(beepSound, nonBlocking);
        }

        public async Task<Hero3Camera> BeepSoundAsync(BeepSound beepSound)
        {
            return await PrepareCommand<CommandCameraBeepSound>().Select(beepSound).ExecuteAsync() as Hero3Camera;
        }

        public async Task<BeepSound> BeepSoundAsync()
        {
            return (await ExtendedSettingsAsync()).BeepSound;
        }

        public Hero3Camera BeepSound(out BeepSound beepSound)
        {
            beepSound = ExtendedSettings().BeepSound;
            return this;
        }

        public Hero3Camera Protune(bool state, bool nonBlocing = false)
        {
            return ExecuteBooleanCommand<CommandCameraProtune>(state, nonBlocing);
        }

        public async Task<Hero3Camera> ProtuneAsync(bool state)
        {
            return await PrepareCommand<CommandCameraProtune>().Set(state).ExecuteAsync() as Hero3Camera;
        }

        public Hero3Camera EnableProtune()
        {
            return Protune(true);
        }

        public async Task<Hero3Camera> EnableProtuneAsync()
        {
            return await ProtuneAsync(true);
        }

        public Hero3Camera DisableProtune()
        {
            return Protune(false);
        }

        public async Task<Hero3Camera> DisableProtuneAsync()
        {
            return await ProtuneAsync(false);
        }

        public bool SupportsProtune()
        {
            return PrepareCommand<CommandCameraProtune>().ValidStates().Any();
        }

        public Hero3Camera Protune(out bool state)
        {
            state = ExtendedSettings().Protune;
            return this;
        }

        public async Task<bool> ProtuneAsync()
        {
            return (await ExtendedSettingsAsync()).Protune;
        }

        public IEnumerable<bool> ValidProtune()
        {
            return PrepareCommand<CommandCameraProtune>().ValidStates();
        }

        public Hero3Camera ValidProtune(out IEnumerable<bool> validProtune)
        {
            validProtune = ValidProtune();
            return this;
        }

        public Hero3Camera AutoLowLight(bool state, bool nonBlocing = false)
        {
            return ExecuteBooleanCommand<CommandCameraAutoLowLight>(state, nonBlocing);
        }

        public async Task<Hero3Camera> AutoLowLightAsync(bool state)
        {
            return await PrepareCommand<CommandCameraAutoLowLight>().Set(state).ExecuteAsync() as Hero3Camera;
        }

        public Hero3Camera EnableAutoLowLight()
        {
            return AutoLowLight(true);
        }

        public async Task<Hero3Camera> EnableAutoLowLightAsync()
        {
            return await AutoLowLightAsync(true);
        }

        public Hero3Camera DisableAutoLowLight()
        {
            return AutoLowLight(false);
        }

        public async Task<Hero3Camera> DisableAutoLowLightAsync()
        {
            return await AutoLowLightAsync(false);
        }

        public bool SupportsAutoLowLight()
        {
            return PrepareCommand<CommandCameraAutoLowLight>().ValidStates().Any();
        }

        public Hero3Camera AutoLowLight(out bool state)
        {
            state = ExtendedSettings().AutoLowLight;
            return this;
        }

        public async Task<bool> AutoLowLightAsync()
        {
            return (await ExtendedSettingsAsync()).AutoLowLight;
        }

        public IEnumerable<bool> ValidAutoLowLight()
        {
            return PrepareCommand<CommandCameraAutoLowLight>().ValidStates();
        }

        public Hero3Camera ValidAutoLowLight(out IEnumerable<bool> autoLowLight)
        {
            autoLowLight = ValidAutoLowLight();
            return this;
        }

        public Hero3Camera PhotoResolution(PhotoResolution resolution , bool nonBlocking = false)
        {
            return ExecuteMultiChoiceCommand<CommandCameraPhotoResolution, PhotoResolution>(resolution, nonBlocking);
        }

        public async Task<Hero3Camera> PhotoResolutionAsync(PhotoResolution resolution)
        {
            return await PrepareCommand<CommandCameraPhotoResolution>().Select(resolution).ExecuteAsync() as Hero3Camera;
        }

        public Hero3Camera PhotoResolution(out PhotoResolution resolution)
        {
            resolution = ExtendedSettings().PhotoResolution;
            return this;
        }

        public async Task<PhotoResolution> PhotoResolutionAsync()
        {
            return (await ExtendedSettingsAsync()).PhotoResolution;
        }

        public IEnumerable<PhotoResolution> ValidPhotoResolution()
        {
            var valid = PrepareCommand<CommandCameraPhotoResolution>().ValidStates();
            return valid;
        }

        public Hero3Camera ValidPhotoResolution(out IEnumerable<PhotoResolution> validPhotoResolution)
        {
            validPhotoResolution = ValidPhotoResolution();
            return this;
        }

        public Hero3Camera VideoStandard(VideoStandard videoStandard , bool nonBlocking = false)
        {
            return ExecuteMultiChoiceCommand<CommandCameraVideoStandard, VideoStandard>(videoStandard, nonBlocking);
        }

        public async Task<Hero3Camera> VideoStandardAsync(VideoStandard videoStandard)
        {
            return await PrepareCommand<CommandCameraVideoStandard>().Select(videoStandard).ExecuteAsync() as Hero3Camera;
        }

        public Hero3Camera VideoStandard(out VideoStandard videoStandard)
        {
            videoStandard = ExtendedSettings().VideoStandard;
            return this;
        }

        public async Task<VideoStandard> VideoStandardAsync()
        {
            return (await ExtendedSettingsAsync()).VideoStandard;
        }

        public Hero3Camera Mode(Mode mode , bool nonBlocking = false)
        {
            return ExecuteMultiChoiceCommand<CommandCameraMode, Mode>(mode, nonBlocking);
        }

        public async Task<Hero3Camera> ModeAsync(Mode mode)
        {
            return await PrepareCommand<CommandCameraMode>().Select(mode).ExecuteAsync() as Hero3Camera;
        }

        public Hero3Camera Mode(out Mode mode)
        {
            mode = ExtendedSettings().Mode;
            return this;
        }

        public async Task<Mode> ModeAsync()
        {
            return (await ExtendedSettingsAsync()).Mode;
        }

        public Hero3Camera Locate(bool state , bool nonBlocking = false)
        {
            return ExecuteBooleanCommand<CommandCameraLocate>(state, nonBlocking);
        }

        public async Task<Hero3Camera> LocateAsync(bool state)
        {
            return await PrepareCommand<CommandCameraLocate>().Set(state).ExecuteAsync() as Hero3Camera;
        }

        public Hero3Camera EnableLocate()
        {
            return Locate(true);
        }

        public async Task<Hero3Camera> EnableLocateAsync()
        {
            return await LocateAsync(true);
        }

        public Hero3Camera DisableLocate()
        {
            return Locate(false);
        }

        public async Task<Hero3Camera> DisableLocateAsync()
        {
            return await LocateAsync(false);
        }

        public Hero3Camera Locate(out bool state)
        {
            state = ExtendedSettings().LocateCamera;
            return this;
        }

        public async Task<bool> LocateAsync()
        {
            return (await ExtendedSettingsAsync()).LocateCamera;
        }

        public Hero3Camera LivePreview(bool state , bool nonBlocking = false)
        {
            return ExecuteBooleanCommand<CommandCameraPreview>(state, nonBlocking);
        }

        public async Task<Hero3Camera> LivePreviewAsync(bool state)
        {
            return await PrepareCommand<CommandCameraPreview>().Set(state).ExecuteAsync() as Hero3Camera;
        }

        public Hero3Camera EnableLivePreview()
        {
            return LivePreview(true);
        }

        public async Task<Hero3Camera> EnableLivePreviewAsync()
        {
            return await LivePreviewAsync(true);
        }

        public Hero3Camera DisableLivePreview()
        {
            return LivePreview(false);
        }

        public async Task<Hero3Camera> DisableLivePreviewAsync()
        {
            return await LivePreviewAsync(false);
        }

        public Hero3Camera LivePreview(out bool state)
        {
            state = ExtendedSettings().PreviewActive;
            return this;
        }

        public async Task<bool> LivePreviewAsync()
        {
            return (await ExtendedSettingsAsync()).PreviewActive;
        }

        public Hero3Camera LedBlink(LedBlink ledBlink , bool nonBlocking = false)
        {
            return ExecuteMultiChoiceCommand<CommandCameraLedBlink, LedBlink>(ledBlink, nonBlocking);
        }

        public async Task<Hero3Camera> LedBlinkAsync(LedBlink ledBlink)
        {
            return await PrepareCommand<CommandCameraLedBlink>().Select(ledBlink).ExecuteAsync() as Hero3Camera;
        }

        public Hero3Camera LedBlink(out LedBlink ledBlink)
        {
            ledBlink = ExtendedSettings().LedBlink;
            return this;
        }

        public async Task<LedBlink> LedBlinkAsync()
        {
            return (await ExtendedSettingsAsync()).LedBlink;
        }

        public Hero3Camera FieldOfView(FieldOfView fieldOfView , bool nonBlocking = false)
        {
            return ExecuteMultiChoiceCommand<CommandCameraFieldOfView, FieldOfView>(fieldOfView, nonBlocking);
        }

        public async Task<Hero3Camera> FieldOfViewAsync(FieldOfView fieldOfView)
        {
            return await PrepareCommand<CommandCameraFieldOfView>().Select(fieldOfView).ExecuteAsync() as Hero3Camera;
        }

        public Hero3Camera FieldOfView(out FieldOfView fieldOfView)
        {
            fieldOfView = ExtendedSettings().FieldOfView;
            return this;
        }

        public async Task<FieldOfView> FieldOfViewAsync()
        {
            return (await ExtendedSettingsAsync()).FieldOfView;
        }

        public IEnumerable<FieldOfView> ValidFieldOfView()
        {
            return PrepareCommand<CommandCameraFieldOfView>().ValidStates();
        }

        public Hero3Camera ValidFieldOfView(out IEnumerable<FieldOfView> validFieldOfView)
        {
            validFieldOfView = ValidFieldOfView();
            return this;
        }

        public Hero3Camera SpotMeter(bool state , bool nonBlocking = false)
        {
            return ExecuteBooleanCommand<CommandCameraSpotMeter>(state, nonBlocking);
        }

        public async  Task<Hero3Camera> SpotMeterAsync(bool state)
        {
            return await  PrepareCommand<CommandCameraSpotMeter>().Set(state).ExecuteAsync() as Hero3Camera;
        }

        public Hero3Camera EnableSpotMeter()
        {
            return SpotMeter(true);
        }

        public async Task<Hero3Camera> EnableSpotMeterAsync()
        {
            return await SpotMeterAsync(true);
        }

        public Hero3Camera DisableSpotMeter()
        {
            return SpotMeter(false);
        }

        public async Task<Hero3Camera> DisableSpotMeterAsync()
        {
            return await SpotMeterAsync(false);
        }

        public Hero3Camera SpotMeter(out bool state)
        {
            state = ExtendedSettings().SpotMeter;
            return this;
        }

        public async Task<bool> SpotMeterAsync()
        {
            return (await ExtendedSettingsAsync()).SpotMeter;
        }

        public Hero3Camera DefaultModeOnPowerOn(Mode mode , bool nonBlocking = false)
        {
            return ExecuteMultiChoiceCommand<CommandCameraDefaultMode, Mode>(mode, nonBlocking);
        }

        public async Task<Hero3Camera> DefaultModeOnPowerOnAsync(Mode mode)
        {
            return await PrepareCommand<CommandCameraDefaultMode>().Select(mode).ExecuteAsync() as Hero3Camera;
        }

        public Hero3Camera DefaultModeOnPowerOn(out Mode mode)
        {
            mode = ExtendedSettings().OnDefault;
            return this;
        }

        public async Task<Mode> DefaultModeOnPowerOnAsync()
        {
            return (await ExtendedSettingsAsync()).OnDefault;
        }

        public Hero3Camera DeleteLastFileOnSdCard(bool nonBlocking = false)
        {
            var task= PrepareCommand<CommandCameraDeleteLastFileOnSd>().ExecuteAsync();

            if (!nonBlocking)
                task.Wait();

            return this;
        }

        public async Task<Hero3Camera> DeleteLastFileOnSdCardAsync()
        {
            return await PrepareCommand<CommandCameraDeleteLastFileOnSd>().ExecuteAsync() as Hero3Camera;
        }

        public Hero3Camera DeleteAllFilesOnSdCard(bool nonBlocking = false)
        {
            var task=PrepareCommand<CommandCameraDeleteAllFilesOnSd>().ExecuteAsync();

            if (!nonBlocking)
                task.Wait();

            return this;
        }

        public async Task<Hero3Camera> DeleteAllFilesOnSdCardAsync()
        {
            return await PrepareCommand<CommandCameraDeleteAllFilesOnSd>().ExecuteAsync() as Hero3Camera;
        }

        public Hero3Camera WhiteBalance(WhiteBalance whiteBalance , bool nonBlocking = false)
        {
            return ExecuteMultiChoiceCommand<CommandCameraWhiteBalance, WhiteBalance>(whiteBalance, nonBlocking);
        }

        public async Task<Hero3Camera> WhiteBalanceAsync(WhiteBalance whiteBalance)
        {
            return await PrepareCommand<CommandCameraWhiteBalance>().Select(whiteBalance).ExecuteAsync() as Hero3Camera;
        }

        public Hero3Camera WhiteBalance(out WhiteBalance whiteBalance)
        {
            whiteBalance = ExtendedSettings().WhiteBalance;
            return this;
        }

        public async Task<WhiteBalance> WhiteBalanceAsync()
        {
            return (await ExtendedSettingsAsync()).WhiteBalance;
        }

        public IEnumerable<WhiteBalance> ValidWhiteBalance()
        {
            var valid = PrepareCommand<CommandCameraWhiteBalance>().ValidStates();
            return valid;
        }

        public Hero3Camera ValidWhiteBalance(out IEnumerable<WhiteBalance> validWhiteBalance)
        {
            validWhiteBalance = ValidWhiteBalance();
            return this;
        }

        public Hero3Camera LoopingVideo(LoopingVideo loopingVideo , bool nonBlocking = false)
        {
            return ExecuteMultiChoiceCommand<CommandCameraLoopingVideo, LoopingVideo>(loopingVideo, nonBlocking);
        }

        public async Task<Hero3Camera> LoopingVideoAsync(LoopingVideo loopingVideo)
        {
            return await PrepareCommand<CommandCameraLoopingVideo>().Select(loopingVideo).ExecuteAsync() as Hero3Camera;
        }

        public Hero3Camera LoopingVideo(out LoopingVideo loopingVideo)
        {
            loopingVideo = ExtendedSettings().LoopingVideoMode;
            return this;
        }

        public async Task<LoopingVideo> LoopingVideoAsync()
        {
            return (await ExtendedSettingsAsync()).LoopingVideoMode;
        }

        public IEnumerable<LoopingVideo> ValidLoopingVideo()
        {
            var valid = PrepareCommand<CommandCameraLoopingVideo>().ValidStates();
            return valid;
        }

        public Hero3Camera ValidLoopingVideo(out IEnumerable<LoopingVideo> validLoopingVideo)
        {
            validLoopingVideo = ValidLoopingVideo();
            return this;
        }

        public Hero3Camera FrameRate(FrameRate frameRate , bool nonBlocking = false)
        {
            return ExecuteMultiChoiceCommand<CommandCameraFrameRate, FrameRate>(frameRate, nonBlocking);
        }

        public async Task<Hero3Camera> FrameRateAsync(FrameRate frameRate)
        {
            return await PrepareCommand<CommandCameraFrameRate>().Select(frameRate).ExecuteAsync() as Hero3Camera;
        }

        public Hero3Camera FrameRate(out FrameRate frameRate)
        {
            frameRate = ExtendedSettings().FrameRate;
            return this;
        }

        public async Task<FrameRate> FrameRateAsync()
        {
            return (await ExtendedSettingsAsync()).FrameRate;
        }

        public IEnumerable<FrameRate> ValidFrameRate()
        {
            var valid = PrepareCommand<CommandCameraFrameRate>().ValidStates();
            return valid;
        }

        public Hero3Camera ValidFrameRate(out IEnumerable<FrameRate> validFrameRate)
        {
            validFrameRate = ValidFrameRate();
            return this;
        }

        public Hero3Camera BurstRate(BurstRate burstRate, bool nonBlocking = false)
        {
            return ExecuteMultiChoiceCommand<CommandCameraBurstRate, BurstRate>(burstRate, nonBlocking);
        }

        public async Task<Hero3Camera> BurstRateAsync(BurstRate burstRate)
        {
            return await PrepareCommand<CommandCameraBurstRate>().Select(burstRate).ExecuteAsync() as Hero3Camera;
        }

        public Hero3Camera BurstRate(out BurstRate burstRate)
        {
            burstRate = ExtendedSettings().BurstRate;
            return this;
        }

        public async Task<BurstRate> BurstRate()
        {
            return (await ExtendedSettingsAsync()).BurstRate;
        }

        public Hero3Camera ContinuousShot(ContinuousShot continuousShot , bool nonBlocking = false)
        {
            return ExecuteMultiChoiceCommand<CommandCameraContinuousShot, ContinuousShot>(continuousShot, nonBlocking);
        }

        public async Task<Hero3Camera> ContinuousShotAsync(ContinuousShot continuousShot)
        {
            return await PrepareCommand<CommandCameraContinuousShot>().Select(continuousShot).ExecuteAsync() as Hero3Camera;
        }

        public Hero3Camera ContinuousShot(out ContinuousShot continuousShot)
        {
            continuousShot = ExtendedSettings().ContinuousShot;
            return this;
        }

        public async Task<ContinuousShot> ContinuousShotAsync()
        {
            return (await ExtendedSettingsAsync()).ContinuousShot;
        }

        public Hero3Camera PhotoInVideo(PhotoInVideo photoInVideo, bool nonBlocking = false)
        {
            return ExecuteMultiChoiceCommand<CommandCameraPhotoInVideo, PhotoInVideo>(photoInVideo, nonBlocking);
        }

        public async Task<Hero3Camera> PhotoInVideoAsync(PhotoInVideo photoInVideo)
        {
            return await PrepareCommand<CommandCameraPhotoInVideo>().Select(photoInVideo).ExecuteAsync() as Hero3Camera;
        }

        public Hero3Camera PhotoInVideo(out PhotoInVideo photoInVideo)
        {
            photoInVideo = ExtendedSettings().PhotoInVideo;
            return this;
        }

        public async Task<PhotoInVideo> PhotoInVideoAsync()
        {
            return (await ExtendedSettingsAsync()).PhotoInVideo;
        }

        public new Hero3Camera Shutter(bool state, bool nonBlocking = false)
        {
            return base.Shutter(state, nonBlocking) as Hero3Camera;
        }

        public new async Task<Hero3Camera> ShutterAsync(bool state)
        {
            return await base.ShutterAsync(state) as Hero3Camera;
        }

        public Hero3Camera OpenShutter()
        {
            return Shutter(true);
        }

        public async Task<Hero3Camera> OpenShutterAsync()
        {
            return await ShutterAsync(true);
        }

        public Hero3Camera CloseShutter()
        {
            return Shutter(false);
        }

        public async Task<Hero3Camera> CloseShutterAsync()
        {
            return await ShutterAsync(false);
        }

        public Hero3Camera Shutter(out bool state)
        {
            state = ExtendedSettings().Shutter || BacpacStatus().ShutterStatus > 0;
            return this;
        }

        public async Task<bool> ShutterAsync()
        {
            return (await ExtendedSettingsAsync()).Shutter || (await BacpacStatusAsync()).ShutterStatus > 0;
        }

        public new Hero3Camera Power(bool state, bool nonBlocking = false)
        {
            return base.Power(state, nonBlocking) as Hero3Camera;
        }

        public new async Task<Hero3Camera> PowerAsync(bool state)
        {
            return await base.PowerAsync(state) as Hero3Camera;
        }

        public Hero3Camera PowerOn()
        {
            return Power(true);
        }

        public async Task<Hero3Camera> PowerOnAsync()
        {
            return await PowerAsync(true);
        }

        public Hero3Camera PowerOff()
        {
            return Power(false);
        }

        public async Task<Hero3Camera> PowerOffAsync()
        {
            return await PowerAsync(false);
        }

        public Hero3Camera Power(out bool state)
        {
            state = BacpacStatus().CameraPower;
            return this;
        }

        public async Task<bool> PowerAsync()
        {
            return (await BacpacStatusAsync()).CameraPower;
        }

        public Hero3Camera Model(out Model model)
        {
            model = BacpacStatusCache().CameraModel;
            return this;
        }

        public Hero3Camera SignalStrength(out SignalStrength signalStrength)
        {
            signalStrength = BacpacStatus().Rssi;
            return this;
        }

        public async Task<SignalStrength> SignalStrengthAsync()
        {
            return (await BacpacStatusAsync()).Rssi;
        }

        public Hero3Camera BatteryStatus(out byte batteryStatus)
        {
            batteryStatus = Settings().Battery;
            return this;
        }

        public async Task<byte> BatteryStatusAsync()
        {
            return (await SettingsAsync()).Battery;
        }

        public Hero3Camera PhotoCount(out int photoCount)
        {
            photoCount = ExtendedSettings().PhotosCount;
            return this;
        }

        public async Task<int> PhotoCountAsync()
        {
            return (await ExtendedSettingsAsync()).PhotosCount;
        }

        public Hero3Camera AvailablePhotoSpace(out int availableSpace)
        {
            availableSpace = ExtendedSettings().PhotosAvailableSpace;
            return this;
        }

        public async Task<int> AvailablePhotoSpaceAsync()
        {
            return (await ExtendedSettingsAsync()).PhotosAvailableSpace;
        }

        public Hero3Camera VideoCount(out int videoCount)
        {
            videoCount = ExtendedSettings().VideosCount;
            return this;
        }

        public async Task<int> VideoCountAsync()
        {
            return (await ExtendedSettingsAsync()).VideosCount;
        }

        public Hero3Camera AvailableVideoSpace(out TimeSpan availableSpace)
        {
            var seconds = ExtendedSettings().VideosAvailableSpace;
            availableSpace = new TimeSpan(0, 0, seconds);
            return this;
        }

        public async Task<TimeSpan> AvailableVideoSpaceAsync()
        {
            var seconds = (await ExtendedSettingsAsync()).VideosAvailableSpace;
            var availableSpace = new TimeSpan(0, 0, seconds);
            return availableSpace;
        }

        public Hero3Camera MacAddress(out string macAddress)
        {
            macAddress = BacpacInformationCache().MacAddress;
            return this;
        }

        public Hero3Camera IpAddress(out string ipAddress)
        {
            ipAddress = Bacpac.Address;
            return this;
        }

        public Hero3Camera Password(out string password)
        {
            password = Bacpac.Password;
            return this;
        }

        public Hero3Camera BootLoader(out Version version)
        {
            version = BacpacInformationCache().BootloaderVersion;
            return this;
        }

        public Hero3Camera Firmware(out Version version)
        {
            version = BacpacInformationCache().FirmwareVersion;
            return this;
        }

        public Hero3Camera Ssid(out string ssid)
        {
            ssid = BacpacInformationCache().Ssid;
            return this;
        }

        public Hero3Camera FullName(out string fullName)
        {
            fullName = Information().Name;
            return this;
        }

        public async Task<string> FullNameAsync()
        {
            return (await InformationAsync()).Name;
        }

        public Hero3Camera Version(out string version)
        {
            version = InformationCache().Version;
            return this;
        }

        public Hero3Camera LivePreviewAvailable(out bool state)
        {
            state = ExtendedSettings().PreviewAvailable;
            return this;
        }

        public async Task<bool> LivePreviewAvailableAsync()
        {
            return (await ExtendedSettingsAsync()).PreviewAvailable;
        }

        private Hero3Camera ExecuteMultiChoiceCommand<T, TO>(TO selection, bool nonBlocking) where T : CommandMultiChoice<TO, ICamera>
        {
            var task = PrepareCommand<T>().Select(selection).ExecuteAsync();

            if (!nonBlocking)
                task.Wait();

            return this;
        }

        private Hero3Camera ExecuteBooleanCommand<T>(bool value, bool nonBlocking) where T : CommandBoolean<ICamera>
        {
            var task = PrepareCommand<T>().Set(value).ExecuteAsync();

            if (!nonBlocking)
                task.Wait();

            return this;
        }
    }
}