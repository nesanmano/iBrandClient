
namespace NomadLogic.App.EventVideoCapture
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.grb_main_info = new System.Windows.Forms.GroupBox();
            this.grp_status = new System.Windows.Forms.GroupBox();
            this.lbl_info_status = new System.Windows.Forms.Label();
            this.lbl_info_led_class_box = new System.Windows.Forms.Label();
            this.btn_quit = new System.Windows.Forms.Button();
            this.lbl_info_led_class = new System.Windows.Forms.Label();
            this.lbl_info_capture_class_box = new System.Windows.Forms.Label();
            this.lbl_info_capture_class = new System.Windows.Forms.Label();
            this.lbl_info_trigger_class_box = new System.Windows.Forms.Label();
            this.lbl_info_trigger_class = new System.Windows.Forms.Label();
            this.lbl_info_rfid_class_box = new System.Windows.Forms.Label();
            this.lbl_info_rfid_class = new System.Windows.Forms.Label();
            this.lbl_info_worker_class_box = new System.Windows.Forms.Label();
            this.lbl_info_worker_class = new System.Windows.Forms.Label();
            this.lbl_info_terminal_datakey_box = new System.Windows.Forms.Label();
            this.lbl_info_terminal_datakey = new System.Windows.Forms.Label();
            this.lbl_info_nomad_ids_box = new System.Windows.Forms.Label();
            this.lbl_info_nomad_ids = new System.Windows.Forms.Label();
            this.lbl_info_project_name = new System.Windows.Forms.Label();
            this.grb_services = new System.Windows.Forms.GroupBox();
            this.flp_services = new System.Windows.Forms.FlowLayoutPanel();
            this.grb_log = new System.Windows.Forms.GroupBox();
            this.txt_log = new System.Windows.Forms.TextBox();
            this.grb_workers = new System.Windows.Forms.GroupBox();
            this.lbl_bkg_worker_service_state = new System.Windows.Forms.Label();
            this.lbl_bkg_worker_service_state_box = new System.Windows.Forms.Label();
            this.lbl_workers_waiting_box = new System.Windows.Forms.Label();
            this.lbl_workers_waiting = new System.Windows.Forms.Label();
            this.lbl_workers_current_box = new System.Windows.Forms.Label();
            this.lbl_workers_current = new System.Windows.Forms.Label();
            this.btn_pause_workers = new System.Windows.Forms.Button();
            this.grb_trigger_delay = new System.Windows.Forms.GroupBox();
            this.lbl_delay_value = new System.Windows.Forms.Label();
            this.lbl_delay_value_box = new System.Windows.Forms.Label();
            this.trk_delay_valeur = new System.Windows.Forms.TrackBar();
            this.grb_rfid_text = new System.Windows.Forms.GroupBox();
            this.grb_rfid_text_box = new System.Windows.Forms.TextBox();
            this.grb_main_info.SuspendLayout();
            this.grp_status.SuspendLayout();
            this.grb_services.SuspendLayout();
            this.grb_log.SuspendLayout();
            this.grb_workers.SuspendLayout();
            this.grb_trigger_delay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trk_delay_valeur)).BeginInit();
            this.grb_rfid_text.SuspendLayout();
            this.SuspendLayout();
            // 
            // grb_main_info
            // 
            this.grb_main_info.Controls.Add(this.grp_status);
            this.grb_main_info.Controls.Add(this.lbl_info_led_class_box);
            this.grb_main_info.Controls.Add(this.btn_quit);
            this.grb_main_info.Controls.Add(this.lbl_info_led_class);
            this.grb_main_info.Controls.Add(this.lbl_info_capture_class_box);
            this.grb_main_info.Controls.Add(this.lbl_info_capture_class);
            this.grb_main_info.Controls.Add(this.lbl_info_trigger_class_box);
            this.grb_main_info.Controls.Add(this.lbl_info_trigger_class);
            this.grb_main_info.Controls.Add(this.lbl_info_rfid_class_box);
            this.grb_main_info.Controls.Add(this.lbl_info_rfid_class);
            this.grb_main_info.Controls.Add(this.lbl_info_worker_class_box);
            this.grb_main_info.Controls.Add(this.lbl_info_worker_class);
            this.grb_main_info.Controls.Add(this.lbl_info_terminal_datakey_box);
            this.grb_main_info.Controls.Add(this.lbl_info_terminal_datakey);
            this.grb_main_info.Controls.Add(this.lbl_info_nomad_ids_box);
            this.grb_main_info.Controls.Add(this.lbl_info_nomad_ids);
            this.grb_main_info.Controls.Add(this.lbl_info_project_name);
            this.grb_main_info.Location = new System.Drawing.Point(12, 12);
            this.grb_main_info.Name = "grb_main_info";
            this.grb_main_info.Size = new System.Drawing.Size(505, 281);
            this.grb_main_info.TabIndex = 1;
            this.grb_main_info.TabStop = false;
            this.grb_main_info.Text = "Global Information";
            // 
            // grp_status
            // 
            this.grp_status.Controls.Add(this.lbl_info_status);
            this.grp_status.Location = new System.Drawing.Point(9, 219);
            this.grp_status.Name = "grp_status";
            this.grp_status.Size = new System.Drawing.Size(332, 56);
            this.grp_status.TabIndex = 15;
            this.grp_status.TabStop = false;
            this.grp_status.Text = "Current state";
            // 
            // lbl_info_status
            // 
            this.lbl_info_status.Font = new System.Drawing.Font("Lucida Console", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_info_status.Location = new System.Drawing.Point(3, 16);
            this.lbl_info_status.Name = "lbl_info_status";
            this.lbl_info_status.Size = new System.Drawing.Size(323, 37);
            this.lbl_info_status.TabIndex = 0;
            this.lbl_info_status.Text = "UNKNOWN";
            this.lbl_info_status.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_info_led_class_box
            // 
            this.lbl_info_led_class_box.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lbl_info_led_class_box.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_info_led_class_box.Enabled = false;
            this.lbl_info_led_class_box.Font = new System.Drawing.Font("Liberation Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_info_led_class_box.Location = new System.Drawing.Point(119, 193);
            this.lbl_info_led_class_box.Name = "lbl_info_led_class_box";
            this.lbl_info_led_class_box.Size = new System.Drawing.Size(380, 23);
            this.lbl_info_led_class_box.TabIndex = 14;
            this.lbl_info_led_class_box.Text = "LED_CLASS_NAME";
            this.lbl_info_led_class_box.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btn_quit
            // 
            this.btn_quit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_quit.Location = new System.Drawing.Point(347, 235);
            this.btn_quit.Name = "btn_quit";
            this.btn_quit.Size = new System.Drawing.Size(152, 23);
            this.btn_quit.TabIndex = 3;
            this.btn_quit.Text = "EXIT";
            this.btn_quit.UseVisualStyleBackColor = true;
            this.btn_quit.Click += new System.EventHandler(this.Btn_quit_Click);
            // 
            // lbl_info_led_class
            // 
            this.lbl_info_led_class.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_info_led_class.Location = new System.Drawing.Point(6, 193);
            this.lbl_info_led_class.Name = "lbl_info_led_class";
            this.lbl_info_led_class.Size = new System.Drawing.Size(107, 23);
            this.lbl_info_led_class.TabIndex = 13;
            this.lbl_info_led_class.Text = "LED controller :";
            this.lbl_info_led_class.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbl_info_capture_class_box
            // 
            this.lbl_info_capture_class_box.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lbl_info_capture_class_box.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_info_capture_class_box.Enabled = false;
            this.lbl_info_capture_class_box.Font = new System.Drawing.Font("Liberation Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_info_capture_class_box.Location = new System.Drawing.Point(119, 170);
            this.lbl_info_capture_class_box.Name = "lbl_info_capture_class_box";
            this.lbl_info_capture_class_box.Size = new System.Drawing.Size(380, 23);
            this.lbl_info_capture_class_box.TabIndex = 12;
            this.lbl_info_capture_class_box.Text = "CAPTURE_CLASS_NAME";
            this.lbl_info_capture_class_box.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbl_info_capture_class
            // 
            this.lbl_info_capture_class.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_info_capture_class.Location = new System.Drawing.Point(6, 170);
            this.lbl_info_capture_class.Name = "lbl_info_capture_class";
            this.lbl_info_capture_class.Size = new System.Drawing.Size(107, 23);
            this.lbl_info_capture_class.TabIndex = 11;
            this.lbl_info_capture_class.Text = "Capture controller :";
            this.lbl_info_capture_class.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbl_info_trigger_class_box
            // 
            this.lbl_info_trigger_class_box.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lbl_info_trigger_class_box.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_info_trigger_class_box.Enabled = false;
            this.lbl_info_trigger_class_box.Font = new System.Drawing.Font("Liberation Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_info_trigger_class_box.Location = new System.Drawing.Point(119, 147);
            this.lbl_info_trigger_class_box.Name = "lbl_info_trigger_class_box";
            this.lbl_info_trigger_class_box.Size = new System.Drawing.Size(380, 23);
            this.lbl_info_trigger_class_box.TabIndex = 10;
            this.lbl_info_trigger_class_box.Text = "TRIGGER_CLASS_NAME";
            this.lbl_info_trigger_class_box.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbl_info_trigger_class
            // 
            this.lbl_info_trigger_class.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_info_trigger_class.Location = new System.Drawing.Point(6, 147);
            this.lbl_info_trigger_class.Name = "lbl_info_trigger_class";
            this.lbl_info_trigger_class.Size = new System.Drawing.Size(107, 23);
            this.lbl_info_trigger_class.TabIndex = 9;
            this.lbl_info_trigger_class.Text = "Trigger class :";
            this.lbl_info_trigger_class.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbl_info_rfid_class_box
            // 
            this.lbl_info_rfid_class_box.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lbl_info_rfid_class_box.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_info_rfid_class_box.Enabled = false;
            this.lbl_info_rfid_class_box.Font = new System.Drawing.Font("Liberation Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_info_rfid_class_box.Location = new System.Drawing.Point(119, 124);
            this.lbl_info_rfid_class_box.Name = "lbl_info_rfid_class_box";
            this.lbl_info_rfid_class_box.Size = new System.Drawing.Size(380, 23);
            this.lbl_info_rfid_class_box.TabIndex = 8;
            this.lbl_info_rfid_class_box.Text = "RFID_CLASS_NAME";
            this.lbl_info_rfid_class_box.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbl_info_rfid_class
            // 
            this.lbl_info_rfid_class.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_info_rfid_class.Location = new System.Drawing.Point(6, 124);
            this.lbl_info_rfid_class.Name = "lbl_info_rfid_class";
            this.lbl_info_rfid_class.Size = new System.Drawing.Size(107, 23);
            this.lbl_info_rfid_class.TabIndex = 7;
            this.lbl_info_rfid_class.Text = "Reader class :";
            this.lbl_info_rfid_class.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbl_info_worker_class_box
            // 
            this.lbl_info_worker_class_box.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lbl_info_worker_class_box.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_info_worker_class_box.Enabled = false;
            this.lbl_info_worker_class_box.Font = new System.Drawing.Font("Liberation Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_info_worker_class_box.Location = new System.Drawing.Point(119, 101);
            this.lbl_info_worker_class_box.Name = "lbl_info_worker_class_box";
            this.lbl_info_worker_class_box.Size = new System.Drawing.Size(380, 23);
            this.lbl_info_worker_class_box.TabIndex = 6;
            this.lbl_info_worker_class_box.Text = "WORKER_CLASS_NAME";
            this.lbl_info_worker_class_box.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbl_info_worker_class
            // 
            this.lbl_info_worker_class.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_info_worker_class.Location = new System.Drawing.Point(6, 101);
            this.lbl_info_worker_class.Name = "lbl_info_worker_class";
            this.lbl_info_worker_class.Size = new System.Drawing.Size(107, 23);
            this.lbl_info_worker_class.TabIndex = 5;
            this.lbl_info_worker_class.Text = "Worker class :";
            this.lbl_info_worker_class.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbl_info_terminal_datakey_box
            // 
            this.lbl_info_terminal_datakey_box.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lbl_info_terminal_datakey_box.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_info_terminal_datakey_box.Enabled = false;
            this.lbl_info_terminal_datakey_box.Font = new System.Drawing.Font("Liberation Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_info_terminal_datakey_box.Location = new System.Drawing.Point(119, 78);
            this.lbl_info_terminal_datakey_box.Name = "lbl_info_terminal_datakey_box";
            this.lbl_info_terminal_datakey_box.Size = new System.Drawing.Size(380, 23);
            this.lbl_info_terminal_datakey_box.TabIndex = 4;
            this.lbl_info_terminal_datakey_box.Text = "BOOTH_ID";
            this.lbl_info_terminal_datakey_box.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbl_info_terminal_datakey
            // 
            this.lbl_info_terminal_datakey.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_info_terminal_datakey.Location = new System.Drawing.Point(6, 78);
            this.lbl_info_terminal_datakey.Name = "lbl_info_terminal_datakey";
            this.lbl_info_terminal_datakey.Size = new System.Drawing.Size(107, 23);
            this.lbl_info_terminal_datakey.TabIndex = 3;
            this.lbl_info_terminal_datakey.Text = "Location Key name :";
            this.lbl_info_terminal_datakey.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbl_info_nomad_ids_box
            // 
            this.lbl_info_nomad_ids_box.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lbl_info_nomad_ids_box.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_info_nomad_ids_box.Enabled = false;
            this.lbl_info_nomad_ids_box.Font = new System.Drawing.Font("Liberation Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_info_nomad_ids_box.Location = new System.Drawing.Point(119, 55);
            this.lbl_info_nomad_ids_box.Name = "lbl_info_nomad_ids_box";
            this.lbl_info_nomad_ids_box.Size = new System.Drawing.Size(380, 23);
            this.lbl_info_nomad_ids_box.TabIndex = 2;
            this.lbl_info_nomad_ids_box.Text = "Pid/Qid/Tid";
            this.lbl_info_nomad_ids_box.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbl_info_nomad_ids
            // 
            this.lbl_info_nomad_ids.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_info_nomad_ids.Location = new System.Drawing.Point(6, 55);
            this.lbl_info_nomad_ids.Name = "lbl_info_nomad_ids";
            this.lbl_info_nomad_ids.Size = new System.Drawing.Size(107, 23);
            this.lbl_info_nomad_ids.TabIndex = 1;
            this.lbl_info_nomad_ids.Text = "Nomad Project IDs :";
            this.lbl_info_nomad_ids.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbl_info_project_name
            // 
            this.lbl_info_project_name.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lbl_info_project_name.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_info_project_name.Location = new System.Drawing.Point(6, 22);
            this.lbl_info_project_name.Name = "lbl_info_project_name";
            this.lbl_info_project_name.Size = new System.Drawing.Size(493, 23);
            this.lbl_info_project_name.TabIndex = 0;
            this.lbl_info_project_name.Text = "PROJECT NAME";
            this.lbl_info_project_name.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // grb_services
            // 
            this.grb_services.Controls.Add(this.flp_services);
            this.grb_services.Location = new System.Drawing.Point(12, 299);
            this.grb_services.Name = "grb_services";
            this.grb_services.Size = new System.Drawing.Size(505, 281);
            this.grb_services.TabIndex = 2;
            this.grb_services.TabStop = false;
            this.grb_services.Text = "Services availability";
            // 
            // flp_services
            // 
            this.flp_services.AutoScroll = true;
            this.flp_services.Enabled = false;
            this.flp_services.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flp_services.Location = new System.Drawing.Point(6, 19);
            this.flp_services.Name = "flp_services";
            this.flp_services.Size = new System.Drawing.Size(493, 256);
            this.flp_services.TabIndex = 0;
            // 
            // grb_log
            // 
            this.grb_log.Controls.Add(this.txt_log);
            this.grb_log.Location = new System.Drawing.Point(523, 12);
            this.grb_log.Name = "grb_log";
            this.grb_log.Size = new System.Drawing.Size(750, 445);
            this.grb_log.TabIndex = 4;
            this.grb_log.TabStop = false;
            this.grb_log.Text = "Log";
            // 
            // txt_log
            // 
            this.txt_log.BackColor = System.Drawing.SystemColors.Menu;
            this.txt_log.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_log.Location = new System.Drawing.Point(6, 19);
            this.txt_log.Multiline = true;
            this.txt_log.Name = "txt_log";
            this.txt_log.ReadOnly = true;
            this.txt_log.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txt_log.Size = new System.Drawing.Size(738, 420);
            this.txt_log.TabIndex = 0;
            // 
            // grb_workers
            // 
            this.grb_workers.Controls.Add(this.lbl_bkg_worker_service_state);
            this.grb_workers.Controls.Add(this.lbl_bkg_worker_service_state_box);
            this.grb_workers.Controls.Add(this.lbl_workers_waiting_box);
            this.grb_workers.Controls.Add(this.lbl_workers_waiting);
            this.grb_workers.Controls.Add(this.lbl_workers_current_box);
            this.grb_workers.Controls.Add(this.lbl_workers_current);
            this.grb_workers.Controls.Add(this.btn_pause_workers);
            this.grb_workers.Location = new System.Drawing.Point(523, 463);
            this.grb_workers.Name = "grb_workers";
            this.grb_workers.Size = new System.Drawing.Size(244, 117);
            this.grb_workers.TabIndex = 5;
            this.grb_workers.TabStop = false;
            this.grb_workers.Text = "Background workers";
            // 
            // lbl_bkg_worker_service_state
            // 
            this.lbl_bkg_worker_service_state.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_bkg_worker_service_state.Location = new System.Drawing.Point(8, 16);
            this.lbl_bkg_worker_service_state.Name = "lbl_bkg_worker_service_state";
            this.lbl_bkg_worker_service_state.Size = new System.Drawing.Size(61, 40);
            this.lbl_bkg_worker_service_state.TabIndex = 14;
            this.lbl_bkg_worker_service_state.Text = "State :";
            this.lbl_bkg_worker_service_state.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbl_bkg_worker_service_state_box
            // 
            this.lbl_bkg_worker_service_state_box.Font = new System.Drawing.Font("Lucida Console", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_bkg_worker_service_state_box.Location = new System.Drawing.Point(75, 16);
            this.lbl_bkg_worker_service_state_box.Name = "lbl_bkg_worker_service_state_box";
            this.lbl_bkg_worker_service_state_box.Size = new System.Drawing.Size(161, 40);
            this.lbl_bkg_worker_service_state_box.TabIndex = 13;
            this.lbl_bkg_worker_service_state_box.Text = "UNKNOWN";
            this.lbl_bkg_worker_service_state_box.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_workers_waiting_box
            // 
            this.lbl_workers_waiting_box.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lbl_workers_waiting_box.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_workers_waiting_box.Enabled = false;
            this.lbl_workers_waiting_box.Font = new System.Drawing.Font("Liberation Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_workers_waiting_box.Location = new System.Drawing.Point(196, 56);
            this.lbl_workers_waiting_box.Name = "lbl_workers_waiting_box";
            this.lbl_workers_waiting_box.Size = new System.Drawing.Size(39, 23);
            this.lbl_workers_waiting_box.TabIndex = 12;
            this.lbl_workers_waiting_box.Text = "NBR";
            this.lbl_workers_waiting_box.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_workers_waiting
            // 
            this.lbl_workers_waiting.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_workers_waiting.Location = new System.Drawing.Point(126, 56);
            this.lbl_workers_waiting.Name = "lbl_workers_waiting";
            this.lbl_workers_waiting.Size = new System.Drawing.Size(64, 23);
            this.lbl_workers_waiting.TabIndex = 11;
            this.lbl_workers_waiting.Text = "Waiting :";
            this.lbl_workers_waiting.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbl_workers_current_box
            // 
            this.lbl_workers_current_box.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lbl_workers_current_box.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_workers_current_box.Enabled = false;
            this.lbl_workers_current_box.Font = new System.Drawing.Font("Liberation Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_workers_current_box.Location = new System.Drawing.Point(74, 56);
            this.lbl_workers_current_box.Name = "lbl_workers_current_box";
            this.lbl_workers_current_box.Size = new System.Drawing.Size(46, 23);
            this.lbl_workers_current_box.TabIndex = 10;
            this.lbl_workers_current_box.Text = "NBR";
            this.lbl_workers_current_box.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_workers_current
            // 
            this.lbl_workers_current.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_workers_current.Location = new System.Drawing.Point(7, 56);
            this.lbl_workers_current.Name = "lbl_workers_current";
            this.lbl_workers_current.Size = new System.Drawing.Size(61, 23);
            this.lbl_workers_current.TabIndex = 9;
            this.lbl_workers_current.Text = "Current :";
            this.lbl_workers_current.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btn_pause_workers
            // 
            this.btn_pause_workers.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_pause_workers.Location = new System.Drawing.Point(11, 88);
            this.btn_pause_workers.Name = "btn_pause_workers";
            this.btn_pause_workers.Size = new System.Drawing.Size(75, 23);
            this.btn_pause_workers.TabIndex = 8;
            this.btn_pause_workers.Text = "Pause";
            this.btn_pause_workers.UseVisualStyleBackColor = true;
            this.btn_pause_workers.Click += new System.EventHandler(this.Btn_pause_workers_Click_1);
            // 
            // grb_trigger_delay
            // 
            this.grb_trigger_delay.Controls.Add(this.lbl_delay_value);
            this.grb_trigger_delay.Controls.Add(this.lbl_delay_value_box);
            this.grb_trigger_delay.Controls.Add(this.trk_delay_valeur);
            this.grb_trigger_delay.Location = new System.Drawing.Point(773, 463);
            this.grb_trigger_delay.Name = "grb_trigger_delay";
            this.grb_trigger_delay.Size = new System.Drawing.Size(283, 117);
            this.grb_trigger_delay.TabIndex = 6;
            this.grb_trigger_delay.TabStop = false;
            this.grb_trigger_delay.Text = "Trigger to capture delay";
            // 
            // lbl_delay_value
            // 
            this.lbl_delay_value.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_delay_value.Location = new System.Drawing.Point(6, 71);
            this.lbl_delay_value.Name = "lbl_delay_value";
            this.lbl_delay_value.Size = new System.Drawing.Size(61, 40);
            this.lbl_delay_value.TabIndex = 16;
            this.lbl_delay_value.Text = "Delay :";
            this.lbl_delay_value.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbl_delay_value_box
            // 
            this.lbl_delay_value_box.Font = new System.Drawing.Font("Lucida Console", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_delay_value_box.Location = new System.Drawing.Point(73, 71);
            this.lbl_delay_value_box.Name = "lbl_delay_value_box";
            this.lbl_delay_value_box.Size = new System.Drawing.Size(161, 40);
            this.lbl_delay_value_box.TabIndex = 15;
            this.lbl_delay_value_box.Text = "UNKNOWN";
            this.lbl_delay_value_box.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // trk_delay_valeur
            // 
            this.trk_delay_valeur.AutoSize = false;
            this.trk_delay_valeur.Location = new System.Drawing.Point(6, 19);
            this.trk_delay_valeur.Maximum = 3000;
            this.trk_delay_valeur.Name = "trk_delay_valeur";
            this.trk_delay_valeur.Size = new System.Drawing.Size(271, 45);
            this.trk_delay_valeur.SmallChange = 100;
            this.trk_delay_valeur.TabIndex = 0;
            this.trk_delay_valeur.TickFrequency = 200;
            this.trk_delay_valeur.Value = 500;
            // 
            // grb_rfid_text
            // 
            this.grb_rfid_text.Controls.Add(this.grb_rfid_text_box);
            this.grb_rfid_text.Location = new System.Drawing.Point(1062, 463);
            this.grb_rfid_text.Name = "grb_rfid_text";
            this.grb_rfid_text.Size = new System.Drawing.Size(211, 117);
            this.grb_rfid_text.TabIndex = 17;
            this.grb_rfid_text.TabStop = false;
            this.grb_rfid_text.Text = "RFID Keyboard capture";
            this.grb_rfid_text.Visible = false;
            // 
            // grb_rfid_text_box
            // 
            this.grb_rfid_text_box.Location = new System.Drawing.Point(6, 19);
            this.grb_rfid_text_box.Name = "grb_rfid_text_box";
            this.grb_rfid_text_box.Size = new System.Drawing.Size(199, 20);
            this.grb_rfid_text_box.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1285, 584);
            this.ControlBox = false;
            this.Controls.Add(this.grb_rfid_text);
            this.Controls.Add(this.grb_trigger_delay);
            this.Controls.Add(this.grb_workers);
            this.Controls.Add(this.grb_log);
            this.Controls.Add(this.grb_services);
            this.Controls.Add(this.grb_main_info);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nomad Logic Trigger Capture";
            this.grb_main_info.ResumeLayout(false);
            this.grp_status.ResumeLayout(false);
            this.grb_services.ResumeLayout(false);
            this.grb_log.ResumeLayout(false);
            this.grb_log.PerformLayout();
            this.grb_workers.ResumeLayout(false);
            this.grb_trigger_delay.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.trk_delay_valeur)).EndInit();
            this.grb_rfid_text.ResumeLayout(false);
            this.grb_rfid_text.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox grb_main_info;
        private System.Windows.Forms.Label lbl_info_capture_class_box;
        private System.Windows.Forms.Label lbl_info_capture_class;
        private System.Windows.Forms.Label lbl_info_trigger_class_box;
        private System.Windows.Forms.Label lbl_info_trigger_class;
        private System.Windows.Forms.Label lbl_info_rfid_class_box;
        private System.Windows.Forms.Label lbl_info_rfid_class;
        private System.Windows.Forms.Label lbl_info_worker_class_box;
        private System.Windows.Forms.Label lbl_info_worker_class;
        private System.Windows.Forms.Label lbl_info_terminal_datakey_box;
        private System.Windows.Forms.Label lbl_info_terminal_datakey;
        private System.Windows.Forms.Label lbl_info_nomad_ids_box;
        private System.Windows.Forms.Label lbl_info_nomad_ids;
        private System.Windows.Forms.Label lbl_info_project_name;
        private System.Windows.Forms.GroupBox grb_services;
        private System.Windows.Forms.FlowLayoutPanel flp_services;
        private System.Windows.Forms.Button btn_quit;
        private System.Windows.Forms.GroupBox grb_log;
        private System.Windows.Forms.GroupBox grb_workers;
        private System.Windows.Forms.Label lbl_info_led_class_box;
        private System.Windows.Forms.Label lbl_info_led_class;
        private System.Windows.Forms.Label lbl_workers_waiting_box;
        private System.Windows.Forms.Label lbl_workers_waiting;
        private System.Windows.Forms.Label lbl_workers_current_box;
        private System.Windows.Forms.Label lbl_workers_current;
        private System.Windows.Forms.Button btn_pause_workers;
        private System.Windows.Forms.GroupBox grp_status;
        private System.Windows.Forms.Label lbl_info_status;
        private System.Windows.Forms.TextBox txt_log;
        private System.Windows.Forms.Label lbl_bkg_worker_service_state;
        private System.Windows.Forms.Label lbl_bkg_worker_service_state_box;
        private System.Windows.Forms.GroupBox grb_trigger_delay;
        private System.Windows.Forms.Label lbl_delay_value;
        private System.Windows.Forms.Label lbl_delay_value_box;
        private System.Windows.Forms.TrackBar trk_delay_valeur;
        private System.Windows.Forms.GroupBox grb_rfid_text;
        private System.Windows.Forms.TextBox grb_rfid_text_box;
    }
}

