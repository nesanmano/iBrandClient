using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.iBrand.Common;

namespace NomadLogic.iBrand.Ui.CameraRing
{
    public partial class Picture : UserControl
    {

        public OrderChange _on_order_change;
        
        int _index = -1;
        int _cameraNo = -1;



        public Picture(int index, int cameraNo, String name)
        {
            InitializeComponent();

            _index = index;
            _cameraNo = cameraNo;

            label.Text = name;

            AllowDrop = true;
            
            DragDrop += OnDragDrop;
            image_panel.MouseDown += OnMouseDown;
            DragEnter += OnDragEnter;
        }

        public String LabelName
        {
            set
            {
                label.Text = value; 
            }

            get
            {
                return label.Text;
            }
        }

        public override string ToString()
        {
            return "_index :" + _index + " _camera_no :" + _cameraNo; 
        }

        public int PictureIndex
        {
            set
            {
                _index = value;
            }

            get
            {
                return _index;
            }
        }

        public int CameraNo
        {
            get
            {
                return _cameraNo;
            }

            set
            {
                _cameraNo = value;
            }
        
        }

        public void DisposeImage()
        {
            image_panel.Image.Dispose();
        }

        public Image Image
        {
            set
            {
                Image img = null;
                if (value.Height != image_panel.Height || value.Width != image_panel.Width)
                {
                    img = new Bitmap(value, image_panel.Width, image_panel.Height);
                }
                else
                    img = value;

                image_panel.Image = img;
            }

            get
            {
                return image_panel.Image;
            }
        }

        public void SetSize(int width, int height)
        {
            Size = new Size(width, height);
            image_panel.Size = new Size(width, height - 20);
            label.Location = new Point(4, height - 20);
            label.Size = new Size(width, 18);
        }

   
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            //we will pass the data that user wants to drag DoDragDrop method is used for holding data
            //DoDragDrop accepts two paramete first paramter is data(image,file,text etc) and second paramter 
            //specify either user wants to copy the data or move data

            DoDragDrop(this, DragDropEffects.Copy);
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            //As we are interested in Image data only we will check this as follows
            if (e.Data.GetDataPresent(typeof(Picture)))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            //target control will accept data here 
            Picture source = (Picture)e.Data.GetData(typeof(Picture));
            Picture destination = (Picture)sender;

            if (destination.PictureIndex != source.PictureIndex)
            {
                /*String temp_name = destination.LabelName;
                Image temp_image = destination.Image;
                int temp_camera_no = destination.CameraNo;

                destination.LabelName = source.LabelName;
                destination.Image = source.Image;
                destination.CameraNo = source.CameraNo;

                source.LabelName = temp_name;
                source.Image = temp_image;
                source.CameraNo = temp_camera_no;

                source.Invalidate();
                destination.Invalidate();*/

                if (_on_order_change != null)
                    _on_order_change(source.PictureIndex, destination.PictureIndex);
            }
            
        }

        public override bool Equals(object obj)
        {
            if (obj is Picture)
            {
                return (_cameraNo == ((Picture)obj).CameraNo);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _cameraNo.GetHashCode();
        }

    }
}
