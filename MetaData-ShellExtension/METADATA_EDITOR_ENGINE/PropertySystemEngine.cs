#pragma warning disable CS8618, CS8600, CS8602, CS8604
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace MetadataEditor.Engine
{
    public static class PropertySystemEngine
    {
        [ComImport]
        [Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPropertyStore
        {
            uint GetCount([Out] out uint cProps);
            uint GetAt([In] uint iProp, out PROPERTYKEY pkey);
            uint GetValue([In] ref PROPERTYKEY key, [Out] PROPVARIANT pv);
            uint SetValue([In] ref PROPERTYKEY key, [In] PROPVARIANT pv);
            uint Commit();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct PROPERTYKEY
        {
            public Guid fmtid;
            public uint pid;
        }

        [StructLayout(LayoutKind.Explicit)]
        public class PROPVARIANT : IDisposable
        {
            [FieldOffset(0)] public ushort vt;
            [FieldOffset(8)] public IntPtr ptr;
            [FieldOffset(8)] public int iVal;
            [FieldOffset(8)] public long lVal;
            [FieldOffset(8)] public double dVal;

            public static PROPVARIANT FromString(string val)
            {
                PROPVARIANT pv = new PROPVARIANT();
                pv.vt = 31; // VT_LPWSTR
                pv.ptr = Marshal.StringToCoTaskMemUni(val);
                return pv;
            }

            public void Dispose()
            {
                if (vt == 31 && ptr != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ptr);
                    ptr = IntPtr.Zero;
                }
            }
        }

        [DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int SHGetPropertyStoreFromParsingName(
            [MarshalAs(UnmanagedType.LPWStr)] string pszPath,
            IntPtr zero,
            GETPROPERTYSTOREFLAGS flags,
            [In] ref Guid riid,
            [Out, MarshalAs(UnmanagedType.Interface)] out IPropertyStore ppv);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int PSGetPropertyKeyFromName(
            [MarshalAs(UnmanagedType.LPWStr)] string pszName,
            out PROPERTYKEY pkey);

        private enum GETPROPERTYSTOREFLAGS : uint
        {
            GPS_DEFAULT = 0,
            GPS_HANDLERPROPERTIESONLY = 0x1,
            GPS_READWRITE = 0x2,
            GPS_TEMPORARY = 0x4,
            GPS_FASTPROPERTIESONLY = 0x8,
            GPS_OPENSLOWITEM = 0x10,
            GPS_DELAYCREATION = 0x20,
            GPS_BESTEFFORT = 0x40,
            GPS_NO_OPLOCK = 0x80,
            GPS_PREFERQUERYPROPERTIES = 0x100,
            GPS_EXTRINSICPROPERTIES = 0x200,
            GPS_EXTRINSICPROPERTIESONLY = 0x400
        }

        private static Guid IID_IPropertyStore = new Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99");

        public static bool WriteProperty(string path, string canonicalName, string value)
        {
            try
            {
                PROPERTYKEY key;
                int hr = PSGetPropertyKeyFromName(canonicalName, out key);
                if (hr != 0)
                {
                    // Fallback: Some names are display names, not canonical names. 
                    // This engine primarily works with canonical names (e.g. System.Author).
                    // We will try a few common ones or just return false if it's purely a shell UI index.
                    return false;
                }

                IPropertyStore store;
                hr = SHGetPropertyStoreFromParsingName(path, IntPtr.Zero, GETPROPERTYSTOREFLAGS.GPS_READWRITE, ref IID_IPropertyStore, out store);
                if (hr != 0) return false;

                using (PROPVARIANT pv = PROPVARIANT.FromString(value))
                {
                    hr = (int)store.SetValue(ref key, pv);
                    if (hr >= 0)
                    {
                        store.Commit();
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        // We can use the Shell32 index mapping to get canonical names if we want to be fancy,
        // but for now we'll stick to the common property keys.
    }
}
