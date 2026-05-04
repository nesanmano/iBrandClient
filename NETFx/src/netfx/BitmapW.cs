using System;
using System.Drawing;

/*
 This class is a wrap around the original bitmap so the rest of the code is independant of the C# Bitmap
 This is a simple class in this C# implementation but if the code is ported then this would become an important class.
 * 
 */
namespace NetFx
{
    public class BitmapW
    {
        private readonly LockBitmap _holder;
        private bool _bitmaplock;

        public BitmapW(String filename)
        {
            _holder = new LockBitmap(new Bitmap(filename));
            if (_bitmaplock) return;
            _holder.LockBits(); _bitmaplock = true;
        }

        public BitmapW(Bitmap a)
        {
            _holder = new LockBitmap(a);
            if (_bitmaplock) return;
            _holder.LockBits(); _bitmaplock = true;
        }

        public void SetPixel(int x, int y, Color color)
        {
            if (!_bitmaplock) { _holder.LockBits(); _bitmaplock = true; }
            _holder.SetPixel(x, y, color);
        }

        public Color GetPixel(int x, int y)
        {
            return _holder.GetPixel(x, y);
        }

        public int Width()
        {
            return _holder.Width;
        }

        public int Height()
        {
            return _holder.Height;
        }
        public Bitmap GetBitmap()
        {
            if (!_bitmaplock) return _holder.GetBitmap();
            _holder.UnlockBits(); _bitmaplock = false;
            return _holder.GetBitmap();
        }
        public BitmapW Clone()
        {
            if (!_bitmaplock) return new BitmapW((Bitmap) _holder.GetBitmap().Clone());
            _holder.UnlockBits(); _bitmaplock = false;
            return new BitmapW((Bitmap)_holder.GetBitmap().Clone());
        }

    }
}
