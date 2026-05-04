using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;

namespace NomadLogic.iBrandDesktop.Ui
{
    public class TouchScreenControllerWhitoutProject
    {
        readonly List<AbstractScreen> _screenList = new List<AbstractScreen>();

        AbstractScreen _activeScreen;

        readonly Panel _screenPanel;

        public TouchScreenControllerWhitoutProject(Panel screenPanel)
        {
            _screenPanel = screenPanel;
        }

        private void OnNextScreen(int nbrPage)
        {
            if (_activeScreen == null)
                return;

            var index = _screenList.IndexOf(_activeScreen);

            if (index == _screenList.Count - 1)//Last page
            {
                RecycleScreens();

                ShowFirstScreen();

                return;
            }

            index = nbrPage + index;

            SetNextScreen(index);
        }

        private void RecycleScreens()
        {
            foreach (var screen in _screenList)
            {
                screen.Recycle();
            }
        }

        public void ShowFirstScreen()
        {
            _activeScreen = _screenList[0];

            _activeScreen.SetActivated();
            _activeScreen.BringToFront();
            _activeScreen.Invalidate();
            _screenPanel.Invalidate();
        }

        public void AddScreen(AbstractScreen screen)
        {
            _screenList.Add(screen);

            screen.Visible = true;

            screen.OnFirstScreen += OnFirstScreen;
            screen.OnNext += OnNextScreen;
            screen.OnBack += OnBackScreen;
            screen.OnGoTo += OnGoToScreen;

            var lsi = screen as ILanguageScreenInterface;
            if (lsi != null)
            {
                lsi.OnLanguageSelection += OnLanguageSelection;
            }
        }

        private void SetNextScreen(int index)
        {
            if (index < _screenList.Count)
            {
                _activeScreen = _screenList[index];
                _activeScreen.SetActivated();
                _activeScreen.BringToFront();
                _activeScreen.Invalidate();
                _screenPanel.Invalidate();
            }
        }

        public void OnFirstScreen(object sender, EventArgs e)
        {
            RecycleScreens();

            ShowFirstScreen();
        }

        private void OnBackScreen(int nbrPage)
        {
            if (_activeScreen == null)
                return;

            var index = _screenList.IndexOf(_activeScreen);

            index = index - nbrPage;

            if (index == -1) return;

            _activeScreen = _screenList[index];
            _activeScreen.BringToFront();
            _activeScreen.Invalidate();
            _activeScreen.SetActivated();
            _screenPanel.Invalidate();
        }

        private void OnGoToScreen(int pageNo)
        {
            if (_activeScreen == null)
                return;

            if (pageNo == -1 || pageNo >= _screenList.Count) return;

            _activeScreen = _screenList[pageNo];
            _activeScreen.BringToFront();
            _activeScreen.Invalidate();
            _activeScreen.SetActivated();
            _screenPanel.Invalidate();
        }

        private void OnLanguageSelection(string language)
        {
            foreach (var screen in _screenList)
            {
                screen.ChangeLanguage(language);
            }
        }
    }
}

