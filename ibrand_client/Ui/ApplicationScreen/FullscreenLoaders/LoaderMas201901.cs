using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders
{
    [PublicAPI]
    public class LoaderMas201901 : IFullscreenLoader
    {
        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
        {
            var uiDir = DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none);

            var baseCancel = new Rectangle(1325, 33, 336, 112);

            // 1- Home
            var home = new GenericScreen(uiDir + "1-Accueil.jpg", null, sc)
            {
                DebugWritePropertiesToFiles = false,
                DefinitionLanguageZones = new ScreenDefinitionLanguageZones
                {
                    ConfigFileName = "01_languages.json"
                }
            };
            screensController.AddScreen(home);
            
            //3 - Prepare for Countdown
            var skipTake = new GenericScreen(uiDir + "3-Start.jpg", uiDir + "3-Start_fr.jpg", sc)
            {
                DebugWritePropertiesToFiles = false,
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ConfigFileName = "03_navigation.json"
                        /*
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            Zone = new Rectangle(272,455,1135,141),
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions()
                        },
                        new SectionSkipScreenData
                        {
                            Zone = baseCancel,
                            PageChangeOptions = ScreenChangeOptions.GenerateCancelOptions()
                        }
                    }
                    */
                }
            };
            screensController.AddScreen(skipTake);
            
            // 4- Take picture
            var takePict = new TakePictureAutoScreen(sc, uiDir + "4-Countdown.jpg", uiDir + "4-Countdown_fr.jpg", null)
            {
                CountImageSequence = new AnimatedImageSequence
                {
                    DrawLocation = new Point(733, 614),
                    ImageGenerators = new List<IAnimatedImageSequenceGenerator>()
                    {
                        new AnimatedImageSequenceGeneratorFromText()
                        {
                            ProceduralImageTextInfo = new ProceduralImageTextInfo
                            {
                                Color = Color.Black,
                                FontSize = 300,
                                BitmapSize = new Size(300,400)
                            },
                            Texts = new List<string> {"5", "4", "3", "2", "1"}
                        }
                    }
                },
                ImageListTranformOptions = new ImageListTransformOptions
                {
                    FirstPictureOptions = new ImageListTransformOptionsFirstPicture
                    {
                        NewSize = new Size(825, 1125),
                        SaveDataModel = new DataModelQuestionnaire
                        {
                            Identifier = new QuestionIdentifier(2)
                        },
                    }
                },
                CreatingImageSequence = new AnimatedImageSequence
                {
                    DrawLocation = new Point(733, 614),
                    Loop = true,
                    ImageGenerators = new List<IAnimatedImageSequenceGenerator>()
                    {
                        new AnimatedImageSequenceGeneratorFromText()
                        {
                            ProceduralImageTextInfo = new ProceduralImageTextInfo
                            {
                                Color = Color.White,
                                FontSize = 50,
                                BitmapSize = new Size(600,200),
                                Center = false
                            },
                            TextsLang = new Dictionary<string, IList<string>>
                            {
                                {"fr", new List<string>
                                    {
                                        "Traitement.",
                                        "Traitement..",
                                        "Traitement..."
                                    }
                                },
                                {"en", new List<string>
                                    {
                                        "Working.",
                                        "Working..",
                                        "Working..."
                                    }
                                }
                            }
                        }
                    }
                }
            };
            screensController.AddScreen(takePict);

            // 5- Écran de prévisualisation
            var preview = new PreviewScreen(uiDir + "5-Preview.jpg", uiDir + "5-Preview_fr.jpg", sc, baseCancel, 
                new Rectangle(1090, 900, 378, 75), // next
                new Rectangle(230, 900, 378, 75), // back
                85, new Size(550, 711) 
                )
            {
                
            };
            screensController.AddScreen(preview);

            // 6- Thanks Bye!
            var thxSCreen = new ThanksEndScreen(uiDir + "6-Thanks.jpg", sc, uiDir + "6-Thanks_fr.jpg")
            {
            };
            screensController.AddScreen(thxSCreen);
        }
    }
}