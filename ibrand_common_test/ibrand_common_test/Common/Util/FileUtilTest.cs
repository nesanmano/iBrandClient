using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NomadLogic.Common.Condition;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Networking.VimeoApi;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace NomadLogic.Test.Common.Util
{
    [TestFixture]
    public class FileUtilTest : TestBaseNunit
    {
        [Test]
        public void TestUpdateJsonWithGenericClass()
        {
            var jsonFile = BaseDir + "list_album.json";
            Assert.That(File.Exists(jsonFile));
            var data = new VimeoRequestResultInfo<VimeoAlbumInfo>();
            Assert.IsNull(data.Data);

            Assert.IsTrue(JsonUtil.UpdateFromJson(new FileInfo(jsonFile), data));
            Assert.That(data.Data != null && data.Data.Count == 3);
        }

        [Test]
        public void TestConditionSerializing()
        {
            #region Tests sur condition de question
            var conditionQuestion = new QuestionCondition
            {
                QuestionNo = 1,
                ChoiceNo = 2
            };
            
            // Test
            {
                var json = JsonConvert.SerializeObject(conditionQuestion);
                var deserialized = JsonConvert.DeserializeObject<QuestionCondition>(json);
                Assert.AreEqual(conditionQuestion, deserialized);
            }

            var conditionQuestion2 = new QuestionCondition
            {
                QuestionNo = 10,
                ChoiceNo = 20
            };

            var selector = new ImageSelectorQuestionnaire<QuestionCondition>
            {
                Tuples = new HashSet<ConditionFileTuple<QuestionCondition>>
                {
                    new ConditionFileTuple<QuestionCondition> { File = "tuple1", Condition = conditionQuestion },
                    new ConditionFileTuple<QuestionCondition> { File = "tuple2", Condition = conditionQuestion2 }
                }
            };

            // Test
            {
                var json = JsonConvert.SerializeObject(selector);
                var deserialized = JsonConvert.DeserializeObject<ImageSelectorQuestionnaire<QuestionCondition>>(json);
                Assert.AreEqual(selector, deserialized);
            }
            #endregion

            #region Test sur condition d'événement

            var eventCondition1 = new EventCondition
            {
                EventId = 100
            };

            {
                var json = JsonConvert.SerializeObject(eventCondition1);
                var deserialized = JsonConvert.DeserializeObject<EventCondition>(json);
                Assert.AreEqual(eventCondition1, deserialized);
            }

            // Test sur une liste d'élément

            var eventCondition2 = new EventCondition
            {
                EventId = 200
            };

            {
                var list = new[] { eventCondition1, eventCondition2};
                var json = JsonConvert.SerializeObject(list);
                var deserialized = JsonConvert.DeserializeObject<List<EventCondition>>(json);
                Assert.AreEqual(list, deserialized);
            }

            #endregion

            #region Test sur interface
            
            {
                var json = JsonConvert.SerializeObject(eventCondition1);
                var deserialized = JsonConvert.DeserializeObject<ICondition>(json, new ConditionJsonConverter());
                Assert.AreEqual(eventCondition1, deserialized);
            }

            var interfaceCond = new List<ICondition>
            {
                eventCondition1,
                conditionQuestion,
                eventCondition2,
                conditionQuestion2
            };

            {
                var json = JsonConvert.SerializeObject(interfaceCond);
                var deserialized = JsonConvert.DeserializeObject<List<ICondition>>(json, new ConditionJsonConverter());
                Assert.AreEqual(interfaceCond, deserialized);
            }

            #endregion
        }

        [Test]
        public void TestImageListTransformJsonMarshalling()
        {
            var baseDir = GetFullOutputDir();

            var masterConfigFile = new FileInfo(baseDir + Path.DirectorySeparatorChar + "configTransformDefault.config");
            EnsureFileNotExists(masterConfigFile.FullName);

            var trans = new ImageListTransformOptions();

            #region Tests avec valeurs de base

            JsonUtil.WriteJsonFile(masterConfigFile, trans);
            {
                Assert.That(File.Exists(masterConfigFile.FullName));
                var test = JsonUtil.CreateInstanceFromJsonConfigFile<ImageListTransformOptions>(masterConfigFile);
                Assert.That(test.BundleOptions == null);
                Assert.That(test.FirstPictureOptions == null);
                Assert.That(test.GifOptions == null);
                Assert.That(test.PrintOptions == null);
                Assert.That(test.SocialWallPictureOptions == null);
                Assert.That(test.VideoOptions == null);
            }

            trans.BundleOptions = new ImageListTransformOptionsBundle();
            trans.FirstPictureOptions = new ImageListTransformOptionsFirstPicture();
            trans.GifOptions = new ImageListTransformOptionsGif();
            trans.SocialWallPictureOptions = new ImageListTransformOptionsSocialWall();
            trans.PrintOptions = new ImageListTransformOptionsPrint();
            trans.VideoOptions = new ImageListTransformOptionsVideo();

            JsonUtil.WriteJsonFile(masterConfigFile, trans);
            {
                Assert.That(File.Exists(masterConfigFile.FullName));
                var test = JsonUtil.CreateInstanceFromJsonConfigFile<ImageListTransformOptions>(masterConfigFile);
                Assert.That(test.BundleOptions != null);
                Assert.That(test.FirstPictureOptions != null);
                Assert.That(test.GifOptions != null);
                Assert.That(test.PrintOptions != null);
                Assert.That(test.SocialWallPictureOptions != null);
                Assert.That(test.VideoOptions != null);
            }

            #endregion

            #region Test avec config pleine (sauf sélecteur)

            trans.PrintOptions = new ImageListTransformOptionsPrint
            {
                BackgroundColor = Color.Aqua,
                UseFullFramePicture = true,
                FrameOver = true,
                OffsetExtraTop = 2,
                UseOnlyFirstPicture = true,
                OffsetExtraSide = -9,
                SaveToQuestion = 45,
                JpegSaveQuality = 85,
                OffsetPerc = 0.02f,
                RotateFlip = RotateFlipType.Rotate270FlipXY
            };

            JsonUtil.WriteJsonFile(masterConfigFile, trans);
            {
                Assert.That(File.Exists(masterConfigFile.FullName));
                var test = JsonUtil.CreateInstanceFromJsonConfigFile<ImageListTransformOptions>(masterConfigFile);
                Assert.AreEqual(trans.PrintOptions, test.PrintOptions);
            }

            #endregion

            #region Tests sur les sélecteurs

            var configFileSingle = new FileInfo(baseDir + Path.DirectorySeparatorChar + "singleSelector.config");
            IImageSelector selector = new ImageSelectorSingle(BaseDir + "frame_4x6.png");

            trans.PrintOptions.PrintFrameSelector = selector;

            JsonUtil.WriteJsonFile(configFileSingle, trans);
            {
                Assert.That(File.Exists(configFileSingle.FullName));
                var test = JsonUtil.CreateInstanceFromJsonConfigFile<ImageListTransformOptions>(configFileSingle);
                Assert.AreEqual(selector, test.PrintOptions.PrintFrameSelector);
            }

            // Différent sélecteur

            var configFileQuestionCOndition = new FileInfo(baseDir + Path.DirectorySeparatorChar + "questionConditionSelector.config");
            selector = new ImageSelectorQuestionnaire<QuestionCondition>(new HashSet<ConditionFileTuple<QuestionCondition>>
            {
                new ConditionFileTuple<QuestionCondition> { File = "data1", Condition = new QuestionCondition
                    {
                        ChoiceNo = 2,
                        QuestionNo = 56
                    }
                },
                new ConditionFileTuple<QuestionCondition> { File = "data2", Condition = new QuestionCondition
                {
                    QuestionNo = 456
                }
                }
            });

            trans.PrintOptions.PrintFrameSelector = selector;

            JsonUtil.WriteJsonFile(configFileQuestionCOndition, trans);
            {
                Assert.That(File.Exists(configFileQuestionCOndition.FullName));
                var test = JsonUtil.CreateInstanceFromJsonConfigFile<ImageListTransformOptions>(configFileQuestionCOndition);
                Assert.AreEqual(selector, test.PrintOptions.PrintFrameSelector);
            }

            var configFileEventCondition = new FileInfo(baseDir + Path.DirectorySeparatorChar + "eventConditionSelector.config");
            selector = new ImageSelectorQuestionnaire<EventCondition>(new HashSet<ConditionFileTuple<EventCondition>>
            {
                new ConditionFileTuple<EventCondition> { File = "event1", Condition = new EventCondition { EventId = 7897} }
            });

            trans.PrintOptions.PrintFrameSelector = selector;

            JsonUtil.WriteJsonFile(configFileEventCondition, trans);
            {
                Assert.That(File.Exists(configFileEventCondition.FullName));
                var test = JsonUtil.CreateInstanceFromJsonConfigFile<ImageListTransformOptions>(configFileEventCondition);
                Assert.AreEqual(selector, test.PrintOptions.PrintFrameSelector);
            }

            #endregion
        }

        [Test]
        public void TestJsonMarshallingCondition()
        {
            var baseDir = GetFullOutputDir();

            var masterConfigFile = new FileInfo(baseDir + Path.DirectorySeparatorChar + "PreviewOverlayInfos.config");
            EnsureFileNotExists(masterConfigFile.FullName);

            var singleConfigFile = new FileInfo(baseDir + Path.DirectorySeparatorChar + "PreviewOverlayInfoSingle.config");
            EnsureFileNotExists(singleConfigFile.FullName);

            var overlays = new[]
            {
                new PreviewOverlayInfo(baseDir + "statue_model_01.png", new Point(0, 0), new Rectangle(163, 72, 567, 378),
                    new QuestionCondition(2, 1)),
                new PreviewOverlayInfo(baseDir + "statue_model_02.png", new Point(0, 0), new Rectangle(0, 0, 750, 500),
                    new QuestionCondition(2, 2)),
                new PreviewOverlayInfo(baseDir + "statue_model_03.png", new Point(0, 0), new Rectangle(94, 71, 561, 357),
                    new QuestionCondition(2, 3)),
                new PreviewOverlayInfo(baseDir + "statue_model_04.png", new Point(0, 0), new Rectangle(146, 95, 586, 391),
                    new EventCondition(700))
            };

            // Sérialisation d'un seul objet.
            Assert.True(JsonUtil.WriteJsonFile(singleConfigFile, overlays[0]));
            {
                Assert.That(File.Exists(singleConfigFile.FullName));
                var test = JsonUtil.ReadJsonFile<PreviewOverlayInfo>(singleConfigFile);
                Assert.AreEqual(overlays[0], test);
            }

            // Serialize -> deserialize de groupe.
            Assert.True(JsonUtil.WriteJsonFile(masterConfigFile, overlays));
            {
                Assert.That(File.Exists(masterConfigFile.FullName));
                var test = JsonUtil.ReadJsonFile<List<PreviewOverlayInfo>>(masterConfigFile);
                CollectionAssert.AreEqual(overlays, test);
            }
        }

        [Test]
        public async Task TestBase64FileConversion()
        {
            var output = GetFullOutputDir();
            EnsureDirEmpty(output);
            var fileToConvert = new FileInfo(BaseDir + "swing_slow_crop_green.MP4");
            EnsureFileExists(fileToConvert.FullName);
            var outputfile = output + "output.base64";
           
            Console.Out.WriteLine("Original file size: " + fileToConvert.Length);

            Assert.IsTrue(await FileUtil.ConvertFiletoBase64(fileToConvert, new FileInfo(outputfile), CancellationToken.None, output));

            var resultFileInfo = new FileInfo(outputfile);
            Assert.IsTrue(resultFileInfo.Exists);

            Console.Out.WriteLine("File size: " + resultFileInfo.Length);

            Assert.GreaterOrEqual(resultFileInfo.Length, fileToConvert.Length);
        }

        [Test]
        public async Task TestBase64BackAndForth()
        {
            var output = GetFullOutputDir();
            EnsureDirEmpty(output);
            var fileToConvert = new FileInfo(BaseDir + "swing_green.mp4");
            EnsureFileExists(fileToConvert.FullName);
            var outputfile = output + "output.base64";
            var outputfileBack = output + "output.mp4";

            // Vers Base64
            Assert.IsTrue(await FileUtil.ConvertFiletoBase64(fileToConvert, new FileInfo(outputfile), CancellationToken.None, output));

            var resultFileInfo = new FileInfo(outputfile);
            Assert.IsTrue(resultFileInfo.Exists);

            // Retour vers original
            Assert.IsTrue(await FileUtil.ConvertFileBackFromBase64(resultFileInfo, new FileInfo(outputfileBack), CancellationToken.None, output));

            var resultFileBack = new FileInfo(outputfileBack);
            Assert.IsTrue(resultFileBack.Exists);

            // Comparaison MD5
            var md5Ori = MD5.Create();
            var oriHash = md5Ori.ComputeHash(new FileStream(fileToConvert.FullName, FileMode.Open));
            var backHash = md5Ori.ComputeHash(new FileStream(resultFileBack.FullName, FileMode.Open));

            Assert.IsTrue(fileToConvert.Length == resultFileBack.Length, "Lenght of original and result file are not the same.");
            Assert.AreEqual(oriHash, backHash, "Hash table not the same.");
        }
    }
}
