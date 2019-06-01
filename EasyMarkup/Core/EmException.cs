﻿namespace EasyMarkup.Core
{
    using System;

    public class EmException : Exception
    {
        internal StringBuffer CurrentBuffer { get; }

        public EmException()
        {
        }

        public EmException(string message)
            : base(message)
        {
        }

        public EmException(string message, StringBuffer currentBuffer)
            : base(message)
        {
            this.CurrentBuffer = currentBuffer;
        }

        public EmException(StringBuffer currentBuffer)
        {
            this.CurrentBuffer = currentBuffer;
        }

        public override string ToString()
        {
            if (!(this.CurrentBuffer is null) && !this.CurrentBuffer.IsEmpty)
            {
                return $"Error reported: {this.Message}{Environment.NewLine}" + $"Current text in buffer: {this.CurrentBuffer}";
            }

            return base.ToString();
        }
    }
}