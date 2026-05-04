using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Condition;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom._360;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders
{
    [PublicAPI]
    public class Loader360201904 : IFullscreenLoader
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
        {
            var uiDir = DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none);
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId);

            var configFile = new FileInfo(baseDir + "terminal.json");

            if (!configFile.Exists)
            {
                // On réutilise celui de batimo. Because why not. Mais un fichier de terminal plus générique serait mieux éventuellement.
                var config = new AlphaGeneTerminal();
                JsonUtil.WriteJsonFile(configFile, config);
                configFile.Refresh();
            }

            var terminalConfig = JsonUtil.ReadJsonFile<AlphaGeneTerminal>(configFile);

            var boxTextSize = 20;

            #region Start Screen

            var startSCreen = new GenericScreen(uiDir + "1_Accueil.jpg", uiDir + "1_Accueil.jpg", sc)
            {
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            Zone = new Rectangle(0, 0, 1280, 800)
                        }
                    }
                }
            };
            screensController.AddScreen(startSCreen);

            #endregion

            #region Questionnaire A

            var bsts = new GenericScreen(uiDir + "2_questionnaire A.jpg", uiDir + "2_questionnaire A.jpg", sc)
            {
                DefinitionTextbox = new ScreenDefinitionTextBoxes
                {
                    DefaultFontDefinition = new FontDefinitionInfo
                    {
                        Size = boxTextSize
                    },
                    TextboxDefinitions = new List<TextboxDefinition>
                    {
                        new TextboxDefinition
                        {
                            Position = new Rectangle(400, 61, 541, 26),
                            AllowEmpty = false,
                            Mandatory = true,
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(1)
                            }},
                            FieldName = "Nom",
                            Modifications = TextModificationsEnum.Trim,
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                { UserErrorEnum.Mandatory, new Dictionary<string, string>
                                    {
                                        { "fr", "Vous devez indiquez votre nom." }
                                    }
                                }
                            }
                        },
                        new TextboxDefinition
                        {
                            Position = new Rectangle(400, 122, 541, 26),
                            AllowEmpty = false,
                            Mandatory = true,
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(2)
                            }},
                            FieldName = "Courriel",
                            Modifications = TextModificationsEnum.Trim,
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Email
                            },
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                { UserErrorEnum.Mandatory, new Dictionary<string, string>
                                    {
                                        { "fr", "Vous devez indiquez votre courriel." }
                                    }
                                },
                                { UserErrorEnum.Validation, new Dictionary<string, string>
                                    {
                                        { "fr", "Format de courriel invalide." }
                                    }
                                }
                            }
                        },
                        new TextboxDefinition
                        {
                            Position = new Rectangle(400, 185, 541, 26),
                            AllowEmpty = false,
                            Mandatory = true,
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(3)
                            }},
                            FieldName = "Téléphone",
                            Modifications = TextModificationsEnum.Trim,
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Sms
                            },
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                { UserErrorEnum.Mandatory, new Dictionary<string, string>
                                    {
                                        { "fr", "Vous devez indiquez votre numéro de téléphone." }
                                    }
                                },
                                { UserErrorEnum.Validation, new Dictionary<string, string>
                                    {
                                        { "fr", "Format de téléphone invalide." }
                                    }
                                }
                            }
                        },
                        new TextboxDefinition
                        {
                            Position = new Rectangle(400, 249, 541, 26),
                            AllowEmpty = false,
                            Mandatory = true,
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(4)
                            }},
                            FieldName = "organisation",
                            Modifications = TextModificationsEnum.Trim,
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                { UserErrorEnum.Mandatory, new Dictionary<string, string>
                                    {
                                        { "fr", "Vous devez indiquez le nom de votre organisation ou entreprise." }
                                    }
                                }
                            }
                        }
                    }
                },
                DefinitionComboxbox = new ScreenDefinitionComboBoxes
                {
                    DefaultFontDefinition = new FontDefinitionInfo
                    {
                        Size = boxTextSize
                    },
                    ComboboxDefinitions = new List<ComboboxDefinition>
                    {
                        new ComboboxDefinition
                        {
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(5)
                            }},
                            Mandatory = true,
                            Position = new Rectangle(398, 313, 541, 26),
                            FieldName = "Secteur",
                            TextValues = new Dictionary<string, IList<Tuple<string, string>>>
                            {
                                {"fr", new List<Tuple<string, string>>
                                {
                                    new Tuple<string, string>("Étudiant", null)
                                    ,new Tuple<string, string>("Producteur", null)
                                    ,new Tuple<string, string>("Employé de ferme", null)
                                    ,new Tuple<string, string>("Intervenant", null)
                                    ,new Tuple<string, string>("Autre", null)
                                }}

                            },
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                { UserErrorEnum.Mandatory, new Dictionary<string, string>
                                    {
                                        { "fr", "Vous devez indiquez votre secteur d'entreprise." }
                                    }
                                }
                            }
                        }
                    }
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateCancelOptions(),
                            Zone = new Rectangle(1180, 0, 100, 100),
                            SkipValidation = true
                        },
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = new ScreenChangeOptions
                            {
                                Options = new List<ScreenChangeOption>
                                {
                                    new ScreenChangeOption
                                    {
                                        ValidationTrigger = true,
                                        Step = 1,
                                        Condition = new QuestionCondition
                                        {
                                            QuestionNo = 5,
                                            ChoiceNo = 1,
                                            //SubChoiceNo = 2
                                        }
                                    },
                                    new ScreenChangeOption
                                    {
                                        ValidationTrigger = true,
                                        Step = 2,
                                        Condition = new QuestionCondition
                                        {
                                            QuestionNo = 5,
                                            ChoiceNo = 1,
                                            //SubChoiceNo = 2,
                                            Not = true
                                        }
                                    }
                                }
                            },
                            Zone = new Rectangle(519, 713, 241, 68)
                        },
                        
                    }
                },
                DefinitionKeyboard = new ScreenDefinitionKeyboard
                {
                    Position = new Rectangle(-1, 365, 1129, 335),
                    KeyboardType = KeyboardTypeEnum.Multilingual,
                    FontDefinition = new FontDefinitionInfo
                    {
                        Size = 30
                    }
                }
            };
            screensController.AddScreen(bsts);

            #endregion
            
            #region Questionnaire B

            var qB = new GenericScreen(uiDir + "3_questionnaire B.jpg", uiDir + "3_questionnaire B.jpg", sc)
            {
                DefinitionTextbox = new ScreenDefinitionTextBoxes
                {
                    DefaultFontDefinition = new FontDefinitionInfo
                    {
                        Size = boxTextSize
                    },
                    TextboxDefinitions = new List<TextboxDefinition>
                    {
                        new TextboxDefinition
                        {
                            Position = new Rectangle(470, 187, 479, 30),
                            AllowEmpty = false,
                            Mandatory = true,
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(11)
                            }},
                            FieldName = "Nbr animeaux",
                            Modifications = TextModificationsEnum.Trim,
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Integer
                            },
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                { UserErrorEnum.Mandatory, new Dictionary<string, string>
                                    {
                                        { "fr", "Vous devez indiquez le nombre d'animeaux que vous prenez soin. Zéro est acceptable." }
                                    }
                                },
                                { UserErrorEnum.Validation, new Dictionary<string, string>
                                    {
                                        { "fr", "Vous devez entrez un nombre entier." }
                                    }
                                }
                            }
                        }
                    }
                },
                DefinitionComboxbox = new ScreenDefinitionComboBoxes
                {
                    DefaultFontDefinition = new FontDefinitionInfo
                    {
                        Size = boxTextSize
                    },
                    ComboboxDefinitions = new List<ComboboxDefinition>
                    {
                        new ComboboxDefinition
                        {
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(6)
                            }},
                            Mandatory = true,
                            Position = new Rectangle(470, 123, 479, 25),
                            FieldName = "Type prod",
                            TextValues = new Dictionary<string, IList<Tuple<string, string>>>
                            {
                                {"fr", new List<Tuple<string, string>>
                                {
                                    new Tuple<string, string>("Naisseur", null)
                                    ,new Tuple<string, string>("Naisseur-pouponnière", null)
                                    ,new Tuple<string, string>("Naisseur-finisseur", null)
                                    ,new Tuple<string, string>("Pouponnière", null)
                                    ,new Tuple<string, string>("Pouponnière-Engrais", null)
                                    ,new Tuple<string, string>("Engraissement", null)
                                    ,new Tuple<string, string>("Autre", null)
                                }}

                            },
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                { UserErrorEnum.Mandatory, new Dictionary<string, string>
                                    {
                                        { "fr", "Vous devez indiquez votre type de production animale." }
                                    }
                                }
                            }
                        },
                        new ComboboxDefinition
                        {
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(7)
                            }},
                            Mandatory = true,
                            Position = new Rectangle(470, 248, 479, 24),
                            FieldName = "Gen mâle",
                            TextValues = new Dictionary<string, IList<Tuple<string, string>>>
                            {
                                {"fr", new List<Tuple<string, string>>
                                {
                                    new Tuple<string, string>("AlphaGene", null)
                                    ,new Tuple<string, string>("PIC", null)
                                    ,new Tuple<string, string>("DNA", null)
                                    ,new Tuple<string, string>("Fast", null)
                                    ,new Tuple<string, string>("Hypor", null)
                                    ,new Tuple<string, string>("Autre", null)
                                }}

                            },
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                { UserErrorEnum.Mandatory, new Dictionary<string, string>
                                    {
                                        { "fr", "Vous devez indiquez votre type de génétique (mâle)." }
                                    }
                                }
                            }
                        },
                        new ComboboxDefinition
                        {
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(8)
                            }},
                            Mandatory = true,
                            Position = new Rectangle(470, 311, 479, 24),
                            FieldName = "Gen femelle",
                            TextValues = new Dictionary<string, IList<Tuple<string, string>>>
                            {
                                {"fr", new List<Tuple<string, string>>
                                {
                                    new Tuple<string, string>("AlphaGene", null)
                                    ,new Tuple<string, string>("PIC", null)
                                    ,new Tuple<string, string>("DNA", null)
                                    ,new Tuple<string, string>("Fast", null)
                                    ,new Tuple<string, string>("Hypor", null)
                                    ,new Tuple<string, string>("Autre", null)
                                }}

                            },
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                { UserErrorEnum.Mandatory, new Dictionary<string, string>
                                    {
                                        { "fr", "Vous devez indiquez votre type de génétique (femelle)." }
                                    }
                                }
                            }
                        }
                    }
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateCancelOptions(),
                            Zone = new Rectangle(1180, 0, 100, 100),
                            SkipValidation = true
                        },
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = new ScreenChangeOptions
                            {
                                Options = new List<ScreenChangeOption>
                                {
                                    new ScreenChangeOption
                                    {
                                        ValidationTrigger = true,
                                        Step = 1
                                    }
                                }
                            },
                            Zone = new Rectangle(519, 713, 241, 68)
                        },
                    }
                },
                DefinitionKeyboard = new ScreenDefinitionKeyboard
                {
                    Position = new Rectangle(-1, 353, 442, 339),
                    KeyboardType = KeyboardTypeEnum.Number,
                    FontDefinition = new FontDefinitionInfo
                    {
                        Size = 30
                    }
                }
            };
            screensController.AddScreen(qB);

            #endregion
            
            #region Scan et concours

            log.Info("360-2019-04: Concours chargé avec l'identifiant suivant: " + terminalConfig.TabletteId);
            var debugMode = terminalConfig.TabletteId == 0;
            string tabFileName = baseDir + $"tablette_{(debugMode ? "test" : terminalConfig.TabletteId.ToString())}.json";
            string sweepFileName = baseDir + $"sweepListE{(debugMode ? "_test" : "")}.json";

            var scanSweep = new RfidScanAlphaGene(uiDir + "4_Scan.jpg", uiDir + "4_Scan.jpg", sc, new Rectangle(500, 380, 293, 37))
            {
                ScanQuestionNo = 9,
                SweepstakesIdsFile = sweepFileName,
                SweepstakesDefinitionFile = tabFileName,
                SweepstakesBlockId = terminalConfig.TabletteId,
                NextLocation = new Rectangle(348, 283, 573, 42),
                SweepstakeCategoryConditions = new List<SweepStakesConditions>
                {
                    new SweepStakesConditions
                    {
                        Category = 'A',
                        Condition = new MultipleCondition()
                        {
                            Operator = BooleanOperatorEnum.AND,
                            Conditions = new List<ICondition>
                            {
                                new QuestionCondition
                                {
                                    QuestionNo = 5,
                                    ChoiceNo = 1,
                                    //SubChoiceNo = 2
                                },
                                new MultipleCondition()
                                {
                                    Operator = BooleanOperatorEnum.OR,
                                    Conditions = new List<ICondition>
                                    {
                                        new QuestionCondition
                                        {
                                            QuestionNo = 6,
                                            ChoiceNo = 1,
                                            //SubChoiceNo = 1
                                        },
                                        new QuestionCondition
                                        {
                                            QuestionNo = 6,
                                            ChoiceNo = 1,
                                            //SubChoiceNo = 2
                                        },
                                        new QuestionCondition
                                        {
                                            QuestionNo = 6,
                                            ChoiceNo = 1,
                                            //SubChoiceNo = 3
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new SweepStakesConditions
                    {
                        Category = 'B',
                        Condition = new QuestionCondition
                        {
                            QuestionNo = 5,
                            ChoiceNo = 1,
                            //SubChoiceNo = 2
                        }
                    },
                    new SweepStakesConditions
                    {
                        Category = 'C',
                        Condition = new QuestionCondition
                        {
                            QuestionNo = 5,
                            ChoiceNo = 1,
                            //SubChoiceNo = 2
                        }
                    },
                    new SweepStakesConditions
                    {
                        Category = 'D',
                        Condition = new QuestionCondition
                        {
                            QuestionNo = 5,
                            ChoiceNo = 1,
                            //SubChoiceNo = 1
                        }
                    }
                }
            };
            screensController.AddScreen(scanSweep);
            #endregion

        }
    }
}