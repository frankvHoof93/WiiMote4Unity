using UnityEngine;
using System.Collections;
using System;
using System.ComponentModel;

namespace Assets
{
    public enum ExceptionType
    {
        OS_ERROR,
        CONNECTION,
        DEVICE_ERROR,
        WRITE_ERROR,
        MISC
    }

    public class HIDException : Exception 
    {
        public ExceptionType exceptionType { get; protected set; }
        public string errorMessage { get; protected set; }
        public Exception innerException { get; protected set; }

        public HIDException(string msg) : base(msg) { }
        public HIDException(Exception e) : base("", e) { }
        public HIDException(ExceptionType type, string msg) : base(msg) {
            this.innerException = null;
            this.exceptionType = type;
            this.errorMessage = msg;
        }
        public HIDException(Exception e, ExceptionType type, string msg) : base(e.Message, e) 
        {
            this.innerException = e;
            this.exceptionType = type;
            this.errorMessage = msg;
        }

        public override string ToString()
        {
            return "HIDException: " + errorMessage
                + " Type: " + exceptionType.ToString()
                + " -- InnerException: " + Environment.NewLine
                + base.ToString();
        }
    }
}