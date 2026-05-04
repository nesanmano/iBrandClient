using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace NomadLogic.Multimedia.Gif
{
    /// <summary>
    /// Classe principale pour créer un GIF. Source ouverte, voir ici pour plus de détails:
    /// http://www.codeproject.com/Articles/11505/NGif-Animated-GIF-Encoder-for-NET
    /// </summary>
    public class AnimatedGifEncoder : IDisposable
    {
        protected int _width; // image size
        protected int _height;
        protected Color _transparent = Color.Empty; // transparent color if given
        protected int _trans_index; // transparent index in color table
        protected int _repeat = -1; // no repeat
        protected int _delay; // frame delay (hundredths)
        protected bool _started; // ready to output frames
        //	protected BinaryWriter bw;
        protected FileStream _fs;

        protected Image _image; // current frame
        protected byte[] _pixels; // BGR byte array from frame
        protected byte[] _indexed_pixels; // converted frame indexed to palette
        protected int _color_depth; // number of bit planes
        protected byte[] _color_tab; // RGB palette
        protected bool[] _used_entry = new bool[256]; // active palette entries
        protected int _pal_size = 7; // color table size (bits-1)
        protected int _dispose = -1; // disposal code (-1 = use default)
        protected bool _close_stream; // close stream when finished
        protected bool _first_frame = true;
        protected bool _size_set; // if false, get size from first frame
        protected int _sample = 10; // default sample interval for quantizer

        /**
         * Sets the delay time between each frame, or changes it
         * for subsequent frames (applies to last frame added).
         *
         * @param ms int delay time in milliseconds
         */
        public void SetDelay(int ms)
        {
            _delay = (int)System.Math.Round(ms / 10.0f);
        }

        /**
         * Sets the GIF frame disposal code for the last added frame
         * and any subsequent frames.  Default is 0 if no transparent
         * color has been set, otherwise 2.
         * @param code int disposal code.
         */
        public void SetDispose(int code)
        {
            if (code >= 0)
            {
                _dispose = code;
            }
        }

        /**
         * Sets the number of times the set of GIF frames
         * should be played.  Default is 1; 0 means play
         * indefinitely.  Must be invoked before the first
         * image is added.
         *
         * @param iter int number of iterations.
         * @return
         */
        public void SetRepeat(int iter)
        {
            if (iter >= 0)
            {
                _repeat = iter;
            }
        }

        /**
         * Sets the transparent color for the last added frame
         * and any subsequent frames.
         * Since all colors are subject to modification
         * in the quantization process, the color in the final
         * palette for each frame closest to the given color
         * becomes the transparent color for that frame.
         * May be set to null to indicate no transparent color.
         *
         * @param c Color to be treated as transparent on display.
         */
        public void SetTransparent(Color c)
        {
            _transparent = c;
        }

        /**
         * Adds next GIF frame.  The frame is not written immediately, but is
         * actually deferred until the next frame is received so that timing
         * data can be inserted.  Invoking <code>finish()</code> flushes all
         * frames.  If <code>setSize</code> was not invoked, the size of the
         * first image is used for all subsequent frames.
         *
         * @param im BufferedImage containing frame to write.
         * @return true if successful.
         */
        public bool AddFrame(Image im)
        {
            if ((im == null) || !_started)
            {
                return false;
            }
            var ok = true;
            try
            {
                if (!_size_set)
                {
                    // use first frame's size
                    SetSize(im.Width, im.Height);
                }
                _image = im;
                GetImagePixels(); // convert to correct format if necessary
                AnalyzePixels(); // build color table & map pixels
                if (_first_frame)
                {
                    WriteLsd(); // logical screen descriptior
                    WritePalette(); // global color table
                    if (_repeat >= 0)
                    {
                        // use NS app extension to indicate reps
                        WriteNetscapeExt();
                    }
                }
                WriteGraphicCtrlExt(); // write graphic control extension
                WriteImageDesc(); // image descriptor
                if (!_first_frame)
                {
                    WritePalette(); // local color table
                }
                WritePixels(); // encode and write pixel data
                _first_frame = false;
            }
            catch (IOException)
            {
                ok = false;
            }

            return ok;
        }

        /**
         * Flushes any pending data and closes output file.
         * If writing to an OutputStream, the stream is not
         * closed.
         */
        public bool Finish()
        {
            if (!_started) return false;
            var ok = true;
            _started = false;
            try
            {
                _fs.WriteByte(0x3b); // gif trailer
                _fs.Flush();
                if (_close_stream)
                {
                    _fs.Close();
                }
            }
            catch (IOException)
            {
                ok = false;
            }

            // reset for subsequent use
            _trans_index = 0;
            _fs = null;
            _image = null;
            _pixels = null;
            _indexed_pixels = null;
            _color_tab = null;
            _close_stream = false;
            _first_frame = true;

            return ok;
        }

        public void Dispose()
        {
            Finish();
        }

        /**
         * Sets frame rate in frames per second.  Equivalent to
         * <code>setDelay(1000/fps)</code>.
         *
         * @param fps float frame rate (frames per second)
         */
        public void SetFrameRate(float fps)
        {
            if (fps != 0f)
            {
                _delay = (int)System.Math.Round(100f / fps);
            }
        }

        /**
         * Sets quality of color quantization (conversion of images
         * to the maximum 256 colors allowed by the GIF specification).
         * Lower values (minimum = 1) produce better colors, but slow
         * processing significantly.  10 is the default, and produces
         * good color mapping at reasonable speeds.  Values greater
         * than 20 do not yield significant improvements in speed.
         *
         * @param quality int greater than 0.
         * @return
         */
        public void SetQuality(int quality)
        {
            if (quality < 1) quality = 1;
            _sample = quality;
        }

        /**
         * Sets the GIF frame size.  The default size is the
         * size of the first frame added if this method is
         * not invoked.
         *
         * @param w int frame width.
         * @param h int frame width.
         */
        public void SetSize(int w, int h)
        {
            if (_started && !_first_frame) return;
            _width = w;
            _height = h;
            if (_width < 1) _width = 320;
            if (_height < 1) _height = 240;
            _size_set = true;
        }

        /**
         * Initiates GIF file creation on the given stream.  The stream
         * is not closed automatically.
         *
         * @param os OutputStream on which GIF images are written.
         * @return false if initial write failed.
         */
        public bool Start(FileStream os)
        {
            if (os == null) return false;
            var ok = true;
            _close_stream = false;
            _fs = os;
            try
            {
                WriteString("GIF89a"); // header
            }
            catch (IOException)
            {
                ok = false;
            }
            return _started = ok;
        }

        /**
         * Initiates writing of a GIF file with the specified name.
         *
         * @param file String containing output file name.
         * @return false if open or initial write failed.
         */
        public bool Start(string file)
        {
            bool ok;
            try
            {
                //			bw = new BinaryWriter( new FileStream( file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None ) );
                _fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                ok = Start(_fs);
                _close_stream = true;
            }
            catch (IOException)
            {
                ok = false;
            }
            return _started = ok;
        }

        /**
         * Analyzes image colors and creates color map.
         */
        protected void AnalyzePixels()
        {
            var len = _pixels.Length;
            var nPix = len / 3;
            _indexed_pixels = new byte[nPix];
            var nq = new NeuQuant(_pixels, len, _sample);
            // initialize quantizer
            _color_tab = nq.Process(); // create reduced palette
            // convert map from BGR to RGB
            //			for (int i = 0; i < colorTab.Length; i += 3) 
            //			{
            //				byte temp = colorTab[i];
            //				colorTab[i] = colorTab[i + 2];
            //				colorTab[i + 2] = temp;
            //				usedEntry[i / 3] = false;
            //			}
            // map image pixels to new palette
            var k = 0;
            for (var i = 0; i < nPix; i++)
            {
                var index =
                    nq.Map(_pixels[k++] & 0xff,
                    _pixels[k++] & 0xff,
                    _pixels[k++] & 0xff);
                _used_entry[index] = true;
                _indexed_pixels[i] = (byte)index;
            }
            _pixels = null;
            _color_depth = 8;
            _pal_size = 7;
            // get closest match to transparent color if specified
            if (_transparent != Color.Empty)
            {
                _trans_index = FindClosest(_transparent);
            }
        }

        /**
         * Returns index of palette color closest to c
         *
         */
        protected int FindClosest(Color c)
        {
            if (_color_tab == null) return -1;
            int r = c.R;
            int g = c.G;
            int b = c.B;
            var minpos = 0;
            var dmin = 256 * 256 * 256;
            var len = _color_tab.Length;
            for (var i = 0; i < len; )
            {
                var dr = r - (_color_tab[i++] & 0xff);
                var dg = g - (_color_tab[i++] & 0xff);
                var db = b - (_color_tab[i] & 0xff);
                var d = dr * dr + dg * dg + db * db;
                var index = i / 3;
                if (_used_entry[index] && (d < dmin))
                {
                    dmin = d;
                    minpos = index;
                }
                i++;
            }
            return minpos;
        }

        /**
         * Extracts image pixels into byte array "pixels"
         * Modifié avec le site suivant:
         * https://dotnetpuzzle.wordpress.com/2012/05/09/animated-gif-image-with-c/
         */
        protected void GetImagePixels()
        {
            var w = _image.Width;
            var h = _image.Height;
            if ((w != _width) || (h != _height))
            {
                // create new image with right size/format
                using (Image temp = new Bitmap(_width, _height))
                {
                    using (var g = Graphics.FromImage(temp))
                    {
                        g.DrawImage(_image, 0, 0);
                        _image = temp;
                    }
                }
            }
            //
            // improved performance
            var tempBitmap = (Bitmap)_image;
            BitmapData bitmapData = null;
            try
            {
                bitmapData = tempBitmap.LockBits(
                      new Rectangle(0, 0, tempBitmap.Width, tempBitmap.Height)
                    , ImageLockMode.ReadOnly
                    , PixelFormat.Format24bppRgb
                    );
                var pixelAddress = bitmapData.Scan0;
                var physicalLineLength = bitmapData.Stride;
                var sourcePixels
                    = new byte[physicalLineLength * tempBitmap.Height];
                Marshal.Copy(
                      pixelAddress
                    , sourcePixels
                    , 0
                    , sourcePixels.Length
                    );
                var sourceIndex = 0;
                var targetIndex = 0;
                var logicalLineLength = 3 * tempBitmap.Width;
                _pixels = new byte[logicalLineLength * tempBitmap.Height];
                for (var th = 0; th < tempBitmap.Height; th++)
                {
                    Array.Copy(
                          sourcePixels
                        , sourceIndex
                        , _pixels
                        , targetIndex
                        , logicalLineLength
                        );
                    sourceIndex += bitmapData.Stride;
                    targetIndex += logicalLineLength;
                }
                for (var tw = 0; tw <= _pixels.Length - 3; tw += 3)
                {
                    var tempColor = _pixels[tw];
                    _pixels[tw] = _pixels[tw + 2];
                    _pixels[tw + 2] = tempColor;
                }
            }
            finally
            {
                if (bitmapData != null)
                {
                    tempBitmap.UnlockBits(bitmapData);
                }
            }
        }

        /**
         * Writes Graphic Control Extension
         */
        protected void WriteGraphicCtrlExt()
        {
            _fs.WriteByte(0x21); // extension introducer
            _fs.WriteByte(0xf9); // GCE label
            _fs.WriteByte(4); // data block size
            int transp, disp;
            if (_transparent == Color.Empty)
            {
                transp = 0;
                disp = 0; // dispose = no action
            }
            else
            {
                transp = 1;
                disp = 2; // force clear if using transparent color
            }
            if (_dispose >= 0)
            {
                disp = _dispose & 7; // user override
            }
            disp <<= 2;

            // packed fields
            _fs.WriteByte(Convert.ToByte(0 | // 1:3 reserved
                disp | // 4:6 disposal
                0 | // 7   user input - 0 = none
                transp)); // 8   transparency flag

            WriteShort(_delay); // delay x 1/100 sec
            _fs.WriteByte(Convert.ToByte(_trans_index)); // transparent color index
            _fs.WriteByte(0); // block terminator
        }

        /**
         * Writes Image Descriptor
         */
        protected void WriteImageDesc()
        {
            _fs.WriteByte(0x2c); // image separator
            WriteShort(0); // image position x,y = 0,0
            WriteShort(0);
            WriteShort(_width); // image size
            WriteShort(_height);
            // packed fields
            if (_first_frame)
            {
                // no LCT  - GCT is used for first (or only) frame
                _fs.WriteByte(0);
            }
            else
            {
                // specify normal LCT
                _fs.WriteByte(Convert.ToByte(0x80 | // 1 local color table  1=yes
                    0 | // 2 interlace - 0=no
                    0 | // 3 sorted - 0=no
                    0 | // 4-5 reserved
                    _pal_size)); // 6-8 size of color table
            }
        }

        /**
         * Writes Logical Screen Descriptor
         */
        protected void WriteLsd()
        {
            // logical screen size
            WriteShort(_width);
            WriteShort(_height);
            // packed fields
            _fs.WriteByte(Convert.ToByte(0x80 | // 1   : global color table flag = 1 (gct used)
                0x70 | // 2-4 : color resolution = 7
                0x00 | // 5   : gct sort flag = 0
                _pal_size)); // 6-8 : gct size

            _fs.WriteByte(0); // background color index
            _fs.WriteByte(0); // pixel aspect ratio - assume 1:1
        }

        /**
         * Writes Netscape application extension to define
         * repeat count.
         */
        protected void WriteNetscapeExt()
        {
            _fs.WriteByte(0x21); // extension introducer
            _fs.WriteByte(0xff); // app extension label
            _fs.WriteByte(11); // block size
            WriteString("NETSCAPE" + "2.0"); // app id + auth code
            _fs.WriteByte(3); // sub-block size
            _fs.WriteByte(1); // loop sub-block id
            WriteShort(_repeat); // loop count (extra iterations, 0=repeat forever)
            _fs.WriteByte(0); // block terminator
        }

        /**
         * Writes color table
         */
        protected void WritePalette()
        {
            _fs.Write(_color_tab, 0, _color_tab.Length);
            var n = (3 * 256) - _color_tab.Length;
            for (var i = 0; i < n; i++)
            {
                _fs.WriteByte(0);
            }
        }

        /**
         * Encodes and writes pixel data
         */
        protected void WritePixels()
        {
            var encoder =
                new LzwEncoder(_width, _height, _indexed_pixels, _color_depth);
            encoder.Encode(_fs);
        }

        /**
         *    Write 16-bit value to output stream, LSB first
         */
        protected void WriteShort(int value)
        {
            _fs.WriteByte(Convert.ToByte(value & 0xff));
            _fs.WriteByte(Convert.ToByte((value >> 8) & 0xff));
        }

        /**
         * Writes string to output stream
         */
        protected void WriteString(string s)
        {
            var chars = s.ToCharArray();
            foreach (var t in chars)
            {
                _fs.WriteByte((byte)t);
            }
        }
    }
}
