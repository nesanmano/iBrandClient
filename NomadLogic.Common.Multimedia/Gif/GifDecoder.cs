using System;
using System.Collections;
using System.Drawing;
using System.IO;

namespace NomadLogic.Multimedia.Gif
{
    public class GifDecoder
    {

        /**
         * File read status: No errors.
         */
        public static readonly int StatusOk = 0;

        /**
         * File read status: Error decoding file (may be partially decoded)
         */
        public static readonly int StatusFormatError = 1;

        /**
         * File read status: Unable to open source.
         */
        public static readonly int StatusOpenError = 2;

        protected Stream _inStream;
        protected int _status;

        protected int _width; // full image width
        protected int _height; // full image height
        protected bool _gctFlag; // global color table used
        protected int _gctSize; // size of global color table
        protected int _loopCount = 1; // iterations; 0 = repeat forever

        protected int[] _gct; // global color table
        protected int[] _lct; // local color table
        protected int[] _act; // active color table

        protected int _bgIndex; // background color index
        protected int _bgColor; // background color
        protected int _lastBgColor; // previous bg color
        protected int _pixelAspect; // pixel aspect ratio

        protected bool _lctFlag; // local color table flag
        protected bool _interlace; // interlace flag
        protected int _lctSize; // local color table size

        protected int _ix, _iy, _iw, _ih; // current image rectangle
        protected Rectangle _lastRect; // last image rect
        protected Image _image; // current frame
        protected Bitmap _bitmap;
        protected Image _lastImage; // previous frame

        protected byte[] _block = new byte[256]; // current data block
        protected int _blockSize; // block size

        // last graphic control extension info
        protected int _dispose;
        // 0=no action; 1=leave in place; 2=restore to bg; 3=restore to prev
        protected int _lastDispose;
        protected bool _transparency; // use transparent color
        protected int _delay; // delay in milliseconds
        protected int _transIndex; // transparent color index

        protected static readonly int MaxStackSize = 4096;
        // max decoder pixel stack size

        // LZW decoder working arrays
        protected short[] _prefix;
        protected byte[] _suffix;
        protected byte[] _pixelStack;
        protected byte[] _pixels;

        protected ArrayList _frames; // frames read from current file
        protected int _frameCount;

        public class GifFrame
        {
            public GifFrame(Image im, int del)
            {
                _image = im;
                _delay = del;
            }
            public Image _image;
            public int _delay;
        }

        /**
         * Gets display duration for specified frame.
         *
         * @param n int index of frame
         * @return delay in milliseconds
         */
        public int GetDelay(int n)
        {
            //
            _delay = -1;
            if ((n >= 0) && (n < _frameCount))
            {
                _delay = ((GifFrame)_frames[n])._delay;
            }
            return _delay;
        }

        /**
         * Gets the number of frames read from file.
         * @return frame count
         */
        public int GetFrameCount()
        {
            return _frameCount;
        }

        /**
         * Gets the first (or only) image read.
         *
         * @return BufferedImage containing first frame, or null if none.
         */
        public Image GetImage()
        {
            return GetFrame(0);
        }

        /**
         * Gets the "Netscape" iteration count, if any.
         * A count of 0 means repeat indefinitiely.
         *
         * @return iteration count if one was specified, else 1.
         */
        public int GetLoopCount()
        {
            return _loopCount;
        }

        /**
         * Creates new frame image from current data (and previous
         * frames as specified by their disposition codes).
         */
        int[] GetPixels(Bitmap bitmap)
        {
            var pixels = new int[3 * _image.Width * _image.Height];
            var count = 0;
            for (var th = 0; th < _image.Height; th++)
            {
                for (var tw = 0; tw < _image.Width; tw++)
                {
                    var color = bitmap.GetPixel(tw, th);
                    pixels[count] = color.R;
                    count++;
                    pixels[count] = color.G;
                    count++;
                    pixels[count] = color.B;
                    count++;
                }
            }
            return pixels;
        }

        void SetPixels(int[] pixels)
        {
            var count = 0;
            for (var th = 0; th < _image.Height; th++)
            {
                for (var tw = 0; tw < _image.Width; tw++)
                {
                    var color = Color.FromArgb(pixels[count++]);
                    _bitmap.SetPixel(tw, th, color);
                }
            }
        }

        protected void SetPixels()
        {
            // expose destination image's pixels as int array
            //		int[] dest =
            //			(( int ) image.getRaster().getDataBuffer()).getData();
            var dest = GetPixels(_bitmap);

            // fill in starting image contents based on last image's dispose code
            if (_lastDispose > 0)
            {
                if (_lastDispose == 3)
                {
                    // use image before last
                    var n = _frameCount - 2;
                    _lastImage = n > 0 ? GetFrame(n - 1) : null;
                }

                if (_lastImage != null)
                {
                    //				int[] prev =
                    //					((DataBufferInt) lastImage.getRaster().getDataBuffer()).getData();
                    var prev = GetPixels(new Bitmap(_lastImage));
                    Array.Copy(prev, 0, dest, 0, _width * _height);
                    // copy pixels

                    if (_lastDispose == 2)
                    {
                        // fill last image rect area with background color
                        var g = Graphics.FromImage(_image);
                        var c = _transparency ? Color.FromArgb(0, 0, 0, 0) : Color.FromArgb(_lastBgColor);
                        Brush brush = new SolidBrush(c);
                        g.FillRectangle(brush, _lastRect);
                        brush.Dispose();
                        g.Dispose();
                    }
                }
            }

            // copy each source line to the appropriate place in the destination
            var pass = 1;
            var inc = 8;
            var iline = 0;
            for (var i = 0; i < _ih; i++)
            {
                var line = i;
                if (_interlace)
                {
                    if (iline >= _ih)
                    {
                        pass++;
                        switch (pass)
                        {
                            case 2:
                                iline = 4;
                                break;
                            case 3:
                                iline = 2;
                                inc = 4;
                                break;
                            case 4:
                                iline = 1;
                                inc = 2;
                                break;
                        }
                    }
                    line = iline;
                    iline += inc;
                }
                line += _iy;
                if (line < _height)
                {
                    var k = line * _width;
                    var dx = k + _ix; // start of line in dest
                    var dlim = dx + _iw; // end of dest line
                    if ((k + _width) < dlim)
                    {
                        dlim = k + _width; // past dest edge
                    }
                    var sx = i * _iw; // start of line in source
                    while (dx < dlim)
                    {
                        // map color and insert in destination
                        var index = _pixels[sx++] & 0xff;
                        var c = _act[index];
                        if (c != 0)
                        {
                            dest[dx] = c;
                        }
                        dx++;
                    }
                }
            }
            SetPixels(dest);
        }

        /**
         * Gets the image contents of frame n.
         *
         * @return BufferedImage representation of frame, or null if n is invalid.
         */
        public Image GetFrame(int n)
        {
            Image im = null;
            if ((n >= 0) && (n < _frameCount))
            {
                im = ((GifFrame)_frames[n])._image;
            }
            return im;
        }

        /**
         * Gets image size.
         *
         * @return GIF image dimensions
         */
        public Size GetFrameSize()
        {
            return new Size(_width, _height);
        }

        /**
         * Reads GIF image from stream
         *
         * @param BufferedInputStream containing GIF file.
         * @return read status code (0 = no errors)
         */
        public int Read(Stream inStream)
        {
            Init();
            if (inStream != null)
            {
                _inStream = inStream;
                ReadHeader();
                if (!Error())
                {
                    ReadContents();
                    if (_frameCount < 0)
                    {
                        _status = StatusFormatError;
                    }
                }
                inStream.Close();
            }
            else
            {
                _status = StatusOpenError;
            }
            return _status;
        }

        /**
         * Reads GIF file from specified file/URL source  
         * (URL assumed if name contains ":/" or "file:")
         *
         * @param name String containing source
         * @return read status code (0 = no errors)
         */
        public int Read(string name)
        {
            _status = StatusOk;
            try
            {
                name = name.Trim().ToLower();
                _status = Read(new FileInfo(name).OpenRead());
            }
            catch (IOException)
            {
                _status = StatusOpenError;
            }

            return _status;
        }

        /**
         * Decodes LZW image data into pixel array.
         * Adapted from John Cristy's ImageMagick.
         */
        protected void DecodeImageData()
        {
            const int nullCode = -1;
            var npix = _iw * _ih;
            int bits,
                code,
                count,
                i, first,
                top,
                bi,
                pi;

            if ((_pixels == null) || (_pixels.Length < npix))
            {
                _pixels = new byte[npix]; // allocate new pixel array
            }
            if (_prefix == null) _prefix = new short[MaxStackSize];
            if (_suffix == null) _suffix = new byte[MaxStackSize];
            if (_pixelStack == null) _pixelStack = new byte[MaxStackSize + 1];

            //  Initialize GIF data stream decoder.

            var dataSize = Read();
            var clear = 1 << dataSize;
            var endOfInformation = clear + 1;
            var available = clear + 2;
            var oldCode = nullCode;
            var codeSize = dataSize + 1;
            var codeMask = (1 << codeSize) - 1;
            for (code = 0; code < clear; code++)
            {
                _prefix[code] = 0;
                _suffix[code] = (byte)code;
            }

            //  Decode GIF pixel stream.

            var datum = bits = count = first = top = pi = bi = 0;

            for (i = 0; i < npix; )
            {
                if (top == 0)
                {
                    if (bits < codeSize)
                    {
                        //  Load bytes until there are enough bits for a code.
                        if (count == 0)
                        {
                            // Read a new data block.
                            count = ReadBlock();
                            if (count <= 0)
                                break;
                            bi = 0;
                        }
                        datum += (_block[bi] & 0xff) << bits;
                        bits += 8;
                        bi++;
                        count--;
                        continue;
                    }

                    //  Get the next code.

                    code = datum & codeMask;
                    datum >>= codeSize;
                    bits -= codeSize;

                    //  Interpret the code

                    if ((code > available) || (code == endOfInformation))
                        break;
                    if (code == clear)
                    {
                        //  Reset decoder.
                        codeSize = dataSize + 1;
                        codeMask = (1 << codeSize) - 1;
                        available = clear + 2;
                        oldCode = nullCode;
                        continue;
                    }
                    if (oldCode == nullCode)
                    {
                        _pixelStack[top++] = _suffix[code];
                        oldCode = code;
                        first = code;
                        continue;
                    }
                    var inCode = code;
                    if (code == available)
                    {
                        _pixelStack[top++] = (byte)first;
                        code = oldCode;
                    }
                    while (code > clear)
                    {
                        _pixelStack[top++] = _suffix[code];
                        code = _prefix[code];
                    }
                    first = _suffix[code] & 0xff;

                    //  Add a new string to the string table,

                    if (available >= MaxStackSize)
                        break;
                    _pixelStack[top++] = (byte)first;
                    _prefix[available] = (short)oldCode;
                    _suffix[available] = (byte)first;
                    available++;
                    if (((available & codeMask) == 0)
                        && (available < MaxStackSize))
                    {
                        codeSize++;
                        codeMask += available;
                    }
                    oldCode = inCode;
                }

                //  Pop a pixel off the pixel stack.

                top--;
                _pixels[pi++] = _pixelStack[top];
                i++;
            }

            for (i = pi; i < npix; i++)
            {
                _pixels[i] = 0; // clear missing pixels
            }

        }

        /**
         * Returns true if an error was encountered during reading/decoding
         */
        protected bool Error()
        {
            return _status != StatusOk;
        }

        /**
         * Initializes or re-initializes reader
         */
        protected void Init()
        {
            _status = StatusOk;
            _frameCount = 0;
            _frames = new ArrayList();
            _gct = null;
            _lct = null;
        }

        /**
         * Reads a single byte from the input stream.
         */
        protected int Read()
        {
            var curByte = 0;
            try
            {
                curByte = _inStream.ReadByte();
            }
            catch (IOException)
            {
                _status = StatusFormatError;
            }
            return curByte;
        }

        /**
         * Reads next variable length block from input.
         *
         * @return number of bytes stored in "buffer"
         */
        protected int ReadBlock()
        {
            _blockSize = Read();
            var n = 0;
            if (_blockSize > 0)
            {
                try
                {
                    while (n < _blockSize)
                    {
                        var count = _inStream.Read(_block, n, _blockSize - n);
                        if (count == -1)
                            break;
                        n += count;
                    }
                }
                catch (IOException)
                {
                }

                if (n < _blockSize)
                {
                    _status = StatusFormatError;
                }
            }
            return n;
        }

        /**
         * Reads color table as 256 RGB integer values
         *
         * @param ncolors int number of colors to read
         * @return int array containing 256 colors (packed ARGB with full alpha)
         */
        protected int[] ReadColorTable(int ncolors)
        {
            var nbytes = 3 * ncolors;
            int[] tab = null;
            var c = new byte[nbytes];
            var n = 0;
            try
            {
                n = _inStream.Read(c, 0, c.Length);
            }
            catch (IOException)
            {
                // rien
            }
            if (n < nbytes)
            {
                _status = StatusFormatError;
            }
            else
            {
                tab = new int[256]; // max size to avoid bounds checks
                var i = 0;
                var j = 0;
                while (i < ncolors)
                {
                    var r = ((uint)c[j++]) & 0xff;
                    var g = ((uint)c[j++]) & 0xff;
                    var b = ((uint)c[j++]) & 0xff;
                    tab[i++] = (int)(0xff000000 | (r << 16) | (g << 8) | b);
                }
            }
            return tab;
        }

        /**
         * Main file parser.  Reads GIF content blocks.
         */
        protected void ReadContents()
        {
            // read GIF file content blocks
            var done = false;
            while (!(done || Error()))
            {
                var code = Read();
                switch (code)
                {

                    case 0x2C: // image separator
                        ReadImage();
                        break;

                    case 0x21: // extension
                        code = Read();
                        switch (code)
                        {
                            case 0xf9: // graphics control extension
                                ReadGraphicControlExt();
                                break;

                            case 0xff: // application extension
                                ReadBlock();
                                var app = "";
                                for (var i = 0; i < 11; i++)
                                {
                                    app += (char)_block[i];
                                }
                                if (app.Equals("NETSCAPE2.0"))
                                {
                                    ReadNetscapeExt();
                                }
                                else
                                    Skip(); // don't care
                                break;

                            default: // uninteresting extension
                                Skip();
                                break;
                        }
                        break;

                    case 0x3b: // terminator
                        done = true;
                        break;

                    case 0x00: // bad byte, but keep going and see what happens
                        break;

                    default:
                        _status = StatusFormatError;
                        break;
                }
            }
        }

        /**
         * Reads Graphics Control Extension values
         */
        protected void ReadGraphicControlExt()
        {
            Read(); // block size
            var packed = Read(); // packed fields
            _dispose = (packed & 0x1c) >> 2; // disposal method
            if (_dispose == 0)
            {
                _dispose = 1; // elect to keep old image if discretionary
            }
            _transparency = (packed & 1) != 0;
            _delay = ReadShort() * 10; // delay in milliseconds
            _transIndex = Read(); // transparent color index
            Read(); // block terminator
        }

        /**
         * Reads GIF file header information.
         */
        protected void ReadHeader()
        {
            var id = "";
            for (var i = 0; i < 6; i++)
            {
                id += (char)Read();
            }
            if (!id.StartsWith("GIF"))
            {
                _status = StatusFormatError;
                return;
            }

            ReadLsd();
            if (_gctFlag && !Error())
            {
                _gct = ReadColorTable(_gctSize);
                _bgColor = _gct[_bgIndex];
            }
        }

        /**
         * Reads next frame image
         */
        protected void ReadImage()
        {
            _ix = ReadShort(); // (sub)image position & size
            _iy = ReadShort();
            _iw = ReadShort();
            _ih = ReadShort();

            var packed = Read();
            _lctFlag = (packed & 0x80) != 0; // 1 - local color table flag
            _interlace = (packed & 0x40) != 0; // 2 - interlace flag
            // 3 - sort flag
            // 4-5 - reserved
            _lctSize = 2 << (packed & 7); // 6-8 - local color table size

            if (_lctFlag)
            {
                _lct = ReadColorTable(_lctSize); // read table
                _act = _lct; // make local table active
            }
            else
            {
                _act = _gct; // make global table active
                if (_bgIndex == _transIndex)
                    _bgColor = 0;
            }
            var save = 0;
            if (_transparency)
            {
                save = _act[_transIndex];
                _act[_transIndex] = 0; // set transparent color if specified
            }

            if (_act == null)
            {
                _status = StatusFormatError; // no color table defined
            }

            if (Error()) return;

            DecodeImageData(); // decode pixel data
            Skip();

            if (Error()) return;

            _frameCount++;

            // create new image to receive frame data
            //		image =
            //			new BufferedImage(width, height, BufferedImage.TYPE_INT_ARGB_PRE);

            _bitmap = new Bitmap(_width, _height);
            _image = _bitmap;
            SetPixels(); // transfer pixel data to image

            _frames.Add(new GifFrame(_bitmap, _delay)); // add image to frame list

            if (_transparency)
            {
                if (_act != null) _act[_transIndex] = save;
            }
            ResetFrame();

        }

        /**
         * Reads Logical Screen Descriptor
         */
        protected void ReadLsd()
        {

            // logical screen size
            _width = ReadShort();
            _height = ReadShort();

            // packed fields
            var packed = Read();
            _gctFlag = (packed & 0x80) != 0; // 1   : global color table flag
            // 2-4 : color resolution
            // 5   : gct sort flag
            _gctSize = 2 << (packed & 7); // 6-8 : gct size

            _bgIndex = Read(); // background color index
            _pixelAspect = Read(); // pixel aspect ratio
        }

        /**
         * Reads Netscape extenstion to obtain iteration count
         */
        protected void ReadNetscapeExt()
        {
            do
            {
                ReadBlock();
                if (_block[0] == 1)
                {
                    // loop count sub-block
                    var b1 = _block[1] & 0xff;
                    var b2 = _block[2] & 0xff;
                    _loopCount = (b2 << 8) | b1;
                }
            } while ((_blockSize > 0) && !Error());
        }

        /**
         * Reads next 16-bit value, LSB first
         */
        protected int ReadShort()
        {
            // read 16-bit value, LSB first
            return Read() | (Read() << 8);
        }

        /**
         * Resets frame state for reading next image.
         */
        protected void ResetFrame()
        {
            _lastDispose = _dispose;
            _lastRect = new Rectangle(_ix, _iy, _iw, _ih);
            _lastImage = _image;
            _lastBgColor = _bgColor;
            //		int dispose = 0;
//            bool transparency = false;
//            int delay = 0;
            _lct = null;
        }

        /**
         * Skips variable length blocks up to and including
         * next zero length block.
         */
        protected void Skip()
        {
            do
            {
                ReadBlock();
            } while ((_blockSize > 0) && !Error());
        }
    }
}

