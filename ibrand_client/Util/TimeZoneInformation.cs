using System;
using System.Collections;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace NomadLogic.iBrandDesktop.Util
{
    public class TimeZoneInformation
    {
        private TimeZoneInformation()
        {
        }

        private static TimeZoneInformation[] _sZones;
        private static readonly object s_LockZones = new object();

        /// <summary>
        /// Get the currently selected time zone
        /// </summary>
        public static TimeZoneInformation CurrentTimeZone
        {
            get
            {
                // The currently selected time zone information can
                // be retrieved using the Win32 GetTimeZoneInformation call,
                // but it only gives us names, offsets and dates - crucially,
                // not the Index.

                TIME_ZONE_INFORMATION tziNative;
                var zones = EnumZones();

                NativeMethods.GetTimeZoneInformation(out tziNative);

                // Getting the identity is tricky; the best we can do
                // is a match on the properties.

                // If the OS 'Automatically adjust clock for daylight saving changes' checkbox
                // is unchecked, the structure returned by GetTimeZoneInformation has
                // the DaylightBias and DaylightName members Set the same as the corresponding
                // Standard members. Therefore we check against both values in case this has
                // been done.

                for (var idx = 0; idx < zones.Length; ++idx)
                {
                    if (zones[idx]._mTzi.bias == tziNative.Bias &&
                         zones[idx]._mTzi.standardBias == tziNative.StandardBias &&
                         zones[idx]._mStandardName == tziNative.StandardName &&
                         (zones[idx]._mTzi.daylightBias == tziNative.DaylightBias ||
                           zones[idx]._mTzi.standardBias == tziNative.DaylightBias) &&
                         (zones[idx]._mDaylightName == tziNative.DaylightName ||
                           zones[idx]._mStandardName == tziNative.DaylightName))
                    {
                        return zones[idx];
                    }
                }

                return null;
            }
        }


        /// <summary>
        /// Get a TimeZoneInformation for a supplied index.
        /// </summary>
        /// <param name="index">The time zone to find.</param>
        /// <returns>The corresponding TimeZoneInformation.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the index is not found.</exception>
        public static TimeZoneInformation FromIndex(int index)
        {
            var zones = EnumZones();

            for (var i = 0; i < zones.Length; ++i)
            {
                if (zones[i].Index == index)
                    return zones[i];
            }

            throw new ArgumentOutOfRangeException("index", index, "Unknown time zone index");
        }


        /// <summary>
        /// Enumerate the available time zones
        /// </summary>
        /// <returns>The list of known time zones</returns>
        public static TimeZoneInformation[] EnumZones()
        {
            if (_sZones == null)
            {
                lock (s_LockZones)
                {
                    if (_sZones == null)
                    {
                        var zones = new ArrayList();

                        using (var key =
                                    Registry.LocalMachine.OpenSubKey(
                                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones"))
                        {
                            var zoneNames = key.GetSubKeyNames();

                            foreach (var zoneName in zoneNames)
                            {
                                using (var subKey = key.OpenSubKey(zoneName))
                                {
                                    var tzi = new TimeZoneInformation();
                                    tzi._mName = zoneName;
                                    tzi._mDisplayName = (string)subKey.GetValue("Display");
                                    tzi._mStandardName = (string)subKey.GetValue("Std");
                                    tzi._mDaylightName = (string)subKey.GetValue("Dlt");
                                    tzi._mIndex = (int)(subKey.GetValue("Index"));

                                    tzi.InitTzi((byte[])subKey.GetValue("Tzi"));

                                    zones.Add(tzi);
                                }
                            }
                        }

                        _sZones = new TimeZoneInformation[zones.Count];

                        zones.CopyTo(_sZones);
                    }
                }
            }

            return _sZones;
        }


        /// <summary>
        /// The zone's name.
        /// </summary>
        public string Name => _mName;

        /// <summary>
        /// The zone's display name, e.g. '(GMT) Greenwich Mean Time : Dublin, Edinburgh, Lisbon, London'.
        /// </summary>
        public string DisplayName => _mDisplayName;

        /// <summary>
        /// The zone's index. No obvious pattern.
        /// </summary>
        public int Index => _mIndex;

        /// <summary>
        /// The zone's name during 'standard' time (not daylight savings).
        /// </summary>
        public string StandardName => _mStandardName;

        /// <summary>
        /// The zone's name during daylight savings time.
        /// </summary>
        public string DaylightName => _mDaylightName;

        public override string ToString()
        {
            return _mDisplayName;
        }

        /// <summary>
        /// The standard Windows SYSTEMTIME structure.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Systemtime
        {
            public readonly ushort wYear;
            public readonly ushort wMonth;
            public readonly ushort wDayOfWeek;
            public readonly ushort wDay;
            public readonly ushort wHour;
            public readonly ushort wMinute;
            public readonly ushort wSecond;
            public readonly ushort wMilliseconds;
        }

        // FILETIME is already declared in System.Runtime.InteropServices.

        /// <summary>
        /// The layout of the Tzi value in the registry.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Tzi
        {
            public readonly int bias;
            public readonly int standardBias;
            public readonly int daylightBias;
            public readonly Systemtime standardDate;
            public readonly Systemtime daylightDate;
        }


        /// <summary>
        /// The standard Win32 TIME_ZONE_INFORMATION structure.
        /// Thanks to www.pinvoke.net.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct TIME_ZONE_INFORMATION
        {
            [MarshalAs(UnmanagedType.I4)]
            public int Bias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public readonly string StandardName;
            public Systemtime StandardDate;
            [MarshalAs(UnmanagedType.I4)]
            public int StandardBias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public readonly string DaylightName;
            public Systemtime DaylightDate;
            [MarshalAs(UnmanagedType.I4)]
            public int DaylightBias;
        }


        /// <summary>
        /// A container for P/Invoke declarations.
        /// </summary>
        private struct NativeMethods
        {
            private const string Kernel32 = "kernel32.dll";

            [DllImport(Kernel32)]
            public static extern uint GetTimeZoneInformation(out TIME_ZONE_INFORMATION
                lpTimeZoneInformation);

            [DllImport(Kernel32)]
            public static extern bool SystemTimeToTzSpecificLocalTime(
                [In] ref TIME_ZONE_INFORMATION lpTimeZone,
                [In] ref Systemtime lpUniversalTime,
                out Systemtime lpLocalTime);

            [DllImport(Kernel32)]
            public static extern bool SystemTimeToFileTime(
                [In] ref Systemtime lpSystemTime,
                out FILETIME lpFileTime);

            [DllImport(Kernel32)]
            public static extern bool FileTimeToSystemTime(
                [In] ref FILETIME lpFileTime,
                out Systemtime lpSystemTime);

            /// <summary>
            /// Convert a local time to UTC, using the supplied time zone information.
            /// Windows XP and Server 2003 and later only.
            /// </summary>
            /// <param name="lpTimeZone">The time zone to use.</param>
            /// <param name="lpLocalTime">The local time to convert.</param>
            /// <param name="lpUniversalTime">The resultant time in UTC.</param>
            /// <returns>true if successful, false otherwise.</returns>
            [DllImport(Kernel32)]
            public static extern bool TzSpecificLocalTimeToSystemTime(
                [In] ref TIME_ZONE_INFORMATION lpTimeZone,
                [In] ref Systemtime lpLocalTime,
                out Systemtime lpUniversalTime);
        }


        /// <summary>
        /// Initialise the m_tzi member.
        /// </summary>
        /// <param name="info">The Tzi data from the registry.</param>
        private void InitTzi(byte[] info)
        {
            if (info.Length != Marshal.SizeOf(_mTzi))
            {
                throw new ArgumentException("Information size is incorrect", "info");
            }

            // Could have sworn there's a Marshal operation to pack bytes into
            // a structure, but I can't see it. Do it manually.

            var h = GCHandle.Alloc(info, GCHandleType.Pinned);

            try
            {
                _mTzi = (Tzi)Marshal.PtrToStructure(h.AddrOfPinnedObject(), typeof(Tzi));
            }
            finally
            {
                h.Free();
            }
        }

        /// <summary>
        /// The offset from UTC. Local = UTC + Bias.
        /// </summary>
        public int Bias => -_mTzi.bias;

        /// <summary>
        /// The offset from UTC during standard time.
        /// </summary>
        public int StandardBias => -(_mTzi.bias + _mTzi.standardBias);

        /// <summary>
        /// The offset from UTC during daylight time.
        /// </summary>
        public int DaylightBias => -(_mTzi.bias + _mTzi.daylightBias);


        private TIME_ZONE_INFORMATION TziNative()
        {
            var tziNative = new TIME_ZONE_INFORMATION();

            tziNative.Bias = _mTzi.bias;
            tziNative.StandardDate = _mTzi.standardDate;
            tziNative.StandardBias = _mTzi.standardBias;
            tziNative.DaylightDate = _mTzi.daylightDate;
            tziNative.DaylightBias = _mTzi.daylightBias;

            return tziNative;
        }


        /// <summary>
        /// Convert a time interpreted as UTC to a time in this time zone.
        /// </summary>
        /// <param name="utc">The UTC time to convert.</param>
        /// <returns>The corresponding local time in this zone.</returns>
        public DateTime FromUniversalTime(DateTime utc)
        {
            // Convert to SYSTEMTIME
            var stUtc = DateTimeToSystemTime(utc);

            // Set up the TIME_ZONE_INFORMATION

            var tziNative = TziNative();

            Systemtime stLocal;

            NativeMethods.SystemTimeToTzSpecificLocalTime(ref tziNative, ref stUtc, out stLocal);

            // Convert back to DateTime
            return SystemTimeToDateTime(ref stLocal);
        }


        /// <summary>
        /// Convert a time from UTC to the time zone with the supplied index.
        /// </summary>
        /// <param name="index">The time zone index.</param>
        /// <param name="utc">The time to convert.</param>
        /// <returns>The converted time.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is not found.</exception>
        public static DateTime FromUniversalTime(int index, DateTime utc)
        {
            var tzi = FromIndex(index);

            return tzi.FromUniversalTime(utc);
        }


        /// <summary>
        /// Convert a time interpreted as a local time in this zone to the equivalent UTC.
        /// Note that there may be different possible interpretations at the daylight
        /// time boundaries.
        /// </summary>
        /// <param name="local">The local time to convert.</param>
        /// <returns>The corresponding UTC.</returns>
        /// <exception cref="NotSupportedException">Thrown if the method failed due to missing platform support.</exception>
        public DateTime ToUniversalTime(DateTime local)
        {
            var stLocal = DateTimeToSystemTime(local);

            var tziNative = TziNative();

            Systemtime stUtc;

            try
            {
                NativeMethods.TzSpecificLocalTimeToSystemTime(ref tziNative, ref stLocal, out stUtc);

                return SystemTimeToDateTime(ref stUtc);
            }
            catch (EntryPointNotFoundException e)
            {
                throw new NotSupportedException("This method is not supported on this operating system", e);
            }
        }


        /// <summary>
        /// Convert a time from the time zone with the supplied index to UTC.
        /// </summary>
        /// <param name="index">The time zone index.</param>
        /// <param name="utc">The time to convert.</param>
        /// <returns>The converted time.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is not found.</exception>
        /// <exception cref="NotSupportedException">Thrown if the method failed due to missing platform support.</exception>
        public static DateTime ToUniversalTime(int index, DateTime local)
        {
            var tzi = FromIndex(index);

            return tzi.ToUniversalTime(local);
        }


        private static Systemtime DateTimeToSystemTime(DateTime dt)
        {
            Systemtime st;
            var ft = new FILETIME();

            ft.dwHighDateTime = (int)(dt.Ticks >> 32);
            ft.dwLowDateTime = (int)(dt.Ticks & 0xFFFFFFFFL);

            NativeMethods.FileTimeToSystemTime(ref ft, out st);

            return st;
        }


        private static DateTime SystemTimeToDateTime(ref Systemtime st)
        {
            var ft = new FILETIME();

            NativeMethods.SystemTimeToFileTime(ref st, out ft);

            var dt = new DateTime((((long)ft.dwHighDateTime) << 32) | (uint)ft.dwLowDateTime);

            return dt;
        }


        private Tzi _mTzi;
        private string _mName;
        private string _mDisplayName;
        private int _mIndex;
        private string _mStandardName;
        private string _mDaylightName;
    }
}
