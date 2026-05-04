using System;
using System.Linq;
using System.Threading;
using NLog;
using SlimDX.DirectInput;

namespace NomadLogic.Common.Util.Trigger.LaserBeam
{
    /// <summary>
    /// Cette classe gère la connection avec un périphérique laser pour détecter un bris du faisceau lumineux.
    /// <br>Ce laser fonctionne avec un pilote USB de manette de jeu, et gère son identification et les intrants
    /// qui sont provoqués par le matériel du laser.</br>
    /// <p>Grosso modo, pour utiliser cette classe, il faut l'initialiser avec init(), lui fournir une méthode
    /// déléguée qui sera invoquée quand le faisceau sera coupé, puis démarrer la lecture avec start() (qui se fait dans un
    /// fil d'exécution séparé). On peut stopper le tout avec la méthode stop().</p>
    /// <p>Le laser est fait de telle façon que l'on ne peut détecter un bris qu'aux 2-3 secondes environ.</p>
    /// </summary>
    public class LaserBeamManager : TriggerManagerBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Classe interne pour un fil d'exécution qui observe le laser.
        /// </summary>
        private class LaserBeamManagerWorker
        {
            private readonly LaserBeamManager _managerBase;
            private volatile bool _stop;

            /// <summary>
            /// Constructeur principal.
            /// </summary>
            /// <param name="managerBase">Référence sur le manager, pour avoir accès à ses événements et membres.</param>
            public LaserBeamManagerWorker(LaserBeamManager managerBase)
            {
                _managerBase = managerBase;
            }

            public void Run()
            {
                _stop = false;

                while (!_stop)
                {
                    try
                    {
                        Thread.Sleep(20);
                        if (_managerBase._pad != null)
                        {
                            _managerBase._pad.Poll();

                            /*
                            // Pour trouver les boutons du gamepad
                            for(int i = 0; i < 100; i++)
                            {
                                if (manager.pad.idButtonPressed(i))
                                {
                                    Console.Out.WriteLine("Button on: " + i);
                                }
                            }
                            */

                            if (_managerBase._pad.IdButtonPressed(_managerBase.TriggerButton)) // Index du laser.
                            {
                                _managerBase.InvokeTrigger();
                                Thread.Sleep(1000);
                            }
                        }
                        else
                        {
                            _stop = true;
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        _stop = true;
                    }
                    catch (ThreadStateException)
                    {
                        _stop = true;
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }

                }
            }

            internal void Stop()
            {
                _stop = true;
            }
        }

        private DetectorGamePad _pad;
        private LaserBeamManagerWorker _worker;
        private Thread _fil;
        private DirectInput _dinput;

        /// <summary>
        /// Constructeur principal.
        /// </summary>
        public LaserBeamManager(string gamepadName, int buttonTriggerId)
        {
            GamepadName = gamepadName;
            TriggerButton = buttonTriggerId;
        }

        /// <summary>
        /// Constructeur vide.
        /// </summary>
        public LaserBeamManager()
        {
            _worker = new LaserBeamManagerWorker(this);
        }

        /// <summary>
        /// Une portion (unique si possible) du nom du pilote de manette pour identifier
        /// le bon pilote à chercher.
        /// </summary>
        public string GamepadName
        {
            get;
            set;
        }

        /// <summary>
        /// Le numéro du bouton qui agit comme déclencheur pour le manager.
        /// </summary>
        public int TriggerButton
        {
            get;
            set;
        }

        /// <summary>
        /// Destructeur.
        /// </summary>
        ~LaserBeamManager()
        {
            Dispose();
        }

        /// <summary>
        /// Détruit toutes les ressources systèmes de cette instance.
        /// Si un suivi de manette est en cours, il sera stoppé au préalable.
        /// </summary>
        public override void Dispose()
        {
            Stop();

            if (_pad != null)
            {
                _pad.Dispose();
                _pad = null;
            }

            if (_dinput != null)
            {
                _dinput.Dispose();
                _dinput = null;
            }
        }

        /// <summary>
        /// Indique si l'instance est présentement en train de surveiller le laser.
        /// </summary>
        /// <returns></returns>
        public override bool IsRunning
        {
            get => _fil != null && _fil.IsAlive;
            protected set
            {
                // NO-OP
            }
        }

        public override bool IsInitialized
        {
            get => _pad != null;
            protected set
            {
                // NO-OP
            }
        }

        /// <summary>
        /// Initialise le lien entre le matériel du laser ("Nostromo n40 Gamepad" par USB) et l'instance.
        /// </summary>
        /// <returns>TRUE si la matériel est connecté et prêt à être utilisé.</returns>
        public override bool Init()
        {
            if (_pad == null)
            {
                FindGamePad();
            }
            return _pad != null;
        }

        /// <summary>
        /// Démarre un fil d'exécution qui vérifie périodiquement si le laser est coupé.
        /// <br/>Ne démarre que si la méthode init() retourne TRUE.
        /// </summary>
        public override void Start()
        {
            if (!Init() || IsRunning) return;
            if (_worker == null)
                _worker = new LaserBeamManagerWorker(this);

            if (_fil == null)
            {
                _fil = new Thread(_worker.Run) {Name = "LaserBeamManagerWorker"};
            }

            _fil.Start();
        }

        /// <summary>
        /// Stoppe la vérification, mais ne détruit pas les informations de la manette.
        /// <br/>Ne fait rien si la vérification n'est pas en cours.
        /// </summary>
        public override void Stop()
        {
            if (_fil == null || _worker == null) return;
            _worker.Stop();
            _worker = null;
            _fil = null;
        }
        
        public override string DefaultConfigFileName { get; } = "trigger_laserbeam_config.json";

        // /// <summary>
        // /// Délégué pour la méthode de "callback" lorsque le rayon est coupé.
        // /// </summary>
        // public delegate void LaserBeamInterruptedHandler();
        //
        // /// <summary>
        // /// Événement appellé lorsque le rayon est coupé.
        // /// </summary>
        // public event LaserBeamInterruptedHandler BeamOccluded;

        /// <summary>
        /// Cherche une manette de jeu avec le nom du pilote du laser.
        /// </summary>
        /// <returns></returns>
        private void FindGamePad()
        {
            if (_pad != null)
            {
                _pad.Dispose();
                _pad = null;
            }

            if (_dinput == null)
            {
                _dinput = new DirectInput();
            }
            foreach (var di in _dinput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly).Where(di => di.InstanceName.Contains(GamepadName)))
            {
                _pad = new DetectorGamePad
                {
                    Guid = di.InstanceGuid,
                    Name = di.InstanceName
                };
                _pad.Acquire(_dinput);
            }

        }
    }
}
