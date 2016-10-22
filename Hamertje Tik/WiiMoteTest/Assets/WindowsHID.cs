using Microsoft.Win32.SafeHandles;
using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;




namespace Assets
{
    public class WindowsHID : HIDAPI
    {
        #region Constants
        /// <summary>Used in SetupDiClassDevs to get devices present in the system</summary>
        protected const int DIGCF_PRESENT = 0x02;
        /// <summary>Used in SetupDiClassDevs to get device interface details</summary>
        protected const int DIGCF_DEVICEINTERFACE = 0x10;
        /// <summary>Value of invalid Handle(s)</summary>
        public const uint INVALID_HANDLE_VALUE = 0xFFFFFFFF;
        /// <summary>CreateFile : Open handle for overlapped operations (doesn't exist in .net 2 FileAttributes)</summary>
        protected const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        #endregion

        /// <summary>
        /// Constructor for API
        /// </summary>
        public WindowsHID() {}

        #region Structs
        #region HIDConnect
        /// <summary>
        /// Provides (Limited) info about a Single USB-Device
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct DEVICE_INTERFACE_DATA
        {
            public Int32 cbSize;
            public Guid interfaceClassGuid;
            public Int32 flags;
            public UIntPtr reserved;
        }

        /// <summary>
        /// Detailed data for a Device (includes DevicePath)
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        public struct DEVICE_INTERFACE_DETAIL_DATA
        {
            public int Size;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string DevicePath;
        }

        /// <summary>
        /// Provides (Limited) info on a device  (UNUSED) (Crashed if used)
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct DEVINFO_DATA
        {
            public uint cbSize;
            public Guid classGuid;
            public uint devInst;
            public IntPtr reserved;
        }

        /// <summary>
        /// Provides the capabilities (buffersize, etc.) of a HID device
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        protected struct HidCaps
        {
            public short Usage;
            public short UsagePage;
            public short InputReportByteLength;
            public short OutputReportByteLength;
            public short FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public short[] Reserved;
            public short NumberLinkCollectionNodes;
            public short NumberInputButtonCaps;
            public short NumberInputValueCaps;
            public short NumberInputDataIndices;
            public short NumberOutputButtonCaps;
            public short NumberOutputValueCaps;
            public short NumberOutputDataIndices;
            public short NumberFeatureButtonCaps;
            public short NumberFeatureValueCaps;
            public short NumberFeatureDataIndices;
        }
        #endregion

        #region WinApi
        /* ---------------------------------------------------------
         * WINAPI STUFF    -- Taken from John Franco's Blogspot post (WinFileApi) in 2009
         * See also : http://buiba.blogspot.nl/2009/06/using-winapi-createfile-readfile.html
         * ------------------------------------------------------ */
        /// <summary>Flags for Device-Access (Read/Write)</summary>
        [Flags]
        public enum DesiredAccess : uint
        {
            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000
        }

        /// <summary>Flags for ShareMode (Sharing Read/Write)</summary>
        [Flags]
        public enum ShareMode : uint
        {
            FILE_SHARE_NONE = 0x0,
            FILE_SHARE_READ = 0x1,
            FILE_SHARE_WRITE = 0x2,
            FILE_SHARE_DELETE = 0x4,

        }

        /// <summary>Flags for Device-FileCreation (and Disposition)</summary>
        [Flags]
        public enum CreationDisposition : uint
        {
            CREATE_NEW = 1,
            CREATE_ALWAYS = 2,
            OPEN_EXISTING = 3,
            OPEN_ALWAYS = 4,
            TRUNCATE_EXSTING = 5
        }

        /// <summary>Extended flags for Devices</summary>
        [Flags]
        public enum FlagsAndAttributes : uint
        {
            FILE_ATTRIBUTES_ARCHIVE = 0x20,
            FILE_ATTRIBUTE_HIDDEN = 0x2,
            FILE_ATTRIBUTE_NORMAL = 0x80,
            FILE_ATTRIBUTE_OFFLINE = 0x1000,
            FILE_ATTRIBUTE_READONLY = 0x1,
            FILE_ATTRIBUTE_SYSTEM = 0x4,
            FILE_ATTRIBUTE_TEMPORARY = 0x100,
            FILE_FLAG_WRITE_THROUGH = 0x80000000,
            FILE_FLAG_OVERLAPPED = 0x40000000,
            FILE_FLAG_NO_BUFFERING = 0x20000000,
            FILE_FLAG_RANDOM_ACCESS = 0x10000000,
            FILE_FLAG_SEQUENTIAL_SCAN = 0x8000000,
            FILE_FLAG_DELETE_ON = 0x4000000,
            FILE_FLAG_POSIX_SEMANTICS = 0x1000000,
            FILE_FLAG_OPEN_REPARSE_POINT = 0x200000,
            FILE_FLAG_OPEN_NO_CALL = 0x100000
        }        
        #endregion
        #endregion

        #region P/Invoke
        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern SafeFileHandle CreateFile(string fileName, DesiredAccess dwDesiredAccess, ShareMode dwShareMode, IntPtr lpSecurityAttributes, CreationDisposition dwCreationDisposition, FlagsAndAttributes dwFlagsAndAttributes, IntPtr hTemplateFile);

        /// <summary>
        /// Closes a filehandle
        /// </summary>
        /// <param name="hObject">object to close</param>
        /// <returns>True if succesfull</returns>
        [DllImport("kernel32.dll", SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);


        /// <summary>
        /// Gets the Windows GUID for the HID class Devices
        /// </summary>
        /// <param name="guid">Out: GUID for HID</param>
        [DllImport("hid.dll", SetLastError = true)]
        protected static extern void HidD_GetHidGuid(out Guid guid);

        /// <summary>
        /// Retrieve a device information set for the devices in a specified class.
        /// Reserves a block of memory which must be freed.
        /// </summary>
        /// <param name="ClassGuid">GUID for set (GUID for covering HID-interface)</param>
        /// <param name="Enumerator">Enumerator for Hardware-Interfaces</param>
        /// <param name="hwndParent">Interface-Parent</param>
        /// <param name="Flags">Flags for set (e.g. PRESENT)</param>
        /// <returns>Handle for (covering) HID-Interface</returns>
        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, [MarshalAs(UnmanagedType.LPTStr)] string Enumerator, IntPtr hwndParent, uint Flags);

        /// <summary>
        /// Retrieve info about a device in a given set
        /// </summary>
        /// <param name="hDevInfo">Handle for Device-Set</param>
        /// <param name="devInfo">Limited Device Info (UNUSED)</param>
        /// <param name="interfaceClassGuid">GUID for top-level interface (HID-GUID)</param>
        /// <param name="memberIndex">Index of device to look for</param>
        /// <param name="deviceInterfaceData">Limited Device Info</param>
        /// <returns>True if successfull</returns>
        [DllImport("setupapi.dll", SetLastError = true)]
        protected static extern Boolean SetupDiEnumDeviceInterfaces(IntPtr hDevInfo, uint devInfo, ref Guid interfaceClassGuid, uint memberIndex, ref DEVICE_INTERFACE_DATA deviceInterfaceData);

        /// <summary>
        /// Only used to set requiredSize (Should fail)
        /// </summary>
        /// <param name="hDevInfo">Handle for Device-Set</param>
        /// <param name="deviceInterfaceData">Device-Data</param>
        /// <param name="deviceInterfaceDetailData">Should be set to IntPtr.Zero (to get sizeNeeded)</param>
        /// <param name="deviceInterfaceDetailDataSize">Should be 0</param>
        /// <param name="requiredSize">Out: NeededSize for succesfull call to SetupDiGetDeviceInterfaceDetail</param>
        /// <param name="deviceInfoData">Should be IntPtr.Zero</param>
        /// <returns>False if used correctly (should fail)</returns>
        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Boolean SetupDiGetDeviceInterfaceDetail(IntPtr hDevInfo, ref DEVICE_INTERFACE_DATA deviceInterfaceData, IntPtr deviceInterfaceDetailData, uint deviceInterfaceDetailDataSize, out uint requiredSize, IntPtr deviceInfoData);

        /// <summary>
        /// Actually gets Detailed Device Information (Needs correct parameters)
        /// </summary>
        /// <param name="hDevInfo">Handle for Device-Set</param>
        /// <param name="deviceInterfaceData">Reference to Device_Data</param>
        /// <param name="deviceInterfaceDetailData">Reference to Detailed_Data (Will be filled)</param>
        /// <param name="deviceInterfaceDetailDataSize">Size of Detailed_Data (to be set beforehand)</param>
        /// <param name="requiredSize">Required Size (Set through other SetupDiGetDeviceInterfaceDetail</param>
        /// <param name="deviceInfoData">Should be IntPtr.Zero</param>
        /// <returns>True if sucessfull</returns>
        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Boolean SetupDiGetDeviceInterfaceDetail(IntPtr hDevInfo, ref DEVICE_INTERFACE_DATA deviceInterfaceData, ref DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData, uint deviceInterfaceDetailDataSize, out uint requiredSize, IntPtr deviceInfoData);

        /// <summary>
        /// Frees up the memory used by the DeviceSet
        /// </summary>
        /// <param name="DeviceInfoSet">Ptr to deviceset</param>
        /// <returns>True if successfull</returns>
        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        /// <summary>
        /// Gets details from a device. 
        /// Reserves a block of memory which must be freed.
        /// </summary>
        /// <param name="hFile">Device file handle</param>
        /// <param name="lpData">Out: Reference to the preparsed data block</param>
        /// <returns>True if successfull</returns>
        [DllImport("hid.dll", SetLastError = true)]
        protected static extern bool HidD_GetPreparsedData(IntPtr hFile, out IntPtr lpData);

        /// <summary>
        /// Frees the block of memory used by PreparsedData
        /// </summary>
        /// <param name="PreparsedData">Reference to the preparsed data block</param>
        /// <returns>True if successfull</returns>
        [DllImport("hid.dll", SetLastError = true)]
        static extern Boolean HidD_FreePreparsedData(ref IntPtr PreparsedData);

        /// <summary>
        /// Gets a device's capabilities from preparsed data.
        /// </summary>
        /// <param name="lpData">Preparsed data reference</param>
        /// <param name="oCaps">HidCaps structure to receive the capabilities</param>
        /// <returns>True if successful</returns>
        [DllImport("hid.dll", SetLastError = true)]
        protected static extern int HidP_GetCaps(IntPtr lpData, out HidCaps oCaps);
        /*
        /// <summary>
        /// Opens a file-handle to a device
        /// Makes use of flags detailed in this class
        /// </summary>
        /// <param name="lpFileName">Device-Path to open</param>
        /// <param name="dwDesiredAccess">Desired Access Mode</param>
        /// <param name="dwShareMode">Share-Mode</param>
        /// <param name="lpSecurityAttributes">Security-Flags</param>
        /// <param name="dwCreationDisposition">Mode to open/dispose</param>
        /// <param name="dwFlagsAndAttributes">Extended Flags</param>
        /// <param name="hTemplateFile">Template-File (Use IntPtr.Zero)</param>
        /// <returns>SafeFileHandle (Which is valid if successfull</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern SafeFileHandle CreateFile(string lpFileName, DesiredAccess dwDesiredAccess, ShareMode dwShareMode, IntPtr lpSecurityAttributes, CreationDisposition dwCreationDisposition, FlagsAndAttributes dwFlagsAndAttributes, IntPtr hTemplateFile);
        */
        /// <summary>
        /// Send an output-report to the HID-Device
        /// </summary>
        /// <param name="HidDeviceObject">Pointer to (Handle of) device</param>
        /// <param name="lpReportBuffer">Report to write</param>
        /// <param name="ReportBufferLength">Report Length</param>
        /// <returns>True if successfull</returns>
        [DllImport("hid.dll")]
        internal extern static bool HidD_SetOutputReport(IntPtr HidDeviceObject, byte[] lpReportBuffer, uint ReportBufferLength);
        
        /// <summary>
        /// Reads from an opened Device
        /// </summary>
        /// <param name="hFile">Handle of (Pointer to) Device</param>
        /// <param name="aBuffer">Buffer to read</param>
        /// <param name="cbToRead">Number of bytes to read</param>
        /// <param name="cbThatWereRead">Reference to number of bytes that were read</param>
        /// <param name="pOverlapped">Overlapped (Should be IntPtr.Zero)</param>
        /// <returns>True if Successfull</returns>
        [DllImport("kernel32", SetLastError = true)]
        internal static extern bool ReadFile(IntPtr hFile, Byte[] aBuffer, UInt32 cbToRead, ref UInt32 cbThatWereRead, IntPtr pOverlapped);

        /// <summary>
        /// Writes to a file (NOT USED FOR HID-DEVICES, USE HidD_SetOutputReport INSTEAD)
        /// </summary>
        /// <param name="hFile">File to write to</param>
        /// <param name="aBuffer">Buffer to write</param>
        /// <param name="cbToWrite">Number of bytes to write</param>
        /// <param name="cbThatWereWritten">Reference to number of bytes that were written</param>
        /// <param name="pOverlapped">Overlapped</param>
        /// <returns>True if Successfull</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool WriteFile(SafeFileHandle hFile, Byte[] aBuffer, UInt32 cbToWrite, ref UInt32 cbThatWereWritten, IntPtr pOverlapped);
        #endregion

        /// <summary>
        /// Following the 5 Step Process From:
        /// https://msdn.microsoft.com/en-us/library/windows/hardware/ff538731(v=vs.85).aspx
        /// We will only get the DevicePaths, Not Open a File (This is handled by the HIDDevice-class itself)
        /// Throws HIDExceptions on errors
        /// </summary>
        /// <returns>List of all HIDDevices attached to the System</returns>
        public override List<HIDDevice> GetDevices()
        {
            List<HIDDevice> devices = new List<HIDDevice>();
            
            // STEP 1
            Guid gHid;
            HidD_GetHidGuid(out gHid);

            // STEP 2
            IntPtr hDevInfo = SetupDiGetClassDevs(ref gHid, null, IntPtr.Zero, DIGCF_DEVICEINTERFACE | DIGCF_PRESENT);
            if (hDevInfo == new IntPtr(-1))
                throw new HIDException("No valid HID-Interface Found");
            DEVICE_INTERFACE_DATA diData = new DEVICE_INTERFACE_DATA();
            diData.cbSize = Marshal.SizeOf(diData); // 32
            uint index = 0; // Index to loop through all Devices
            try
            {
                // STEP 3
                while (SetupDiEnumDeviceInterfaces(hDevInfo, 0, ref gHid, index, ref diData))
                {
                    index++; // up index
                    uint sizeNeeded;
                    bool result = WindowsHID.SetupDiGetDeviceInterfaceDetail(hDevInfo, ref diData, IntPtr.Zero, 0, out sizeNeeded, IntPtr.Zero); // sizeNeeded becomes 180u
                    if (result || Marshal.GetLastWin32Error() != 122) throw new HIDException("Expeted GetDeviceInterfaceDetail to fail (error 122 for setting sizeNeeded)");                    
                    DEVICE_INTERFACE_DETAIL_DATA detailData = new DEVICE_INTERFACE_DETAIL_DATA();
                    detailData.Size = (IntPtr.Size == 8 ? 8 : 6); // Set 8 for 64-bit, and 6 (shouldn't it be 5?) for 32-bit

                    // STEP 4
                    if (!SetupDiGetDeviceInterfaceDetail(hDevInfo, ref diData, ref detailData, sizeNeeded, out sizeNeeded, IntPtr.Zero))
                    {
                        throw new HIDException("Error getting device's detailed information");
                    }
                    HIDDevice newDevice = new HIDDevice(detailData.DevicePath, this);
                    devices.Add(newDevice);
                }
            } catch (Exception exception)
            {
                HandleException(exception, "Exception in A Device");
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(hDevInfo);
            }
            return devices;
        }

        

        
        public override void GetDeviceInfo(IntPtr dev_Handle, out int inputLength, out int outputLength)
        {
            IntPtr device_data = IntPtr.Zero;
            try
            {
                if (!HidD_GetPreparsedData(dev_Handle, out device_data))
                {
                    throw new HIDException("Unable to get Preparsed data from device");
                }
                HidCaps device_capabilities;
                HidP_GetCaps(device_data, out device_capabilities);

                inputLength = device_capabilities.InputReportByteLength;
                outputLength = device_capabilities.OutputReportByteLength;
            } catch (Exception e)
            {
                inputLength = 0;
                outputLength = 0;
                throw new HIDException(e);
            }
            finally { HidD_FreePreparsedData(ref device_data); }
        }


        public override IntPtr Connect(string dev_Path)
        {
            return ConnectMe(dev_Path).DangerousGetHandle();
        }

       /// <summary>
       /// Connects to a device (opens/creates a file)
       /// Copied From John Franco's blogpost class (WinFileAPI)
       /// </summary>
       /// <param name="devicePath"></param>
       /// <param name="fDesiredAccess"></param>
       /// <param name="fShareMode"></param>
       /// <param name="fCreationDisposition"></param>
       /// <param name="fFlagsAndAttributes"></param>
       /// <returns></returns>
        public SafeFileHandle ConnectMe(string devicePath)
        {
            if (devicePath.Length == 0)
                throw new ArgumentNullException("DevicePath Incorrect");
            SafeFileHandle handle = null;
            //handle = CreateFile(devicePath, FileAccess.Read | FileAccess.Write, FileShare.Read | FileShare.Write, IntPtr.Zero, FileMode.Open, FILE_FLAG_OVERLAPPED, IntPtr.Zero);
            //handle = CreateFile(devicePath, DesiredAccess.GENERIC_READ, ShareMode.FILE_SHARE_READ, IntPtr.Zero, CreationDisposition.OPEN_EXISTING, 0, IntPtr.Zero);
            handle = CreateFile(devicePath, DesiredAccess.GENERIC_READ, ShareMode.FILE_SHARE_READ | ShareMode.FILE_SHARE_WRITE, IntPtr.Zero, CreationDisposition.OPEN_EXISTING, 0, IntPtr.Zero);
            if (handle == null || handle.IsInvalid)
            {
                handle = null;
                HandleException(new FileNotFoundException("Device not found (is it connected correctly?)"), "HIDException");
            }
            return handle;
        }
        
        /// <summary>
        /// Reads from a file (WinFileAPI)
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="cbToRead"></param>
        /// <returns></returns>
        public override uint Read(HIDDevice device, byte[] buffer, uint cbToRead)
        {
            SafeFileHandle handle = ConnectMe(device.devicePath);
            uint cbThatWereRead = 0;
            if (!ReadFile(handle.DangerousGetHandle(), buffer, cbToRead, ref cbThatWereRead, IntPtr.Zero))
                HandleException(new HIDException("Error reading from Device"), "HIDException");
            CloseHandle(handle.DangerousGetHandle());
            return cbThatWereRead;
        }

        /// <summary>
        /// Writes to a HID-Device (OutputReport)
        /// </summary>
        /// <param name="device">Device to write to</param>
        /// <param name="buffer">Buffer to write</param>
        /// <param name="cbToWrite">number of bytes</param>
        /// <returns>Number of bytes written</returns>
        public override uint Write(HIDDevice device, byte[] buffer, uint cbToWrite)
        {
            SafeFileHandle handle = ConnectMe(device.devicePath);
            uint cbThatWereWritten = 0;
            if (!HidD_SetOutputReport(handle.DangerousGetHandle(), buffer, cbToWrite))
                HandleException(new HIDException("Error writing to Device"), "HIDException");
            Disconnect(handle.DangerousGetHandle());
            return cbThatWereWritten;
        }

        /// <summary>
        /// Closes a File
        /// </summary>
        /// <param name="devHandle"></param>
        public override bool Disconnect(IntPtr devHandle)
        {
            try
            {
                return CloseHandle(devHandle);
            } catch (Exception e)
            {
                HandleException(e, "Error Closing");
                return false;
            }
        }        
    }
}