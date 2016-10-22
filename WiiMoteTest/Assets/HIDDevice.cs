using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using Assets;
using System;
using Microsoft.Win32.SafeHandles;
using System.Threading;

public class HIDDevice {
    public string devicePath { get; private set; }
    public IntPtr filePointer { get; private set; }
    public bool isConnected { get {
        //Debug.Log("CONNECTED: " + !filePointer.Equals(new IntPtr(-1)));
        return !filePointer.Equals(new IntPtr(-1)); } }
    public HIDAPI api { get; private set; }
    public ushort vendorID { get; private set; }
    public ushort productID { get; private set; }
    public ushort versionNumber { get; private set; }
    public short inputReportLength { get; private set; }
    public short outputReportLength { get; private set; }
    public short featureReportLength { get; private set; }

    public HIDDevice(string devicePath, HIDAPI api)
    {
        if (devicePath == null || devicePath.Length == 0)
            throw new ArgumentNullException("devicePath", "Incorrect- or No DevicePath Set");
        if (api == null)
            throw new ArgumentNullException("api", "No API set");
        this.devicePath = devicePath;
        this.api = api;
    }

    internal void SetDeviceInfo(ushort vendorID, ushort productID, ushort versionNumber, 
        short inputReportLength, short outputReportLength, short featureReportLength, short noLinkCollectionNodes, 
        short noInputButtonsCaps, short noInputValueCaps, short noInputDataIndices, 
        short noOutputButtonCaps, short noOutputValueCaps, short noOutputDataIndices,
        short noFeatureButtonCaps, short noFeatureValueCaps, short noFeatureDataIndices)
    {
        this.vendorID = vendorID;
        this.productID = productID;
        this.versionNumber = versionNumber;
        this.inputReportLength = inputReportLength;
        this.outputReportLength = outputReportLength;
        this.featureReportLength = featureReportLength;
    }

    public bool ConnectAndInitialise(uint fileMode, uint flags)
    {
        if (Connect(flags, fileMode))
            Initialise();
        return isConnected;
    }

    public void Initialise()
    {
        if (!isConnected) throw new HIDException(ExceptionType.CONNECTION, "Please Connect first");
        api.InitialiseDeviceInfo(this);
    }

    public bool Connect(uint fileMode, uint flags)
    {
        try
        {
            this.filePointer = api.Connect(devicePath, flags, fileMode);
        }
        catch (HIDException e)
        {
            Debug.LogError("An exception occured when Connecting your HID-Device");
            Debug.LogException(e);
            this.filePointer = new IntPtr(-1);
        }
        return this.isConnected;
    }

    public bool Disconnect()
    {
        if (!isConnected || api.Disconnect(this.filePointer))
        {
            this.filePointer = new IntPtr(-1);
            return true;
        }
        return false;
    }

    public byte[] Read()
    {
        if (!this.isConnected)
            throw new HIDException(new InvalidOperationException("Not Connected"));
        byte[] mBuff = api.Read(this);
        return mBuff;
    }

    public byte[] GetInputReport()
    {
        return null;
    }
    
    public static byte[] numberToBytes(int number, bool BigEndian, int numberofBytes)
    {
        if (number >= Math.Pow(2, numberofBytes * 8))
            throw new ArgumentOutOfRangeException("Number too large for bytesize");
        byte[] result = new byte[numberofBytes];
        for (int i = numberofBytes - 1; i >= 0; i--)
        {
            result[i] = (byte)(number >> (i * 8) & 0xFF);
        }
        if (BigEndian)
            Array.Reverse(result);
        return result;
    }

    public static uint bitsToNumber(byte[] number, bool BigEndian)
    {
        if (number.Length > 4)
            throw new ArgumentException("32-Bit numbers only");
        uint result = 0;
        if (!BigEndian)
            Array.Reverse(number);
        if (number.Length < 4)
        {
            byte[] temp = new byte[4];
            number.CopyTo(temp, 4 - number.Length);
            number = temp;
        }
        if (BitConverter.IsLittleEndian)
            Array.Reverse(number); // Bitconverter expects little-endian
        result = (uint)BitConverter.ToInt32(number, 0);
        return result;
    }
}
