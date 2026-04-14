#pragma warning disable CS8618
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace MetadataEditor.Engine
{
    public class AdsEngine
    {
        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr FindFirstStreamW(string lpFileName, STREAM_INFO_LEVELS InfoLevel, [In, Out, MarshalAs(UnmanagedType.LPStruct)] WIN32_FIND_STREAM_DATA lpFindStreamData, uint dwFlags);

        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool FindNextStreamW(IntPtr hFindStream, [In, Out, MarshalAs(UnmanagedType.LPStruct)] WIN32_FIND_STREAM_DATA lpFindStreamData);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FindClose(IntPtr hFindFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteFile(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        private enum STREAM_INFO_LEVELS { FindStreamInfoStandard = 0 }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private class WIN32_FIND_STREAM_DATA
        {
            public long StreamSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 296)]
            public string cStreamName;
        }

        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint OPEN_EXISTING = 3;
        private const uint CREATE_ALWAYS = 2;

        public class AdsStreamInfo
        {
            public string Name { get; set; }
            public long Size { get; set; }
        }

        public static List<AdsStreamInfo> EnumerateStreams(string filePath)
        {
            List<AdsStreamInfo> streams = new List<AdsStreamInfo>();
            WIN32_FIND_STREAM_DATA findStreamData = new WIN32_FIND_STREAM_DATA();
            IntPtr hFind = FindFirstStreamW(filePath, STREAM_INFO_LEVELS.FindStreamInfoStandard, findStreamData, 0);

            if (hFind != new IntPtr(-1)) // INVALID_HANDLE_VALUE
            {
                do
                {
                    if (!string.IsNullOrEmpty(findStreamData.cStreamName) && findStreamData.cStreamName != "::$DATA")
                    {
                        // Streams come back as ":streamname:$DATA"
                        string name = findStreamData.cStreamName.Split(':')[1];
                        streams.Add(new AdsStreamInfo { Name = name, Size = findStreamData.StreamSize });
                    }
                } while (FindNextStreamW(hFind, findStreamData));
                FindClose(hFind);
            }
            return streams;
        }

        public static string ReadStream(string filePath, string streamName)
        {
            string fullPath = filePath + ":" + streamName;
            using (SafeFileHandle handle = CreateFile(fullPath, GENERIC_READ, 1 | 2, IntPtr.Zero, OPEN_EXISTING, 0x02000000, IntPtr.Zero))
            {
                if (handle.IsInvalid) throw new Exception("Failed to open stream for reading.");
                using (FileStream fs = new FileStream(handle, FileAccess.Read))
                using (StreamReader reader = new StreamReader(fs))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static void WriteStream(string filePath, string streamName, string content)
        {
            string fullPath = filePath + ":" + streamName;
            using (SafeFileHandle handle = CreateFile(fullPath, GENERIC_WRITE, 1 | 2, IntPtr.Zero, CREATE_ALWAYS, 0x02000000, IntPtr.Zero))
            {
                if (handle.IsInvalid) throw new Exception("Failed to open stream for writing.");
                using (FileStream fs = new FileStream(handle, FileAccess.Write))
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.Write(content);
                }
            }
        }

        public static bool DeleteStream(string filePath, string streamName)
        {
            return DeleteFile(filePath + ":" + streamName);
        }
    }
}
