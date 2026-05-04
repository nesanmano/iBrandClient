using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NomadLogic.Common.Util;
using NomadLogic.PrintStation.Json;

namespace NomadLogic.PrintStation.Ui
{
    public class SearchPicturesControl : GalleryControl
    {
        private readonly string _userName;
        protected Rectangle _clear_search_area;
        protected Image _clear_search_button;

        public SearchPicturesControl(string userName, PrintStationUser userSession, int width, int height) : base(userSession, width, height)
        {
            _userName = userName;
            _searchTextbox.Text = _userName;
        }

        protected override void PaintEvent(object sender, PaintEventArgs e)
        {
            base.PaintEvent(sender, e);

            if (_clear_search_button != null)
            {
                e.Graphics.DrawImageUnscaled(_clear_search_button, _clear_search_area.Location);
            }
        }

        protected override void AddSearchElements()
        {
            var conf = RunningFeatureConfig.GetInstance();
            var panelHeight = conf.GetConfiguration<int>(FeatureConfigEnum.GallerySearchPanelHeight);

            var seacrhTextFileName = UiFilesUtil.GetFileName(UiFilesUtil.SearchText, _user.Id);
            var clearSearchButtonFileName = UiFilesUtil.GetFileName(UiFilesUtil.ClearSearchButton, _user.Id);

            if (!File.Exists(seacrhTextFileName) || !File.Exists(clearSearchButtonFileName)) return;

            _searchText = FileUtil.LoadBitmap(seacrhTextFileName);

            _searchTextbox = new TextBox
            {
                Size = new Size(400, 40),
                Font = new Font(FontFamily.GenericSansSerif, 20)
            };
            _searchTextbox.Click += OnTextBoxGotFocus;
            _searchTextbox.LostFocus += OnTextBoxLostFocus;

            _clear_search_button = FileUtil.LoadBitmap(clearSearchButtonFileName);

            var x = Width/2 - (_searchText.Width + 5 + _searchTextbox.Width + 5 + _clear_search_button.Width)/2;
            var y = (panelHeight / 2) - (_searchText.Height/2);

            _searchTextLocation = new Point(x, y);

            x = _searchTextLocation.X + _searchText.Width + 5;
            y = (panelHeight / 2) - (_searchTextbox.Height/2);

            _searchTextbox.Location = new Point(x, y);

            x = _searchTextbox.Location.X + _searchTextbox.Width + 5;
            y = (panelHeight / 2) - (_clear_search_button.Height/2);

            _clear_search_area = new Rectangle(new Point(x, y), _clear_search_button.Size);
        }

        protected override void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (InArea(e.X, e.Y, _previousArea))
            {
                if (!_previousButtonVisible) return;

                long minId = 0;
                if (_postsToShow.Count > 0)
                    minId = _postsToShow[0].Id;

                _postsToShow = _database.GetPosts(_userName, minId - 1, false, _nbrPictureByPage,
                    _user.EventId);
                _postsToShow = _postsToShow.OrderBy(post => post.Id).ToList();

                ShowPictures(_postsToShow);

                ShowNavigationButtons();
            }
            else if (InArea(e.X, e.Y, _nextArea))
            {
                if (!_nextButtonVisible) return;

                long maxId = 0;
                if (_postsToShow.Count > 0)
                    maxId = _postsToShow[_postsToShow.Count - 1].Id;

                _postsToShow = _database.GetPosts(_userName, maxId + 1, true, _nbrPictureByPage,
                    _user.EventId);
                _postsToShow = _postsToShow.OrderBy(post => post.Id).ToList();

                ShowPictures(_postsToShow);

                ShowNavigationButtons();
            }
            else if (InArea(e.X, e.Y, _clear_search_area))
            {
                _onRemovePicturesControl?.Invoke(this);
            }
        }

        public void InitPostsToShow(List<Post> postToShow)
        {
            _postsToShow = postToShow;

            _minId = _database.GetMinPostId(_userName, _user.EventId);

            if (_postsToShow.Count > 0)
            {
                _maxId = _postsToShow[_postsToShow.Count - 1].Id;
            }
        }
    }
}