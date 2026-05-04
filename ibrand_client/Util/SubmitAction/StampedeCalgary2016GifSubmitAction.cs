using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Ionic.Zip;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker;
using NomadLogic.Multimedia.Gif;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    public class StampedeCalgary2016GifSubmitAction : AbstractSubmitActionSendFilesToPrintstation
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("transformation_proshot", 1);
            AddSubmitActionWorkerPool("send_print_proshot", 1);
        }

        public override void TransformNotReadyToSyncRespondents(CancellationToken? ct)
        {
            var db = ServicesContainer.GetService<IbrandDatabase>();
            var respondentList = db.GetRespondentsBySyncStatus(Convert.ToString(ProjectId));

            UiSynchro?.SetProgressBarMinMax(0, respondentList.Count);

            foreach (var id in respondentList)
            {
                if (ct != null && ct.Value.IsCancellationRequested) break;

                var resp = db.GetRespondent(id);
                resp.Answers = db.GetAnswers(id);

                var worker = new StampedeCalgary2016GifSubmitActionWorker(this, resp, id, null);

                worker.DoWork(ct);

                var worker2 = worker.SecondWorkerStep;
                if (worker2 != null)
                {
                    foreach (var subWorker in worker2)
                    {
                        subWorker.DoWork(ct);
                    }
                }

                UiSynchro?.AddProgressStep(1);
            }
        }

        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new StampedeCalgary2016GifSubmitActionWorker(this, resp, resp.Id, _submitActionWorkerPools[1]);
        }

        private struct FileKeyMapping
        {
            public FileKeyMapping(int index, Dictionary<int, int> map)
            {
                TabIndex = index;
                FileMapper = map;
            }

            public readonly int TabIndex;
            public readonly Dictionary<int, int> FileMapper;
        }

        // ReSharper disable once InconsistentNaming
        private class StampedeCalgary2016GifSubmitActionWorker : SubmitActionWorkerAbstract<StampedeCalgary2016GifSubmitAction>
        {
            private readonly SubmitActionWorkerPool _secondPool;

            public SendFilesToPrintstationWorker[] SecondWorkerStep { get; private set; }

            public StampedeCalgary2016GifSubmitActionWorker(StampedeCalgary2016GifSubmitAction action, Respondent resp,
                int respId, SubmitActionWorkerPool secondPool) : base(action, resp, respId)
            {
                // Rien de plus
                _secondPool = secondPool;
            }

            public override void DoWork(CancellationToken? ct)
            {
                if (ct?.IsCancellationRequested ?? false) return;

                if (_action._stopped) return;

                try
                {
                    var db = ServicesContainer.GetService<IbrandDatabase>();

                    // ReSharper disable once SimplifyLinqExpression
                    if (!_respondent.Answers.Any(a => a.QuestionNo == 1)) // Si on a répondu à la question 1, c'est un répondant modifié.
                    {
                        var newResps = new Respondent[4];

                        /* Étape 1 - Assignation des images aux répondants, et création des chemins de fichiers pour tous ce beau monde. */
                        #region Init Variables

                        var dirBundleOriginal = _respondent.GetAnswerText(4);
                        var indexWinner = int.Parse(_respondent.GetAnswerText(5)) - 1;
                        var filesOriginal = new[]
                        {
                            new string[4],
                            new string[4],
                            new string[4],
                            new string[4]
                        };
                        var filesModified = new[]
                        {
                            new string[4],
                            new string[4],
                            new string[4],
                            new string[4]
                        };
                        var baseDirQuest = DirectoryUtil.GetLocalIbrandTerminalDir(_respondent.ProjectId, _respondent.QuestionnaireId, _respondent.TerminalId);
                        var baseDirTransPicts = DirectoryUtil.GetLocalIbrandTerminalDataDir(_respondent.ProjectId,
                            _respondent.QuestionnaireId, _respondent.TerminalId,
                            DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                            DirectoryUtil.ProjectDataDirectoriesModEnum.transformed);
                        var newDirBundles = new[]
                        {
                            new DirectoryInfo(DirectoryUtil.GetLocalApplicationWorkDir() + GenerateUniqueId.GetId()),
                            new DirectoryInfo(DirectoryUtil.GetLocalApplicationWorkDir() + GenerateUniqueId.GetId()),
                            new DirectoryInfo(DirectoryUtil.GetLocalApplicationWorkDir() + GenerateUniqueId.GetId()),
                            new DirectoryInfo(DirectoryUtil.GetLocalApplicationWorkDir() + GenerateUniqueId.GetId())
                        };
                        var filesGif = new[]
                        {
                            baseDirTransPicts + GenerateUniqueId.GetId() + ".gif",
                            baseDirTransPicts + GenerateUniqueId.GetId() + ".gif",
                            baseDirTransPicts + GenerateUniqueId.GetId() + ".gif",
                            baseDirTransPicts + GenerateUniqueId.GetId() + ".gif"
                        };
                        var filesFirstPic = new[]
                        {
                            baseDirTransPicts + GenerateUniqueId.GetId() + ".jpg",
                            baseDirTransPicts + GenerateUniqueId.GetId() + ".jpg",
                            baseDirTransPicts + GenerateUniqueId.GetId() + ".jpg",
                            baseDirTransPicts + GenerateUniqueId.GetId() + ".jpg"
                        };
                        var filesZip = new[]
                        {
                            baseDirTransPicts + GenerateUniqueId.GetId() + ".zip",
                            baseDirTransPicts + GenerateUniqueId.GetId() + ".zip",
                            baseDirTransPicts + GenerateUniqueId.GetId() + ".zip",
                            baseDirTransPicts + GenerateUniqueId.GetId() + ".zip"
                        };
                        foreach (var dir in newDirBundles)
                        {
                            dir.Create();
                        }

                        // Comme l'"Author" des caméra peut être n'importe quelle valeur, on doit "mapper" ces valeurs à des indices de tableau valides,
                        // ou on risque de créer des "ArrayOutOfBoundException" à la pelle.
                        // Même chose pour les fichiers, ou il peut manquer des numéro dans les séquences.
                        var keyMapCam = new Dictionary<int, FileKeyMapping>();

                        // On assigne chacune des images du répertoire à un tableau qui représente les quatres répondants et leur quatres images.
                        // Un tableau similaire est créé avec des fichiers en attente pour les versions modifiées pour les bundles uniques à chaque nouveau répondant.
                        foreach (var file in Directory.EnumerateFiles(dirBundleOriginal))
                        {
                            var tmp = StringUtil.ExtractUniqueIdsFromFileName(StringUtil.GetFileName(file));

                            var info = new[] { int.Parse(tmp[0]), int.Parse(tmp[1])};

                            // Va se remplir à partir de la valeur zéro.
                            if (!keyMapCam.ContainsKey(info[0])) keyMapCam.Add(info[0], new FileKeyMapping(keyMapCam.Count, new Dictionary<int, int>()));

                            var keyFile = keyMapCam[info[0]];
                            if (keyFile.FileMapper.Count >= 4) continue; // Déjà quatre fichiers associés à la caméra, on rejete les autres.

                            if (!keyFile.FileMapper.ContainsKey(info[1])) keyFile.FileMapper.Add(info[1], keyFile.FileMapper.Count);

                            var newFile = new FileInfo(newDirBundles[keyFile.TabIndex].FullName + Path.DirectorySeparatorChar +
                                                       string.Format(CultureInfo.InvariantCulture, "{000}", info[1]) + ".jpg");

                            var fileIndex = keyFile.FileMapper[info[1]];
                            filesOriginal[keyFile.TabIndex][fileIndex] = file;
                            filesModified[keyFile.TabIndex][fileIndex] = newFile.FullName; // Sera créé éventuellement.
                        }
                        #endregion

                        /* Étape 2 - Création de l'image display du gagnant. */

                        #region Create Winner Display Image
                        
                        using (var originalFile = FileUtil.LoadBitmap(filesOriginal[indexWinner][3]))
                        {
                            using (var resized = originalFile.Resize(384, 256))
                            {
                                using (var overlayBitmap = FileUtil.LoadBitmap(baseDirQuest + "d5.png"))
                                {
                                    resized.AddOverlay(overlayBitmap);

                                    // On sauvegarde le fichier bitmap modifié dans le répertoire approprié.
                                    resized.Save(90, DirectoryUtil.GetLocalShowPicturesOnTvPicturesDir() + GenerateUniqueId.GetId() + ".jpg");
                                }
                            }
                        }

                        #endregion

                        /* Étape 3 - Création des GIFs et images modifiées de tous. */
                        #region Create files Losers

                        for (var i = 0; i < filesOriginal.Length; i++)
                        {
                            var isWinner = i == indexWinner;

                            // Création du gif principal
                            using (var a = new AnimatedGifEncoder())
                            {
                                a.Start(filesGif[i]);
                                a.SetDelay(1000);
                                a.SetRepeat(0);
                                a.SetQuality(255);

                                using (var splash = FileUtil.LoadBitmap(baseDirQuest + "1.jpg"))
                                {
                                    using (var reduced = splash.Resize(480, 480))
                                    {
                                        a.AddFrame(reduced);
                                    }
                                }
                                // Copie du fichier en tant qu'image de base pour le bundle printstation.
                                File.Copy(baseDirQuest + "1.jpg", newDirBundles[i].FullName + Path.DirectorySeparatorChar + "0.jpg");

                                for (var j = 0; j < filesOriginal[i].Length; j++)
                                {

                                    var file = filesOriginal[i][j];
                                    if (file == null) continue;

                                    using (var image = FileUtil.LoadBitmap(filesOriginal[i][j]))
                                    {
                                        using (var b = image.Frame(1080, 1080))
                                        {
                                            string fileOverlay = null;
                                            switch (j)
                                            {
                                                case 0:
                                                    fileOverlay = "2.png";
                                                    break;
                                                case 1:
                                                    fileOverlay = "3.png";
                                                    break;
                                                case 2:
                                                    fileOverlay = "4.png";
                                                    break;
                                                case 3:
                                                    fileOverlay = isWinner ? "5.png" : "3.png";
                                                    break;
                                            }

                                            if (fileOverlay == null)
                                            {
                                                throw new SystemException("Unknown index for file overlay: " + j);
                                            }

                                            using (var overlayBitmap = FileUtil.LoadBitmap(baseDirQuest + fileOverlay))
                                            {
                                                b.AddOverlay(overlayBitmap);

                                                // On sauvegarde le fichier bitmap modifié dans le répertoire approprié.
                                                b.Save(90, filesModified[i][j]);

                                                using (var reduced = b.Resize(480, 480))
                                                {
                                                    a.AddFrame(reduced);
                                                }
                                            }
                                        }
                                    }
                                }
                                a.Finish();
                            }
                        }

                        // On génère les "First Pictures" (copie avec un nom différent).
                        for (var i = 0; i < filesModified.Length; i++)
                        {
                            File.Copy(filesModified[i][1], filesFirstPic[i]);
                        }

                        // On génère les fichiers Zips
                        for (var i = 0; i < newDirBundles.Length; i++)
                        {
                            var zip = new ZipFile();

                            foreach (var file in newDirBundles[i].EnumerateFiles())
                            {
                                zip.AddFile(file.FullName, "");
                            }
                            zip.Save(filesZip[i]);
                        }

                        #endregion
                        
                        /* Étape 4 - Assignation des réponses. */
                        #region Assign Answers

                        for (var i = 0; i < newResps.Length; i++)
                        {
                            if (i != indexWinner) // Traitement des perdants
                            {
                                // Répondant par copie sommaire (pas profonde)
                                newResps[i] = new Respondent(_respondent);
                                // On créé les réponses à main. Question 1 à 4
                                var answerLoser = new List<Answer>
                                {
//                                    new Answer(QuestionTypeEnum.Combo, 1, 1, 2), // Question indiquant si on est le gagnant.
//                                    new Answer(QuestionTypeEnum.PhotoText, 2, 1, filesFirstPic[i]),
//                                    // Question première image (n'est pas celle qui est similaire pour tous)
//                                    new Answer(QuestionTypeEnum.PhotoText, 3, 1, filesGif[i]), // Question GIF
//                                    new Answer(QuestionTypeEnum.PhotoBundle, 4, 1, filesZip[i])
                                };
                                newResps[i].Answers = answerLoser;

                                var respId = db.InsertRespondent(newResps[i], false);
                                newResps[i].Id = respId;
                            }
                            else // Traitement du gagnant (on modifie le répondant original)
                            {
                                /*newResps[i] = _respondent;
                                // On enlève la question 5
                                var questFive = _respondent.Answers.Find(a => a.QuestionNo == 5);
                                _respondent.Answers.Remove(questFive);
                                db.DeleteAnswer(_respondent.IdInt, questFive);
                                // On remplace la question 4 (elle contenait le bundle original)
                                var questFour = _respondent.Answers.Find(a => a.QuestionNo == 4);
                                questFour.Response = filesZip[i];
                                questFour.QuestionType = QuestionTypeEnum.PhotoBundle;
                                db.DeleteAnswer(_respondent.IdInt, questFour);
                                db.InsertAnswer(_respondent.IdInt, questFour);
                                // On ajoute les questions 1 à 3
                                var questOne = new Answer(QuestionTypeEnum.Video, 1, 1, 1);
                                _respondent.Answers.Add(questOne);
                                db.InsertAnswer(_respondent.IdInt, questOne);
                                var questTwo = new Answer(QuestionTypeEnum.PhotoText, 2, 1, filesFirstPic[i]);
                                _respondent.Answers.Add(questTwo);
                                db.InsertAnswer(_respondent.IdInt, questTwo);
                                var questThree = new Answer(QuestionTypeEnum.PhotoText, 3, 1, filesGif[i]);
                                _respondent.Answers.Add(questThree);
                                db.InsertAnswer(_respondent.IdInt, questThree);*/
                            }
                        }

                        #endregion

                        // Attention! Il ne faut pas que ces répondants soient prêts à être synchronisés!
                        // Aussi, un des répondant doit être la version corrigé du répondant initial (celui qui est splitté).
                        SecondWorkerStep = new[]
                        {
                            new SendFilesToPrintstationWorker(_action, newResps[0], newResps[0].Id),
                            new SendFilesToPrintstationWorker(_action, newResps[1], newResps[1].Id),
                            new SendFilesToPrintstationWorker(_action, newResps[2], newResps[2].Id),
                            new SendFilesToPrintstationWorker(_action, newResps[3], newResps[3].Id)
                        };

                        // On chaîne maintenant la deuxième partie de la transformation dans le deuxième bassin. Si ce bassin existe.
                        foreach (var worker in SecondWorkerStep)
                        {
                            _secondPool?.AddWorker(worker);
                        }

                        foreach (var resp in newResps)
                        {
                            db.SetRespondentReadyToSync(resp.IdInt);
                        }
                    }
                    else
                    {
                        // Il s'agit d'un répondant déjà modifié, et on fait juste la deuxième étape.
                        SecondWorkerStep = new[]
                        {
                            new SendFilesToPrintstationWorker(_action, _respondent, _respId)
                        };

                        _secondPool?.AddWorker(SecondWorkerStep[0]);
                    }
                }
                catch (System.Exception ex)
                {
                    var str = "Error while doing submit action for respondent: " + _respId;
                    log.Fatal(ex, str);
                    _action.UiSynchro?.AddUiMessage(str, 4);
                }
                finally
                {
                    _action.OnWorkFinished(_respId);
                }
            }

            protected override ISubmitActionWorker NextWorkerInstance(SubmitActionWorkerNextStepInfo next)
            {
                throw new NotImplementedException();
            }

            protected override void DoWorkExecute(IbrandDatabase db, CancellationToken? ct)
            {
                throw new NotImplementedException();
            }
        }

    }
}
