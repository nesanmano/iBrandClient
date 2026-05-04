using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Math;
using NomadLogic.Common.Util.Win32;
// ReSharper disable StaticMemberInGenericType

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    [PublicAPI]
    public class PaginationUiElements<T> : IOnLoad, IPaint, IDisposable, IChangeLanguage, IEvaluateScreenZones, IHasDefaultPropertyValue<FontDefinitionInfo> where T : IComparable<T>
    {
        /// <summary>
        /// Classe pour afficher un nombre dans une zone de texte. Ce nombre change en fonction de l'index de la page représentée par la zone.
        /// Comme la page affichée change en fonction de la page actuelle, cette valeur est à recalculer très souvent, et doit donc appartenir à
        /// la classe qui gère les éléments de pagination UI.
        /// </summary>
        private class ScreenClickZoneCondensedPagination : ScreenClickZoneText
        {
            public bool IsZeroBased { get; set; }
            public int PaginationOffset { get; set; }
            public IPaginationMonitor<T> PaginationMonitor { get; set; }

            public override bool SkipValidation
            {
                get => true;
                set
                {
                    // nothing
                }
            }

            public override bool OnLoad(AbstractScreen screen)
            {
                if (!base.OnLoad(screen)) return false;
                OnClick = () =>
                {
                    var target = (uint) (PaginationMonitor.CurrentPage + PaginationOffset);
                    log.Debug("Offset page UI clicked. Going to page {0}", target);
                    PaginationMonitor.GotoPage(target);
                };
                return true;
            }

            public override bool IsVisible
            {
                get
                {
                    try
                    {
                        var outOfBound = PaginationMonitor.CurrentPage + PaginationOffset;
                        var vi = outOfBound >= 0 && outOfBound <= PaginationMonitor.MaxPage;
                        if (log.IsTraceEnabled)
                        {
                            log.Trace("Condensed pagination control '{0}' visibility check (current: {3}, bounds: [0..{1}]): {2}", PaginationOffset, PaginationMonitor.MaxPage, vi, outOfBound);
                        }

                        return vi;
                    }
                    catch (System.Exception e)
                    {
                        log.Error(e);
                        return false;
                    }
                }
                set { } // On ne peut pas indiquer si cet élément est invisible: c'est dynamique.
            }

            // ReSharper disable once UnusedMember.Local
            public override string GetCurrentText()
            { 
                var indexValue = PaginationMonitor.CurrentPage + PaginationOffset;
                if (!IsZeroBased) indexValue++;
                return indexValue.ToString();
            }
        }

        // ReSharper disable once InconsistentNaming
        private static readonly Logger log;

        static PaginationUiElements()
        {
            log = LogManager.GetCurrentClassLogger();
        }

        public const string PREVIOUS_CHARACTER = "◄";
        public const string NEXT_CHARACTER = "►";
        public const string FIRSTPAGE_CHARACTER = "«";
        public const string LASTPAGE_CHARACTER = "»";

        private IPaginationMonitor<T> _paginationMonitor;
        private ScreenClickZone[] _condensedClickZone;
        private int _previousCondensedIndex;
        private int _nextCondensedIndex;
        private AbstractScreen _screen;
        private volatile bool _disabled;
        
        public ScreenClickZone PreviousZone { get; set; }

        public ScreenClickZone NextZone { get; set; }

        public Rectangle? CondensedPaginationControls { get; set; }

        public bool CondensedUseFullJumps { get; set; }

        public ushort CondensedNumberOfPagesOptions { get; set; } = 5;

        public FontDefinitionInfo DefaultFontDefinition { get; set; }

        public string CondensedFirstCommandImagePath { get; set; }

        public string CondensedPreviousCommandImagePath { get; set; }

        public string CondensedNextCommandImagePath { get; set; }

        public string CondensedLastCommandImagePath { get; set; }

        /// <summary>
        /// Indique si on doit afficher les page selon un index à base zéro (comme calculé à l'interne).
        /// Si faux, on assume que c'est pour un public qui compte à partir de 1 (comme l'ensemble des mortels).
        /// Cette valeur est fausse par défaut.
        /// </summary>
        public bool ShowZeroBased { get; set; }

        /// <summary>
        /// Indique que les zones doivent être invisible et ne pas réagir.
        /// Utilisé lors de chargements visuels pour que cette instance se désactive temporairerment.
        /// </summary>
        /// <param name="disable">Si TRUE, désactive les visuels et la logique de clic.</param>
        public void DisableSwitch(bool disable)
        {
            if (_disabled == disable) return;
            log.Trace("Disabled switch for pagination UI instance set to: {0}", disable);
            _disabled = disable;
        }
        
        public bool OnLoad(AbstractScreen screen)
        {
            _screen = screen;

            _paginationMonitor = ClassUtil.GetFirstValidPropertyValueFromInstance<IPaginationMonitor<T>>(screen);
            if (_paginationMonitor == null)
            {
                log.Error("No properties in screen parent are Pagination monitors.");
                return false;
            }
            _paginationMonitor.UiRefresh += PaginationMonitorOnUiRefresh;

            if (PreviousZone != null)
            {
                PreviousZone.OnClick = () =>
                {
                    if (!_paginationMonitor.HasPreviousPage) return;

                    var current = _paginationMonitor.CurrentPage;
                    if (log.IsTraceEnabled)
                    {
                        log.Trace("Going to page {0} from {1}", current - 1, current);
                    }
                    _paginationMonitor.GotoPage(current - 1);
                };
                if (!PreviousZone.OnLoad(screen))
                {
                    log.Error("Could not load previous zone instance.");
                    return false;
                }

                PreviousZone.SkipValidation = true;
            }

            if (NextZone != null)
            {
                NextZone.OnClick = () =>
                {
                    if (!_paginationMonitor.HasNextPage) return;

                    var current = _paginationMonitor.CurrentPage;
                    if (log.IsTraceEnabled)
                    {
                        log.Trace("Going to page {0} from {1}", current + 1, current);
                    }
                    _paginationMonitor.GotoPage(current + 1);
                };
                if (!NextZone.OnLoad(screen))
                {
                    log.Error("Could not load next zone instance.");
                    return false;
                }

                NextZone.SkipValidation = true;
            }

            // Charge Labels de la pagination condensée
            if (CondensedPaginationControls == null || _condensedClickZone != null) return true;

            log.Debug("Initializing condensed controls.");

            // Minimum trois, et nombre impair.
            var count = CondensedNumberOfPagesOptions;
            if (count < 5) count = 5;
            if (CondensedUseFullJumps) count += 2;
            if (count % 2 == 0) count++;

            var condensedCoords = MathUtil.CalculatePresentationCoordinatesOfSubElements(new Size(count, 1), CondensedPaginationControls.Value, 0, 1);
            if (condensedCoords.Count != count)
            {
                log.Error("Could not calculate positions of condensed pagination labels. Got {0}, needed {1}", condensedCoords.Count, count);
                return false;
            }
            if (condensedCoords.Any(c => c.Size.Width == 0 || c.Size.Height == 0))
            {
                log.Error("Positions of labels are zero. Postion generated:\n\t" + StringUtil.FormatValueEnumerable(condensedCoords, true, 1));
                return false;
            }



            log.Trace("Creating {0} condensed controls.", count);

            if (DefaultFontDefinition == null)
            {
                log.Trace("Creating default font definition with dynamic size.");
                DefaultFontDefinition = new FontDefinitionInfo
                {
                    Color = Color.White,
                    Size = (int) (condensedCoords.First().Width * 0.8),
                    FamilyName = FontFamily.GenericMonospace.Name
                };
            }

            _previousCondensedIndex = CondensedUseFullJumps ? 1 : 0;
            var lastIndex = condensedCoords.Count - 1;
            _nextCondensedIndex = CondensedUseFullJumps ? lastIndex - 1 : lastIndex;
            var midIndex = (int)Math.Floor(condensedCoords.Count / 2.0);

            if (log.IsTraceEnabled)
            {
                log.Trace("Condensed indexes: UseFullJump={0}, Previous index={1}, Next index={2}, Mid Index={3}", CondensedUseFullJumps, _previousCondensedIndex, _nextCondensedIndex, midIndex);
            }

            _condensedClickZone = new ScreenClickZone[count];
            
            // Juste les zones de numéro de page pour l'instant.
            for(var i = _previousCondensedIndex; i < _nextCondensedIndex; i++)
            {
                var offset = i - midIndex;
                var newInst = new ScreenClickZoneCondensedPagination
                {
                    PaginationOffset = offset,
                    PaginationMonitor = _paginationMonitor,
                    IsZeroBased = ShowZeroBased,
                    FontDefinition = DefaultFontDefinition,
                    Zone = condensedCoords[i],
                    SkipValidation = true
                };

                _condensedClickZone[i] = newInst;
                log.Trace("Created new Condensed pagination element: Zone={0}", newInst.Zone);
            }

            // On assigne les fonctions spéciales de chaque.
            if (CondensedUseFullJumps)
            {
                // LAST et FIRST
                InitCondensedScreenZone(condensedCoords, CondensedFirstCommandImagePath, 0, FIRSTPAGE_CHARACTER, () =>
                {
                    log.Debug("First page UI clicked.");
                    _paginationMonitor.GotoPage(0);
                });

                InitCondensedScreenZone(condensedCoords, CondensedLastCommandImagePath, count-1, LASTPAGE_CHARACTER, () =>
                {
                    log.Debug("Last page UI clicked.");
                    _paginationMonitor.GotoPage(_paginationMonitor.MaxPage);
                });
            }
            
            // Previous et Next
            InitCondensedScreenZone(condensedCoords, CondensedPreviousCommandImagePath, _previousCondensedIndex, PREVIOUS_CHARACTER, () =>
            {
                log.Debug("Previous page UI clicked.");
                if (!_paginationMonitor.HasPreviousPage) return;
                _paginationMonitor.GotoPage(_paginationMonitor.CurrentPage - 1);
            });

            InitCondensedScreenZone(condensedCoords, CondensedNextCommandImagePath, _nextCondensedIndex, NEXT_CHARACTER, () =>
            {
                log.Debug("Next page UI clicked.");
                if (!_paginationMonitor.HasNextPage) return;
                _paginationMonitor.GotoPage(_paginationMonitor.CurrentPage+1);
            });
            

            foreach (var ccz in _condensedClickZone.Where(ccz => !ccz.OnLoad(screen)))
            {
                log.Error("Could not initialize condensed element: " + ccz);
                return false;
            }

            log.Trace("Initialization of condensed pagination UI completed.");

            return true;
        }

        private void InitCondensedScreenZone(List<Rectangle> coords, string path, int tabIndex, string textGlyph, Action onClick)
        {
            ScreenClickZone cz = null;

            if (!string.IsNullOrEmpty(path))
            {
                if (!File.Exists(path))
                {
                    log.Warn("Could not find image file for condensed command. Reverting to text mode. File Path: " + path);
                }
                else
                {
                    cz = new ScreenClickZoneImage
                    {
                        ImageFile = path,
                        ForceResizeOfImage = true
                    };
                }
            }

            if (cz == null)
            {
                cz = new ScreenClickZoneText
                {
                    TextDictionary = new Dictionary<string, string> {{"en", textGlyph}},
                    FontDefinition = DefaultFontDefinition
                };
            }

            cz.Zone = coords[tabIndex];
            cz.IsVisible = true;
            cz.OnClick = onClick;
            cz.SkipValidation = true;

            _condensedClickZone[tabIndex] = cz;
        }

        private void PaginationMonitorOnUiRefresh(object sender, EventArgs e)
        {
            log.Trace("UI refresh needed.");
            if (PreviousZone != null)
            {
                if (log.IsTraceEnabled)
                {
                    if (PreviousZone.IsVisible != _paginationMonitor.HasPreviousPage)
                    {
                        log.Trace("Previous zone visibility changed to: " + _paginationMonitor.HasPreviousPage);
                    }
                }
                PreviousZone.IsVisible = _paginationMonitor.HasPreviousPage;
            }
            if (NextZone != null)
            {
                if (log.IsTraceEnabled)
                {
                    if (NextZone.IsVisible != _paginationMonitor.HasNextPage)
                    {
                        log.Trace("Next zone visibility changed to: " + _paginationMonitor.HasNextPage);
                    }
                }
                NextZone.IsVisible = _paginationMonitor.HasNextPage;
            }

            _screen.SafeCall(() => _screen.Refresh());
        }

        public void Paint(Graphics g, bool debug = false)
        {
            if (debug) ScreenUtil.DrawDebugRectangle(g, ScreenUtil.DebugBlackPen, CondensedPaginationControls);

            if (_disabled)
            {
                if (debug)
                {
                    ScreenUtil.DrawDebugInactiveRectangle(g, ScreenUtil.DebugBlackPen, PreviousZone?.Zone);
                    ScreenUtil.DrawDebugInactiveRectangle(g, ScreenUtil.DebugBlackPen, NextZone?.Zone);
                    ScreenUtil.DrawDebugInactiveRectangle(g, ScreenUtil.DebugBlackPen, CondensedPaginationControls);
                }
                return;
            }

            PreviousZone?.Paint(g, debug);
            NextZone?.Paint(g, debug);
            if (_condensedClickZone == null) return;
            log.Trace("Paint call: condensed elements");
            foreach (var ccz in _condensedClickZone)
            {
                ccz.Paint(g, debug);
            }
        }

        public bool GetDefaultPropertyValue(out FontDefinitionInfo toInitialize)
        {
            toInitialize = DefaultFontDefinition;
            return DefaultFontDefinition != null;
        }
        
        public void Dispose()
        {
            PreviousZone?.Dispose();
            NextZone?.Dispose();
            if (_condensedClickZone == null) return;
            foreach (var ccz in _condensedClickZone)
            {
                ccz.Dispose();
            }
        }

        public ScreenEvaluationResult EvaluateZones(AbstractScreen screen, int x, int y, Func<ScreenEvaluationResult> validationMethod)
        {
            if (_disabled)
            {
                return new ScreenEvaluationResult();
            }

            var results = new List<ScreenEvaluationResult>
            {
                PreviousZone?.EvaluateZones(screen, x, y, validationMethod), 
                NextZone?.EvaluateZones(screen, x, y, validationMethod)
            };

            if (_condensedClickZone == null) return ScreenEvaluationResult.AggregateResults(results);

            var tmp = _condensedClickZone.Select(cz => cz.EvaluateZones(screen, x, y, validationMethod));
            results.AddRange(tmp);

            return ScreenEvaluationResult.AggregateResults(results);
        }

        public void ChangeLanguage(AbstractScreen screen, string languageCode)
        {
            PreviousZone?.ChangeLanguage(screen, languageCode);
            NextZone?.ChangeLanguage(screen, languageCode);
        }
    }
}