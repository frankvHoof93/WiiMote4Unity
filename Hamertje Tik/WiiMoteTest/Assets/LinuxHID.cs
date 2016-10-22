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
        
        /// <summary>
        /// Constructor for API
        /// </summary>
        public LinuxHID() {}


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
            
            return devices;
        }
        
        public override void GetDeviceInfo(IntPtr dev_Handle, out int inputLength, out int outputLength)
        {
            inputLength = 22;
            outputLength = 22;
        }

        public override IntPtr Connect(string dev_Path)
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
        /// <param name="cbToRead"></param>
        /// <returns></returns>
        public override uint Read(HIDDevice device, byte[] buffer, uint cbToRead)
        {
            uint cbThatWereRead = 0;
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
            uint cbThatWereWritten = 0;
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
                // TODO   
                return true;
            } catch (Exception e)
            {
                HandleException(e, "Error Closing");
                return false;
            }
        }        
    }
}
