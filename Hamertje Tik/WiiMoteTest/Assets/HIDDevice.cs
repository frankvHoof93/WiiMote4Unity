using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using Assets;
using System;
using Microsoft.Win32.SafeHandles;
using System.Threading;

public /*abstract*/ class HIDDevice {
    public string devicePath { get; protected set; }
    public HIDAPI api { get; private set; }

    public SafeFileHandle fileHandle;
    public IntPtr filePointer = new IntPtr(-1);
    protected int inputBufferLength;
    protected int outputBufferLength;
    protected Thread readingThread;

    protected bool isConnected = false;
    protected bool isReading = false;

    public HIDDevice(string devicePath, HIDAPI api)
    {
        this.devicePath = devicePath;
        this.api = api;
    }

    public void Initialise()
    {
        if (this.devicePath.Length == 0)
            throw new ArgumentNullException("devicePath", "No DevicePath Set");
        try
        {
            IntPtr file = api.Connect(this.devicePath);
            this.isConnected = true;
            this.SetHandle(file);
            int inputBuff, outputBuff;
            api.GetDeviceInfo(file, out inputBuff, out outputBuff);
            // TODO - Add other device info
            SetBufferLengths(inputBuff, outputBuff);
        }
        catch (HIDException e)
        {
            Debug.Log(e.Message);
            this.isConnected = false;
        }
    }

    private void SetBufferLengths(int input, int output) 
    {
        this.inputBufferLength = input;
        this.outputBufferLength = output;
    }

    public bool Disconnect()
    {
        this.isReading = false;
        return (this.isConnected ? api.Disconnect(this.filePointer) : true);
    }

    public byte[] Read()
    {
        if (!this.isConnected)
            throw new InvalidOperationException("Not Connected");
        byte[] mBuff = new byte[22];
        api.Read(this, mBuff, (uint)22);
        return mBuff;
    }

    public void StartReading()
    {
        isReading = true;
        readingThread = new Thread(new ThreadStart(DoRead));
        readingThread.Start();
    }

    public void StopReading()
    {
        if (readingThread == null) return;
        readingThread.Interrupt();
        readingThread = null;
    }

    public virtual void DoRead()
    {
        Debug.Log("FALSEREAD");
    }

    protected void SetHandle(IntPtr handle)
    {
        SetHandle(new SafeFileHandle(handle, true));
    }

    protected void SetHandle(SafeFileHandle handle)
    {
        this.fileHandle = handle;
        this.filePointer = handle.DangerousGetHandle();
    }

    public static byte[] numberToBytes(int number, bool BigEndian, int numberofBytes)
    {
        if (number >= Math.Pow(2, numberofBytes * 8))
            throw new ArgumentOutOfRangeException("Number too large for bytesize");
        byte[] result = new byte[numberofBytes];
        for (int i = 0; i < numberofBytes; i++)
        {
            result[i] = (byte)(number >> (i * 8) & 0xFF);
        }
        if (BigEndian)
            Array.Reverse(result);
        return result;
    }

    public static uint bitsToNumber(byte[] number, bool BigEndian)
    {
        uint result = 0;
        if (BigEndian)
            Array.Reverse(number);
        for (int i = 0; i < number.Length; i++ ) // Look through bytes in a row, left to right
            for (int j = 7; j >= 0; j--) // Check the byte itself, left to right (first bit is checked by bitshifting 0x01 left by 7)
            {
                if ((number[i] & (0x01 << j)) == (0x01 << j))
                {
                    result += (uint)((i * 8) + (8 - j));
                }
            }
        return result;
    }
}
