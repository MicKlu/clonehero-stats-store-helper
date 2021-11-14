using System;
using HarmonyLib;

namespace StatsStoreHelper.MyWrappers
{
    public class MyWrapper
    {
        private object originalObject;
        private Type originalObjectType;

        public MyWrapper(object _originalObject, string _originalObjectTypeName)
        {
            this.originalObject = _originalObject;
            this.originalObjectType = AccessTools.TypeByName(_originalObjectTypeName);
        }

        public MyWrapper(object _originalObject, Type _originalObjectType)
        {
            this.originalObject = _originalObject;
            this.originalObjectType = _originalObjectType;
        }

        protected object GetFieldValue(string fieldName)
        {
            var field = AccessTools.Field(this.originalObjectType, fieldName);
            return field.GetValue(this.originalObject);
        }

        protected object GetPropertyValue(string propertyName)
        {
            var property = AccessTools.Property(this.originalObjectType, propertyName);
            return property.GetValue(this.originalObject);
        }
    }
}