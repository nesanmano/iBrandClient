using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Math;

namespace NomadLogic.Multimedia.Cli.ImageMagickCLI
{
    /// <summary>
    /// Regroupement des méthodes de création, d'exécution et d'analyse de commandes par ligne de commande système, pour ImageMagick.
    /// Contient également des méthodes pratiques pour analyser des fichiers d'images ou des commandes fréquentes, sans passer par la
    /// classe ImageMagickCliManager, qui est utile pour des scripts réutilisables.
    /// Il est possible de configurer et d'exécuter un script de commandes d'ImageMagick uniquement à partir de cette classe, au besoin.
    /// </summary>
    [PublicAPI]
    public static class ImageMagickCliUtil
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private const string REGEX_IM_VERSION = @"[0-9]+\.[0-9]+\.[0-9]+-([0-9]+) Q[0-9]+";
        private const string REGEX_FILE_GEOMETRY = @"[0-9]+x[0-9]+";

        /// <summary>
        /// Obtient la taille d'une image sur disque.
        /// Retourne faux si ImageMagick n'a pas été capable d'exécuter, ou si le fichier est invalide, illisible ou n'est pas une image selon ImageMagick.
        /// Met la taille de l'image dans le paramètre de sortie si lu correctement.
        /// </summary>
        /// <param name="file">Le fichier d'image à analyser.</param>
        /// <param name="imageDimensions">Les dimension de l'image en sortie.</param>
        /// <param name="executablePath">Un chemin spécifique vers l'exécutable. Si null, tente de lire le chemin de l'exécutable via la configuration
        /// NomadLogic de base.</param>
        /// <returns>TRUE si aucun problème lors de l'activation de la ligne de commande.</returns>
        [Obsolete("Utilisez la version asynchrone si possible.")]
        public static bool GetImageDimensions(FileInfo file, out Size imageDimensions, string executablePath = null)
        {
            var result = GetImageDimensionsAsync(CancellationToken.None, file, executablePath).GetAwaiter().GetResult();

            imageDimensions = result ?? new Size(-1, -1);

            return result != null;
        }

        /// <summary>
        /// Cette méthode créer un nouveau fichier d'image qui est une copie d'un fichier d'image, redimensionnée.
        /// Il est possible de ré-encoder dans un format d'image différent et à un endroit précis. Mais on ne peut
        /// pas donner de nom d'image précis. La méthode tente de ne pas créer de collision avec des fichiers existants
        /// en décidant du nom à donner.
        /// SI on ne fournir pas d'encodeur précis via l'extension, le format sera toujours PNG au maximum de compression (9).
        /// </summary>
        /// <param name="originalImageFile">La référence du fichier d'image original. Requis.</param>
        /// <param name="desiredSize">La nouvelle taille voulue. Si le ratio n'est pas pareil, un élaguage surviendra. Requis.</param>
        /// <param name="targetDirectory">Optionel. Le répertoire où envoyer le fichier. Si omis, sera créer dans un répertoire temporaire.</param>
        /// <param name="extension">L'extension voulue de la copie. Si omis, on utilise celle du fichier original. Cette valeur va influencer l'encodage.</param>
        /// <param name="compressionLevel">Le niveau de compression. Varie selon l'encodeur.</param>
        /// <returns>NULL si une erreur s'est produite, sinon une référence vers le fichier créé.</returns>
        public static FileInfo ResizeAndCopyImageFile(FileInfo originalImageFile, Size desiredSize, 
            DirectoryInfo targetDirectory = null, string extension = null, int? compressionLevel = null)
        {
            if (originalImageFile == null || !originalImageFile.Exists)
            {
                log.Warn("Original image file does not exists: " + (originalImageFile?.FullName ?? "null"));
                return null;
            }
            
            var dimensions = GetImageDimensionsAsync(CancellationToken.None, originalImageFile).GetAwaiter().GetResult();

            if (desiredSize.Equals(Size.Empty) || dimensions == null || dimensions.Equals(Size.Empty))
            {
                log.Error("Cannot read image dimensions. Cannot proceed with resize print. Desired size={0}", desiredSize);
                return null;
            }
            var dim = dimensions.Value;

            var finalSize = desiredSize;

            // Détection d'orientation
            if (dim.Height > dim.Width)
            {
                // Portrait
                if (finalSize.Width > finalSize.Height)
                {
                    finalSize = new Size(desiredSize.Height, desiredSize.Width);
                }
            }
            else
            {
                // Landscape ou carré
                if (finalSize.Width < finalSize.Height)
                {
                    finalSize = new Size(desiredSize.Height, desiredSize.Width);
                }
            }

            FileInfo modFile = null;
            
            if (log.IsTraceEnabled)
            {
                var ratioOri = dim.Width / dim.Height;
                var ratioDesi = finalSize.Width / finalSize.Height;
                log.Trace($"Resizing image, from {dim} ({ratioOri:#.###}) to {finalSize} ({ratioDesi:#.###})");
            }

            // Todo Il faudrait utiliser la librarie de Image Magick ici plutôt que une référence à la librarie de de .NET ici.
            try
            {
                using (var bmp = FileUtil.LoadBitmap(originalImageFile.FullName))
                {
                    using (var bmPfinal = bmp.Frame(finalSize.Width, finalSize.Height))
                    {
                        var path = DirectoryUtil.CreateLocalTemporaryFile(
                            $"{GenerateUniqueId.GetId()}.{(extension ?? "png").ToLowerInvariant()}", 
                            targetDirectory?.FullName); // Peut être null.
                        bmPfinal.Save(compressionLevel ?? 9, path);
                        modFile = new FileInfo(path);
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Error during file modification or save.");
            }

            return modFile;
        }

        /// <summary>
        /// Obtient la taille d'une image sur disque.
        /// Retourne faux si ImageMagick n'a pas été capable d'exécuter, ou si le fichier est invalide, illisible ou n'est pas une image selon ImageMagick.
        /// Met la taille de l'image dans le paramètre de sortie si lu correctement.
        /// </summary>
        /// <param name="ct">Le jeton de cancellation asynchrone.</param>
        /// <param name="file">Le fichier d'image à analyser.</param>
        /// <param name="executablePath">Un chemin spécifique vers l'exécutable. Si null, tente de lire le chemin de l'exécutable via la configuration
        /// NomadLogic de base.</param>
        /// <returns>Retourne une référence non nulle si la lecture des dimensions a réussie.</returns>
        public static async Task<Size?> GetImageDimensionsAsync(CancellationToken ct, FileInfo file, string executablePath = null)
        {
            // https://imagemagick.org/script/escape.php
            // https://imagemagick.org/script/identify.php

            if (file == null || !file.Exists)
            {
                return null;
            }

            if (executablePath == null )
            {
                var nomadConfig = ServicesContainer.GetService<NomadConfiguration>();
                if (nomadConfig == null)
                {
                    return null;
                }

                if (!File.Exists(nomadConfig.ImageMagickLocation))
                {
                    log.Warn("The imagemagick executable, found in the configuration, does not exists: " + nomadConfig.ImageMagickLocation);
                    return null;
                }

                executablePath = nomadConfig.ImageMagickLocation;
            }

            // Vérification de la taille de l'image.
            var tmp = new FileInfo(DirectoryUtil.CreateLocalTemporaryFile());

            await LaunchProcessUtil.LaunchProcessAsync(ct, executablePath, $"identify -format \"%wx%h\" \"{file.FullName}\"", tmp, null);

            tmp.Refresh();
            if (!tmp.Exists)
            {
                return null;
            }

            var lines = File.ReadAllText(tmp.FullName);
            log.Trace("Image Magick image file metadata call: {0}", lines);

            // devrait être quelque chose comme "1920x1080"
            var match = new Regex(REGEX_FILE_GEOMETRY).Match(lines);
            if (!match.Success)
            {
                return null;
            }

            var dims = match.Value.Split('x');
            if (dims.Length < 2 || !(int.TryParse(dims[0], out var x) && int.TryParse(dims[1], out var y)))
            {
                return null;
            }

            return new Size(x, y);
        }

        /// <summary>
        /// Vérifie que l'exécutable de ImageMagick existe bel et bien.
        /// </summary>
        /// <param name="nomadConfig">La configuration de Nomad Logic qui contient la référence vers l'exécutable.</param>
        /// <param name="minVersion">Optionel. La version minimale de ImageMagick que doit avoir l'exécutable.</param>
        /// <returns>True, si l'exécutable est bel et bien là.</returns>
        public static bool TestExecutableLocation(NomadConfiguration nomadConfig, VersionInfo minVersion = null)
        {
            if (nomadConfig == null || !File.Exists(nomadConfig.ImageMagickLocation)) return false;

            var tmp = new FileInfo(DirectoryUtil.CreateLocalTemporaryFile());

            try
            {
                LaunchProcessUtil.LaunchProcess(nomadConfig.ImageMagickLocation, "-version", tmp);

                tmp.Refresh();
                if (!tmp.Exists) return false;

                if (minVersion == null)
                {
                    return true; // On ne fait pas de test sur le numéro de version, juste l'existence de résultat, si null.
                }

                var lines = File.ReadAllText(tmp.FullName);
                log.Trace("Image Magick version call: {0}", lines);

                var match = new Regex(REGEX_IM_VERSION).Match(lines);
                if (!match.Success) return false;

                var ver = match.Value; // Format "X.X.X-ZZ QYY", exemple "7.0.3-34 Q32"
                var s1 = ver.Split(' '); // Sépare le quantifiant mémoire (QYY) du numéro de version.
                var s2 = (s1.Length >= 1 ? s1[0] : "").Split('-'); // Sépare la révision des autre témoins de version
                var v = (s2.Length >= 2 ? s2[0] : "").Split('.'); // Sépare les numéro de version Maj-Min-Dev
                return v.Length == 3 && minVersion.CheckVersion(v[0], v[1], v[2], s2[1]);
            }
            finally
            {
                try
                {
                    File.Delete(tmp.FullName);
                }
                catch (Exception)
                {
                    // Die nice
                }
            }
        }

        /// <summary>
        /// Vérifie si un fichier de thumbnail avec le bon nom existe déjà.
        /// Un fichier dit "thumbnail" est un fichier JPG (toujours) avec un nom de fichier original préfixé avec des dimensions précises.
        /// SI un tel fichier n'existe, on peut alors le créer avec la méthode "CreateThumbnailAsync".
        /// </summary>
        /// <param name="baseFile">Fichier de base.</param>
        /// <param name="thumbDirectory">Répertoire où se trouve les thumbnails.</param>
        /// <param name="wantedSize">Taille désirée du thumbnail.</param>
        /// <returns>un FileInfo si le fichier existe, null sinon.</returns>
        public static FileInfo CheckForThumbnailFile(FileInfo baseFile, DirectoryInfo thumbDirectory, Size wantedSize)
        {
            // Identifie le fichier original.
            if (baseFile == null)
            {
                log.Trace("Thumbnail detection: Null argument for main file.");
                return null;
            }

            var fileName = StringUtil.RemoveExtension(baseFile.Name);
            
            var quickThb = $"{wantedSize.Width}x{wantedSize.Height}_{fileName}.jpg";

            var quickTbhFile = new FileInfo(Path.Combine(thumbDirectory.FullName, quickThb));

            return quickTbhFile.Exists ? quickTbhFile : null;
        }

        /// <summary>
        /// Cette méthode créé des fichiers d'image plus petit (un "thumbnail" en anglais) que le fichier original. Cette méthode, quand appellé avec les mêmes
        /// paramètres d'emplacement, va stocker les images et les nommer avec un code de taille pour ne générer qu'une seule fois l'opération de création. Quand un fichier
        /// candidat existe déjà, il est immédiatement retourné.
        /// Quand on spécifie un fichier extrant explicite, la fonction de nommage unique ne tient plus et c'est à l'appellant de gérer ce cas de figure. Dans cette situation,
        /// la méthode va échouer si le fichier extrant explicite existe déjà. C'est le travail de l'appellant de s'assurer que ce fichier n'existe pas en premier lieu.
        /// On n'est pas non plus obligé d'utiliser une nouvelle taille qui respecte le ratio original. Dans cette situation, on peut spécifier si on veut élaguer ou non le résultat,
        /// ainsi que quelle couleurs les bandes résultantes doivent être (si aucun élaguage). L'image élaguée sera toujours centrée sur un seul axe, celui qui perd des informations.
        /// </summary>
        /// <param name="baseFile">Le fichier original à reproduire en plus petit.</param>
        /// <param name="thumbDirectory">Le répertoire où son stockées les petits fichier.</param>
        /// <param name="wantedSize">La taille désirée. Si la taille est la même que l'original, on retourne la référence de l'original, sans créer de nouveau fichier. Ne fonctionne pas si un fichier explicite est donné.</param>
        /// <param name="fitInsideSize">Si le ratio d'image est différent, et TRUE, ne fera aucun élaguage et on crééra des bandes de couleur pour remplir.</param>
        /// <param name="baseColor">La couleur des bandes si ratio différent et aucun élaguage n'est demandé. Si aucun n'est spécifié, blanc sera choisi.</param>
        /// <param name="desiredOutputFile">Force une création de fichier à se faire. Mettre un valeur ici change plusieurs comportements de la méthode.</param>
        /// <returns>NULL si l'opération est impossible ou a échouée. Sinon, fait une référence vers un fichier avec les caractéritiques demandée. Sauf si un fichier explicite est donné, cela peu être la référence du fichier original (ce qui implique un NO-OP).</returns>
        public static async Task<FileInfo> CreateThumbnailAsync(FileInfo baseFile, DirectoryInfo thumbDirectory, Size? wantedSize,
            bool fitInsideSize = false, Color? baseColor = null, FileInfo desiredOutputFile = null)
        {
            // Identifie le fichier original.
            if (baseFile == null)
            {
                log.Trace("Thumbnail generation: Null argument for main file.");
                return null;
            }

            var fileName = StringUtil.RemoveExtension(baseFile.Name);

            if (wantedSize != null)
            {
                var quickThb = $"{wantedSize.Value.Width}x{wantedSize.Value.Height}_{fileName}.jpg";

                var quickTbhFile = desiredOutputFile ?? new FileInfo(Path.Combine(thumbDirectory.FullName, quickThb));

                if (quickTbhFile.Exists)
                {
                    return quickTbhFile; // Si le fichier n'est pas forcé, et qu'un fichier existe déjà, on retourne le fichier thumbnail déjà créé.
                }
            }

            baseFile.Refresh();
            if (!baseFile.Exists)
            {
                log.Trace("Thumbnail generation: Base file does not exists. {0}", baseFile.FullName);
                return null;
            }

            var size = await GetImageDimensionsAsync(CancellationToken.None, baseFile);
            if (size == null)
            {
                log.Trace("Thumbnail generation: Could not extract size information for base file: {0}", baseFile.FullName);
                return null;
            }

            var initialSize = size.Value;

            // Un fichier forcé doit toujours êtr inexistant à la base.
            if (desiredOutputFile != null && desiredOutputFile.Exists)
            {
                log.Trace("Thumbnail generation: Forced output file already exists. {0}", desiredOutputFile.FullName);
                return null;
            }

            var desiredSize = wantedSize ?? initialSize;
            
            var thbFile = desiredOutputFile ?? 
                          new FileInfo(Path.Combine(
                                thumbDirectory.FullName, 
                                $"{desiredSize.Width}x{desiredSize.Height}_{fileName}.jpg"));

            // On charge l'image originale si la taille désirée est la même, ou si le répertoire de thumbnail n'existe pas.
            // Si un fichier spécifique est donné, on ne fait pas cette vérification.
            if (desiredOutputFile == null && (initialSize.Equals(desiredSize) || !thumbDirectory.Exists))
            {
                log.Trace("Thumbnail generation: Desired size is the same as base file size.");
                return baseFile;
            }
            
            var color = baseColor ?? Color.White;
                
            // Créer le thumbnail
            var commands = THUMBNAIL_COMMANDS2;
            var s = fitInsideSize ?
                MathUtil.GetNewSizeToFitInsideContainer(desiredSize.Width, desiredSize.Height, initialSize.Width, initialSize.Height).ToSize() :
                MathUtil.GetNewSizeToFitContainerWithMinimalLoss(desiredSize.Width, desiredSize.Height, initialSize.Width, initialSize.Height).Size;
            var inValues = new Dictionary<string, string>
            {
                {"ori",baseFile.FullName},
                {"size1",$"{s.Width}x{s.Height}"},
                {"size2",$"{desiredSize.Width}x{desiredSize.Height}"},
                {"color",$"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}"}
            };

            var fileRefs = CreateFileRefs(commands, new DirectoryInfo(DirectoryUtil.GetLocalApplicationWorkDir()), null, false, inValues);
            var results = await ExecuteCommands(commands, fileRefs, async (c, r, ct) => await c.ExecuteAsync(r, inValues, ct), CancellationToken.None);

            if (results != null && results.Success)
            {
                var resultFile = results.ResultFiles["final"];

                resultFile.CopyTo(thbFile.FullName);
                thbFile.Refresh();
                log.Debug("Thumbnail generation: Thumbnail file generated for file: {0}", thbFile.FullName);
            }
            else
            {
                log.Warn("Thumbnail creation failed.");
                thbFile = null;
            }

            return thbFile;
        }

        /// <summary>
        /// Effectue une opération de création de thumbnail avec vérification standard, et charge immédiatement le fichier en mémoire. Si le retour est nul, alors il a été
        /// impossible de faire un thumbnail.
        /// </summary>
        public static async Task<Bitmap> LoadImageFromThumbnailAsync(FileInfo baseFile, DirectoryInfo thumbDirectory, Size? wantedSize, bool fitInsideSize = false, Color? baseColor = null)
        {
            var thb = await CreateThumbnailAsync(baseFile, thumbDirectory, wantedSize, fitInsideSize, baseColor);
            // Charge le fichier thumbnail
            return thb != null ? FileUtil.LoadBitmap(thb.FullName) : null;
        }
        
        /// <summary>
        /// Cette méthode charge un Bitmap avec la fonction spécifique de créer/réutiliser un fichier de thumbnail créé pour cette image selon la taille voulue.
        /// Grâce à un répertoire précis et une nomenclature unique par fichier, on créé des fichier thumbnails qui ont exactement la taille voulue au besoin lors
        /// du "premier" appel de cette méthode avec un fichier et taille précis. Une fois le fichier créé, s'il correspond exactement au fichier avec la taille voulue,
        /// il sera réutilisé pour chaque appel subséquent. Ce qui fait que les calculs de redimensions ne sont fait qu'une et une seule fois, et que le fichier résultant
        /// prenne un minimum de place en mémoire, tout en étant plus rapide à charger depuis le disque.
        /// Le fichier original, duquel est basé les thumbnails, ne sera jamais modifié par cette méthode. Si par chance le fichier original possède déjà les bonnes
        /// dimensions, le fichier original sera chargé en mémoire.
        /// Comme la transformation peut contenir de la transparence (ratio inégal entre image original et dimension de thumbnail), on sauvegarde les thumbnails en PNG.
        /// La création du thumbnail se fait via ImageMagick, dont cette méthode requiert une configuration fonctionnelle du CLI de ImageMagick pour fonctionner.
        /// </summary>
        /// <param name="baseFile">Le fichier original, duquel basé la génération de thumbnail.</param>
        /// <param name="thumbDirectory">Le répertoire où stocker les fichiers de thumbnail.</param>
        /// <param name="wantedSize">La dimension d'affichage. Cette dimension sera utilisée pour créer les thumbnails.</param>
        /// <param name="loadedBitmap">OUT. L'instance de BITMAP résultante.</param>
        /// <param name="fitInsideSize">Si le ratio de l'image ne correspond pas au ratio de la taille voulue, ce paramètre indique le choix à faire: couper l'image pour remplir la totalité de la zone voulue, ou placer toute l'image sans perte, avec une couleur de fond. Par défaut, pas de perte avec couleur de fond.</param>
        /// <param name="baseColor">Si on utilise une couleur de fond pour le thumbnail, ce sera cette couleur. Par défaut, transparent.</param>
        /// <returns>Si l'opération s'est bien effectuée, c'est à dire qu'il y a une instance de Bitmap qui a été créée.</returns>
        [Obsolete("Dangeureux. Utiliser la version asynchrone.")]
        public static bool LoadImageFromThumbnail(FileInfo baseFile, DirectoryInfo thumbDirectory, Size? wantedSize, 
            out Bitmap loadedBitmap, bool fitInsideSize = false, Color? baseColor = null)
        {
            // Identifie le fichier original.
            if (baseFile == null || !baseFile.Exists)
            {
                log.Trace("Thumbnail generation: Base file does not exists. {0}", baseFile?.FullName);
                loadedBitmap = null;
                return false;
            }

            FileInfo thbFile;

            // Check un
            // Vérifie si un fichier existe déjà avec les spécifications voulues. Si oui, on le charge immédiatement.
            if (wantedSize.HasValue && !wantedSize.Value.IsEmpty)
            {
                // Vérifie si un thumbnail de cette taille existe déjà
                var ext = baseFile.Extension;
                var fileName = StringUtil.RemoveExtension(baseFile.Name);
                var thbFileName = $"{wantedSize.Value.Width}x{wantedSize.Value.Height}_{fileName}{ext}";

                thbFile = new FileInfo(Path.Combine(thumbDirectory.FullName, thbFileName));

                if (thbFile.Exists)
                {
                    // Charge le fichier thumbnail
                    loadedBitmap = FileUtil.LoadBitmap(thbFile.FullName);
                    return true;
                }
            }
            else
            {
                // Aucune taille spécifiée... Implique une initialisation très précoce. On charge les fichiers originaux, puisque aucun thumbnail ne peut exister.
                log.Trace("Thumbnail generation: No wanted size. Passing original file");
                loadedBitmap = FileUtil.LoadBitmap(baseFile.FullName);
                return true;
            }

            // Étape 2: le fichier de thumbnail n'existe pas, et donc il faut le générer.
            if (!thumbDirectory.Exists)
            {
                log.Error("The thumbnail directory does not exists.");
                loadedBitmap = null;
                return false;
            }

            if (!GetImageDimensions(baseFile, out var initialSize))
            {
                log.Trace("Thumbnail generation: Could not extract size information for base file: {0}", baseFile.FullName);
                loadedBitmap = null;
                return false;
            }
            
            // On charge l'image originale si la taille désirée est la même, ou si le répertoire de thumbnail n'existe pas.
            if (initialSize.Equals(wantedSize.Value))
            {
                log.Trace("Thumbnail generation: Desired size is the same as base file size.");
                loadedBitmap = FileUtil.LoadBitmap(baseFile.FullName);
                return true;
            }

            var color = baseColor ?? Color.Transparent;
            var dx = wantedSize.Value.Width;
            var dy = wantedSize.Value.Height;
            
            // Créer le thumbnail
            var commands = THUMBNAIL_COMMANDS2;
            var s = fitInsideSize ?
                MathUtil.GetNewSizeToFitInsideContainer(dx, dy, initialSize.Width, initialSize.Height).ToSize() :
                MathUtil.GetNewSizeToFitContainerWithMinimalLoss(dx, dy, initialSize.Width, initialSize.Height).Size;
            var inValues = new Dictionary<string, string>
            {
                {"ori", baseFile.FullName},
                {"size1", $"{s.Width}x{s.Height}"},
                {"size2", $"{dx}x{dy}"},
                // Le code de couleur pour transparent est plus clair si on utilise "none"
                {"color", color != Color.Transparent ? $"\"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}\"" : "none"}
            };

            var fileRefs = CreateFileRefs(commands, new DirectoryInfo(DirectoryUtil.GetLocalApplicationWorkDir()), null, false, inValues);

            async Task ExecFunc(ImageMagickCliCommand c, CliTmpFileDictionaryRefs r, CancellationToken ct)
            {
                await c.ExecuteAsync(r, inValues, ct);
            }

            var resultsTask = ExecuteCommands(commands, fileRefs, ExecFunc, CancellationToken.None);
            resultsTask.Wait(2000); // Délais maximal. La fonction peut se terminer bien avant.
            var results = resultsTask.Result;

            if (results != null && results.Success)
            {
                var resultFile = results.ResultFiles["final"];

                resultFile.CopyTo(thbFile.FullName, true);
                thbFile.Refresh();
                log.Debug("Thumbnail generation: Thumbnail file generated for file: {0}", thbFile.FullName);
            }
            else
            {
                log.Warn("Thumbnail creation failed.");
                thbFile = baseFile;
            }
            
            // Charge le fichier thumbnail
            loadedBitmap = FileUtil.LoadBitmap(thbFile.FullName);
            return true;
        }

        /// <summary>
        /// Cette méthode charge un Bitmap avec la fonction spécifique de créer/réutiliser un fichier de thumbnail créé pour cette image selon la taille voulue.
        /// Grâce à un répertoire précis et une nomenclature unique par fichier, on créé des fichier thumbnails qui ont exactement la taille voulue au besoin lors
        /// du "premier" appel de cette méthode avec un fichier et taille précis. Une fois le fichier créé, s'il correspond exactement au fichier avec la taille voulue,
        /// il sera réutilisé pour chaque appel subséquent. Ce qui fait que les calculs de redimensions ne sont fait qu'une et une seule fois, et que le fichier résultant
        /// prenne un minimum de place en mémoire, tout en étant plus rapide à charger depuis le disque.
        /// Le fichier original, duquel est basé les thumbnails, ne sera jamais modifié par cette méthode. Si par chance le fichier original possède déjà les bonnes
        /// dimensions, le fichier original sera chargé en mémoire.
        /// Comme la transformation peut contenir de la transparence (ratio inégal entre image original et dimension de thumbnail), on sauvegarde les thumbnails en PNG.
        /// La création du thumbnail se fait via ImageMagick, dont cette méthode requiert une configuration fonctionnelle du CLI de ImageMagick pour fonctionner.
        /// </summary>
        /// <param name="ct">Le jeton de cancellation à utiliser.</param>
        /// <param name="baseFile">Le fichier original, duquel basé la génération de thumbnail.</param>
        /// <param name="thumbDirectory">Le répertoire où stocker les fichiers de thumbnail.</param>
        /// <param name="wantedSize">La dimension d'affichage. Cette dimension sera utilisée pour créer les thumbnails.</param>
        /// <param name="fitInsideSize">Si le ratio de l'image ne correspond pas au ratio de la taille voulue, ce paramètre indique le choix à faire: couper l'image pour remplir la totalité de la zone voulue, ou placer toute l'image sans perte, avec une couleur de fond. Par défaut, pas de perte avec couleur de fond.</param>
        /// <param name="baseColor">Si on utilise une couleur de fond pour le thumbnail, ce sera cette couleur. Par défaut, transparent.</param>
        /// <returns>Si l'opération s'est bien effectuée, le retour sera non nul.</returns>
        public static async Task<Bitmap> LoadImageFromThumbnailAsync(CancellationToken ct, FileInfo baseFile, DirectoryInfo thumbDirectory, Size? wantedSize, bool fitInsideSize = false, Color? baseColor = null)
        {
            // Identifie le fichier original.
            if (baseFile == null || !baseFile.Exists)
            {
                log.Trace("Thumbnail generation: Base file does not exists. {0}", baseFile?.FullName);
                return null;
            }

            FileInfo thbFile;

            // Check un
            // Vérifie si un fichier existe déjà avec les spécifications voulues. Si oui, on le charge immédiatement.
            if (wantedSize.HasValue && !wantedSize.Value.IsEmpty)
            {
                // Vérifie si un thumbnail de cette taille existe déjà
                var ext = baseFile.Extension;
                var fileName = StringUtil.RemoveExtension(baseFile.Name);
                var thbFileName = $"{wantedSize.Value.Width}x{wantedSize.Value.Height}_{fileName}{ext}";

                thbFile = new FileInfo(Path.Combine(thumbDirectory.FullName, thbFileName));

                if (thbFile.Exists)
                {
                    // Charge le fichier thumbnail
                    return FileUtil.LoadBitmap(thbFile.FullName);
                }
            }
            else
            {
                // Aucune taille spécifiée... Implique une initialisation très précoce. On charge les fichiers originaux, puisque aucun thumbnail ne peut exister.
                log.Trace("Thumbnail generation: No wanted size. Passing original file");
                return FileUtil.LoadBitmap(baseFile.FullName);
            }

            // Étape 2: le fichier de thumbnail n'existe pas, et donc il faut le générer.
            if (!thumbDirectory.Exists)
            {
                log.Error("The thumbnail directory does not exists.");
                return null;
            }

            var dims = await GetImageDimensionsAsync(ct, baseFile);

            if (dims == null)
            {
                log.Trace("Thumbnail generation: Could not extract size information for base file: {0}", baseFile.FullName);
                return null;
            }

            var initialSize = dims.Value;
            
            // On charge l'image originale si la taille désirée est la même, ou si le répertoire de thumbnail n'existe pas.
            if (initialSize.Equals(wantedSize.Value))
            {
                log.Trace("Thumbnail generation: Desired size is the same as base file size.");
                return FileUtil.LoadBitmap(baseFile.FullName);
            }

            var color = baseColor ?? Color.Transparent;
            var dx = wantedSize.Value.Width;
            var dy = wantedSize.Value.Height;
            
            // Créer le thumbnail
            var commands = THUMBNAIL_COMMANDS2;
            var s = fitInsideSize ?
                MathUtil.GetNewSizeToFitInsideContainer(dx, dy, initialSize.Width, initialSize.Height).ToSize() :
                MathUtil.GetNewSizeToFitContainerWithMinimalLoss(dx, dy, initialSize.Width, initialSize.Height).Size;
            var inValues = new Dictionary<string, string>
            {
                {"ori", baseFile.FullName},
                {"size1", $"{s.Width}x{s.Height}"},
                {"size2", $"{dx}x{dy}"},
                // Le code de couleur pour transparent est plus clair si on utilise "none"
                {"color", color != Color.Transparent ? $"\"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}\"" : "none"}
            };

            var fileRefs = CreateFileRefs(commands, new DirectoryInfo(DirectoryUtil.GetLocalApplicationWorkDir()), null, false, inValues);

            async Task ExecFunc(ImageMagickCliCommand c, CliTmpFileDictionaryRefs r, CancellationToken canTok)
            {
                await c.ExecuteAsync(r, inValues, ct);
            }

            var results = await ExecuteCommands(commands, fileRefs, ExecFunc, ct);
            
            if (results != null && results.Success)
            {
                var resultFile = results.ResultFiles["final"];

                resultFile.CopyTo(thbFile.FullName, true);
                thbFile.Refresh();
                log.Debug("Thumbnail generation: Thumbnail file generated for file: {0}", thbFile.FullName);
            }
            else
            {
                log.Warn("Thumbnail creation failed.");
                thbFile = baseFile;
            }
            
            // Charge le fichier thumbnail
            return FileUtil.LoadBitmap(thbFile.FullName);
        }

        private static readonly List<ImageMagickCliCommand> THUMBNAIL_COMMANDS2 = CreateCommands<ImageMagickCliCommand>(new []
        {
            "convert {IN:ori} -thumbnail {IN_VAL:size1} -size {IN_VAL:size2} xc:{IN_VAL:color} -posterize 80 +swap -gravity center -composite {OUT:final}"
        });

        #region Commandes avec jetons

        /// <summary>
        /// Créer des commandes complètes à partir de la forme avec jeton de ces mêmes commandes.
        /// </summary>
        /// <typeparam name="T">Le type de commandes à créer.</typeparam>
        /// <param name="commandLines">Les commandes, sous forme de texte avec jetons.</param>
        /// <returns>La liste de commandes ainsi créées.</returns>
        public static List<T> CreateCommands<T>(IEnumerable<string> commandLines) where T : ImageMagickCliCommand
        {
            var resultCommands = new List<T>();

            foreach (var line in commandLines)
            {
                var com = Activator.CreateInstance<T>();

                com.CommandToken = line;
                com.TemporaryFileData = StringUtil.IdentifyTokenizedTemporaryFilesTag(line);

                resultCommands.Add(com);
                if (log.IsTraceEnabled) log.Trace("Command created from string:\n\t{0}\n\t", line, StringUtil.FormatDictionary((IDictionary)com.TemporaryFileData));
            }

            return resultCommands;
        }

        /// <summary>
        /// Créer les références finales requises par les commandes une fois transformées. En gros, transforment les jetons en valeurs utilisables,
        /// et dans le cas des fichiers, qu'ils soient au bon endroit.
        /// Représentent soit des fichiers temporaires, des valeurs intrants, ou l'emplacement des fichiers finaux.
        /// </summary>
        /// <param name="commands">La liste de commandes compilées.</param>
        /// <param name="baseOutputDir">Le répertoire où doivent apparaîtres les fichiers finaux.</param>
        /// <param name="workDir">Le répertoire de travail voulu. Si non-précisé, on utilise le répertoire de travail de l'application.</param>
        /// <param name="useOutputIdAsName">Si mis à TRUE, ne créer pas des noms aléatoires pour les fichiers, mais utilise les identifiants des jetons comme noms. Utile pour des
        /// tests ou des situations où il n'est pas important qu'il y ait des "clash" de noms de fichiers.</param>
        /// <param name="inValues">Les valeurs intrants des commandes. En temps normal, ces valeurs changent mais la structure des commandes non.</param>
        /// <returns>Le dictionnaire de références de fichiers et valeurs des commandes.</returns>
        public static CliTmpFileDictionaryRefs CreateFileRefs(IEnumerable<ImageMagickCliCommand> commands, DirectoryInfo baseOutputDir, string workDir = null, bool useOutputIdAsName = false, Dictionary<string, string> inValues = null)
        {
            // Maintenant que l'on a déterminé les fichiers temporaires, il faut faire une liste des tags pour être capable pour chaque commande
            // de vérifier les pré-conditions:
            // 1- Fichiers (TMP_IN) doivent exister avant l'appel.
            // 2- Fichiers (tmp + finaux) OUT doivent ne pas exister avant l'appel.
            // 3- Fichiers (tmp + finaux) OUT doivent exister après l'appel
            // Donc, on doit fusionner les identifiants des fichiers temporaires (TMP_IN, TMP_OUT) en une liste cohérente. La liste "OUT" reste inchangée.
            // Les dictionnaires ne contiennent pas encore de référence vers un fichier, donc, on stocke null. Mais l'identifiant existe.
            // Les valeurs IN et IN_VAL ne sont pas testées, car ce ne sont pas des fichiers ou peuvent être composé avec des valeurs externent de jetons (BASE_DIR).
            // Elles ne sont donc pas testées, car trop variables. C'est au module qui veut exécuter ces commandes de s'assurer que ces valeurs existent correctement.

            var refs = new CliTmpFileDictionaryRefs();

            foreach (var com in commands)
            {
                foreach (var kvp in com.TemporaryFileData)
                {
                    foreach (var fti in kvp.Value)
                    {
                        var nom = (fti.TokenType == FileTokenType.OUT && useOutputIdAsName) ? fti.CreateFileName(fti.Tag) : fti.CreateFileName();
                        switch (kvp.Key)
                        {
                            case FileTokenType.TMP_IN:
                            case FileTokenType.TMP_OUT:
                                if (!refs.TempRefs.ContainsKey(fti.Tag)) refs.TempRefs.Add(fti.Tag, 
                                    new FileInfo(DirectoryUtil.CreateLocalTemporaryFile(nom, workDir)));
                                break;
                            case FileTokenType.OUT:
                                if (!refs.OutRefs.ContainsKey(fti.Tag)) refs.OutRefs.Add(fti.Tag, 
                                    new FileInfo(baseOutputDir.FullName + nom));
                                break;
                            case FileTokenType.IN_VAL:
                            case FileTokenType.IN:
                                if (!refs.InRefs.ContainsKey(fti.Tag))
                                {
                                    if (inValues != null && inValues.TryGetValue(fti.Tag, out var val))
                                    {
                                        refs.InRefs.Add(fti.Tag, val);
                                    }
                                    else
                                    {
                                        refs.InRefs.Add(fti.Tag, nom);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            
            return refs;
        }

        /// <summary>
        /// Méthode d'exécution principale des commandes du CLI.
        /// </summary>
        /// <typeparam name="T">Le type de commandes.</typeparam>
        /// <param name="commands">La liste complète des commandes du script.</param>
        /// <param name="refs">La référence vers la table de fichiers temporaires.</param>
        /// <param name="executeFunc">La fonction d'exécution de la commande.</param>
        /// <param name="cancelToken">Le jeton de cancellation.</param>
        /// <param name="progressReport">Optionel. Lien vers un IProgressReport.</param>
        /// <param name="debugKeepTmpFiles">Optionel (false). Indique si on garde ou non les fichiers temporaires créés.</param>
        /// <returns>Le résultat de l'exécution des commandes.</returns>
        public static async Task<ImageMagickCliResult> ExecuteCommands<T>(
            List<T> commands, 
            CliTmpFileDictionaryRefs refs,
            Func<T, CliTmpFileDictionaryRefs, CancellationToken, Task> executeFunc,
            CancellationToken cancelToken, IProgress<ImageMagickCliUiUpdate> progressReport = null, bool debugKeepTmpFiles = false)
            where T : ImageMagickCliCommand
        {
            // Vérification des pré-conditions.
            if (commands == null || !commands.Any() || cancelToken.IsCancellationRequested)
            {
                return new ImageMagickCliResult
                {
                    Success = false,
                    ResultFiles = new Dictionary<string, FileInfo>(),
                    TotalTime = TimeSpan.Zero
                };
            }

            var mainTimer = new Stopwatch();
            var result = new ImageMagickCliResult();
            
            try
            {
                var timerUse = progressReport != null || log.IsTraceEnabled;

                mainTimer.Start();
                var stepNbr = 0;
                var stepTotal = commands.Count;
                var stepTimer = timerUse ? new Stopwatch() : null;
                foreach (var command in commands.TakeWhile(command => !cancelToken.IsCancellationRequested))
                {
                    stepNbr++;
                    if (timerUse)
                    {
                        stepTimer.Restart();
                    }

                    // Vérification des pré-conditions de la commande.
                    CheckPreconditions(command, refs);

                    // Exécution
                    await executeFunc(command, refs, cancelToken);

                    // Vérification des posts-conditions.
                    CheckPostconditions(command, refs);

                    // Mise-à-jour du progrès
                    if (timerUse) stepTimer.Stop();
                    if (progressReport != null)
                    {
                        stepTimer.Stop();
                        var reportCard = new ImageMagickCliUiUpdate
                        {
                            CurrentStep = stepNbr,
                            StepTotal = stepTotal,
                            PercentCompletion = stepNbr / (double) stepTotal * 100,
                            CurrentStepDescription = $"ImageMagick transformation command step {stepNbr}",
                            RunningTime = mainTimer.Elapsed,
                            StepTime = stepTimer.Elapsed
                        };
                        progressReport.Report(reportCard);
                    }
                    else if (log.IsTraceEnabled)
                    {
                        log.Trace($"Command executed: step={stepNbr}/{stepTotal} ({stepNbr / (double) stepTotal * 100}%), Timer={stepTimer?.Elapsed}/{mainTimer.Elapsed}\n\t{command.CommandToken}");
                    }
                }
                mainTimer.Stop();

                // On collige maintenant les fichiers finaux
                result.ResultFiles = cancelToken.IsCancellationRequested
                    ? new Dictionary<string, FileInfo>()
                    : new Dictionary<string, FileInfo>(refs.OutRefs);
                result.Success = !cancelToken.IsCancellationRequested;
                result.TotalTime = mainTimer.Elapsed;
            }
            catch (Exception e)
            {
                mainTimer.Stop();
                log.Error(e);
                result.ResultFiles = new Dictionary<string, FileInfo>();
                result.Success = false;
                result.TotalTime = mainTimer.Elapsed;
            }
            finally
            {
                // On vide les dictionnaires et les fichiers temporaires
                if (!debugKeepTmpFiles)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        foreach (var file in refs.OutRefs.Values.Where(file => file != null))
                        {
                            try
                            {
                                DeleteFile(file);
                            }
                            catch (Exception) // Die nice
                            {
                                log.Warn(
                                    "Could not delete out file from post-execution cleanup (cancellation was detected).");
                            }
                        }
                    }

                    foreach (var file in refs.TempRefs.Values.Where(file => file != null))
                    {
                        try
                        {
                            DeleteFile(file);
                        }
                        catch (Exception) // Die nice
                        {
                            log.Warn("Could not delete temporary file from post-execution cleanup.");
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Comme un fichier temporaire de ImageMagick est composé de deux fichiers distinct (un header, un de données brutes), il faut s'assurer de
        /// détruit les deux quand on détruit l'un.
        /// </summary>
        /// <param name="f">Le fichier sur disque à détruire.</param>
        private static void DeleteFile(FileInfo f)
        {
            f.Refresh();
            if (f.Name.EndsWith(".mpc", StringComparison.InvariantCultureIgnoreCase))
            {
                var fn =  f.FullName;
                var cacheFile = fn.Substring(0, fn.Length - 4) + ".cache";
                if (File.Exists(cacheFile)) File.Delete(cacheFile);
            }
            f.Delete();
        }

        /// <summary>
        /// Vérification des Post-Conditions.
        /// </summary>
        /// <param name="command">La commande à vérifier.</param>
        /// <param name="refs">Les fichiers de référence.</param>
        private static void CheckPostconditions(ImageMagickCliCommand command, CliTmpFileDictionaryRefs refs)
        {
            foreach (var kvp in command.TemporaryFileData)
            {
                switch (kvp.Key)
                {
                    case FileTokenType.TMP_OUT:
                        // Must Exist
                        foreach (var tag in kvp.Value) CheckCondition(tag.Tag, refs.TempRefs, true, "TMP_OUT", "Postcondition", command.CommandToken);
                        break;
                    case FileTokenType.OUT:
                        // Must Exist
                        foreach (var tag in kvp.Value) CheckCondition(tag.Tag, refs.OutRefs, true, "OUT", "Postcondition", command.CommandToken);
                        break;
                }
            }
        }

        /// <summary>
        /// Vérification des Pré-Conditions.
        /// </summary>
        /// <param name="command">La commande à vérifier.</param>
        /// <param name="refs">Les fichiers de référence.</param>
        private static void CheckPreconditions(ImageMagickCliCommand command, CliTmpFileDictionaryRefs refs)
        {
            foreach (var kvp in command.TemporaryFileData)
            {
                switch (kvp.Key)
                {
                    case FileTokenType.TMP_IN:
                        // Must Exist
                        foreach (var tag in kvp.Value) CheckCondition(tag.Tag, refs.TempRefs, true, "TMP_IN", "Precondition", command.CommandToken);
                        break;
                    case FileTokenType.TMP_OUT:
                        // Peut exister ou non (réutilisation de fichiers intermédiaires).
                        // foreach (var tag in kvp.Value) CheckCondition(tag.Tag, refs.TempRefs, false, "TMP_OUT", "Precondition", command.CommandToken);
                        break;
                    case FileTokenType.OUT:
                        // Must NOT Exist
                        foreach (var tag in kvp.Value) CheckCondition(tag.Tag, refs.OutRefs, false, "OUT", "Precondition", command.CommandToken);
                        break;
                }
            }
        }

        /// <summary>
        /// Vérification des fichiers intrants-extrant d'une commande.
        /// </summary>
        /// <param name="tag">L'identifiant du fichier dans la commande (Jeton initial)</param>
        /// <param name="dico">Le dictionnaire de fichier contenant les références.</param>
        /// <param name="not">Faut-il inverser le résultat du test?</param>
        /// <param name="prefix">Post ou Pre (pour construire les messages d'erreur).</param>
        /// <param name="type">Le type d'intrants. Pour messages d'erreur.</param>
        /// <param name="commandToken">La commandes pure, avec jeton. Pour journalisation.</param>
        private static void CheckCondition(string tag, Dictionary<string, FileInfo> dico, bool not, string prefix, string type, string commandToken)
        {
            var fileOnDisk = File.Exists(dico[tag].FullName);
            if (not ? !fileOnDisk : fileOnDisk)
            {
                throw new FileNotFoundException($"{prefix} fail: File {(not ? "does not " : "")}exists for {type} parameter '{tag}' in command '{commandToken}'");
            }
        }

        #endregion
    }
}