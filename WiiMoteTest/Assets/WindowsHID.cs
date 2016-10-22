using Microsoft.Win32.SafeHandles;
using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;



namespace Assets
{
    public class WindowsHID : HIDAPI
    {
        #region DWORDS
        /// <summary>
        /// Generic Access Rights
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/aa446632(v=vs.85).aspx
        /// </summary>
        [Flags]
        public enum File_Access : uint
        {
            /// <summary>
            /// Read access
            /// </summary>
            GENERIC_READ = 0x80000000,
            /// <summary>
            /// Write access
            /// </summary>
            GENERIC_WRITE = 0x40000000,
            /// <summary>
            /// Execute access
            /// </summary>
            GENERIC_EXECUTE = 0x2000000,
            /// <summary>
            /// All possible access rights
            /// </summary>
            GENERIC_ALL = 0x10000000
        }

        /// <summary>
        /// ShareMode for CreateFile
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/aa363858(v=vs.85).aspx
        /// </summary>
        [Flags]
        public enum File_Share : uint
        {
            /// <summary>
            /// Prevents other processes from opening a file or device if they request delete, read, or write access
            /// </summary>
            FILE_SHARE_NONE = 0x0,
            /// <summary>
            /// Enables subsequent open operations on a file or device to request read access
            /// </summary>
            FILE_SHARE_READ = 0x1,
            /// <summary>
            /// Enables subsequent open operations on a file or device to request write access
            /// </summary>
            FILE_SHARE_WRITE = 0x2,
            /// <summary>
            /// Enables subsequent open operations on a file or device to request delete access
            /// Note: Delete access allows both delete and rename operations
            /// </summary>
            FILE_SHARE_DELETE = 0x4
        }

        /// <summary>
        /// Creation and Disposition for CreateFile
        /// See MSDN for error codes:
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/aa363858(v=vs.85).aspx
        /// </summary>
        [Flags]
        public enum File_Mode : uint
        {
            /// <summary>
            /// Creates a new file, only if it does not already exist
            /// </summary>
            CREATE_NEW = 1,
            /// <summary>
            /// Creates a new file, always
            /// </summary>
            CREATE_ALWAYS = 2,
            /// <summary>
            /// Opens a file or device, only if it exists
            /// </summary>
            OPEN_EXISTING = 3,
            /// <summary>
            /// Opens a file, always
            /// </summary>
            OPEN_ALWAYS = 4,
            /// <summary>
            /// Opens a file and truncates it so that its size is zero bytes, only if it exists
            /// </summary>
            TRUNCATE_EXSTING = 5
        }

        /// <summary>
        /// Several Flags and Attributes for a File.
        /// </summary>
        [Flags]
        public enum File_Attributes_Flags : uint
        {
            FILE_ATTRIBUTE_READONLY = 0x00000001,
            FILE_ATTRIBUTE_HIDDEN = 0x00000002,
            FILE_ATTRIBUTE_SYSTEM = 0x00000004,
            FILE_ATTRIBUTE_DIRECTORY = 0x00000010,
            FILE_ATTRIBUTE_ARCHIVE = 0x00000020,
            FILE_ATTRIBUTE_DEVICE = 0x00000040,
            FILE_ATTRIBUTE_NORMAL = 0x00000080,
            FILE_ATTRIBUTE_TEMPORARY = 0x00000100,
            FILE_ATTRIBUTE_SPARSEFILE = 0x00000200,
            FILE_ATTRIBUTE_REPARSEPOINT = 0x00000400,
            FILE_ATTRIBUTE_COMPRESSED = 0x00000800,
            FILE_ATTRIBUTE_OFFLINE = 0x00001000,
            FILE_ATTRIBUTE_NOTCONTENTINDEXED = 0x00002000,
            FILE_ATTRIBUTE_ENCRYPTED = 0x00004000,

            FILE_FLAG_WRITE_THROUGH = 0x80000000,
            FILE_FLAG_OVERLAPPED = 0x40000000,
            FILE_FLAG_NO_BUFFERING = 0x20000000,
            FILE_FLAG_RANDOM_ACCESS = 0x10000000,
            FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000,
            FILE_FLAG_DELETE_ON_CLOSE = 0x04000000,
            FILE_FLAG_BACKUP_SEMANTICS = 0x02000000,
            FILE_FLAG_POSIX_SEMANTICS = 0x01000000,
            FILE_FLAG_SESSION_AWARE = 0x00800000,
            FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000,
            FILE_FLAG_OPEN_NO_RECALL = 0x00100000
        }

        /// <summary>
        /// Control Flags for device operation
        /// </summary>
        [Flags]
        public enum Device_Control_Flags : uint
        {
            /// <summary>
            /// Only valid with DIGCF_DEVICEINTERFACE
            /// </summary>
            DIGCF_DEFAULT = 0x00000001,
            /// <summary>
            /// Returns only currently present devices
            /// </summary>
            DIGCF_PRESENT = 0x00000002,
            /// <summary>
            /// Interface of all classes
            /// </summary>
            DIGCF_ALLCLASSES = 0x00000004,
            /// <summary>
            /// Returns only devices that are part of the current hardware profile
            /// </summary>
            DIGCF_PROFILE = 0x00000008,
            /// <summary>
            /// Device Interface
            /// </summary>
            DIGCF_DEVICEINTERFACE = 0x00000010
        }

        /// <summary>
        /// Flags for Device-Interfaces
        /// </summary>
        [Flags]
        public enum Interface_Flags : uint
        {
            /// <summary>
            /// The interface is active (enabled)
            /// </summary>
            SPINT_ACTIVE  = 0x1,
            /// <summary>
            /// The interface is the default interface for the device class
            /// </summary>
            SPINT_DEFAULT = 0x2,
            /// <summary>
            /// The interface is removed
            /// </summary>
            SPINT_REMOVED = 0x4
        }

        /// <summary>
        /// Status Codes for HIDP
        /// </summary>
        [Flags]
        public enum HIDP_Status_Codes : uint
        {
            HIDP_STATUS_SUCCES                  = 0x00110000,
            HIDP_STATUS_NULL                    = 0x80110001,
            HIDP_STATUS_INVALID_PREPARSED_DATA  = 0xC0110001,
            HIDP_STATUS_INVALID_REPORT_TYPE     = 0xC0110002,
            HIDP_STATUS_INVALID_REPORT_LENGTH   = 0xC0110003,
            HIDP_STATUS_USAGE_NOT_FOUND         = 0xC0110004,
            HIDP_STATUS_VALUE_OUT_OF_RANGE      = 0xC0110005,
            HIDP_STATUS_BAD_LOG_PHY_VALUES      = 0xC0110006,
            HIDP_STATUS_BUFFER_TOO_SMALL        = 0xC0110006,
            HIDP_STATUS_INTERNAL_ERROR          = 0xC0110008,
            HIDP_STATUS_I8042_TRANS_UNKNOWN     = 0xC0110009,
            HIDP_STATUS_INCOMPATIBLE_REPORT_ID  = 0xC011000A,
            HIDP_STATUS_NOT_VALUE_ARRAY         = 0xC011000B,
            HIDP_STATUS_IS_VALUE_ARRAY          = 0xC011000C,
            HIDP_STATUS_DATA_INDEX_NOT_FOUND    = 0xC011000D,
            HIDP_STATUS_DATA_INDEX_OUT_OF_RANGE = 0xC011000E,
            HIDP_STATUS_BUTTON_NOT_PRESSED      = 0xC011000F,
            HIDP_STATUS_REPORT_DOES_NOT_EXIST   = 0xC0110010,
            HIDP_STATUS_NOT_IMPLEMENTED         = 0xC0110020
        }
        #endregion
        
        #region Structs
        #region HIDConnect
        /// <summary>
        /// A DEVICE_INTERFACE_DATA structure defines a device interface in a device information set
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct DEVICE_INTERFACE_DATA
        {
            /// <summary>
            /// The size, in bytes, of the structure
            /// </summary>
            public int cbSize;
            /// <summary>
            /// The GUID for the class to which the device interface belongs
            /// </summary>
            public Guid interfaceClassGuid;
            /// <summary>
            /// Flags for this Device Interface
            /// </summary>
            public Interface_Flags flags;
            /// <summary>
            /// Reserved. Do not use
            /// </summary>
            public IntPtr reserved;
        }

        /// <summary>
        /// Contains the path for a device interface
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
        public struct DEVICE_INTERFACE_DETAIL_DATA
        {
            /// <summary>
            /// The size, in bytes, of the structure
            /// </summary>
            public int cbSize;
            /// <summary>
            /// A NULL-terminated string that contains the device interface path
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string devicePath;
        }

        /// <summary>
        /// The HIDD_ATTRIBUTES structure contains vendor information about a HIDClass device
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
        public struct HIDD_ATTRIBUTES
        {
            /// <summary>
            /// Specifies the size, in bytes, of this structure
            /// </summary>
            public ulong cbSize;
            /// <summary>
            /// Specifies a HID device's Vendor ID
            /// </summary>
            public ushort vendorID;
            /// <summary>
            /// Specifies a HID device's Product ID
            /// </summary>
            public ushort productID;
            /// <summary>
            /// Specifies the manufacturer's revision number for a HIDClass device
            /// </summary>
            public ushort versionNumber;
        }

        /// <summary>
        /// The HIDP_CAPS structure contains information about a top-level collection's capability
        /// Provides the capabilities (buffersize, etc.) of a HID device
        /// https://msdn.microsoft.com/en-us/library/windows/hardware/ff539697(v=vs.85).aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HIDP_CAPS
        {
            /// <summary>
            /// Specifies a top-level collection's usage ID
            /// </summary>
            public short Usage;
            /// <summary>
            /// Specifies the top-level collection's usage page
            /// </summary>
            public short UsagePage;
            /// <summary>
            /// Specifies the maximum size, in bytes, of all the input reports
            /// </summary>
            public short InputReportByteLength;
            /// <summary>
            /// Specifies the maximum size, in bytes, of all the output reports
            /// </summary>
            public short OutputReportByteLength;
            /// <summary>
            /// Specifies the maximum length, in bytes, of all the feature reports
            /// </summary>
            public short FeatureReportByteLength;
            /// <summary>
            /// Reserved for internal system use
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public short[] Reserved;
            /// <summary>
            /// Specifies the number of HIDP_LINK_COLLECTION_NODE structures that are returned for this top-level collection by HidP_GetLinkCollectionNodes
            /// </summary>
            public short NumberLinkCollectionNodes;
            /// <summary>
            /// Specifies the number of input HIDP_BUTTON_CAPS structures that HidP_GetButtonCaps returns
            /// </summary>
            public short NumberInputButtonCaps;
            /// <summary>
            /// Specifies the number of input HIDP_VALUE_CAPS structures that HidP_GetValueCaps returns
            /// </summary>
            public short NumberInputValueCaps;
            /// <summary>
            /// Specifies the number of data indices assigned to buttons and values in all input reports
            /// </summary>
            public short NumberInputDataIndices;
            /// <summary>
            /// Specifies the number of output HIDP_BUTTON_CAPS structures that HidP_GetButtonCaps returns
            /// </summary>
            public short NumberOutputButtonCaps;
            /// <summary>
            /// Specifies the number of output HIDP_VALUE_CAPS structures that HidP_GetValueCaps returns
            /// </summary>
            public short NumberOutputValueCaps;
            /// <summary>
            /// Specifies the number of data indices assigned to buttons and values in all output reports
            /// </summary>
            public short NumberOutputDataIndices;
            /// <summary>
            /// Specifies the total number of feature HIDP_BUTTONS_CAPS structures that HidP_GetButtonCaps returns
            /// </summary>
            public short NumberFeatureButtonCaps;
            /// <summary>
            /// Specifies the total number of feature HIDP_VALUE_CAPS structures that HidP_GetValueCaps returns
            /// </summary>
            public short NumberFeatureValueCaps;
            /// <summary>
            /// Specifies the number of data indices assigned to buttons and values in all feature reports
            /// </summary>
            public short NumberFeatureDataIndices;
        }
        #endregion

        #region Misc
        /// <summary>
        /// Security Attributes used in File creation
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            /// <summary>
            /// The size, in bytes, of this structure
            /// </summary>
            public int nLength;
            /// <summary>
            /// Optional Pointer to SECURITY_DESCRIPTOR structure that controls access to the object
            /// </summary>
            public IntPtr lpSecurityDescriptor;
            /// <summary>
            /// A Boolean value that specifies whether the returned handle is inherited when a new process is created
            /// If this member is TRUE, the new process inherits the handle
            /// </summary>
            public bool bInheritHandle;
        }

        /// <summary>
        /// Security Descriptor used in Security Attributes
        /// </summary>
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct SECURITY_DESCRIPTOR
        {
            public byte revision;
            public byte size;
            public short control;
            public IntPtr owner;
            public IntPtr group;
            public IntPtr sacl;
            public IntPtr dacl;
        }
        #endregion
        #endregion        
        
        #region P/Invoke
        private const string kernel32 = "kernel32.dll";
        private const string setupapi = "setupapi.dll";
        private const string hid = "hid.dll";
        
        // All Info taken from Windows MSDN References
        #region File
        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/aa363858(v=vs.85).aspx
        /// Creates or opens a file or I/O device.
        /// </summary>
        /// <param name="fileName">The name of the file or device to be created or opened. You may use either forward slashes (/) or backslashes (\) in this name.</param>
        /// <param name="dwDesiredAccess">The requested access to the file or device, which can be summarized as read, write, both or neither</param>
        /// <param name="dwShareMode">The requested sharing mode of the file or device, which can be read, write, both, delete, all of these, or none</param>
        /// <param name="lpSecurityAttributes">OPTIONAL SECURITY_ATTRIBUTES structure that contains two separate but related data members: an optional security descriptor, and a Boolean value that determines whether the returned handle can be inherited by child processes</param>
        /// <param name="dwCreationDisposition">An action to take on a file or device that exists or does not exist. For devices other than files, this parameter is usually set to OPEN_EXISTING</param>
        /// <param name="dwFlagsAndAttributes">The file or device attributes and flags</param>
        /// <param name="hTemplateFile">Optional Pointer to Template file with the GENERIC_READ access right. The template file supplies file attributes and extended attributes for the file that is being created</param>
        /// <returns></returns>
        [DllImport(kernel32, SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern IntPtr CreateFile(string fileName, File_Access dwDesiredAccess, File_Share dwShareMode, ref SECURITY_ATTRIBUTES lpSecurityAttributes, File_Mode dwCreationDisposition, File_Attributes_Flags dwFlagsAndAttributes, IntPtr hTemplateFile);

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/aa363858(v=vs.85).aspx
        /// Creates or opens a file or I/O device.
        /// </summary>
        /// <param name="fileName">The name of the file or device to be created or opened. You may use either forward slashes (/) or backslashes (\) in this name.</param>
        /// <param name="dwDesiredAccess">The requested access to the file or device, which can be summarized as read, write, both or neither</param>
        /// <param name="dwShareMode">The requested sharing mode of the file or device, which can be read, write, both, delete, all of these, or none</param>
        /// <param name="lpSecurityAttributes">OPTIONAL SECURITY_ATTRIBUTES structure that contains two separate but related data members: an optional security descriptor, and a Boolean value that determines whether the returned handle can be inherited by child processes</param>
        /// <param name="dwCreationDisposition">An action to take on a file or device that exists or does not exist. For devices other than files, this parameter is usually set to OPEN_EXISTING</param>
        /// <param name="dwFlagsAndAttributes">The file or device attributes and flags</param>
        /// <param name="hTemplateFile">Optional Pointer to Template file with the GENERIC_READ access right. The template file supplies file attributes and extended attributes for the file that is being created</param>
        /// <returns></returns>
        [DllImport(kernel32, SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern IntPtr CreateFile(string fileName, File_Access dwDesiredAccess, File_Share dwShareMode, IntPtr lpSecurityAttributes, File_Mode dwCreationDisposition, File_Attributes_Flags dwFlagsAndAttributes, IntPtr hTemplateFile);

        /// <summary>
        /// Reads data from the specified file or input/output (I/O) device.
        /// </summary>
        /// <param name="hFile">Handle of (Pointer to) File. The hFile parameter must have been created with read access</param>
        /// <param name="aBuffer">Buffer to read to</param>
        /// <param name="numberOfBytesToRead">Maximum number of bytes to read</param>
        /// <param name="numberOfBytesRead">A pointer to the variable that receives the number of bytes read when using a synchronous hFile parameter. ReadFile sets this value to zero before doing any work or error checking. Use NULL for this parameter if this is an asynchronous operation to avoid potentially erroneous results.
        /// This parameter can be NULL only when the lpOverlapped parameter is not NULL.</param>
        /// <param name="lpOverlapped">Overlapped (Should be IntPtr.Zero, unless opened with OVERLAPPED flag)</param>
        /// <returns>True if Successfull</returns>
        [DllImport(kernel32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        protected static extern bool ReadFile(SafeFileHandle hFile, byte[] aBuffer, uint numberOfBytesToRead, ref uint numberOfBytesRead, IntPtr lpOverlapped);

        /// <summary>
        /// Writes to a file 
        /// (NOT USED FOR HID-DEVICES, USE HidD_SetOutputReport INSTEAD)
        /// </summary>
        /// <param name="hFile">File to write to</param>
        /// <param name="aBuffer">Buffer to write</param>
        /// <param name="cbToWrite">Number of bytes to write</param>
        /// <param name="cbThatWereWritten">Reference to number of bytes that were written</param>
        /// <param name="lpOverlapped">Overlapped (Should be IntPtr.Zero, unless opened with OVERLAPPED flag)</param>
        /// <returns>True if Successfull</returns>
        [DllImport(kernel32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        protected static extern bool WriteFile(IntPtr hFile, byte[] aBuffer, uint cbToWrite, out uint cbThatWereWritten, IntPtr pOverlapped);

        /// <summary>
        /// Closes a filehandle
        /// </summary>
        /// <param name="hObject">object to close</param>
        /// <returns>True if succesfull</returns>
        [DllImport(kernel32, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        protected static extern bool CloseHandle(IntPtr hObject);
        #endregion

        #region HID
        #region Setup
        /// <summary>
        /// The SetupDiGetClassDevs function returns a handle to a device information set that contains requested device information elements for a local computer
        /// Reserves a block of memory which must be freed.
        /// https://msdn.microsoft.com/en-us/library/windows/hardware/ff551069(v=vs.85).aspx
        /// </summary>
        /// <param name="ClassGuid">A pointer to the GUID for a device setup class or a device interface class</param>
        /// <param name="Enumerator">Enumerator for Plug-and-Play Hardware-Interfaces</param>
        /// <param name="hwndParent">A handle to the top-level window to be used for a user interface that is associated with installing a device instance in the device information set</param>
        /// <param name="Flags">A variable of type DWORD that specifies control options that filter the device information elements that are added to the device information set</param>
        /// <returns>Handle for (covering) HID-Interface</returns>
        [DllImport(setupapi, SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, string Enumerator, IntPtr hwndParent, Device_Control_Flags Flags);

        /// <summary>
        /// Enumerates the device interfaces that are contained in a device information set
        /// https://msdn.microsoft.com/en-us/library/windows/hardware/ff551015(v=vs.85).aspx
        /// </summary>
        /// <param name="hDevInfoSet">A pointer to a device information set that contains the device interfaces for which to return information</param>
        /// <param name="devInfoData">Nullable SP_DEVINFO_DATA structure that specifies a device information element in DeviceInfoSet. This parameter is optional and can be NULL</param>
        /// <param name="interfaceClassGuid">A GUID that specifies the device interface class for the requested interface</param>
        /// <param name="memberIndex">A zero-based index into the list of interfaces in the device information set</param>
        /// <param name="deviceInterfaceData">A pointer to a caller-allocated buffer that contains, on successful return, a completed SP_DEVICE_INTERFACE_DATA structure that identifies an interface that meets the search parameters</param>
        /// <returns>True if successfull</returns>
        [DllImport(setupapi, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        protected static extern bool SetupDiEnumDeviceInterfaces(IntPtr hDevInfoSet, IntPtr devInfoData, ref Guid interfaceClassGuid, uint memberIndex, ref DEVICE_INTERFACE_DATA deviceInterfaceData);

        /// <summary>
        /// The SetupDiGetDeviceInterfaceDetail function returns details about a device interface
        /// If used to set requiredSize, this should fail (Use IntPtr.Zero for DEVICE_INTERFACE_DETAIL_DATA)
        /// https://msdn.microsoft.com/en-us/library/windows/hardware/ff551120(v=vs.85).aspx
        /// </summary>
        /// <param name="hDevInfo">A pointer to the device information set that contains the interface for which to retrieve details</param>
        /// <param name="deviceInterfaceData">A pointer to an SP_DEVICE_INTERFACE_DATA structure that specifies the interface in DeviceInfoSet for which to retrieve details</param>
        /// <param name="deviceInterfaceDetailData">A pointer to an SP_DEVICE_INTERFACE_DETAIL_DATA structure to receive information about the specified interface. This parameter is optional and can be NULL. This parameter must be NULL if DeviceInterfaceDetailSize is zero. If this parameter is specified, the caller must set DeviceInterfaceDetailData.cbSize to sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA) before calling this function. Should be set to IntPtr.Zero to get sizeNeeded</param>
        /// <param name="deviceInterfaceDetailDataSize">The size of the DeviceInterfaceDetailData buffer. The buffer must be at least (offsetof(SP_DEVICE_INTERFACE_DETAIL_DATA, DevicePath) + sizeof(TCHAR)) bytes, to contain the fixed part of the structure and a single NULL to terminate an empty MULTI_SZ string. This parameter must be zero if DeviceInterfaceDetailData is NULL</param>
        /// <param name="requiredSize">A variable that receives the required size of the DeviceInterfaceDetailData buffer</param>
        /// <param name="deviceInfoData">A pointer to a buffer that receives information about the device that supports the requested interface. This parameter is optional and can be NULL</param>
        /// <returns>True if the function completed without error</returns>
        [DllImport(setupapi, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        protected static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr hDevInfo, ref DEVICE_INTERFACE_DATA deviceInterfaceData, IntPtr deviceInterfaceDetailData, uint deviceInterfaceDetailDataSize, out uint requiredSize, IntPtr deviceInfoData);

        /// <summary>
        /// The SetupDiGetDeviceInterfaceDetail function returns details about a device interface
        /// https://msdn.microsoft.com/en-us/library/windows/hardware/ff551120(v=vs.85).aspx
        /// </summary>
        /// <param name="hDevInfo">A pointer to the device information set that contains the interface for which to retrieve details</param>
        /// <param name="deviceInterfaceData">A pointer to an SP_DEVICE_INTERFACE_DATA structure that specifies the interface in DeviceInfoSet for which to retrieve details</param>
        /// <param name="deviceInterfaceDetailData">A pointer to an SP_DEVICE_INTERFACE_DETAIL_DATA structure to receive information about the specified interface. This parameter is optional and can be NULL. This parameter must be NULL if DeviceInterfaceDetailSize is zero. If this parameter is specified, the caller must set DeviceInterfaceDetailData.cbSize to sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA) before calling this function. Should be set to IntPtr.Zero to get sizeNeeded</param>
        /// <param name="deviceInterfaceDetailDataSize">The size of the DeviceInterfaceDetailData buffer. The buffer must be at least (offsetof(SP_DEVICE_INTERFACE_DETAIL_DATA, DevicePath) + sizeof(TCHAR)) bytes, to contain the fixed part of the structure and a single NULL to terminate an empty MULTI_SZ string. This parameter must be zero if DeviceInterfaceDetailData is NULL</param>
        /// <param name="requiredSize">A variable that receives the required size of the DeviceInterfaceDetailData buffer</param>
        /// <param name="deviceInfoData">A pointer to a buffer that receives information about the device that supports the requested interface. This parameter is optional and can be NULL</param>
        /// <returns>True if the function completed without error</returns>
        [DllImport(setupapi, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        protected static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr hDevInfo, ref DEVICE_INTERFACE_DATA deviceInterfaceData, ref DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData, uint deviceInterfaceDetailDataSize, out uint requiredSize, IntPtr deviceInfoData);

        /// <summary>
        /// Deletes a device information set and frees all associated memory
        /// https://msdn.microsoft.com/en-us/library/windows/hardware/ff550996(v=vs.85).aspx
        /// </summary>
        /// <param name="DeviceInfoSet">A handle to the device information set to delete</param>
        /// <returns>True if successfull</returns>
        [DllImport(setupapi, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        protected static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);
        #endregion

        #region Settings
        /// <summary>
        /// Returns the device interfaceGUID for HIDClass devices
        /// https://msdn.microsoft.com/en-us/library/windows/hardware/ff538924(v=vs.85).aspx
        /// </summary>
        /// <param name="guid">Out: Pointer to a caller-allocated GUID buffer that the routine uses to return the device interface GUID for HIDClass devices</param>
        [DllImport(hid, SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern void HidD_GetHidGuid(out Guid guid);

        /// <summary>
        /// The HidD_GetAttributes routine returns the attributes of a specified top-level collection
        /// https://msdn.microsoft.com/en-us/library/windows/hardware/ff538900(v=vs.85).aspx
        /// </summary>
        /// <param name="hidDeviceObject">Specifies an open handle to a top-level collection</param>
        /// <param name="attributes">Pointer to a caller-allocated HIDD_ATTRIBUTES structure that returns the attributes of the collection specified by hidDeviceObject</param>
        /// <returns>True if successfull</returns>
        [DllImport(hid, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        protected static extern bool HidD_GetAttributes(IntPtr hidDeviceObject, out HIDD_ATTRIBUTES attributes);

        /// <summary>
        /// Returns a top-level collection's HIDP_CAPS structure
        /// https://msdn.microsoft.com/en-us/library/windows/hardware/ff539715(v=vs.85).aspx
        /// </summary>
        /// <param name="lpData">Pointer to a top-level collection's preparsed data</param>
        /// <param name="oCaps">Pointer to a caller-allocated buffer that the routine uses to return a collection's HIDP_CAPS structure</param>
        /// <returns>True if successful</returns>
        [DllImport(hid, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.I4)]
        protected static extern int HidP_GetCaps(IntPtr lpData, out HIDP_CAPS oCaps);
        #endregion

        #region Data
        /// <summary>
        /// Returns an input report from a top-level collection
        /// https://msdn.microsoft.com/en-us/library/ff538945(v=vs.85).aspx
        /// </summary>
        /// <param name="HidDeviceObject">Specifies an open handle to a top-level collection</param>
        /// <param name="lpReportBuffer">Pointer to a caller-allocated input report buffer that the caller uses to specify a HID report ID and HidD_GetInputReport uses to return the specified input report</param>
        /// <param name="ReportBufferLength">Specifies the size, in bytes, of the report buffer</param>
        /// <returns>True if successfull</returns>
        [DllImport(hid, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        protected extern static bool HidD_GetInputReport(IntPtr HidDeviceObject, out byte[] lpReportBuffer, ulong ReportBufferLength);

        /// <summary>
        /// Sends an output report to a top-level collection
        /// https://msdn.microsoft.com/en-us/library/windows/hardware/ff539690(v=vs.85).aspx
        /// </summary>
        /// <param name="HidDeviceObject">Specifies an open handle to a top-level collection</param>
        /// <param name="lpReportBuffer">Pointer to a caller-allocated output report buffer that the caller uses to specify a report ID</param>
        /// <param name="ReportBufferLength">Specifies the size, in bytes, of the report buffer</param>
        /// <returns>True if successfull</returns>
        [DllImport(hid, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        protected extern static bool HidD_SetOutputReport(IntPtr HidDeviceObject, byte[] lpReportBuffer, ulong ReportBufferLength);

        /// <summary>
        /// Returns a top-level collection's preparsed data
        /// Reserves a block of memory which must be freed.
        /// https://msdn.microsoft.com/en-us/library/windows/hardware/ff539679(v=vs.85).aspx
        /// </summary>
        /// <param name="hFile">Specifies an open handle to a top-level collection</param>
        /// <param name="lpData">Out: Pointer to the address of a routine-allocated buffer that contains a collection's preparsed data</param>
        /// <returns>True if successfull</returns>
        [DllImport(hid, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        protected static extern bool HidD_GetPreparsedData(IntPtr hFile, out IntPtr lpData);

        /// <summary>
        /// Releases the resources that the HID class driver allocated to hold a top-level collection's preparsed data
        /// https://msdn.microsoft.com/en-us/library/windows/hardware/ff538893(v=vs.85).aspx
        /// </summary>
        /// <param name="PreparsedData">Pointer to the buffer, returned by HidD_GetPreparsedData, that is freed</param>
        /// <returns>True if successfull</returns>
        [DllImport(hid, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        protected static extern bool HidD_FreePreparsedData(ref IntPtr PreparsedData);
        #endregion
        #endregion
        #endregion

        /// <summary>
        /// Constructor for API
        /// </summary>
        public WindowsHID() {}

        /// <summary>
        /// Following the 5 Step Process From:
        /// https://msdn.microsoft.com/en-us/library/windows/hardware/ff538731(v=vs.85).aspx
        /// We will only get the DevicePaths, Not Open a File (This is handled by the HIDDevice-class itself)
        /// Throws HIDExceptions on errors
        /// </summary>
        /// <returns>List of all HIDDevices attached to the System</returns>
        public override List<HIDDevice> GetDevices()
        {
            List<HIDDevice> foundDevices = new List<HIDDevice>();

            // STEP 1
            Guid gHid;
            HidD_GetHidGuid(out gHid);
            // STEP 2
            IntPtr hDevInfo = SetupDiGetClassDevs(ref gHid, null, IntPtr.Zero, Device_Control_Flags.DIGCF_DEVICEINTERFACE | Device_Control_Flags.DIGCF_PRESENT);
            if (hDevInfo == new IntPtr(-1))
                throw new HIDException(new Win32Exception(), ExceptionType.OS_ERROR, "No valid HID-Interface Found");
            DEVICE_INTERFACE_DATA diData = new DEVICE_INTERFACE_DATA();
            diData.cbSize = Marshal.SizeOf(diData);
            uint index = 0; // Index to loop through all Devices
            // STEP 3
            while (SetupDiEnumDeviceInterfaces(hDevInfo, IntPtr.Zero, ref gHid, index, ref diData))
            {
                index++; // up index
                uint sizeNeeded;
                bool result = WindowsHID.SetupDiGetDeviceInterfaceDetail(hDevInfo, ref diData, IntPtr.Zero, 0, out sizeNeeded, IntPtr.Zero); // sizeNeeded becomes 180u
                if (result || Marshal.GetLastWin32Error() != 122) throw new HIDException("Expected GetDeviceInterfaceDetail to fail (error 122 for setting sizeNeeded)");
                DEVICE_INTERFACE_DETAIL_DATA detailData = new DEVICE_INTERFACE_DETAIL_DATA();
                detailData.cbSize = (IntPtr.Size == 8 ? 8 : 6); // Set 8 for 64-bit, and 6 (shouldn't it be 5?) for 32-bit

                // STEP 4
                if (!SetupDiGetDeviceInterfaceDetail(hDevInfo, ref diData, ref detailData, sizeNeeded, out sizeNeeded, IntPtr.Zero))
                {
                    throw new HIDException(new Win32Exception(), ExceptionType.DEVICE_ERROR, "Error getting device's detailed information");
                }
                HIDDevice newDevice = new HIDDevice(detailData.devicePath, this);
                foundDevices.Add(newDevice);
            }
            int errorCode = Marshal.GetLastWin32Error();
            if (errorCode != 0x103)
            {
                throw new HIDException(new Win32Exception(errorCode), ExceptionType.OS_ERROR, "Expecting ERROR_NO_MORE_ITEMS");
            }
            if (!SetupDiDestroyDeviceInfoList(hDevInfo))
                throw new HIDException(new Win32Exception(), ExceptionType.OS_ERROR, "Error destroying devicelist");
            foreach (HIDDevice dev in foundDevices)
            {
                string devPath = dev.devicePath;
                foreach (HIDDevice devi in devices)
                    if (devi.devicePath.Equals(dev.devicePath))
                        goto skipAdd;
                devices.Add(dev);
                skipAdd: ;
            }
            return foundDevices;
        }

        public override void InitialiseDeviceInfo(HIDDevice device)
        {
            if (!device.isConnected)
                throw new HIDException(ExceptionType.CONNECTION, "Device is not Connected");
            IntPtr device_data = IntPtr.Zero;
            if (!HidD_GetPreparsedData(device.filePointer, out device_data))
                throw new HIDException(new Win32Exception(), ExceptionType.DEVICE_ERROR, "Unable to get Preparsed data from device");
            HIDD_ATTRIBUTES attributes = new HIDD_ATTRIBUTES();
            attributes.cbSize = (ulong)Marshal.SizeOf(attributes);
            if (!HidD_GetAttributes(device.filePointer, out attributes))
                throw new HIDException(new Win32Exception(), ExceptionType.DEVICE_ERROR, "Unable to get Attributes from device");
            HIDP_CAPS capabilities;
            HIDP_Status_Codes status = (HIDP_Status_Codes)((uint)HidP_GetCaps(device_data, out capabilities));
            if (status != HIDP_Status_Codes.HIDP_STATUS_SUCCES)
                throw new HIDException(ExceptionType.DEVICE_ERROR, "Could not retrieve Device Capabilities");
            device.SetDeviceInfo(attributes.vendorID, attributes.productID, attributes.versionNumber,
                capabilities.InputReportByteLength, capabilities.OutputReportByteLength, capabilities.FeatureReportByteLength, capabilities.NumberLinkCollectionNodes,
                capabilities.NumberInputButtonCaps, capabilities.NumberInputValueCaps, capabilities.NumberInputDataIndices,
                capabilities.NumberOutputButtonCaps, capabilities.NumberOutputValueCaps, capabilities.NumberOutputDataIndices,
                capabilities.NumberFeatureButtonCaps, capabilities.NumberFeatureValueCaps, capabilities.NumberFeatureDataIndices);
        }

        public override IntPtr Connect(string dev_Path, uint flags, uint fileMode)
        {
            if (dev_Path == null || dev_Path.Length == 0)
                throw new HIDException(new ArgumentNullException("dev_Path", "DevicePath Incorrect"), ExceptionType.DEVICE_ERROR, "Incorrect DevicePath");
            IntPtr handle = CreateFile(dev_Path, (File_Access)flags, File_Share.FILE_SHARE_READ | File_Share.FILE_SHARE_WRITE, IntPtr.Zero, (File_Mode)fileMode, 0, IntPtr.Zero);
            if (handle == null || handle == new IntPtr(-1))
            {
                handle = new IntPtr(-1);
                throw new HIDException(new FileNotFoundException("Device not found (is it connected correctly?)"), ExceptionType.DEVICE_ERROR, "Cannot Find Device");
            }
            return handle;
        }
        
        /// <summary>
        /// Reads from a file/Device
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="numberOfBytesToRead"></param>
        /// <returns></returns>
        public override byte[] Read(HIDDevice device)
        {
            Debug.Log(GetDevices().Count);
            byte[] buffer = new byte[device.inputReportLength];
            uint cbThatWereRead = 0;
            SafeFileHandle handle = new SafeFileHandle(device.filePointer, true);
            if (!ReadFile(handle, buffer, (uint)device.inputReportLength, ref cbThatWereRead, IntPtr.Zero))
                throw new HIDException(ExceptionType.DEVICE_ERROR, "Error reading from Device");
            return buffer;
        }

        /// <summary>
        /// Writes to a HID-Device (OutputReport)
        /// </summary>
        /// <param name="device">Device to write to</param>
        /// <param name="buffer">Buffer to write</param>
        public override void Write(HIDDevice device, byte[] buffer)
        {
            uint cbThatWereWritten = 0;
            if (!WriteFile(device.filePointer, buffer, (uint)buffer.Length, out cbThatWereWritten, IntPtr.Zero))
                throw new HIDException("Error writing to Device");
        }

        public override void SetOutputReport(HIDDevice device, byte[] report)
        {
            uint cbToWrite = (uint)report.Length;
            if (!HidD_SetOutputReport(device.filePointer, report, cbToWrite))
                throw new HIDException(ExceptionType.DEVICE_ERROR, "Error writing to Device");
        }

        /// <summary>
        /// Gets Input report from a HID-Device
        /// </summary>
        /// <param name="device">Device to Get Report from</param>
        /// <returns>Report, if Available</returns>
        public override byte[] GetInputReport(HIDDevice device)
        {
            if (device == null) Debug.Log("AAAAAAAAAAAAAAAAARGH");
            if (device.filePointer == null) Debug.Log("AAAAAAAAAAAAAAAAARGH");
            if (device.inputReportLength == null) Debug.Log("AAAAAAAAAAAAAAAAARGH");
            byte[] buff = new byte[device.inputReportLength];
            Debug.Log(device.filePointer.ToString());
            Debug.Log(device.inputReportLength.ToString());
            Debug.Log(buff.ToString());
            HidD_GetInputReport(device.filePointer, out buff, (ulong)device.inputReportLength);            
            return buff;
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
                throw new HIDException(e);
            }
        }        
    }
}