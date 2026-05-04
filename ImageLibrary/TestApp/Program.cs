using System;
using System.Drawing;
using Kaliko.ImageLibrary;
using Kaliko.ImageLibrary.Filters;

namespace TestApp {
    using Kaliko.ImageLibrary.Scaling;

    class Program {
        static void Main(string[] args) {
            KalikoImage image = new KalikoImage(@"C:\Users\Nomad Logic\Desktop\NetImageLibrary-master\NetImageLibrary-master\greenscreen_ricardo.jpg");
            /*image.BackgroundColor = Color.Aquamarine;

            image.Scale(new FitScaling(100, 100)).SaveJpg("thumbnail-fit.jpg", 90);

            image.Scale(new CropScaling(100,100)).SaveJpg("thumbnail-crop.jpg", 90);

            image.Scale(new PadScaling(100, 100)).SaveJpg("thumbnail-pad.jpg", 90);*/

            //KalikoImage sharpimg = image.Scale(new CropScaling(100, 100));

            image.ApplyFilter(new ChromaKeyFilter(Color.FromArgb(128, 181, 139), 60, 0.7f, 0.4f));
            //image.ApplyFilter(new UnsharpMaskFilter(1.2f, 0.3f, 1));
            //image.ApplyFilter(new GaussianBlurFilter());
            image.SavePng(@"C:\Users\Nomad Logic\Desktop\NetImageLibrary-master\NetImageLibrary-master\thumbnail.png");
            //image.SavePng(@"C:\Users\Nomad Logic\Desktop\NetImageLibrary-master\NetImageLibrary-master\green.png");

            //KalikoImage image_green = new KalikoImage(@"C:\Users\Nomad Logic\Desktop\NetImageLibrary-master\NetImageLibrary-master\green.png");
            //image_green.ApplyFilter(new UnsharpMaskFilter(1.2f, 0.3f, 1));
            //image_green.SavePng(@"C:\Users\Nomad Logic\Desktop\NetImageLibrary-master\NetImageLibrary-master\thumbnail.png");

            /*sharpimg.ApplyFilter(new DesaturationFilter());
            sharpimg.SaveJpg("thumbnail-gray.jpg", 90);

            sharpimg.ApplyFilter(new ContrastFilter(30));
            sharpimg.SaveJpg("thumbnail-contrast.jpg", 90);*/

        }
    }
}
