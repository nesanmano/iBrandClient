using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

using Canon.Eos.Framework.Helper;
using Canon.Eos.Framework.Internal.SDK;

namespace Canon.Eos.Framework
{
    public abstract class EosObject : EosDisposable
    {
        private readonly IntPtr _handle;

        internal EosObject(IntPtr handle)
        {
            _handle = handle;
        }

        protected internal IntPtr Handle => _handle;

        /// <summary>
        /// Gets the artist.
        /// </summary>
        [EosProperty(Edsdk.PropID_Artist)]
        public string Artist => GetPropertyStringData(Edsdk.PropID_Artist);

        /// <summary>
        /// Gets the copyright.
        /// </summary>
        [EosProperty(Edsdk.PropID_Copyright)]
        public string Copyright => GetPropertyStringData(Edsdk.PropID_Copyright);

        /// <summary>
        /// Gets the focus.
        /// </summary>
        [EosProperty(Edsdk.PropID_FocusInfo)]
        public EosFocus Focus => EosFocus.Create(GetPropertyStruct<Edsdk.EdsFocusInfo>(Edsdk.PropID_FocusInfo, 
            Edsdk.EdsDataType.FocusInfo));

        /// <summary>
        /// Gets the firmware version.
        /// </summary>
        [EosProperty(Edsdk.PropID_FirmwareVersion)]
        public string FirmwareVersion => GetPropertyStringData(Edsdk.PropID_FirmwareVersion);

        /// <summary>
        /// Gets the name of the owner.
        /// </summary>
        /// <value>
        /// The name of the owner.
        /// </value>
        [EosProperty(Edsdk.PropID_OwnerName)]
        public string OwnerName => GetPropertyStringData(Edsdk.PropID_OwnerName);

        /// <summary>
        /// Gets the name of the product.
        /// </summary>
        /// <value>
        /// The name of the product.
        /// </value>
        [EosProperty(Edsdk.PropID_ProductName)]
        public string ProductName => GetPropertyStringData(Edsdk.PropID_ProductName);

        /// <summary>
        /// Gets the serial number.
        /// </summary>
        [EosProperty(Edsdk.PropID_BodyIDEx)]
        public string SerialNumber => GetPropertyStringData(Edsdk.PropID_BodyIDEx);

        /// <summary>
        /// Gets or sets the white balance.
        /// </summary>
        /// <value>
        /// The white balance.
        /// </value>
        [EosProperty(Edsdk.PropID_WhiteBalance)]
        public EosWhiteBalance WhiteBalance
        {
            get { return (EosWhiteBalance)GetPropertyIntegerData(Edsdk.PropID_WhiteBalance); }
            set { SetPropertyIntegerData(Edsdk.PropID_WhiteBalance, (uint)value); }
        }
        
        protected internal override void DisposeUnmanaged()
        {
            if(_handle != IntPtr.Zero)
                Edsdk.EdsRelease(_handle);
            base.DisposeUnmanaged();
        }

        protected virtual void ExecuteSetter(Action action) 
        {
            action();
        }

        protected virtual TResult ExecuteGetter<TResult>(Func<TResult> function)
        {
            return function();
        }
        
        internal int GetPropertyDataSize(uint propertyId, Edsdk.EdsDataType expectedDataType)
        {
            return ExecuteGetter(() =>
            {
                Edsdk.EdsDataType dataType;
                Util.Assert(Edsdk.EdsGetPropertySize(Handle, propertyId, 0, out dataType, out var dataSize),
                    "Failed to get property size.", propertyId);
                //Util.AssertIf(expectedDataType != dataType, "DataType mismatch: Expected <{0}>, Actual <{1}>", expectedDataType, dataType);
                return dataSize;
            });
        }

        internal T GetPropertyStruct<T>(uint propertyId, Edsdk.EdsDataType expectedDataType) where T: struct
        {
            return ExecuteGetter(() =>
            {
                var dataSize = GetPropertyDataSize(propertyId, expectedDataType);
                var ptr = Marshal.AllocHGlobal(dataSize);
                try
                {
                    Util.Assert(Edsdk.EdsGetPropertyData(Handle, propertyId, 0, dataSize, ptr),
                        "Failed to get required struct.", propertyId);
                    return (T)Marshal.PtrToStructure(ptr, typeof(T));
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            });
        }

        public Edsdk.EdsPropertyDesc GetPropertyDescription(uint propertyId)
        {
            return ExecuteGetter(() =>
            {
                Util.Assert(Edsdk.EdsGetPropertyDesc(Handle, propertyId, out var desc), 
                    $"Failed to get property description for data: propertyId {propertyId}", propertyId);
                return desc;
            });
        }
        
        public uint GetPropertyIntegerData(uint propertyId)
        {
            return ExecuteGetter(() =>
            {
                Util.Assert(Edsdk.EdsGetPropertyData(Handle, propertyId, 0, out uint data),
                    $"Failed to get property integer data: propertyId {propertyId}", propertyId);
                return data;
            });
        }

        protected Point GetPropertyPointData(uint propertyId)
        {
            var point = GetPropertyStruct<Edsdk.EdsPoint>(propertyId, Edsdk.EdsDataType.Point);
            return new Point { X = point.x, Y = point.y };
        }

        protected Rectangle GetPropertyRectangleData(uint propertyId)
        {
            var rect = GetPropertyStruct<Edsdk.EdsRect>(propertyId, Edsdk.EdsDataType.Rect);
            return new Rectangle { X = rect.x, Y = rect.y, Height = rect.height, Width = rect.width };
        }

        protected long[] GetPropertyIntegerArrayData(uint propertyId)
        {
            return ExecuteGetter(() =>
            {
                var dataSize = GetPropertyDataSize(propertyId, Edsdk.EdsDataType.UInt32_Array);
                var ptr = Marshal.AllocHGlobal(dataSize);
                try
                {
                    Util.Assert(Edsdk.EdsGetPropertyData(Handle, propertyId, 0, dataSize, ptr),
                        "Failed to get required struct.", propertyId);

                    var signed = new int[dataSize / Marshal.SizeOf(typeof(uint))];
                    Marshal.Copy(ptr, signed, 0, signed.Length);
                    return signed.Select(i => (long)(uint)i).ToArray();
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            });
        }

        public string GetPropertyStringData(uint propertyId)
        {
            return ExecuteGetter(() =>
            {
                Util.Assert(Edsdk.EdsGetPropertyData(Handle, propertyId, 0, out string data),
                    $"Failed to get property string data: propertyId {propertyId}", propertyId);
                return data;
            });
        }
        
        public void SetPropertyIntegerData(uint propertyId, uint data)
        {
            ExecuteSetter(() => Util.Assert(Edsdk.EdsSetPropertyData(Handle, propertyId, 0, Marshal.SizeOf(typeof(uint)), data),
                $"Failed to set property integer data: propertyId {propertyId}, data {data}",
                propertyId, data));
        }

        protected void SetPropertyIntegerArrayData(uint propertyId, uint[] data)
        {
            ExecuteSetter(() => Util.Assert(Edsdk.EdsSetPropertyData(Handle, propertyId, 0, Marshal.SizeOf(typeof(uint)) * data.Length, data),
                $"Failed to set property integer array data: propertyId {propertyId}, data {data}",
                propertyId, data));
        }

        protected void SetPropertyStringData(uint propertyId, string data, int maxByteLength)
        {
            ExecuteSetter(() =>
            {
                var bytes = Util.ConvertStringToBytesWithNullByteAtEnd(data);
                if (bytes.Length > maxByteLength)
                    throw new ArgumentException($"'{data}' converted to bytes is longer than {maxByteLength}.", nameof(data));

                Util.Assert(Edsdk.EdsSetPropertyData(Handle, propertyId, 0, bytes.Length, bytes),
                    $"Failed to set property string data: propertyId {propertyId}, data {data}",
                    propertyId, data);
            });
        }
    }
}
