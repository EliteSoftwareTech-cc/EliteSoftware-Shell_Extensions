using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace MetadataEditor.Engine
{
    public class MetadataEngine
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern uint PrivateExtractIconsW(string szFileName, int nIconIndex, int cxIcon, int cyIcon, IntPtr[] phicon, IntPtr[] piconid, uint nIcons, uint flags);
        
        [DllImport("user32.dll")]
        public static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);
        
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindResource(IntPtr hModule, IntPtr lpName, IntPtr lpType);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LockResource(IntPtr hResData);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

        const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;
        static readonly IntPtr RT_ICON = (IntPtr)3;
        static readonly IntPtr RT_GROUP_ICON = (IntPtr)14;

        delegate bool EnumResNameProc(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool EnumResourceNames(IntPtr hModule, IntPtr lpszType, EnumResNameProc lpEnumFunc, IntPtr lParam);

        public static bool SaveFullIconGroup(string file, int index, string outFile)
        {
            IntPtr hMod = LoadLibraryEx(file, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);
            if (hMod == IntPtr.Zero) return false;

            List<IntPtr> groupNames = new List<IntPtr>();
            EnumResourceNames(hMod, RT_GROUP_ICON, (h, t, name, l) => {
                groupNames.Add(name);
                return true;
            }, IntPtr.Zero);

            if (index < 0 || index >= groupNames.Count) { FreeLibrary(hMod); return false; }

            IntPtr groupName = groupNames[index];
            IntPtr hRes = FindResource(hMod, groupName, RT_GROUP_ICON);
            if (hRes == IntPtr.Zero) { FreeLibrary(hMod); return false; }

            IntPtr hGlobal = LoadResource(hMod, hRes);
            IntPtr pData = LockResource(hGlobal);

            int idCount = Marshal.ReadInt16(pData, 4);

            using (FileStream fs = new FileStream(outFile, FileMode.Create))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write((short)0); 
                bw.Write((short)1); 
                bw.Write((short)idCount);

                int offset = 6 + (idCount * 16);
                List<byte[]> iconDataList = new List<byte[]>();

                for (int i = 0; i < idCount; i++)
                {
                    IntPtr entryPtr = new IntPtr(pData.ToInt64() + 6 + (i * 14));
                    byte bWidth = Marshal.ReadByte(entryPtr);
                    byte bHeight = Marshal.ReadByte(entryPtr, 1);
                    byte bColorCount = Marshal.ReadByte(entryPtr, 2);
                    byte bReserved = Marshal.ReadByte(entryPtr, 3);
                    short wPlanes = Marshal.ReadInt16(entryPtr, 4);
                    short wBitCount = Marshal.ReadInt16(entryPtr, 6);
                    int dwBytesInRes = Marshal.ReadInt32(entryPtr, 8);
                    short nID = Marshal.ReadInt16(entryPtr, 12);

                    bw.Write(bWidth);
                    bw.Write(bHeight);
                    bw.Write(bColorCount);
                    bw.Write(bReserved);
                    bw.Write(wPlanes);
                    bw.Write(wBitCount);
                    bw.Write(dwBytesInRes);
                    bw.Write(offset);

                    offset += dwBytesInRes;

                    IntPtr hIconRes = FindResource(hMod, (IntPtr)nID, RT_ICON);
                    IntPtr hIconGlobal = LoadResource(hMod, hIconRes);
                    IntPtr pIconData = LockResource(hIconGlobal);
                    int iconSize = (int)SizeofResource(hMod, hIconRes);

                    byte[] iconBytes = new byte[iconSize];
                    Marshal.Copy(pIconData, iconBytes, 0, iconSize);
                    iconDataList.Add(iconBytes);
                }

                foreach (byte[] data in iconDataList) { bw.Write(data); }
            }

            FreeLibrary(hMod);
            return true;
        }
    }
}


