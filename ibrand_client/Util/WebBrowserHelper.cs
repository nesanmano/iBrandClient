/*
 * Modified from code originally found here: http://support.microsoft.com/kb/326201
 */

#region Usings

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

#endregion
namespace NomadLogic.iBrandDesktop.Util
{
    /// <summary>
    /// Class for clearing the cache
    /// </summary>
    public static class WebBrowserHelper
    {
        #region Definitions/DLL Imports
        /// <summary>
        /// For PInvoke: Contains information about an entry in the Internet cache
        /// </summary>
        [SuppressMessage("Microsoft.Portability", "CA1900:ValueTypeFieldsShouldBePortable", MessageId = "lpszSourceUrlName"), SuppressMessage("Microsoft.Portability", "CA1900:ValueTypeFieldsShouldBePortable", MessageId = "lpszLocalFileName"), SuppressMessage("Microsoft.Portability", "CA1900:ValueTypeFieldsShouldBePortable", MessageId = "dwReserved"), SuppressMessage("Microsoft.Portability", "CA1900:ValueTypeFieldsShouldBePortable", MessageId = "dwHeaderInfoSize"), SuppressMessage("Microsoft.Portability", "CA1900:ValueTypeFieldsShouldBePortable", MessageId = "dwExemptDelta"), SuppressMessage("Microsoft.Portability", "CA1900:ValueTypeFieldsShouldBePortable", MessageId = "CacheEntryType"), StructLayout(LayoutKind.Explicit, Size = 80)]
        private struct InternetCacheEntryInfoa
        {
            /// <summary>
            /// Struct size
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            [FieldOffset(0)]
            public readonly uint dwStructSize;
            /// <summary>
            /// Source URL name
            /// </summary>
            [FieldOffset(4)]
            public readonly IntPtr lpszSourceUrlName;
            /// <summary>
            /// Local file name
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            [FieldOffset(8)]
            public readonly IntPtr lpszLocalFileName;
            /// <summary>
            /// Entry type
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            [FieldOffset(12)]
            public readonly uint CacheEntryType;
            /// <summary>
            /// Use count
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            [FieldOffset(16)]
            public readonly uint dwUseCount;
            /// <summary>
            /// Hit rate
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            [FieldOffset(20)]
            public readonly uint dwHitRate;
            /// <summary>
            /// Size low
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            [FieldOffset(24)]
            public readonly uint dwSizeLow;
            /// <summary>
            /// Size high
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            [FieldOffset(28)]
            public readonly uint dwSizeHigh;
            /// <summary>
            /// Last modified time
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            [FieldOffset(32)]
            public readonly FILETIME LastModifiedTime;
            /// <summary>
            /// Expire time
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            [FieldOffset(40)]
            public readonly FILETIME ExpireTime;
            /// <summary>
            /// Last access time
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            [FieldOffset(48)]
            public readonly FILETIME LastAccessTime;
            /// <summary>
            /// Last sync time
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            [FieldOffset(56)]
            public readonly FILETIME LastSyncTime;
            /// <summary>
            /// Header info
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            [FieldOffset(64)]
            public readonly IntPtr lpHeaderInfo;
            /// <summary>
            /// Header info size
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            [FieldOffset(68)]
            public readonly uint dwHeaderInfoSize;
            /// <summary>
            /// File extension
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            [FieldOffset(72)]
            public readonly IntPtr lpszFileExtension;
            /// <summary>
            /// Reserved
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            [FieldOffset(76)]
            public readonly uint dwReserved;
            /// <summary>
            /// Exempt delta
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            [FieldOffset(76)]
            public readonly uint dwExemptDelta;
        }

        // For PInvoke: Initiates the enumeration of the cache groups in the Internet cache
        /// <summary>
        /// Finds first url cache group
        /// </summary>
        /// <param name="dwFlags"></param>
        /// <param name="dwFilter"></param>
        /// <param name="lpSearchCondition"></param>
        /// <param name="dwSearchCondition"></param>
        /// <param name="lpGroupId"></param>
        /// <param name="lpReserved"></param>
        /// <returns></returns>
        [DllImport(@"wininet",
            SetLastError = true,
            CharSet = CharSet.Auto,
            EntryPoint = "FindFirstUrlCacheGroup",
            CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr FindFirstUrlCacheGroup(
            int dwFlags,
            int dwFilter,
            IntPtr lpSearchCondition,
            int dwSearchCondition,
            ref long lpGroupId,
            IntPtr lpReserved);

        // For PInvoke: Retrieves the next cache group in a cache group enumeration
        /// <summary>
        /// Finds next URL Cache Group
        /// </summary>
        /// <param name="hFind"></param>
        /// <param name="lpGroupId"></param>
        /// <param name="lpReserved"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), DllImport(@"wininet",
            SetLastError = true,
            CharSet = CharSet.Auto,
            EntryPoint = "FindNextUrlCacheGroup",
            CallingConvention = CallingConvention.StdCall)]
        internal static extern bool FindNextUrlCacheGroup(
            IntPtr hFind,
            ref long lpGroupId,
            IntPtr lpReserved);

        // For PInvoke: Releases the specified GROUPID and any associated state in the cache index file
        /// <summary>
        /// Deletes an URL from a Cache group
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="dwFlags"></param>
        /// <param name="lpReserved"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), DllImport(@"wininet",
            SetLastError = true,
            CharSet = CharSet.Auto,
            EntryPoint = "DeleteUrlCacheGroup",
            CallingConvention = CallingConvention.StdCall)]
        internal static extern bool DeleteUrlCacheGroup(
            long groupId,
            int dwFlags,
            IntPtr lpReserved);

        // For PInvoke: Begins the enumeration of the Internet cache
        /// <summary>
        /// Find first URL cache entry
        /// </summary>
        /// <param name="lpszUrlSearchPattern"></param>
        /// <param name="lpFirstCacheEntryInfo"></param>
        /// <param name="lpdwFirstCacheEntryInfoBufferSize"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "0"), DllImport(@"wininet",
            SetLastError = true,
            CharSet = CharSet.Auto,
            EntryPoint = "FindFirstUrlCacheEntryA",
            CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr FindFirstUrlCacheEntry(
            [MarshalAs(UnmanagedType.LPTStr)] string lpszUrlSearchPattern,
            IntPtr lpFirstCacheEntryInfo,
            ref int lpdwFirstCacheEntryInfoBufferSize);

        // For PInvoke: Retrieves the next entry in the Internet cache
        /// <summary>
        /// Find nexxt URL cache entry
        /// </summary>
        /// <param name="hFind"></param>
        /// <param name="lpNextCacheEntryInfo"></param>
        /// <param name="lpdwNextCacheEntryInfoBufferSize"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), DllImport(@"wininet",
            SetLastError = true,
            CharSet = CharSet.Auto,
            EntryPoint = "FindNextUrlCacheEntryA",
            CallingConvention = CallingConvention.StdCall)]
        internal static extern bool FindNextUrlCacheEntry(
            IntPtr hFind,
            IntPtr lpNextCacheEntryInfo,
            ref int lpdwNextCacheEntryInfoBufferSize);

        // For PInvoke: Removes the file that is associated with the source name from the cache, if the file exists
        /// <summary>
        /// Deletes an URL cache entry
        /// </summary>
        /// <param name="lpszUrlName"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"), DllImport(@"wininet",
            SetLastError = true,
            CharSet = CharSet.Auto,
            EntryPoint = "DeleteUrlCacheEntryA",
            CallingConvention = CallingConvention.StdCall)]
        internal static extern bool DeleteUrlCacheEntry(
            IntPtr lpszUrlName);
        #endregion

        #region Public Static Functions

        /// <summary>
        /// Clears the cache of the web browser
        /// </summary>
        public static void ClearCache()
        {
            // Indicates that all of the cache groups in the user's system should be enumerated
            const int cachegroupSearchAll = 0x0;
            // Indicates that all the cache entries that are associated with the cache group
            // should be deleted, unless the entry belongs to another cache group.
            const int cachegroupFlagFlushurlOndelete = 0x2;
            // File not found.
            const int errorFileNotFound = 0x2;
            // No more items have been found.
            const int errorNoMoreItems = 259;
            // Pointer to a GROUPID variable
            long groupId = 0;

            // Local variables
            var cacheEntryInfoBufferSizeInitial = 0;
            var cacheEntryInfoBufferSize = 0;
            var cacheEntryInfoBuffer = IntPtr.Zero;
            InternetCacheEntryInfoa internetCacheEntry;
            var enumHandle = IntPtr.Zero;
            var returnValue = false;

            // Delete the groups first.
            // Groups may not always exist on the system.
            // For more information, visit the following Microsoft Web site:
            // http://msdn.microsoft.com/library/?url=/workshop/networking/wininet/overview/cache.asp			
            // By default, a URL does not belong to any group. Therefore, that cache may become
            // empty even when the CacheGroup APIs are not used because the existing URL does not belong to any group.			
            enumHandle = FindFirstUrlCacheGroup(0, cachegroupSearchAll, IntPtr.Zero, 0, ref groupId, IntPtr.Zero);
            // If there are no items in the Cache, you are finished.
            if (enumHandle != IntPtr.Zero && errorNoMoreItems == Marshal.GetLastWin32Error())
                return;

            // Loop through Cache Group, and then delete entries.
            while (true)
            {
                if (errorNoMoreItems == Marshal.GetLastWin32Error() || errorFileNotFound == Marshal.GetLastWin32Error()) { break; }
                // Delete a particular Cache Group.
                returnValue = DeleteUrlCacheGroup(groupId, cachegroupFlagFlushurlOndelete, IntPtr.Zero);
                if (!returnValue && errorFileNotFound == Marshal.GetLastWin32Error())
                {
                    returnValue = FindNextUrlCacheGroup(enumHandle, ref groupId, IntPtr.Zero);
                }

                if (!returnValue && (errorNoMoreItems == Marshal.GetLastWin32Error() || errorFileNotFound == Marshal.GetLastWin32Error()))
                    break;
            }

            // Start to delete URLs that do not belong to any group.
            enumHandle = FindFirstUrlCacheEntry(null, IntPtr.Zero, ref cacheEntryInfoBufferSizeInitial);
            if (enumHandle != IntPtr.Zero && errorNoMoreItems == Marshal.GetLastWin32Error())
                return;

            cacheEntryInfoBufferSize = cacheEntryInfoBufferSizeInitial;
            cacheEntryInfoBuffer = Marshal.AllocHGlobal(cacheEntryInfoBufferSize);
            enumHandle = FindFirstUrlCacheEntry(null, cacheEntryInfoBuffer, ref cacheEntryInfoBufferSizeInitial);

            while (true)
            {
                internetCacheEntry = (InternetCacheEntryInfoa)Marshal.PtrToStructure(cacheEntryInfoBuffer, typeof(InternetCacheEntryInfoa));
                if (errorNoMoreItems == Marshal.GetLastWin32Error()) { break; }

                cacheEntryInfoBufferSizeInitial = cacheEntryInfoBufferSize;
                returnValue = DeleteUrlCacheEntry(internetCacheEntry.lpszSourceUrlName);
                if (!returnValue)
                {
                    returnValue = FindNextUrlCacheEntry(enumHandle, cacheEntryInfoBuffer, ref cacheEntryInfoBufferSizeInitial);
                }
                if (!returnValue && errorNoMoreItems == Marshal.GetLastWin32Error())
                {
                    break;
                }
                if (!returnValue && cacheEntryInfoBufferSizeInitial > cacheEntryInfoBufferSize)
                {
                    cacheEntryInfoBufferSize = cacheEntryInfoBufferSizeInitial;
                    cacheEntryInfoBuffer = Marshal.ReAllocHGlobal(cacheEntryInfoBuffer, (IntPtr)cacheEntryInfoBufferSize);
                    returnValue = FindNextUrlCacheEntry(enumHandle, cacheEntryInfoBuffer, ref cacheEntryInfoBufferSizeInitial);
                }
            }
            Marshal.FreeHGlobal(cacheEntryInfoBuffer);
        }
        #endregion
    }
}