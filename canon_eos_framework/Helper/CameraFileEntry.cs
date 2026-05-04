using Canon.Eos.Framework.Internal.SDK;
using System;

namespace Canon.Eos.Framework.Helper
{
    public class CameraFileEntry : IDisposable
    {
        /// <summary>
        /// Pointer to the file entry
        /// </summary>
        public IntPtr Reference { get { return Ref; } }
        /// <summary>
        /// The name of the entry. (volume name, folder name or file name)
        /// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// States if the entry is a folder or not
        /// </summary>
        public bool IsFolder { get; protected set; }
        /// <summary>
        /// States if the entry is a volume or not
        /// </summary>
        public bool IsVolume { get; protected set; }
        /// <summary>
        /// If the entry is a volume or folder, these are the subentries it contains. It's null if no subentries are present.
        /// </summary>
        public CameraFileEntry[] Entries { get; internal protected set; }

        /// <summary>
        /// Pointer to the file entry
        /// </summary>
        protected IntPtr Ref;
        /// <summary>
        /// States if the entry is disposed or not
        /// </summary>
        internal protected bool IsDisposed;

        /// <summary>
        /// Creates a new instance of the <see cref="CameraFileEntry"/> class
        /// </summary>
        /// <param name="Ref"></param>
        /// <param name="Name"></param>
        /// <param name="IsFolder"></param>
        /// <param name="IsVolume"></param>
        internal protected CameraFileEntry(IntPtr Ref, string Name, bool IsFolder, bool IsVolume)
        {
            this.Ref = Ref;
            this.Name = Name;
            this.IsFolder = IsFolder;
            this.IsVolume = IsVolume;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~CameraFileEntry()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases this entry but not the subentries
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases this entry and all the subentries
        /// </summary>
        public void DisposeAll()
        {
            Dispose();
            if (Entries != null) for (int i = 0; i < Entries.Length; i++) Entries[i].DisposeAll();
        }

        /// <summary>
        /// Releases this entry but not the subentries
        /// </summary>
        /// <param name="managed">True if called from Dispose, false if called from the finalizer/destructor</param>
        protected virtual void Dispose(bool managed)
        {
            if (!IsDisposed)
            {
                //if (managed) DisposeThumb();
                if (Reference != IntPtr.Zero) Edsdk.EdsRelease(Reference);
                IsDisposed = true;
            }
        }

        /// <summary>
        /// Dispose the thumbnail in an extension
        /// </summary>
        //partial void DisposeThumb();

        /// <summary>
        /// Set the thumbnail from a stream. The thumbnail depends on the image class you want to use.
        /// </summary>
        /// <param name="stream">The image stream</param>
        /*internal protected virtual void SetThumb(IntPtr stream)
        {
            SetThumbSub(stream);
        }*/

        /// <summary>
        /// Sets the thumbnail
        /// </summary>
        /// <param name="stream">The image stream</param>
        //partial void SetThumbSub(IntPtr stream);


        /// <summary>
        /// Determines whether the specified <see cref="CameraFileEntry"/>s are equal to each other.
        /// </summary>
        /// <param name="x">The first <see cref="CameraFileEntry"/></param>
        /// <param name="y">The second <see cref="CameraFileEntry"/></param>
        /// <returns>True if the <see cref="CameraFileEntry"/>s are equal; otherwise, false</returns>
        public static bool operator ==(CameraFileEntry x, CameraFileEntry y)
        {
            // If both are null, or both are same instance, return true.
            if (object.ReferenceEquals(x, y)) return true;

            // If one is null, but not both, return false.
            if ((object)x == null || (object)y == null) return false;

            return x.Reference == y.Reference;
        }
        /// <summary>
        /// Determines whether the specified <see cref="CameraFileEntry"/>s are unequal to each other.
        /// </summary>
        /// <param name="x">The first <see cref="CameraFileEntry"/></param>
        /// <param name="y">The second <see cref="CameraFileEntry"/></param>
        /// <returns>True if the <see cref="CameraFileEntry"/>s are unequal; otherwise, false</returns>
        public static bool operator !=(CameraFileEntry x, CameraFileEntry y)
        {
            return !(x == y);
        }
        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="CameraFileEntry"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="CameraFileEntry"/></param>
        /// <returns>true if the specified <see cref="object"/> is equal to the current <see cref="CameraFileEntry"/>; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            // If objects have different types, return false.
            if (obj.GetType() != GetType()) return false;

            // If both are null, or both are same instance, return true.
            if (object.ReferenceEquals(this, obj)) return true;

            CameraFileEntry cv = obj as CameraFileEntry;
            if (cv == null) return false;

            return Reference == cv.Reference;
        }
        /// <summary>
        /// Serves as a hash function for a <see cref="CameraFileEntry"/>.
        /// </summary>
        /// <returns>A hash code for the current <see cref="CameraFileEntry"/></returns>
        public override int GetHashCode()
        {
            return Reference.ToInt64().GetHashCode();
        }
    }
}
