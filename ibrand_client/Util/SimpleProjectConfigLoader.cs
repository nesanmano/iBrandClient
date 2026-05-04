using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;

namespace NomadLogic.iBrandDesktop.Util
{
    /// <summary>
    /// Éventuellement, on va utiliser l'écran générique.
    /// </summary>
    [Obsolete]
    public class SimpleProjectConfigLoader
    {

        private  SimpleProjectConfig _config;
        private IDictionary<string, Rectangle> _dicoUiRectangles;
        private int[] _vals;
        public  void LoadConfigFile(string configPath)
        {
                _config = JsonUtil.ReadJsonFile<SimpleProjectConfig>(new FileInfo(configPath));
        }

        public  IDictionary<string,Rectangle> GetHomeScreenRectangles()
        {


             _dicoUiRectangles = new Dictionary<string, Rectangle>();

            foreach (string s in _config.HomeScreen.Keys)
            {
             
                _config.HomeScreen.TryGetValue(s, out _vals);

                if(_vals?.Length>0)
                {
                    _dicoUiRectangles.Add(s,
                    new Rectangle((int)_vals.GetValue(0),(int)_vals.GetValue(1),(int)_vals.GetValue(2),(int)_vals.GetValue(3)));
                    
                }
            }

            return _dicoUiRectangles;

        }


        public  IDictionary<string, Rectangle> GetPreviewScreenRectangle()
        {
            _dicoUiRectangles = new Dictionary<string, Rectangle>();

            foreach (string s in _config.Preview.Keys)
            {
             
                _config.Preview.TryGetValue(s, out _vals);

                if(_vals?.Length>0)
                {
                    _dicoUiRectangles.Add(s,
                        new Rectangle((int)_vals.GetValue(0),(int)_vals.GetValue(1),(int)_vals.GetValue(2),(int)_vals.GetValue(3)));
                    
                }
            }

            return _dicoUiRectangles;
        }

        public  IDictionary<string, Rectangle> GetQuestionnaireScreenUiFeatures()
        {
            _dicoUiRectangles = new Dictionary<string, Rectangle>();

            foreach (string s in _config.Questions.Keys)
            {
             
                _config.Questions.TryGetValue(s, out _vals);

                if(_vals?.Length>0)
                {
                    _dicoUiRectangles.Add(s,
                        new Rectangle((int)_vals.GetValue(0),(int)_vals.GetValue(1),(int)_vals.GetValue(2),(int)_vals.GetValue(3)));
                    
                }
            }

            return _dicoUiRectangles;
        }

        public IDictionary<string, Rectangle> GetPrintScreenRectangles()
        {
            _dicoUiRectangles = new Dictionary<string, Rectangle>();

            foreach (string s in _config.PrintScreen.Keys)
            {

                _config.PrintScreen.TryGetValue(s, out _vals);

                if (_vals?.Length > 0)
                {
                    _dicoUiRectangles.Add(s,
                        new Rectangle((int)_vals.GetValue(0), (int)_vals.GetValue(1), (int)_vals.GetValue(2), (int)_vals.GetValue(3)));

                }
            }

            return _dicoUiRectangles;
        }

        public bool GetPrintOption()
        {
            return _config.Print;
        }

        public IDictionary<string, Rectangle> GetPrintScreenFeatures()
        {
            _dicoUiRectangles = new Dictionary<string, Rectangle>();

            foreach (string s in _config.PrintScreen.Keys)
            {

                _config.PrintScreen.TryGetValue(s, out _vals);

                if (_vals?.Length > 0)
                {
                    _dicoUiRectangles.Add(s,
                        new Rectangle((int)_vals.GetValue(0), (int)_vals.GetValue(1), (int)_vals.GetValue(2), (int)_vals.GetValue(3)));

                }
            }

            return _dicoUiRectangles;
        }

        public bool DoSkipQuestionnaire()
        {
            return _config.SkipQuestionnaire;
        }

        public Rectangle GetTakePictureRectangle()
        {
            var rect = new Rectangle(_config.TakePicture[0],_config.TakePicture[1],_config.TakePicture[2],_config.TakePicture[3]);
            if (rect.Width == 0)
                return new Rectangle(0,0,0,0);
            return rect;
        }

        public int GetPrintLimit()
        {
            return _config.PrintLimit;
        }


        public Rectangle getDisclaimerRectangle()
        {
            var rect = new Rectangle(_config.Disclaimer[0], _config.Disclaimer[1], _config.Disclaimer[2], _config.Disclaimer[3]);
            if (rect.Width == 0)
                return new Rectangle(-1,-1,0,0);
            return rect;
        }
      
    }
}
