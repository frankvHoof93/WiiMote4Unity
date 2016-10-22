using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;




namespace Assets
{
    public class LinuxHID : HIDAPI
    {
		// BUILDERRORS WHEN USED?
		private static string udevLibrary = "libudev.so.1";
		// https://bitbucket.org/ipre/calico/src/a70a3f36d299bb61725d3c5cb6795f15ef17f8e6/modules/hidsharp/HidSharp/Platform/Linux/LinuxHidManager.cs?at=master&fileviewer=file-view-default
		// https://bitbucket.org/ipre/calico/src/a70a3f36d299bb61725d3c5cb6795f15ef17f8e6/modules/hidsharp/HidSharp/Platform/Linux/NativeMethods.cs?at=master&fileviewer=file-view-default


        /// <summary>
        /// Constructor for API
        /// </summary>
        public LinuxHID() {}


        /// <summary>
		/// Following the Process From: http://www.signal11.us/oss/udev/
        /// </summary>
        /// <returns>List of all HIDDevices attached to the System</returns>
        public override List<HIDDevice> GetDevices()
        {
            List<HIDDevice> devices = new List<HIDDevice>();
			IntPtr udev = udev_new ();
			if (udev != IntPtr.Zero) {
				try {
					IntPtr enumerate = udev_enumerate_new(udev);
					if (enumerate != IntPtr.Zero)
					{
						try{
							Debug.Log("Enumerating");
							if (udev_enumerate_add_match_subsystem(enumerate, "hidraw") == 0 &&
							udev_enumerate_scan_devices(enumerate) == 0)
							{
								IntPtr device = udev_enumerate_get_list_entry(enumerate);
								while (device != IntPtr.Zero)
								{
									device = udev_list_entry_get_next(device);
									string devPath = udev_list_entry_get_name(device);
									if (devPath != null){
										devices.Add(new HIDDevice(devPath, this));
										Debug.Log("Path: " + devPath);
									}
								}
							}
							Debug.Log("Working");
						}
						finally {

						}
					}
				}
				finally {
				}
			}
            return devices;
        }

		[DllImport("libudev.so.1", SetLastError = true)]
		public static extern string udev_list_entry_get_name(IntPtr entry);

		[DllImport("libudev.so.1", SetLastError = true)]
		public static extern IntPtr udev_new();

		[DllImport("libudev.so.1", SetLastError = true)]
		public static extern IntPtr udev_enumerate_new(IntPtr udev);

		[DllImport("libudev.so.1", SetLastError = true)]
		public static extern int udev_enumerate_scan_devices(IntPtr enumerate);

		[DllImport("libudev.so.1", SetLastError = true)]
		public static extern int udev_enumerate_add_match_subsystem(IntPtr enumerate,
		                                                            string subsystem);

		[DllImport("libudev.so.1", SetLastError = true)]
		public static extern IntPtr udev_enumerate_get_list_entry(IntPtr enumerate);
		
		[DllImport("libudev.so.1", SetLastError = true)]
		public static extern IntPtr udev_list_entry_get_next(IntPtr entry);

        public override IntPtr Connect(string dev_Path, uint flags, uint fileMode)
        {
            FileStream stream = new FileStream(dev_Path, FileMode.Open);
            Debug.Log(stream.CanRead);
            Debug.Log(stream.CanWrite);
            Debug.Log(stream.Name);
            Debug.Log(stream.Handle);
            return stream.Handle;
        }
        
        /// <summary>
        /// Reads from a file (WinFileAPI)
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="numberOfBytesToRead"></param>
        /// <returns></returns>
        public override byte[] Read(HIDDevice device)
        {
            uint cbThatWereRead = 0;
            return null;
        }

        /// <summary>
        /// Writes to a HID-Device (OutputReport)
        /// </summary>
        /// <param name="device">Device to write to</param>
        /// <param name="buffer">Buffer to write</param>
        /// <param name="cbToWrite">number of bytes</param>
        public override void Write(HIDDevice device, byte[] buffer)
        {
            uint cbThatWereWritten = 0;
        }

        public override byte[] GetInputReport(HIDDevice device)
        {
            throw new NotImplementedException();
        }

        public override void SetOutputReport(HIDDevice device, byte[] report)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Closes a File
        /// </summary>
        /// <param name="devHandle"></param>
        public override bool Disconnect(IntPtr devHandle)
        {
            try
            {
                // TODO   
                return true;
            } catch (Exception e)
            {
                throw new HIDException(e);
            }
        }

        public override void InitialiseDeviceInfo(HIDDevice device)
        {
            throw new NotImplementedException();
        }
    }
}
