using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System;

namespace Assets
{
    public enum ReportingState : byte
    {
        Buttons = 0x30,
        ButtonsAndAccel = 0x31,
        ButtonsAnd8Ext = 0x32,
        ButtonsAccelAnd12IR = 0x33,
        ButtonsAnd19Ext = 0x34,
        ButtonsAccelAnd16Ext = 0x35,
        Buttons10IRAnd9Ext = 0x36,
        ButtonsAccel10IRAnd6Ext = 0x37,
        FullExt = 0x3d,
        Interleaved = 0x3e,
    };

    public enum SpeakerMode : byte
    {
        ADPCM4BIT = 0x00,
        PCM8BIT = 0x40
    };

    public enum IRMode : byte
    {
        Basic = 1,
        Extended = 3,
        Full = 5
    };

    public enum IRSensitivity
    {
        Level1,
        Level2,
        Level3,
        Level4,
        Level5
    };

    public struct AccelerometerState
    {
        public uint xPos, yPos, zPos;
    }

    /// <summary>
    /// References for all Buttons on the Wiimote
    /// </summary>
    public struct ButtonState
    {
        public bool A, B, Plus, Home, Minus, One, Two, Up, Down, Left, Right;
    }

    /// <summary>
    /// References for the Wiimote's LEDs
    /// </summary>
    public struct LEDState
    {
        public bool LED1, LED2, LED3, LED4;
    }

    /// <summary>
    /// Point: X, Y, Size, Intensity
    /// bound: xMin, yMin, xMax, yMax
    /// </summary>
    public struct IRState
    {
        public Vector4 Point1, Point2, Point3, Point4;
        public Vector4 bound1, bound2, bound3, bound4;
    }

    public class WiiMoteState
    {
        /// <summary>
        /// Current State of all Buttons
        /// </summary>
        [DataMember]
        public ButtonState currentState;
        /// <summary>
        /// State of all Buttons when "GetCurrentState() was last called";
        /// </summary>
        [DataMember]
        public ButtonState previousState;

        [DataMember]
        public byte BatteryLevel;

        [DataMember]
        public bool BatteryLow;

        [DataMember]
        public bool ExtensionConnected;

        /// <summary>
        /// State of Accelerometer
        /// </summary>
        [DataMember]
        public AccelerometerState accelState;

        [DataMember]
        public IRState irState { get; private set; }

        /// <summary>
        /// State of all LED's
        /// </summary>
        [DataMember]
        public LEDState ledState;

        [DataMember]
        public bool IsContinuous;

        [DataMember]
        public bool RumbleEnabled;

        [DataMember]
        public bool IREnabled;

        [DataMember]
        public IRMode irMode { get; private set; }

        [DataMember]
        public IRSensitivity irSensitivity { get; private set; }

        [DataMember]
        public bool SpeakerEnabled;

        public SpeakerMode speakerMode { get; private set; }

        [DataMember]
        public byte SpeakerVolume { get; private set; }

        [DataMember]
        public uint SpeakerSampleRate { get; private set; }

        [DataMember]
        public bool SpeakerMuted;

        [DataMember]
        public ReportingState reportingMode = ReportingState.Buttons;

        [DataMember]
        public int LastReportedError { get; private set; }

        public WiiMoteState()
        {
            irSensitivity = IRSensitivity.Level3;
        }

        public ButtonState GetCurrentState()
        {
            previousState = currentState;
            return currentState;
        }

        public ButtonState GetPreviousState()
        {
            return previousState;
        }

        public int GetBatteryPercentage()
        {
            return (int)((this.BatteryLevel / 255) * 100);
        }

        public void ResetError()
        { SetError(0); }

        public void SetError(int error)
        {
            this.LastReportedError = error;
        }

        public void SetupSpeakerConfig(SpeakerMode mode, uint sampleRate, uint volume)
        {
            if (mode == SpeakerMode.PCM8BIT)
            {
                if (volume > 255) throw new ArgumentException("Volume too high (range 0-255)", "volume");
                this.SpeakerSampleRate = 12000000 / sampleRate;
            }
            else // SpeakerMode.ADPCM4BIT
            {
                if (volume >= 64) throw new ArgumentException("Volume too high (range 0-64)", "volume");
                this.SpeakerSampleRate = 6000000 / sampleRate; 
            }
            this.SpeakerVolume = (byte)volume;
        }

        public void SetupIRConfig(SpeakerMode mode, IRSensitivity sensitivity)
        {
            this.speakerMode = mode;
            this.irSensitivity = sensitivity;
        }

        public void UpdateAccel(uint x, uint y, uint z)
        {
            this.accelState.xPos = x;
            this.accelState.yPos = y;
            this.accelState.zPos = z;
        }

        public void UpdateIR(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4)
        {
            Vector3 Point1 = new Vector3(point1.x, point1.y, 1);
            Vector3 Point2 = new Vector3(point2.x, point2.y, 1);
            Vector3 Point3 = new Vector3(point3.x, point3.y, 1);
            Vector3 Point4 = new Vector3(point4.x, point4.y, 1);
            UpdateIR(Point1, Point2, Point3, Point4);
        }

        public void UpdateIR(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4)
        {
            this.irState.Point1.Set(point1.x, point1.y, point1.z, 1);
            this.irState.Point2.Set(point2.x, point2.y, point2.z, 1);
            this.irState.Point3.Set(point3.x, point3.y, point3.z, 1);
            this.irState.Point4.Set(point4.x, point4.y, point4.z, 1);
        }

        public void InterleavedUpdateIR(Vector4 point1, Vector4 bound1, Vector4 point2, Vector4 bound2, bool isFirstMessage)
        {
            if (isFirstMessage)
            {
                this.irState.Point1.Set(point1.x, point1.y, point1.z, point1.w);
                this.irState.Point2.Set(point2.x, point2.y, point2.z, point2.w);
                this.irState.bound1.Set(bound1.x, bound1.y, bound1.z, bound1.w);
                this.irState.bound2.Set(bound2.x, bound2.y, bound2.z, bound2.w);
            }
            else
            {
                this.irState.Point3.Set(point1.x, point1.y, point1.z, point1.w);
                this.irState.Point4.Set(point2.x, point2.y, point2.z, point2.w);
                this.irState.bound3.Set(bound1.x, bound1.y, bound1.z, bound1.w);
                this.irState.bound4.Set(bound2.x, bound2.y, bound2.z, bound2.w);
            }            
        }
    }
}