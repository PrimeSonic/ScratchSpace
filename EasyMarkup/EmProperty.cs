namespace EasyMarkup
{
    using System;
    using System.Reflection;
    using Core;

    internal sealed class EmProperty : EmBase
    {
        private readonly EasyMarkupProperty easyMarkupInfo;
        private readonly PropertyInfo propertyInfo;
        private readonly Type propertyType;
        private readonly Type parentClassType;

        private object objValue;

        public object Value
        {
            get => objValue;
            set => objValue = value.GetType() == propertyType ? value : null;
        }

        internal EmProperty(PropertyInfo propertyInfo, Type parentClass, EasyMarkupProperty easyMarkupInfo)
        {
            this.Key = easyMarkupInfo.Key ?? propertyInfo.Name;
            this.propertyType = propertyInfo.PropertyType;
            this.propertyInfo = propertyInfo;
            this.parentClassType = parentClass;
            this.easyMarkupInfo = easyMarkupInfo;

            if (easyMarkupInfo.DefaultValue != null)
            {
                Value = easyMarkupInfo.DefaultValue;
            }
        }

        public override bool HasValue => objValue != null && objValue.GetType() == propertyType;

        public override string ToString()
        {
            if (HasValue)
                SerializedValue = Convert.ToString(this.Value);

            return base.ToString();
        }

        protected override string ExtractValue(StringBuffer fullString)
        {
            string serialValue = base.ExtractValue(fullString);

            try
            {
                objValue = Convert.ChangeType(serialValue, propertyType);
            }
            catch
            {
                objValue = null;
            }

            return serialValue;
        }

        internal override EmBase Copy()
        {
            var emProperty = new EmProperty(propertyInfo, parentClassType, easyMarkupInfo);

            if (this.HasValue)
            {
                emProperty.Value = this.Value;
            }

            return emProperty;
        }

        internal override bool ValueEquals(EmBase other)
        {
            if (!(other is EmProperty otherTyped))
            {
                return false;
            }

            if (otherTyped.propertyInfo != this.propertyInfo)
            {
                return false;
            }

            return otherTyped.objValue.Equals(this.objValue);
        }

        internal bool TrySetCallerValue(object caller)
        {
            if (caller.GetType() != parentClassType)
            {
                return false;
            }

            try
            {
                propertyInfo.SetValue(caller, this.Value);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        internal bool TryGetCallerValue(object caller)
        {
            if (caller.GetType() != parentClassType)
            {
                return false;
            }

            try
            {
                this.Value = propertyInfo.GetValue(caller);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
}