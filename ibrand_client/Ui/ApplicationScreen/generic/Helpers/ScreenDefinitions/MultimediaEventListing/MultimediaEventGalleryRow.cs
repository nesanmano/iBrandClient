using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Win32;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions.MultimediaEventListing
{

    /// <summary>
    /// Représente une seule ligne d'éléments d'un événement multimédia (composé de plusieures images, ainsi que possiblement d'autre éléments non visualisé).
    /// </summary>
    public class MultimediaEventGalleryRow : IComparable<MultimediaEventGalleryRow>, IDisposable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private Control _parent;
        private TableLayoutPanel _flowControl;
        private PictureBox[] _displayPictureBox;
        private readonly DateTime _creationDate;
        private readonly string _dateLabelText;
        
        private readonly MultimediaEventItemPresentation _presentationData;
        private volatile bool _quickSelectionState;
        private bool _atLeastOneCanBeSelected;
        private Panel _mainPanel;
        private MultimediaEventGalleryRowItem[] _items;

        public event EventHandler<object> SelectionChangedEvent; 

        public MultimediaEventGalleryRow([NotNull] Control parent,
            [NotNull] MultimediaEventItemPresentation presentationData,
            DateTime creationDate)
        {
            _parent = parent;
            _dateLabelText = creationDate.ToString(presentationData.CustomTimeFormat);
            _creationDate = creationDate;
            _presentationData = presentationData;
        }

        /// <summary>
        /// Si on re¸oit ce "callback" d'un événement, il faut refaire le thumbnail de présentation pour cet item.
        /// </summary>
        /// <param name="sender">Doit être un type précis (MultimediaEventGalleryRowItem)</param>
        /// <param name="e">On s'en fout.</param>
        public void RowItemUpdate(object sender, EventArgs e)
        {
            if (!(sender is MultimediaEventGalleryRowItem megi))
            {
                var typez = sender?.GetType().FullName ?? "NULL";
                log.Trace("Sender is not a valid class type. Expecting a MultimediaEventGalleryRowItem, but was a: {0}", typez);
                return;
            }

            log.Trace("Received event to recreate thumbnail for row item: Date='{0}',Index={1}", _dateLabelText, megi.LayoutIndex);
            CreateImageForPBox(megi, _presentationData);
        }

        public bool Init(params MultimediaEventGalleryRowItem[] images)
        {
            if (_parent == null) return false; // Dispose a été appellé.
            if (_parent.InvokeRequired)
            {
                throw new System.Exception("You  must call this method from the UI thread. Review your calls safety process.");
            }

            // Création des éléments de base
            var flowOffset = _presentationData.DateFontDefinition != null ? _presentationData.DateFontDefinition.Size + 3 : 0;
            _mainPanel = new Panel
            {
                Size = new Size(_presentationData.LayoutRectangle.Width, _presentationData.ThmbSize.Height + flowOffset)
            };

            // Création du "label" de l'horodatage.
            if (_presentationData.DateFontDefinition != null)
            {
                var font = StaticCustomFontRepository.CreateCustomFont(_presentationData.DateFontDefinition);
                var dateOffset = _presentationData.QuickSelectionConfig?.Size.Width ?? 0;
                var dateLabel = new Label
                {
                    Text = _dateLabelText,
                    AutoSize = true,
                    Font = font,
                    Location = new Point(dateOffset, 0),
                    ForeColor = _presentationData.DateFontDefinition.Color
                };
                _mainPanel.Controls.Add(dateLabel);
            }

            _flowControl = new TableLayoutPanel
            {
                AutoScroll = false,
                AutoSize = true,
                RowCount = 1,
                GrowStyle = TableLayoutPanelGrowStyle.AddColumns,
                Visible = true,
                Enabled = true,
                Location = new Point(0,flowOffset)
            };
            _flowControl.RowStyles.Add(new RowStyle
            {
                SizeType = SizeType.AutoSize,
                Height = _presentationData.ThmbSize.Height
            });
            _mainPanel.Controls.Add(_flowControl);

            if (images == null ||  images.Length < 1) return false;
            log.Trace("Initializing Event image Group for date: {0}", _dateLabelText);

            _items = images;
            _flowControl.Controls.Clear();
            _flowControl.ColumnStyles.Clear();
            _flowControl.ColumnCount = 0;

            _displayPictureBox = new PictureBox[images.Length];
            _quickSelectionState = false;
            
            _atLeastOneCanBeSelected = false;
            for(var i = 0; i < images.Length; i++)
            {
                var megi = images[i];
                _atLeastOneCanBeSelected |= !megi.CannotBeSelected;
                megi.LayoutIndex = i;
                
                var pBox = new PictureBox
                {
                    Size = _presentationData.ThmbSize
                };
                _displayPictureBox[i] = pBox;

                // TODO Switch avec la logique de click

                pBox.MouseClick += (sender, args) =>  ShowFullImageDialog(megi, images);
                
                Task.Run(() => CreateImageForPBox(megi, _presentationData));
            }
            
            // Vérifie si on doit utiliser un bouton de sélection multiple pour la ligne.
            Panel quickSelectPanel = null;
            if (_presentationData.QuickSelectionConfig != null)
            {
                var qsc = _presentationData.QuickSelectionConfig;
                quickSelectPanel = new Panel
                {
                    Size = new Size(qsc.Size.Width, _presentationData.ThmbSize.Height),
                    AutoSize = false
                };
                var qsPbox = new PictureBox
                {
                    Size = qsc.Size,
                    Location = new Point(0, (_presentationData.ThmbSize.Height - qsc.Size.Height)/2)
                };

                // Si TOUT les éléments ne peuvent être sélectionnés, il n'y aura pas de event associé, car changer l'état devient inutile.
                if (_atLeastOneCanBeSelected)
                {
                    qsPbox.MouseClick += (sender, args) =>
                    {
                        _quickSelectionState = !_quickSelectionState;
                        log.Trace("Quick Selection state changed to: {0}", _quickSelectionState);
                        DoChangeInState(qsPbox, qsc, images);
                    };

                    void Action() { DoChangeInState(qsPbox, qsc); } // Initialisation

                    qsPbox.SafeCall(Action);
                    log.Trace("Enabled quick selection of all event line.");
                }
                else
                {
                    // Ne changera jamais sinon.
                    var bmpPath = Path.Combine(_presentationData.AssetsDirectory, qsc.SelectionUnselectedImageFileName);
                    qsPbox.Image = FileUtil.LoadImage(bmpPath);
                    log.Trace("Quick selection disabled because all images are unselectable.");
                }
                
                quickSelectPanel.Controls.Add(qsPbox);
            }

            // QuickSelect
            if (quickSelectPanel != null)
            {
                _flowControl.ColumnCount++;
                _flowControl.ColumnStyles.Add(new ColumnStyle
                {
                    SizeType = SizeType.AutoSize
                });
                _flowControl.Controls.Add(quickSelectPanel);
            }

            // Ajout des éléments dans le FlowTableLayout
            foreach (var tmp in _displayPictureBox)
            {
                _flowControl.ColumnCount++;
                _flowControl.ColumnStyles.Add(new ColumnStyle
                {
                    SizeType = SizeType.AutoSize
                });
                _flowControl.Controls.Add(tmp);
            }
            log.Trace("Initialising finished for group dated: {0}", _dateLabelText);

            if (_presentationData.CenterRow)
            {
                var finalSize = _flowControl.Size.Width;
                var contSize = _mainPanel.Size.Width;
                _flowControl.Location = new Point((contSize - finalSize)/2, 0);
            }

            return true;
        }

        private delegate void DoChangeInStateDelegate(PictureBox qsPbox, MultimediaEventItemPresentationQuickSelection qsc, MultimediaEventGalleryRowItem[] images = null);

        private void DoChangeInState(PictureBox qsPbox, MultimediaEventItemPresentationQuickSelection qsc, MultimediaEventGalleryRowItem[] images = null)
        {
            if (qsPbox.InvokeRequired)
            {
                var dele = new DoChangeInStateDelegate(DoChangeInState);
                qsPbox.Invoke(dele, qsPbox, qsc, images);
            }

            var bmpPath = Path.Combine(_presentationData.AssetsDirectory, (_quickSelectionState ? qsc.SelectionActivatedImageFileName : qsc.SelectionUnselectedImageFileName));
            var bmp = FileUtil.LoadImage(bmpPath);
            var old = qsPbox.Image;
            qsPbox.Image = bmp;
            old?.Dispose();

            if (images == null) return;
            foreach (var megi in images)
            {
                if (megi.CannotBeSelected) continue;
                megi.Selected = !megi.Selected;
                CreateImageForPBox(megi, _presentationData);
            }
            SelectionChangedEvent?.Invoke(this, null);
        }
        
        private void CreateImageForPBox([NotNull] MultimediaEventGalleryRowItem megi, MultimediaEventItemPresentation presentationData)
        {
            var box = _displayPictureBox[megi.LayoutIndex];
            
            try
            {
                var bmp = megi.GenerateThumbnailImage(presentationData.ThmbSize, presentationData);

                box.SafeCall(() =>
                {
                    var old = box.Image;
                    if (ReferenceEquals(old, bmp)) return;
                    box.Image = bmp;
                    old?.Dispose();
                    box.Refresh();
                    log.Trace("Date='{0}',Index={1}, image refreshed.", _dateLabelText, megi.LayoutIndex);
                });
            }
            catch (System.Exception e)
            {
                log.Error(e, "Could not Create image for display box.");
            }
        }

        private void ShowFullImageDialog([NotNull] MultimediaEventGalleryRowItem megiSelected, MultimediaEventGalleryRowItem[] images)
        {
            if (_flowControl == null) return;
            if (!megiSelected.CanShowFullDialog())
            {
                // L'item refuse la demande de présentation, donc on bloque le changement.
                log.Trace("Skipped full dialog event: Not all files are ready for selected row item: Date='{0}',Index={1}", _dateLabelText, megiSelected.LayoutIndex);
                return;
            }

            Form diag = null;
            switch (megiSelected.OriginalFileType)
            {
                case MultimediaEventGalleryRowItemFileType.Image:
                    switch (_presentationData.FullScreenViewPresentation)
                    {
                        case MultimediaEventItemPresentationDetailSelector multiple:
                        {
                            diag = new MultimediaEventItemSelectorDialog(images, multiple);
                            break;
                        }
                        case MultimediaEventItemPresentationDetailSingle single:
                        {
                            diag = new MultimediaEventItemSingleDialog(megiSelected, single);
                            break;
                        }
                    }
                    break;
                case MultimediaEventGalleryRowItemFileType.Vimeo:
                    var vimeoViewConfig = _presentationData.FullScreenVimeoPresentation;

                    if (vimeoViewConfig == null)
                    {
                        log.Warn("No Vimeo presentation config found. Review configuration details. Date='{0}',Index={1}", _dateLabelText, megiSelected.LayoutIndex);
                        return;
                    }

                    var nomadConfig = ServicesContainer.GetService<NomadConfiguration>();

                    diag = new MultimediaEventItemVimeoDialog(megiSelected, vimeoViewConfig, nomadConfig.CurrentServerConfiguration);
                    break;
                default:
                    throw new System.Exception("No custom presentation exist for item type. Check code.");
            }
            
            if (diag == null)
            {
                if (!megiSelected.CannotBeSelected)
                {
                    var currState = megiSelected.Selected;
                    megiSelected.Selected = !currState;
                    Task.Run(() => CreateImageForPBox(megiSelected, _presentationData));
                }
            }
            else
            {
                var result = diag.ShowDialog(_flowControl);
                if (result == DialogResult.OK)
                {
                    foreach (var megi in images)
                    {
                        Task.Run(() => CreateImageForPBox(megi, _presentationData));
                    }
                }
            }

            SelectionChangedEvent?.Invoke(this, null);
        }

        public int CompareTo(MultimediaEventGalleryRow other)
        {
            if (ReferenceEquals(this, other)) return 0;
            return other is null ? 1 : _creationDate.CompareTo(other._creationDate);
        }

        public Control GetControlToDisplay()
        {
            return _mainPanel;
        }

        public void Dispose()
        {
            _parent = null;
            _mainPanel?.Dispose();
            _mainPanel = null;
            _flowControl?.Dispose();
            _flowControl = null;
        }

        public int GetSelectedNumber()
        {
            if (_items == null) return 0;
            return _items.Sum(i => i.Selected ? 1 : 0);
        }

        /// <summary>
        /// Génère des nouvelles valeurs de groupes dynamiques à partir des éléments sélectionnées et les valeurs d'originale.
        /// Créé un groupe par rangé, avec valeurs multiples. Si aucun élément n'est sélectionné dans cette rangée, retourne un groupe null.
        /// </summary>
        /// <param name="overwriteWithLocalFilePath">Si voulu, on peut remplacer les valeurs de fichiers originaux par leur copie locale.</param>
        /// <returns>Un groupe avec au moins une valeur, sinon null.</returns>
        public DynamicHeader GetDynamicHeaderToSave(bool overwriteWithLocalFilePath)
        {
            DynamicHeader res = null;
            
            foreach (var megi in _items)
            {
                if (!megi.Selected) continue;

                if (res == null)
                {
                    res = new DynamicHeader
                    {
                        DateCreatedStr = megi.OriginalDynamicGroupCreationDate,
                        DataKey = megi.OriginalDynamicGroupDataKey,
                        DataValues = new List<DataValue>()
                    };
                }
                
                if (!overwriteWithLocalFilePath)
                {
                    // Valeur originale
                    res.DataValues.Add(new DataValue(megi.OriginalDynamicValue));
                }
                else
                {
                    // Si fichier local, on remplace
                    res.DataValues.Add(megi.OriginalFileType == MultimediaEventGalleryRowItemFileType.Image ? 
                        new DataValue(megi.FixedMainData) : 
                        new DataValue(megi.OriginalDynamicValue));
                }
            }

            return res;
        }

        /// <summary>
        /// Réinitialize la sélection des éléments.
        /// </summary>
        /// <returns>Le nombre d'éléments qui ont changé d'État</returns>
        public int ResetSelection()
        {
            var nbr = 0;
            foreach (var item in _items)
            {
                if (!item.Selected) continue;
                item.SuspendEvents = true;
                item.Selected = false;
                item.SuspendEvents = false;
                nbr++;
            }
            return nbr;
        }
    }
}