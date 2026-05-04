using System;
using System.Threading;
using System.Threading.Tasks;
using EmisysCardScannerLib;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Util.DateAndTime;

namespace NomadLogic.Common.Util.RfidReader
{
    /// <summary>
    /// Classe pour gérer les lecteurs de carte RFID sur l'ordinateur.
    /// Basé sur le lecteur de carte RFID de marque MR800, avec le système fournit par Emisys comme partenaire.
    /// </summary>
    [PublicAPI]
    // ReSharper disable once InconsistentNaming
    public class MR800_RfidReader : RfidReaderBase, IRfidEvents
    {
        /// <summary>
        /// Liste des couleur possibles pour les LEDs
        /// </summary>
        public enum LedColors
        {
            Yellow = 0x80,
            Green = 0x20,
            Blue = 0x40,
            Red = 0x10
        }

        /// <summary>
        /// Liste des options possibles pour le "buzzer" du lecteur.
        /// </summary>
        [PublicAPI]
        public enum BuzzerOptions
        {
            BeepError = 1,
            BeepRead = 2,
            BeepSuccess = 4
        }

        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private ReaderMR800 _mainReader;
        private readonly string _manufacturerTarget;
        private string _connectedReaderManufacturer;
        private Task _readTask;
        private CancellationTokenSource _cts;

        /// Requis pour identifier la classe dans la désérialisation.
        [JsonRequired]
        public static string ModelName => "MR800";

        /// Événement quand on lit une carte RFID.
        public event EventHandler<RfidReadEventArgs> CardRead;

        /// Événement quand on lit une carte RFID, mais que la valeur ne fait pas de sens (I.E. trop de carte devant le lecteur.
        public event EventHandler<RfidReadEventArgs> InvalidCardRead;

        /// Lancé quand, pour n'importe quelle raison, on n'est plus capable de communiquer avec le lecteur ou qu'un problème est survenu.
        public event EventHandler<EventArgs> HardWareDisconnected;
        

        [JsonIgnore]
        public override string DefaultConfigFileName { get; } = "mr800_config.json";
        
        /// Indique que l'on ne doit pas utiliser les lumières. À mettre à true avec la première connexion.
        /// N'a pas d'effet quand on appelle la méthode "SetLed" directement.
        public bool DoNotUseLights { get; set; }

        /// Indique que l'on ne doit pas utiliser le son. À mettre à true avec la première connexion.
        /// N'a pas d'effet quand on appelle la méthode "SetBuzzer" directement.
        public bool DoNotUseBuzzer { get; set; }

        /// Indique que l'on ne doit pas utiliser l'afficheur. À mettre à true avec la première connexion.
        /// N'a pas d'effet quand on appelle la méthode "SetText" directement.
        public bool DoNotUseText { get; set; }

        public MR800_RfidReader()
        {
            _manufacturerTarget = ModelName;
        }

        /// <summary>
        /// Constructeur de base. On peut spécifier un nom de manufacturier spécifique pour interfacer avec le lecteur.
        /// </summary>
        /// <param name="manufacturerName">Nom du manufacturier spécifique. Par défaut = "MR800"</param>
        public MR800_RfidReader(string manufacturerName)
        {
            _manufacturerTarget = manufacturerName ?? ModelName;
        }

        /// <summary>
        /// Rafraîchit la liste des lecteurs disponibles. Retourne TRUE si au moins un lecteur existe.
        /// Provoque la déconnexion du lecteur.
        /// </summary>
        /// <returns>TRUE si au moins un lecteur est connecté. On ne vérifie pas le manufacturier.</returns>
        public bool RefreshReader()
        {
            if (!InitReaderInstance()) return false;

            var res = _mainReader.UpdateReadersStatus();

            log.Trace("Smart Card reader status refreshed. {0} readers detected.", res);

            return res != 0;
        }

        private bool InitReaderInstance()
        {
            if (_mainReader != null) return true;
            try
            {
                log.Trace("Smart card reader instanciated.");
                _mainReader = new ReaderMR800();
            }
            catch (Exception e)
            {
                log.Error(e, "Could not instanciate smart card reader instance.");
                _mainReader = null;
                return false;
            }
            return true;
        }

        public override bool ConnectReader()
        {
            if (!InitReaderInstance()) return false;

            // Étape un, on cherche le premier lecteur de carte disponible et on se connecte dessus.
            _mainReader.UpdateReadersStatus();
            short readerIndex = -1;
            for (short i = 0; i < _mainReader.ReaderName.Length; i++)
            {
                if (_mainReader.ReaderName[i] == null) continue;
                if (!_mainReader.ReaderName[i].Contains(_manufacturerTarget)) continue;
                readerIndex = i;
                break;
            }

            if (readerIndex == -1)
            {
                log.Warn("No reader with name containing '{0}' was found. Devices list:\n{1}", _manufacturerTarget, StringUtil.FormatEnumerable(_mainReader.ReaderName, true, 1));
                return false;
            }

            _connectedReaderManufacturer = _mainReader.ReaderName[readerIndex];

            var connectResult = _mainReader.ConnectReader(readerIndex);

            if (connectResult)
            {
                if (!DoNotUseBuzzer) SetBuzzer(BuzzerOptions.BeepSuccess);
                if (!DoNotUseText) SetDisplayText("Connected");
                if (DoNotUseLights) return true;
                SetLed(LedColors.Blue);
                SetLed(LedColors.Green);

                // On lance la tâche qui lit les cartes.
                _cts = new CancellationTokenSource();
                _readTask = Task.Run(() => ReadCardLongRunTask(_cts.Token), _cts.Token);

                return true;
            }

            log.Warn("Could not establish connection to reader '{0}'", _connectedReaderManufacturer);
            _connectedReaderManufacturer = null;
            return false;
        }

        public override bool IsReady()
        {
            return _readTask != null;
        }
        
        private void ReadCardLongRunTask(CancellationToken token)
        {
            const short GET_MIFAREONE = 0;
            var cardInfo = new byte[20];
            short receiveLen = 0;
            string lastCardRead = null;
            var nextSameCardReadEvent = DateTime.Now;
            var nextCardReadEvent = DateTime.Now;
            var errorDetected = false;

            try
            {
                while (!token.IsCancellationRequested && !errorDetected)
                {
                    Thread.Sleep(200);
                    if (token.IsCancellationRequested) break;

                    var ret = _mainReader.GetData(GET_MIFAREONE, ref cardInfo, ref receiveLen);
                    log.Trace("Read data attempt: " + ret);

                    if (ret == ReaderMR800.ERROR_GENERAL)
                    {
                        log.Error("Read loop initialized in a invalid state ({0}). Terminating loop.", ReaderMR800.ERROR_GENERAL);
                        errorDetected = true;
                    }

                    if (ret != ReaderMR800.SUCCESS) continue;
                    
                    var cardUid = "";
                    // Reconstruction de la valeur de la carte en hexadecimal.
                    for (var i = 0; i < cardInfo[0] && i < cardInfo.Length - 1; i++)
                    {
                        var y = "00" + cardInfo[i + 1].ToString("X");
                        cardUid += y.Substring(y.Length - 2);
                    }

                    var now = DateTime.Now;
                    var update = !cardUid.Equals(lastCardRead) || now > nextSameCardReadEvent; // Check pour la même carte.

                    if (!update)
                    {
                        log.Trace("Same card read ({0}). Skipping", cardUid);
                        continue;
                    }

                    update = now >= nextCardReadEvent; // Check pour le temps minimal entre lectures valides.
                    if (!update)
                    {
                        log.Trace("Too early to read a card. Time remaining {0}", nextCardReadEvent.Subtract(now).ToStringHourMinSec());
                        continue;
                    }

                    // Mise à jour des timing de lectures.
                    lastCardRead = cardUid;
                    if (TimeBetweenSameCardRead == TimeSpan.MaxValue)
                    {
                        nextSameCardReadEvent = DateTime.MaxValue;
                    }
                    else
                    {
                        nextSameCardReadEvent = now + TimeBetweenSameCardRead;
                    }
                    nextCardReadEvent = now + TimeBetweenRead;
                    log.Trace("Card read ({0}). Time={1}, next valid time={2}", cardUid, now.Ticks, nextSameCardReadEvent.Ticks);
                    
                    // Validation du UID
                    // ReSharper disable once UnusedVariable
                    if (int.TryParse(cardUid, out var zeroVal))
                    {
                        // Cartes multiples détectées. La lecture est faussée.
                        log.Debug("Multiple Card read at same time. Time={0}, next valid time={1}", now.Ticks, nextSameCardReadEvent.Ticks);
                        SetLed(LedColors.Yellow, 2, true);
                        SetBuzzer(BuzzerOptions.BeepError);
                        InvalidCardRead?.Invoke(this, new RfidReadEventArgs(cardUid));
                    }
                    else
                    {
                        SetLed(LedColors.Green);
                        SetBuzzer(BuzzerOptions.BeepSuccess);
                        CardRead?.Invoke(this, new RfidReadEventArgs(cardUid));
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Error while standby in card reader.");
                errorDetected = true;
            }

            if (errorDetected)
            {
                HardWareDisconnected?.Invoke(this, null);
            }
        }

        /// <summary>
        /// Force la déconnexion du lecteur. On garde le lien vers l'instance du manufacturier, pour reconnexion possible.
        /// On peut refaire une tentative de connexion via les méthodes d'initilisation.
        /// Si on veut tout détruire, utiliser la méthode Dispose.
        /// </summary>
        /// <returns>TRUE seulement si le lecteur était connecté auparavant.</returns>
        public bool DisconnectReader()
        {
            try
            {
                if (_mainReader == null) return false;
                if (_connectedReaderManufacturer == null) return false;
                if (!DoNotUseLights) SetLed(LedColors.Red);
                if (!DoNotUseBuzzer) SetBuzzer(BuzzerOptions.BeepSuccess);
                if (!DoNotUseText) SetDisplayText("Disconnected");
            }
            catch (Exception e)
            {
                log.Error(e, "Error during disconnection of reader.");
            }
            finally
            {
                try
                {
                    _cts?.Cancel();
                    _readTask?.Wait(TimeSpan.FromSeconds(2)); // Devrait en principe se terminer avant.
                }
                catch (AggregateException e)
                {
                    log.Error(e, "Aggrgate exception when terminating task.");
                }
                finally
                {
                    _readTask = null;
                    _cts = null;
                    _connectedReaderManufacturer = null;
                }
            }

            return _mainReader?.DisconnectReader() ?? false;
        }

        /// <summary>
        /// Indique que l'on a déjà fait une connexion fonctionnelle à un lecteur.
        /// </summary>
        /// <returns>True si on est prêt à faire des lectures.</returns>
        public bool IsConnected()
        {
            return _connectedReaderManufacturer != null && _mainReader != null;
        }

        public override void Dispose()
        {
            DisconnectReader();
            _mainReader = null;
            _connectedReaderManufacturer = null;
        }
        
        /// <summary>
        /// Allume et/ou fait flasher les lumières sur le lecteur.
        /// Uniquement appeller de l'extérieur de cette classe pour des fins de tests, car le "firmware" du lecteur à des
        /// condigurations spécifiques.
        /// Par exemple:
        /// - Bleu de flashe jamais. Ni rouge.
        /// - Si on met vert ou rouge, quelque soit le temps donné, la couleur disparaît éventuellement et le bleu allume.
        /// - Jaune et bleu, une fois appellés, restent de façon infini.
        /// - Si on appelle bleu, ça fait disparaître jaune.
        /// Donc, les couleurs sont utilisées de la façon suivante automatiquement (sauf si la propriété "DoNotUseLights" est à TRUE):
        /// - Bleu indique que l'on est branché (connected)
        /// - Jaune veut dire que la derière lecture ne s'est pas correctement produite.
        /// - Vert flashe quand on lit une carte avec succès.
        /// - Rouge flashe pour toute erreur.
        /// </summary>
        /// <param name="ledId">L'identifiant de la lumière, via enum.</param>
        /// <param name="nbrFlash">Nombre de flashs ou de secondes à afficher.</param>
        /// <param name="flash">Si on fait flasher la lumière</param>
        public void SetLed(LedColors ledId, int nbrFlash = 1, bool flash = false)
        {
            if (_mainReader == null || !IsConnected()) return;
            
            _mainReader.setLed((int)ledId, nbrFlash, flash);
        }

        /// <summary>
        /// Fait réagir l'émetteur de son.
        /// </summary>
        /// <param name="buzzerId">Le type de son voulu.</param>
        public void SetBuzzer(BuzzerOptions buzzerId)
        {
            if (_mainReader == null || !IsConnected()) return;
            
            _mainReader.setBuzzer((int)buzzerId);
        }

        /// <summary>
        /// Affiche du texte dans la zone texte.
        /// Agit comme une matrice de point. Le point (0,0) est en haut à gauche.
        /// L'encodage final est de type ISO-8859-8.
        /// L'écran redevient normal après un certain temps.
        /// </summary>
        /// <param name="text">Le texte à afficher.</param>
        /// <param name="clear">Vide l'écran au préalable.</param>
        /// <param name="row">L'offset de rangée (X)</param>
        /// <param name="col">L'offset de colonne (Y)</param>
        public void SetDisplayText(string text, bool clear = true, int row = 0, int col = 0)
        {
            if (_mainReader == null || !IsConnected()) return;

            _mainReader.setText(text, clear, row, col);
        }
        
        public override bool Init()
        {
            return ConnectReader();
        }
    }
}
