using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Networking;
using NomadLogic.PrintStation.Json;
using NomadLogic.PrintStation.Services;

namespace NomadLogic.PrintStation.Ui
{
    public partial class LoginControl : UserControl
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected KeyboardControl _keyboard;

        public LoginControl(int width, int height)
        {
            InitializeComponent();

            Size = new Size(width, height);

            center_panel.Location = new Point((Width - center_panel.Width)/2, (Height - center_panel.Height)/4);

            login_button.Click += OnLoginButtonClick;

            pw_text_box.Click += OnTextBoxGotFocus;
            username_text_box.Click += OnTextBoxGotFocus;

            AddKeyboard();
        }

        public event UserInsertedEvent UserInserted;

        private void ShowErrorMessage(string text)
        {
            cant_log_label.Text = text;
            cant_log_label.Visible = true;
            username_text_box.Focus();
            Refresh();
        }

        private void AddKeyboard()
        {
            try
            {
                if (_keyboard == null)
                {
                    _keyboard = new KeyboardControl
                    {
                                                      Size = new Size((int) (Width * 0.8), (int) (Height * 0.4)),
                                                      BackColor = Color.White
                                                  };
                    Controls.Add(_keyboard);
                }

                var x = Width / 2 - _keyboard.Width / 2;
                var y = Height - _keyboard.Height;

                _keyboard.SetLocation(x, y);

                _keyboard.BringToFront();
                _keyboard.Show();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
        
        public void OnTextBoxGotFocus(object sender, EventArgs e)
        {
            AddKeyboard();
        }
        
        private void OnLoginButtonClick(object sender, EventArgs e)
        {
            var username = username_text_box.Text;
            var password = pw_text_box.Text;

            if (username.Length == 0 || password.Length == 0)
            {
                ShowErrorMessage("You must enter username and password.");
                return;
            }

            try
            {
                var configuration = ServicesContainer.GetService<NomadConfiguration>();

                var server = new Server("192.95.36.173", "le-server", "8186", "8186");

                var request = HttpUtil.GenerateRequest(server, "print-station/Login",
                    "&username=" + username + "&password=" + password);
                var response = (HttpWebResponse) request.GetResponse();


                if (response.StatusCode != HttpStatusCode.OK) return;

                var stream = response.GetResponseStream();

                if (stream == null)
                {
                    log.Error("Response stream is null. Cannot serialize PrintStation users.");
                    return;
                }

                var jsonSerializer = new DataContractJsonSerializer(typeof (PrintStationUser));
                var objResponse = jsonSerializer.ReadObject(stream);
                var user = objResponse as PrintStationUser;

                if (user != null && user.Id != -1)
                {
                    var database =
                        ServicesContainer.GetService<PrintStationDatabase>();

                    if (database.IsUserInserted(user.Id))
                        database.SetUserStatus(user, true);
                    else
                        database.InsertUser(user);

                    GetUiFiles(user.Id);

                    GetFeatureConfig(user.FeatureId);

                    UserInserted?.Invoke(user);
                }
                else
                {
                    ShowErrorMessage("Wrong user name/password combinaison.");
                }
            }
            catch (Exception err)
            {
                log.Error(err);
                ShowErrorMessage("Problem while authenticating. Be sure you are connected to the Internet.");
            }
        }

        private void GetUiFiles(int userId)
        {
            var configuration = ServicesContainer.GetService<NomadConfiguration>();

            var server = new Server("192.95.36.173", "le-server", "8186", "8186");

            var request = HttpUtil.GenerateRequest(server, "print-station/GetUiFiles", "&user_id=" + userId);
            var response = (HttpWebResponse) request.GetResponse();


            if (response.StatusCode != HttpStatusCode.OK) return;

            var stream = response.GetResponseStream();

            if (stream == null)
            {
                log.Error("Response stream is null. Cannot serialize file names.");
                return;
            }

            var database = ServicesContainer.GetService<PrintStationDatabase>();
            var jsonSerializer = new DataContractJsonSerializer(typeof (List<UiFile>));
            var objResponse = jsonSerializer.ReadObject(stream);
            var files = objResponse as List<UiFile>;

            database.DeleteUiFiles(userId);

            if (files == null) return;
            foreach (var file in files)
            {
                try
                {
                    SaveFile(server, "ui_files/" + file.FeatureId + "/" + file.Src,
                        DirectoryUtil.GetLocalPrintstationMediaDir(userId,
                            DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.ui) +
                        StringUtil.GetFileName(file.Src));

                    database.InsertUiFile(file, userId);
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            }
        }

        private void GetFeatureConfig(int featureId)
        {
            var configuration = ServicesContainer.GetService<NomadConfiguration>();

            var server = new Server("192.95.36.173", "le-server", "8186", "8186");

            var request = HttpUtil.GenerateRequest(server, "print-station/GetFeatureConfig", "&feature_id=" + featureId);
            var response = (HttpWebResponse) request.GetResponse();


            if (response.StatusCode == HttpStatusCode.OK)
            {
                var database = ServicesContainer.GetService<PrintStationDatabase>();
                var jsonSerializer = new DataContractJsonSerializer(typeof (List<FeatureConfig>));
                // ReSharper disable once AssignNullToNotNullAttribute
                var objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
                var configs = objResponse as List<FeatureConfig>;

                database.DeleteConfig(featureId);

                if (configs == null) return;
                foreach (var config in configs)
                {
                    try
                    {
                        database.InsertConfig(config, featureId);
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }
                }
            }
        }

        public void SaveFile(Server server, string file, string localFile)
        {
            try
            {
                var request = HttpUtil.GenerateRequest(server, file);


                if (File.Exists(localFile))
                {
                    File.Delete(localFile);
                }

                FileStream fs;
                using (fs = new FileStream(localFile, FileMode.Create))
                {
                    Stream s;
                    using (s = request.GetResponse().GetResponseStream())
                    {
                        var buffer = new byte[4096];

                        if (s == null) return;
                        var bytesRead = s.Read(buffer, 0, buffer.Length);
                        while (bytesRead > 0)
                        {
                            fs.Write(buffer, 0, bytesRead);
                            bytesRead = s.Read(buffer, 0, buffer.Length);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException("Could not download file " + file, e);
            }
        }
    }
}