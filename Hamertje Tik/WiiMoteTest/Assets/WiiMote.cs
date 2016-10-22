using UnityEngine;
using System;
using System.Collections.Generic;
using Assets;
using Microsoft.Win32.SafeHandles;

namespace Assets
{
    public class WiimoteException : Exception
    {
        public WiimoteException(string msg) : base(msg) { }
        public WiimoteException(Exception e) : base(e.Message) { }
    }

    public class WiiMote : HIDDevice
    {
        private const string VID_NINTENDO = "vid&0002057e";
        private const string PID_WIIMOTE = "pid&0306"; // BOTH NORMAL AND PLUS ARE GIVING SAME PID (WHICH THEY SHOULDNT)

        // Wiimote output commands
        private enum OutputReport : byte
        {
            LEDs = 0x11,
            ReportingMode = 0x12,
            IR = 0x13,
            Speaker = 0x13,
            StatusRequest = 0x15,
            WriteMemory = 0x16,
            ReadMemory = 0x17,
            SpeakerData = 0x18,
            SpeakerMute = 0x19,
            IR2 = 0x1a
        };

        private enum InputReport : byte
        {
            Status = 0x20,
            MemoryRead = 0x21,
            FunctionOrError = 0x22,
            Buttons = 0x30,
            ButtonsAndAccel = 0x31,
            ButtonsAnd8Ext = 0x32,
            ButtonsAccelAnd12IR = 0x33,
            ButtonsAnd19Ext = 0x34,
            ButtonsAccelAnd16Ext = 0x35,
            Buttons10IRAnd9Ext = 0x36,
            ButtonsAccel10IRAnd6Ext = 0x37,
            FullExt = 0x3d,
            InterleavedFirst = 0x3e,
            InterleavedSecond = 0x3f
        };

        public WiiMoteState wiiMoteState;
        private byte[] interleavedAccel = new byte[2];
        public int playerNumber { get; private set; }

        public WiiMote(string path, HIDAPI api) : base(path, api)
        {
            this.wiiMoteState = new WiiMoteState();
        }
        
        public void SetPlayerNumber(int playerNumber)
        {
            this.playerNumber = playerNumber;
            string playerLeds = Convert.ToString(playerNumber, 2); // Convert to Base2 (Binary)
            Debug.Log(playerLeds);
            if (this.isConnected)
                SetLEDs((playerLeds[0] == '1'), (playerLeds[1] == '1'), (playerLeds[2] == '1'), (playerLeds[3] == '1'));
        }

        public static List<WiiMote> FindWiimotes(List<HIDDevice> devices)
        {
            List<WiiMote> wiiMotes = new List<WiiMote>();
            foreach (HIDDevice dev in devices)
            {
                if (dev.devicePath.Contains(VID_NINTENDO))
                    if (dev.devicePath.Contains(PID_WIIMOTE)) // TODO ADD OTHER PID's (IF EVER FOUND)
                        wiiMotes.Add(new WiiMote(dev.devicePath, dev.api));
            }
            return wiiMotes;
        }

        public override void DoRead()
        {
            byte[] buffer = new byte[22];
            while (isReading)
            {
                Array.Clear(buffer, 0, 22);
                try
                {
                    uint readBytes = api.Read(this, buffer, 22);
                    HandleRead(buffer);
                    Debug.Log("READ:" + readBytes);
                } catch (Exception e)
                {
                    Debug.Log("READFAIL:" + e.Message);
                }
            }
        }

        #region INPUT/OUTPUT
        #region Input
        /// <summary>
        /// Handles a InputReport, updating the current WiiMoteState
        /// </summary>
        /// <param name="message">InputReport to handle</param>
        public void HandleRead(byte[] message)
        {
            switch (message[0])
            {
                case (byte)InputReport.FullExt:
                    HandleExtension(message);
                    return;
                case (byte)InputReport.InterleavedFirst:
                    HandleInterleaved(message);
                    return;
                case (byte)InputReport.InterleavedSecond:
                    HandleInterleaved(message);
                    return;
                case (byte)InputReport.Status:
                    HandleButtonData(message);
                    HandleStatusMessage(message);
                    return;
                case (byte)InputReport.MemoryRead:
                    HandleButtonData(message);
                    HandleMemoryRead(message);
                    return;
                case (byte)InputReport.FunctionOrError:
                    HandleButtonData(message);
                    HandleFunctionResult(message);
                    return;
                
            }
            HandleButtonData(message);
            Debug.Log(this.wiiMoteState.reportingMode.ToString());
            if (this.wiiMoteState.reportingMode.ToString().Contains("Accel"))
            {
                HandleAccelerometer(message);
            }
            if (this.wiiMoteState.reportingMode.ToString().Contains("IR"))
            {
                HandleIR(message);
            }
            if (this.wiiMoteState.reportingMode.ToString().Contains("Ext"))
            {
                HandleExtension(message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void HandleMemoryRead(byte[] message)
        {
            byte[] data = new byte[16];
            byte[] partialAddress = new byte[2];
            Array.ConstrainedCopy(message, 6, data, 0, 16);            
            Array.ConstrainedCopy(message, 4, partialAddress, 0, 2);
            int size = (message[3] >> 4);
            byte errors = (byte)(message[3] & 0x0F);
            if (errors != 0)
            {
                if (errors == 7)
                    throw new WiimoteException("Attempted read from Non-Existant Extension or Write-Only Memory Address");
                else if (errors == 8)
                    throw new WiimoteException("Attempted read from Non-Existant Memory Address");
            }
            // DO SOMETHING WITH THIS?
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void HandleFunctionResult(byte[] message)
        {
            byte isResultOf = message[3];
            byte result = message[4];
            this.wiiMoteState.SetError(result);
            Debug.Log("Got -" + result + " - as a result of - " + isResultOf);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void HandleStatusMessage(byte[] message)
        {
            this.wiiMoteState.BatteryLevel = message[6];
            byte Flags = message[3];
            this.wiiMoteState.BatteryLow = ((Flags & 0x01) == 0x01);
            this.wiiMoteState.ExtensionConnected = ((Flags & 0x02) == 0x02);
            this.wiiMoteState.SpeakerEnabled = ((Flags & 0x04) == 0x04);
            this.wiiMoteState.IREnabled = ((Flags & 0x08) == 0x08);
            this.wiiMoteState.ledState.LED1 = ((Flags & 0x10) == 0x10);
            this.wiiMoteState.ledState.LED2 = ((Flags & 0x20) == 0x20);
            this.wiiMoteState.ledState.LED3 = ((Flags & 0x40) == 0x70);
            this.wiiMoteState.ledState.LED4 = ((Flags & 0x80) == 0x80);
            // Reset DataReportingMode after a Status Report is handled (ONLY NEEDED IF STATUSREPORT WAS NOT REQUESTED, we´ll just do it anyway)
            SetDataReportingMode(this.wiiMoteState.IsContinuous, this.wiiMoteState.reportingMode);
        }

        /// <summary>
        /// Using Full Message, Handles (and sets) Accelerometer Data
        /// </summary>
        /// <param name="message">InputReport to handle</param>
        private void HandleAccelerometer(byte[] message)
        {
            if (this.wiiMoteState.reportingMode == ReportingState.Interleaved)
                throw new InvalidOperationException("Use HandleInterleaved");
            byte[] xPos = new byte[2];
            byte[] yPos = new byte[2];
            byte[] zPos = new byte[2];
            byte LSBs = (byte)((message[1] & 0x60) >> 5);
            xPos[1] = (byte)((message[3] << 2) | LSBs);
            xPos[0] = (byte)(message[3] >> 6);
            LSBs = (byte)((message[2] & 0x40) >> 5);
            yPos[1] = (byte)((message[4] << 2) | LSBs);
            yPos[0] = (byte)(message[4] >> 6);
            LSBs = (byte)((message[2] & 0x20) >> 4);
            zPos[1] = (byte)((message[5] << 2) | LSBs);
            zPos[0] = (byte)(message[5] >> 6);
            this.wiiMoteState.UpdateAccel(bitsToNumber(xPos, true), bitsToNumber(yPos, true), bitsToNumber(zPos, true));
        }

        /// <summary>
        /// Using Full Message, Handles (and sets) IR Data
        /// </summary>
        /// <param name="message">InputReport to handle</param>
        private void HandleIR(byte[] message)
        {
            if (this.wiiMoteState.irMode == IRMode.Full)
                throw new InvalidOperationException("Use HandleInterleaved");            
            if (this.wiiMoteState.irMode == IRMode.Basic)
            {
                byte[] xPos = new byte[2];
                byte[] yPos = new byte[2];
                int offset = 3;
                if (this.wiiMoteState.reportingMode == ReportingState.ButtonsAccel10IRAnd6Ext)
                    offset += 3;
                xPos[1] = message[offset];
                xPos[0] = (byte)((message[offset + 2] & 0x30) >> 4);
                yPos[1] = message[offset + 1];
                yPos[0] = (byte)((message[offset + 2] & 0xC0) >> 6);
                Vector2 Point1 = new Vector2(bitsToNumber(xPos, true), bitsToNumber(yPos, true));
                xPos[1] = message[offset + 3];
                xPos[0] = (byte)((message[offset + 2] & 0x03));
                yPos[1] = message[offset + 4];
                yPos[0] = (byte)((message[offset + 2] & 0x0C) >> 2);
                Vector2 Point2 = new Vector2(bitsToNumber(xPos, true), bitsToNumber(yPos, true));

                offset += 5;

                xPos[1] = message[offset];
                xPos[0] = (byte)((message[offset + 2] & 0x30) >> 4);
                yPos[1] = message[offset + 1];
                yPos[0] = (byte)((message[offset + 2] & 0xC0) >> 6);
                Vector2 Point3 = new Vector2(bitsToNumber(xPos, true), bitsToNumber(yPos, true));
                xPos[1] = message[offset];
                xPos[0] = (byte)((message[offset + 2] & 0x30) >> 4);
                yPos[1] = message[offset + 1];
                yPos[0] = (byte)((message[offset + 2] & 0xC0) >> 6);
                Vector2 Point4 = new Vector2(bitsToNumber(xPos, true), bitsToNumber(yPos, true));
                this.wiiMoteState.UpdateIR(Point1, Point2, Point3, Point4);
            }
            else // if (this.wiiMoteState.irMode == IRMode.Extended)
            {
                byte[] xPos = new byte[2];
                byte[] yPos = new byte[2];
                int offset = 6;
                xPos[1] = message[offset];
                yPos[1] = message[offset + 1];
                xPos[0] = (byte)((message[offset + 2] & 0x30) >> 4);
                yPos[0] = (byte)((message[offset + 2] & 0xc0) >> 6);
                byte size = (byte)(message[offset + 2] & 0x0F);
                Vector3 Point1 = new Vector3(bitsToNumber(xPos, true), bitsToNumber(yPos, true), size);
                offset += 3;
                xPos[1] = message[offset];
                yPos[1] = message[offset + 1];
                xPos[0] = (byte)((message[offset + 2] & 0x30) >> 4);
                yPos[0] = (byte)((message[offset + 2] & 0xc0) >> 6);
                size = (byte)(message[offset + 2] & 0x0F);
                Vector3 Point2 = new Vector3(bitsToNumber(xPos, true), bitsToNumber(yPos, true), size);
                offset += 3;
                xPos[1] = message[offset];
                yPos[1] = message[offset + 1];
                xPos[0] = (byte)((message[offset + 2] & 0x30) >> 4);
                yPos[0] = (byte)((message[offset + 2] & 0xc0) >> 6);
                size = (byte)(message[offset + 2] & 0x0F);
                Vector3 Point3 = new Vector3(bitsToNumber(xPos, true), bitsToNumber(yPos, true), size);
                offset += 3;
                xPos[1] = message[offset];
                yPos[1] = message[offset + 1];
                xPos[0] = (byte)((message[offset + 2] & 0x30) >> 4);
                yPos[0] = (byte)((message[offset + 2] & 0xc0) >> 6);
                size = (byte)(message[offset + 2] & 0x0F);
                Vector3 Point4 = new Vector3(bitsToNumber(xPos, true), bitsToNumber(yPos, true), size);
                this.wiiMoteState.UpdateIR(Point1, Point2, Point3, Point4);
            }            
        }

        /// <summary>
        /// Using full message (22 Bytes), handles (and sets) Button Data
        /// </summary>
        /// <param name="message">InputReport to handle</param>
        private void HandleButtonData(byte[] message)
        {
            if (this.wiiMoteState.reportingMode == ReportingState.FullExt)
                throw new InvalidOperationException("No Core Button Data in this mode");
            else
            {
                byte firstByte = message[1];
                byte secondByte = message[2];
                byte mask = (byte)0x01;
                this.wiiMoteState.currentState.Left = ((firstByte & mask) == mask);
                this.wiiMoteState.currentState.Two = ((secondByte & mask) == mask);
                mask = (byte)0x02;
                this.wiiMoteState.currentState.Right = ((firstByte & mask) == mask);
                this.wiiMoteState.currentState.One = ((secondByte & mask) == mask);
                mask = (byte)0x04;
                this.wiiMoteState.currentState.Down = ((firstByte & mask) == mask);
                this.wiiMoteState.currentState.B = ((secondByte & mask) == mask);
                mask = (byte)0x08;
                this.wiiMoteState.currentState.Up = ((firstByte & mask) == mask);
                this.wiiMoteState.currentState.A = ((secondByte & mask) == mask);
                mask = (byte)0x10;
                this.wiiMoteState.currentState.Plus = ((firstByte & mask) == mask);
                this.wiiMoteState.currentState.Minus = ((secondByte & mask) == mask);
                mask = (byte)0x80;
                this.wiiMoteState.currentState.Home = ((secondByte & mask) == mask);
            }
        }

        private void HandleInterleaved(byte[] message)
        {
            HandleButtonData(message);
            byte[] xPos = new byte[2];
            byte[] yPos = new byte[2];
            int offset = 4;
            xPos[1] = message[offset];
            yPos[1] = message[offset + 1];
            xPos[0] = (byte)((message[offset + 2] & 0x30) >> 4);
            yPos[0] = (byte)((message[offset + 2] & 0xc0) >> 6);
            byte size = (byte)(message[offset + 2] & 0x0F);
            byte xMin = message[offset + 3];
            byte yMin = message[offset + 4];
            byte xMax = message[offset + 5];
            byte yMax = message[offset + 6];
            byte intensity = message[offset + 8];
            Vector4 Point1 = new Vector4(bitsToNumber(xPos, true), bitsToNumber(yPos, true), size, intensity);
            Vector4 bound1 = new Vector4(xMin, yMin, xMax, yMax);
            offset += 8;
            xPos[1] = message[offset];
            yPos[1] = message[offset + 1];
            xPos[0] = (byte)((message[offset + 2] & 0x30) >> 4);
            yPos[0] = (byte)((message[offset + 2] & 0xc0) >> 6);
            size = (byte)(message[offset + 2] & 0x0F);
            xMin = message[offset + 3];
            yMin = message[offset + 4];
            xMax = message[offset + 5];
            yMax = message[offset + 6];
            intensity = message[offset + 8];
            Vector4 Point2 = new Vector4(bitsToNumber(xPos, true), bitsToNumber(yPos, true), size, intensity);
            Vector4 bound2 = new Vector4(xMin, yMin, xMax, yMax);
            if (message[0] == (byte)InputReport.InterleavedFirst)
            {
                interleavedAccel[0] = message[3]; // xPos
                interleavedAccel[2] = (byte)(((message[1] & 0x60) >> 1) | ((message[2] & 0x60) << 1));
                this.wiiMoteState.InterleavedUpdateIR(Point1, bound1, Point2, bound2, true);
            }
            else
            {
                interleavedAccel[1] = message[3];
                interleavedAccel[2] = (byte)(interleavedAccel[2] | (((message[1] & 0x60) >> 5) | ((message[2] & 0x60) >> 3)));
                this.wiiMoteState.UpdateAccel(interleavedAccel[0], interleavedAccel[1], interleavedAccel[2]);
                this.wiiMoteState.InterleavedUpdateIR(Point1, bound1, Point2, bound2, false);
            }
        }

        private void HandleExtension(byte[] message)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Output
        #region Public
        /// <summary>
        /// Set the LEDs on the Wiimote
        /// </summary>
        /// <param name="led1">LED 1</param>
        /// <param name="led2">LED 2</param>
        /// <param name="led3">LED 3</param>
        /// <param name="led4">LED 4</param>
        public void SetLEDs(bool led1, bool led2, bool led3, bool led4)
        {
            if (!this.isConnected)
                throw new InvalidOperationException("Not Connected");
            this.wiiMoteState.ResetError();
            byte[] mBuff = new byte[22];
            mBuff[0] = (byte)OutputReport.LEDs;
            mBuff[1] = (byte)((led1 ? 0x10 : 0x00) |
                              (led2 ? 0x20 : 0x00) |
                              (led3 ? 0x40 : 0x00) |
                              (led4 ? 0x80 : 0x00) |
                              GetRumbleBit());
            try
            {
                api.Write(this, mBuff, (uint)mBuff.Length);
                this.wiiMoteState.ledState.LED1 = led1;
                this.wiiMoteState.ledState.LED2 = led2;
                this.wiiMoteState.ledState.LED3 = led3;
                this.wiiMoteState.ledState.LED4 = led4;
            }
            catch (Exception e)
            {
                Debug.Log(e.InnerException);
                Debug.Log(e.Message);
            }
        }

        /// <summary>
        /// Requests a Status Report form the Wiimote
        /// </summary>
        public void RequestStatus()
        {
            if (!this.isConnected)
                throw new InvalidOperationException("Not Connected");
            this.wiiMoteState.ResetError();
            byte[] mBuff = new byte[22];
            mBuff[0] = (byte)OutputReport.StatusRequest;
            mBuff[1] = (byte)GetRumbleBit();
            try
            {
                api.Write(this, mBuff, (uint)mBuff.Length);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        /// <summary>
        /// Set ReportingState for the device
        /// See wiibrew.org/wiki/Wiimote for more info on states
        /// </summary>
        /// <param name="isContinouous">Continuous Reporting On/Off</param>
        /// <param name="dataReportingMode">Reportingstate to set</param>
        public void SetDataReportingMode(bool isContinouous, ReportingState dataReportingMode)
        {
            if (!this.isConnected)
                throw new InvalidOperationException("Not Connected");
            this.wiiMoteState.ResetError();
            byte[] mBuff = new byte[22];
            mBuff[0] = (byte)OutputReport.ReportingMode;
            if (isContinouous)
                mBuff[1] = (byte)(0x04 | GetRumbleBit());
            else mBuff[1] = (byte)(0x00 | GetRumbleBit());
            mBuff[2] = (byte)dataReportingMode;
            try
            {
                api.Write(this, mBuff, (uint)mBuff.Length);
                this.wiiMoteState.reportingMode = dataReportingMode;
                this.wiiMoteState.IsContinuous = isContinouous;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
       
        /// <summary>
        /// Sets the new rumble state to the device
        /// </summary>
        /// <param name="enable">Set true to Enable Rumble</param>
        public void SetRumble(bool enable)
        {
            if (!this.isConnected)
                throw new InvalidOperationException("Not Connected");
            this.wiiMoteState.ResetError();
            LEDState currstate = this.wiiMoteState.ledState;
            try
            {
                this.wiiMoteState.RumbleEnabled = enable;
                SetLEDs(currstate.LED1, currstate.LED2, currstate.LED3, currstate.LED4);                
            }
            catch (Exception e)
            { }
        }

        /// <summary>
        /// Enables/Disables IR-Camera
        /// Will also initialise on Enable
        /// Call WiiMoteState.SetupIRConfig beforehand
        /// AND SET CORRECT REPORTINGMODE FIRST
        /// </summary>
        /// <param name="enable"></param>
        public void EnableIR(bool enable)
        {
            if (!this.isConnected)
                throw new InvalidOperationException("Not Connected");
            this.wiiMoteState.ResetError();
            byte[] mBuff = new byte[22];
            mBuff[1] = (byte)((enable ? 0x04 : 0x00) | GetRumbleBit());
            try
            {
                if (enable) // Clock on, then camera
                {
                    mBuff[0] = (byte)OutputReport.IR;
                    api.Write(this, mBuff, (uint)mBuff.Length);
                    mBuff[0] = (byte)OutputReport.IR2;
                    api.Write(this, mBuff, (uint)mBuff.Length);
                }
                else // Camera off, then clock
                {
                    mBuff[0] = (byte)OutputReport.IR2;
                    api.Write(this, mBuff, (uint)mBuff.Length);
                    mBuff[0] = (byte)OutputReport.IR;
                    api.Write(this, mBuff, (uint)mBuff.Length);
                }
                this.wiiMoteState.IREnabled = enable;
                if (enable)
                {
                    try { InitIR(); }
                    catch (Exception e)
                    { EnableIR(false); }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        /// <summary>
        /// Enables/Disables Speaker
        /// Will also initialise on Enable
        /// Call WiiMoteState.SetupSpeakerConfig beforehand
        /// </summary>
        /// <param name="enable">Set true to Enable Speaker</param>
        public void EnableSpeaker(bool enable)
        {
            if (!this.isConnected)
                throw new InvalidOperationException("Not Connected");
            if (enable && this.wiiMoteState.SpeakerSampleRate == 0)
                throw new WiimoteException("Set Config First");
            this.wiiMoteState.ResetError();
            byte[] mBuff = new byte[22];
            mBuff[0] = (byte)OutputReport.Speaker;
            mBuff[1] = (byte)((enable ? 0x04 : 0x00) | GetRumbleBit());
            try
            {
                api.Write(this, mBuff, (uint)mBuff.Length);
                this.wiiMoteState.SpeakerEnabled = enable;
                if (enable)
                    InitSpeaker();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        /// <summary>
        /// Mutes/Unmutes Speaker
        /// </summary>
        /// <param name="enable">Set true to Mute Speaker</param>
        public void MuteSpeaker(bool enable)
        {
            if (!this.isConnected)
                throw new InvalidOperationException("Not Connected");
            if (!this.wiiMoteState.SpeakerEnabled)
                throw new InvalidOperationException("Speaker not enabled");
            this.wiiMoteState.ResetError();
            byte[] mBuff = new byte[22];
            mBuff[0] = (byte)OutputReport.SpeakerMute;
            mBuff[1] = (byte)((enable ? 0x04 : 0x00) | GetRumbleBit());
            try
            {
                api.Write(this, mBuff, (uint)mBuff.Length);
                this.wiiMoteState.SpeakerMuted = enable;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }        

        /// <summary>
        /// MUST BE SENT AT PROPER RATE
        /// </summary>
        /// <param name="dataLength"></param>
        /// <param name="data"></param>
        public void SendSpeakerData(int dataLength, byte[] data)
        {
            if (!this.isConnected)
                throw new InvalidOperationException("Not Connected");
            if (!this.wiiMoteState.SpeakerEnabled)
                throw new InvalidOperationException("Speaker not enabled");
            if (data.Length > 20)
                throw new ArgumentOutOfRangeException("Too much data");
            this.wiiMoteState.ResetError();
            byte[] length = numberToBytes(dataLength, true, 1); // BIGENDIAN?
            byte[] mBuff = new byte[22];
            mBuff[0] = (byte)OutputReport.ReportingMode;
            mBuff[1] = (byte)((length[0] << 3) | GetRumbleBit());
            data.CopyTo(mBuff, 2);
            try
            {
                api.Write(this, mBuff, (uint)mBuff.Length);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
        #endregion

        #region Private
        /// <summary>
        /// Adds rumble bit for the speaker (must be done for each outputreport)
        /// </summary>
        /// <returns>0x01 if rumble is enabled</returns>
        private byte GetRumbleBit()
        {
            return (byte)(this.wiiMoteState.RumbleEnabled ? 0x01 : 0x00);
        }

        /// <summary>
        /// Reads from Memory/EEPROM 
        /// (HANDLE WITH CAUTION)
        /// </summary>
        /// <param name="EEPROM">set true to Read from EEPROM</param>
        /// <param name="offset">Offset in Memory/EEPROM to read from</param>
        /// <param name="size">Size of Data to read. Anything larger than 16 bytes will be split to 16-byte packets</param>
        private void ReadMemory(bool EEPROM, int offset, int size)
        {
            if (!this.isConnected)
                throw new InvalidOperationException("Not Connected");
            byte[] mBuff = new byte[22];
            mBuff[0] = (byte)OutputReport.ReadMemory;
            mBuff[1] = (byte)((EEPROM ? 0x00 : 0x04) | GetRumbleBit());
            byte[] offSet = numberToBytes(offset, true, 3);
            byte[] sizeBytes = numberToBytes(size, true, 2);
            offSet.CopyTo(mBuff, 2);
            sizeBytes.CopyTo(mBuff, 5);
            try
            {
                api.Write(this, mBuff, (uint)mBuff.Length);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                throw new WiimoteException(e); // Throw onward for calling method to catch (And stop Reading)
            }
        }

        /// <summary>
        /// Writes to Memory/EEPROM 
        /// (HANDLE WITH CAUTION)
        /// </summary>
        /// <param name="EEPROM">set true to write to EEPROM</param>
        /// <param name="offset">Offset in Memory/EEPROM to write to</param>
        /// <param name="size">Size of Data to write</param>
        private void WriteMemory(bool EEPROM, int offset, int size, byte[] data)
        {
            if (!this.isConnected)
                throw new InvalidOperationException("Not Connected");
            if (size > 16 || data.Length > 16)
                throw new ArgumentOutOfRangeException("Size too large");
            byte[] mBuff = new byte[22];
            mBuff[0] = (byte)OutputReport.WriteMemory;
            mBuff[1] = (byte)((EEPROM ? 0x00 : 0x04) | GetRumbleBit());
            byte[] offSet = numberToBytes(offset, true, 3);
            byte[] sizeBytes = numberToBytes(size, true, 1);
            offSet.CopyTo(mBuff, 2);
            sizeBytes.CopyTo(mBuff, 5);
            data.CopyTo(mBuff, 6);
            try
            {
                api.Write(this, mBuff, (uint)mBuff.Length);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                throw new WiimoteException(e); // Throw onward for calling method to catch (And stop writing)
            }
        }

        /// <summary>
        /// Initialises the Speaker through the procedure as shown on:
        /// Wiibrew.org/wiki/Wiimote
        /// 
        /// The following sequence will initialize the speaker:
        /// Enable speaker (Send 0x04 to Output Report 0x14) (done beforehand)
        /// Mute speaker (Send 0x04 to Output Report 0x19)
        /// Write 0x01 to register 0xa20009 (10616841)    - 1
        /// Write 0x08 to register 0xa20001 (10616833)    - 2
        /// Write 7-byte configuration to registers 0xa20001-0xa20008  - 3
        /// Write 0x01 to register 0xa20008 (10616840)    - 4
        /// Unmute speaker (Send 0x00 to Output Report 0x1)
        /// </summary>
        private bool InitSpeaker()
        {
            if (!this.isConnected)
                throw new InvalidOperationException("Not Connected");
            if (this.wiiMoteState.SpeakerSampleRate == 0)
                throw new WiimoteException("Set Config First");
            if (!this.wiiMoteState.SpeakerEnabled)
                throw new InvalidOperationException("Please use EnableSpeaker");
            try
            {
                MuteSpeaker(true);
                byte[] bytes = new byte[1];
                bytes[0] = (byte)0x01;
                WriteMemory(false, 10616841, 1, bytes); // - 1
                bytes[0] = (byte)0x08;
                WriteMemory(false, 10616833, 1, bytes); // - 2
                byte[] config = new byte[7];
                config[1] = (byte)this.wiiMoteState.speakerMode;
                numberToBytes((int)this.wiiMoteState.SpeakerSampleRate, false, 2).CopyTo(config, 2);
                config[4] = this.wiiMoteState.SpeakerVolume;
                WriteMemory(false, 10616833, 7, config); // - 3
                bytes[0] = (byte)0x01;
                WriteMemory(false, 10616840, 1, bytes); // - 4
                MuteSpeaker(false);
                return true;
            } catch (Exception e)
            { return false; }
        }

        /// <summary>
        /// Initialises the IR-Camera through the procedure as shown on:
        /// Wiibrew.org/wiki/Wiimote
        /// 
        /// The following procedure should be followed to turn on the IR Camera:
        /// Enable IR Camera (Send 0x04 to Output Report 0x13)
        /// Enable IR Camera 2 (Send 0x04 to Output Report 0x1a) - Done beforehand
        /// Write 0x08 to register 0xb00030 (11534384)          - 1
        /// Write Sensitivity Block 1 to registers at 0xb00000 (11534336) - 2
        /// Write Sensitivity Block 2 to registers at 0xb0001a (11534362) - 3
        /// Write Mode Number to register 0xb00033 (11534387)   - 4
        /// Write 0x08 to register 0xb00030 (again) (11534384)  - 5
        /// </summary>
        private bool InitIR()
        {
            if (!this.isConnected)
                throw new InvalidOperationException("Not Connected");
            if (!CheckReportingMode())
            {
                if (this.wiiMoteState.irMode == IRMode.Basic)
                    throw new WiimoteException("Set Proper ReportingMode First");
                else
                {
                    if (this.wiiMoteState.irMode == IRMode.Extended)
                        SetDataReportingMode(this.wiiMoteState.IsContinuous, ReportingState.ButtonsAccelAnd12IR);
                    else // IRMODE.FULL
                        SetDataReportingMode(this.wiiMoteState.IsContinuous, ReportingState.Interleaved);
                }
            }
            try
            {
                byte[] bytes = new byte[1];
                bytes[0] = (byte)0x08;
                WriteMemory(false, 11534384, 1, bytes); // - 1
                byte[] sensBlock1 = new byte[9];
                byte[] sensBlock2 = new byte[2];
                switch (this.wiiMoteState.irSensitivity)
                {
                    case IRSensitivity.Level1:
                        sensBlock1 = new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0x64, 0x00, 0xfe };
                        sensBlock2 = new byte[] { 0xfd, 0x05 };
                        break;
                    case IRSensitivity.Level2:
                        sensBlock1 = new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0x96, 0x00, 0xb4 };
                        sensBlock2 = new byte[] { 0xb3, 0x04 };
                        break;
                    case IRSensitivity.Level3:
                        sensBlock1 = new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0xaa, 0x00, 0x64 };
                        sensBlock2 = new byte[] { 0x63, 0x03 };
                        break;
                    case IRSensitivity.Level4:
                        sensBlock1 = new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0xc8, 0x00, 0x36 };
                        sensBlock2 = new byte[] { 0x35, 0x03 };
                        break;
                    case IRSensitivity.Level5:
                        sensBlock1 = new byte[] { 0x07, 0x00, 0x00, 0x71, 0x01, 0x00, 0x72, 0x00, 0x20 };
                        sensBlock2 = new byte[] { 0x1f, 0x03 };
                        break;
                }
                WriteMemory(false, 11534336, 9, sensBlock1); // - 2
                WriteMemory(false, 11534362, 2, sensBlock2); // - 3
                bytes[0] = (byte)this.wiiMoteState.irMode;
                WriteMemory(false, 11534387, 1, bytes); // - 4
                bytes[0] = (byte)0x08;
                WriteMemory(false, 11534384, 1, bytes); // - 5
                return true;
            } catch (Exception e)
            { return false; }
        }

        /// <summary>
        /// Checks wether IRMode & DataReportingState match
        /// </summary>
        /// <returns>True on match</returns>
        bool CheckReportingMode()
        {
            switch (this.wiiMoteState.irMode)
            {
                case IRMode.Basic:
                    if (this.wiiMoteState.reportingMode == ReportingState.Buttons10IRAnd9Ext ||
                        this.wiiMoteState.reportingMode == ReportingState.ButtonsAccel10IRAnd6Ext)
                        return true;
                    else return false;
                case IRMode.Extended:
                    if (this.wiiMoteState.reportingMode == ReportingState.ButtonsAccelAnd12IR)
                        return true;
                    else return false;
                case IRMode.Full: 
                    if (this.wiiMoteState.reportingMode == ReportingState.Interleaved)
                        return true;
                    else return false;
            }
            return false;
        }
        #endregion
        #endregion
        #endregion
    }
}