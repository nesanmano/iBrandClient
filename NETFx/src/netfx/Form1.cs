using System;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace NetFx
{
    public partial class Form1 : Form
    {
        Effects effects = new Effects(); //contains all the basic effects 
        Layers layers = new Layers(); //photoshop like layer implementation

        public Form1()
        {
            InitializeComponent();
            openFileDialog1.FileName = "";

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.FileName == "") 
            { MessageBox.Show("Select an image first");  return; }

            

            BitmapW a = new BitmapW(openFileDialog1.FileName);
            pictureBox1.Image = a.GetBitmap();
            /*
            BitmapW layer1 = a.Clone(), layer2 = a.Clone(), layer3 = a.Clone(); //make three copies of the image l1,l2,l3
            BitmapW res = a.Clone(); //this will hold the final result

            layer3 = effects.fill(layer3, 167, 118, 12); //layer 3 is a solid fill of this rgb color

            layer2 = effects.blur(layer2, "gaussian"); //layer2 is a blurred version of original image
            layer1 = effects.saturate(layer1, -50); //layer1 is a saturated version


            res = layers.merge("overlay", res, layer1); //merge layer1 onto original with "overlay" layer blending
            res = layers.merge("softLight", res, layer2); //now merge layer2 onto this, with "softlight" layer blending
            res = layers.merge("softLight", res, layer3); //now merge layer3 onto this, with "softlight" layer blending
            res = effects.saturate(res,-40); //apply -40 saturate effect on this now
            res = effects.contrast(res, 10); //apply 10 contrast 

            pictureBox2.Image = res.GetBitmap(); //show the resulting image*/
            /*
            Dictionary<String, LevelParams> parameters = new Dictionary<string, LevelParams>();

            LevelParams param = new LevelParams();
            param.BlackIn = 86;
            param.Gamma = 1.71f;
            param.WhiteIn = 255;
            param.BlackOut = 0;
            param.WhiteOut = 255;
            parameters.Add("RGB", param);

            param = new LevelParams();
            param.BlackIn = 29;
            param.Gamma = 1.11f;
            param.WhiteIn = 225;
            param.BlackOut = 0;
            param.WhiteOut = 255;
            parameters.Add("Red", param);

            param = new LevelParams();
            param.BlackIn = 16;
            param.Gamma = 1.03f;
            param.WhiteIn = 229;
            param.BlackOut = 0;
            param.WhiteOut = 255;
            parameters.Add("Green", param);

            param = new LevelParams();
            param.BlackIn = 1;
            param.Gamma = 1.0f;
            param.WhiteIn = 255;
            param.BlackOut = 0;
            param.WhiteOut = 255;
            parameters.Add("Blue", param);

            BitmapW res = effects.Colorize(a.Clone(), 231f, 0.04f, 0f);
            res = effects.levels(res, parameters);
            pictureBox2.Image = res.GetBitmap();
            */


            // Brannan
            BitmapW image = null;//a.Clone();
            /*image = effects.saturate(image, -30f);
            image = effects.contrast(image, 20);
            image = effects.brighten(image, -3f);*/
            //image = effects.curves(image, new Point(0, 0), new Point(50, 50), new Point(195, 195), new Point(255, 255));
            //image = effects.Brannan(a.Clone());
            //image = effects.EarlyBird(image);
            image = effects.EarlyBird(a.Clone());
            image.GetBitmap().Save(@"C:\Users\User\Documents\projets\bethanny\test earlybird.jpg", ImageFormat.Jpeg);
            pictureBox2.Image = image.GetBitmap();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName == "") return;
            BitmapW a = new BitmapW(openFileDialog1.FileName);
            pictureBox1.Image = a.GetBitmap();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.FileName == "")
            { MessageBox.Show("Select an image first"); return; }
             
            BitmapW a = new BitmapW(openFileDialog1.FileName);
            pictureBox1.Image = a.GetBitmap();

            
            BitmapW res = a.Clone(); //this will hold the final result

            res = effects.vignette(res, 0, 0, 0);

            pictureBox2.Image = res.GetBitmap(); //show the resulting image
             
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.FileName == "")
            { MessageBox.Show("Select an image first"); return; }

            
            BitmapW a = new BitmapW(openFileDialog1.FileName);
            pictureBox1.Image = a.GetBitmap();


            BitmapW res = a.Clone(); //this will hold the final result

            res = effects.blur(res, "gaussian");

            pictureBox2.Image = res.GetBitmap(); //show the resulting image
             
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Shown(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.FileName == "")
            { MessageBox.Show("Select an image first"); return; }


            BitmapW a = new BitmapW(openFileDialog1.FileName);
            pictureBox1.Image = a.GetBitmap(); //show the original image

            //apply effects

            BitmapW res = a.Clone(); //this will hold the final result

            //do some adjustments
            res = effects.brighten(res, 20);
            res = effects.saturate(res, -90);
            
            //add a purplish color
            BitmapW purple = a.Clone();
            purple = effects.fill(purple, 34, 43, 109);
            res = layers.merge("softLight", res, purple);

            //do some more adjustments
            res = effects.gamma(res, -5);
            res = effects.contrast(res, 50);
            

            pictureBox2.Image = res.GetBitmap(); //show the resulting image
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.FileName == "")
            { MessageBox.Show("Select an image first"); return; }


            BitmapW a = new BitmapW(openFileDialog1.FileName);
            pictureBox1.Image = a.GetBitmap(); //show the original image

            //apply effects

            BitmapW layer1 = a.Clone(); //working layer 1
            BitmapW res = a.Clone(); //this will hold the final result

            //do some adjustments
            res = effects.brighten(res, 10);
            res = effects.contrast(res, 30);
            
            //add a sepia softlight to the image
            layer1 = res.Clone();
            layer1 = effects.sepia(layer1);
            layer1 = effects.vignette(layer1, 0, 0, 0); //some vignette too
            res = layers.merge("softLight", res, layer1); //add this layer to our image as softlight

            

            //desaturate a little
            res = effects.saturate(res, -30);

            //tadaaa
            pictureBox2.Image = res.GetBitmap(); //show the resulting image
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.FileName == "")
            { MessageBox.Show("Select an image first"); return; }


            BitmapW a = new BitmapW(openFileDialog1.FileName);
            pictureBox1.Image = a.GetBitmap(); //show the original image

            //apply effect
            BitmapW res = a.Clone();

            res = effects.saturate(res, -100); //grayscale it
            res = effects.contrast(res, 125);
            res = effects.noise(res, 3);
            res = effects.sepia(res);

            res = effects.adjust(res, 8, 2, 4);
            

            //tadaaa
            pictureBox2.Image = res.GetBitmap(); //show the resulting image
        }
    }
}
