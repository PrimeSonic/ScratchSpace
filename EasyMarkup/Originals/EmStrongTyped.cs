namespace EasyMarkup

{
    using System;
    using System.Globalization;
    using Core;

    public class EmStrongTyped<T> : EmBase
        where T : IConvertible
    {
        private bool hasValue = false;

        public string InLineComment { get; set; } = null;

        public override bool HasValue => hasValue;

        public T DefaultValue { get; set; } = default;

        private T ObjValue;

        private readonly Type DataType;

        public T Value
        {
            get => ObjValue;
            set
            {
                ObjValue = value;
                hasValue = value != null;

                SerializedValue = ObjValue?.ToString(CultureInfo.InvariantCulture);
            }
        }

        public EmStrongTyped(string key, T defaultValue = default)
        {
            DataType = typeof(T);
            this.Key = key;
            ObjValue = defaultValue;
            this.DefaultValue = defaultValue;
            SerializedValue = ObjValue?.ToString(CultureInfo.InvariantCulture);
        }

        public override string ToString()
        {
            if (!this.HasValue && this.Optional)
            {
                return string.Empty;
            }

            return $"{base.ToString()}{EmUtils.CommentText(this.InLineComment)}";
        }

        protected override string ExtractValue(StringBuffer fullString)
        {
            string serialValue = base.ExtractValue(fullString);

            this.Value = ConvertFromSerial(serialValue);

            hasValue = true;

            return serialValue;
        }

        public virtual T ConvertFromSerial(string value)
        {
            try
            {
                return DataType.IsEnum ? (T)Enum.Parse(DataType, value, true) : (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                hasValue = false;
                return this.DefaultValue;
            }
        }

        internal override EmBase Copy()
        {
            return new EmStrongTyped<T>(this.Key, this.HasValue ? ObjValue : this.DefaultValue)
            {
                Optional = this.Optional,
                InLineComment = this.InLineComment
            };
        }

        internal override bool ValueEquals(EmBase other)
        {
            if (other is EmStrongTyped<T> otherTyped)
            {
                return this.Value.Equals(otherTyped.Value);
            }

            return false;
        }
    }
}