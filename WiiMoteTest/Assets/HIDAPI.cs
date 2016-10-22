using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Assets
{
    public abstract class HIDAPI
    {
        private static HIDAPI _instance = null;
        public static HIDAPI GetAPI()
        {
            if (_instance != null) return _instance;
            if (SystemInfo.operatingSystem.Contains("Windows"))
                _instance = new WindowsHID();
            else if (SystemInfo.operatingSystem.Contains("Linux"))
                _instance = new LinuxHID();
            else throw new HIDException("Operating System not Recognized");
            return _instance;
        }

        protected List<HIDDevice> devices = new List<HIDDevice>();
        
        /// <summary>
        /// Returns the List of all HID-devices attached to the system
        /// </summary>
        /// <returns>List of all devices attached to the system</returns>
        public abstract List<HIDDevice> GetDevices();
        public abstract void InitialiseDeviceInfo(HIDDevice device);
        public abstract IntPtr Connect(string dev_Path, uint flags, uint fileMode);
        public abstract bool Disconnect(IntPtr device);
        public abstract byte[] Read(HIDDevice device);
        public abstract void Write(HIDDevice device, byte[] buffer);
        public abstract byte[] GetInputReport(HIDDevice device);
        public abstract void SetOutputReport(HIDDevice device, byte[] report);

    }    
}
