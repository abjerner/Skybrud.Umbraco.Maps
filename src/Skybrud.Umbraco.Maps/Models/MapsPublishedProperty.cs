﻿using System;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Skybrud.Umbraco.Maps.Models {

    public class MapsPublishedProperty : IPublishedProperty {

        private readonly Lazy<object> _sourceValue;
        private readonly Lazy<object> _objectValue;
        private readonly Lazy<object> _xpathValue;

        public IPublishedPropertyType PropertyType { get; }

        public object DataValue { get; }

        public string Alias { get; }

        public string PropertyTypeAlias => PropertyType.DataType.EditorAlias;

        public bool HasValue => DataValue != null && DataValue.ToString().Trim().Length > 0;

        public object Value => _objectValue.Value;

        public object XPathValue => _xpathValue.Value;

        public MapsPublishedProperty(IPublishedPropertyType propertyType, string alias, object value) {
            PropertyType = propertyType;
            Alias = alias;
            DataValue = value;
            _sourceValue = new Lazy<object>(() => PropertyType.ConvertSourceToInter(null, DataValue, false));
            _objectValue = new Lazy<object>(() => PropertyType.ConvertInterToObject(null, PropertyCacheLevel.None, _sourceValue.Value, false));
            _xpathValue = new Lazy<object>(() => PropertyType.ConvertInterToXPath(null, PropertyCacheLevel.None, _sourceValue.Value, false));
        }

        bool IPublishedProperty.HasValue(string culture, string segment) {
            return HasValue;
        }

        public object GetSourceValue(string culture = null, string segment = null) {
            return DataValue;
        }

        public object GetValue(string culture = null, string segment = null) {
            return Value;
        }

        public object GetXPathValue(string culture = null, string segment = null) {
            return XPathValue;
        }

    }

}