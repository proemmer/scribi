using System;

namespace Scribi.Attributes
{
    public enum FunctionCallType { Undefined, CallOnStart, CallOnStop, CallOnInterval, CallOnTime, CallOnDelay }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class FunctionCallAttribute : Attribute
    {
        private readonly FunctionCallType _fct;

        public DateTime DateTime { get; set; }
        public double TimeMs { get; set; }

        public FunctionCallAttribute(FunctionCallType fct)
        {
            if (fct == FunctionCallType.CallOnInterval || fct == FunctionCallType.CallOnTime)
                throw new Exception("you have to specify a value (timeMs or dateTime)");
            _fct = fct;
        }


        public FunctionCallAttribute(FunctionCallType fct, double timeMs)
        {
            _fct = fct;
            TimeMs = timeMs;
        }

        public FunctionCallAttribute(FunctionCallType fct, DateTime dateTime)
        {
            _fct = fct;
            DateTime = dateTime;
        }

        public FunctionCallType GetFunctionCallType()
        {
            return _fct;
        }
    }
}
