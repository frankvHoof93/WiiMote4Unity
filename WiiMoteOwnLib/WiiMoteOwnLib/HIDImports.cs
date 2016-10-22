﻿//////////////////////////////////////////////////////////////////////////////////
//	HIDImports.cs
//	For more information: http://wiimotelib.codeplex.com/
//////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace WiiMoteOwnLib
{
    public class HIDImports
    {
        // Flags controlling what is included in the device information set built by SetupDiGetClassDevs
        internal const int DIGCF_DEFAULT = 0x00000001;       // only valid with DIGCF_DEVICEINTERFACE
        internal const int DIGCF_PRESENT = 0x00000002;
        internal const int DIGCF_ALLCLASSES = 0x00000004;
        internal const int DIGCF_PROFILE = 0x00000008;
        internal const int DIGCF_DEVICEINTERFACE = 0x00000010;

        [Flags]
        internal enum EFileAttributes : uint
        {
            Readonly = 0x00000001,
            Hidden = 0x00000002,
            System = 0x00000004,
            Directory = 0x00000010,
            Archive = 0x00000020,
            Device = 0x00000040,
            Normal = 0x00000080,
            Temporary = 0x00000100,
            SparseFile = 0x00000200,
            ReparsePoint = 0x00000400,
            Compressed = 0x00000800,
            Offline = 0x00001000,
            NotContentIndexed = 0x00002000,
            Encrypted = 0x00004000,
            Write_Through = 0x80000000,
            Overlapped = 0x40000000,
            NoBuffering = 0x20000000,
            RandomAccess = 0x10000000,
            SequentialScan = 0x08000000,
            DeleteOnClose = 0x04000000,
            BackupSemantics = 0x02000000,
            PosixSemantics = 0x01000000,
            OpenReparsePoint = 0x00200000,
            OpenNoRecall = 0x00100000,
            FirstPipeInstance = 0x00080000
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public Guid ClassGuid;
            public uint DevInst;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid InterfaceClassGuid;
            public int Flags;
            public IntPtr RESERVED;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public UInt32 cbSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string DevicePath;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HIDD_ATTRIBUTES
        {
            public int Size;
            public short VendorID;
            public short ProductID;
            public short VersionNumber;
        }

        [DllImport(@"hid.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern void HidD_GetHidGuid(out Guid gHid);

        [DllImport("hid.dll")]
        internal static extern Boolean HidD_GetAttributes(IntPtr HidDeviceObject, ref HIDD_ATTRIBUTES Attributes);

        [DllImport("hid.dll")]
        internal extern static bool HidD_SetOutputReport(
                IntPtr HidDeviceObject,
                byte[] lpReportBuffer,
                uint ReportBufferLength);

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SetupDiGetClassDevs(
                ref Guid ClassGuid,
                [MarshalAs(UnmanagedType.LPTStr)] string Enumerator,
                IntPtr hwndParent,
                UInt32 Flags
                );

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern Boolean SetupDiEnumDeviceInterfaces(
                IntPtr hDevInfo,
                IntPtr devInvo,
                ref Guid interfaceClassGuid,
                UInt32 memberIndex,
                ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData
            );

        [DllImport(@"setupapi.dll", SetLastError = true)]
        internal static extern Boolean SetupDiGetDeviceInterfaceDetail(
                IntPtr hDevInfo,
                ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
                IntPtr deviceInterfaceDetailData,
                UInt32 deviceInterfaceDetailDataSize,
                out UInt32 requiredSize,
                IntPtr deviceInfoData
            );

        [DllImport(@"setupapi.dll", SetLastError = true)]
        internal static extern Boolean SetupDiGetDeviceInterfaceDetail(
                IntPtr hDevInfo,
                ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
                ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData,
                UInt32 deviceInterfaceDetailDataSize,
                out UInt32 requiredSize,
                IntPtr deviceInfoData
            );

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern UInt16 SetupDiDestroyDeviceInfoList(IntPtr hDevInfo);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern SafeFileHandle CreateFile(
                string fileName,
                [MarshalAs(UnmanagedType.U4)] FileAccess fileAccess,
                [MarshalAs(UnmanagedType.U4)] FileShare fileShare,
                IntPtr securityAttributes,
                [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
                [MarshalAs(UnmanagedType.U4)] EFileAttributes flags,
                IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);
    }
}