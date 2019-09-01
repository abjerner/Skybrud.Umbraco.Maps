using System;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json;
using Skybrud.Essentials.Json.Extensions;
using Skybrud.Essentials.Maps.Geometry;
using Skybrud.Essentials.Maps.Geometry.Lines;
using Skybrud.Essentials.Maps.Geometry.Shapes;
using Skybrud.Essentials.Maps.Google;
using Skybrud.Umbraco.Maps.Constants;
using Skybrud.Umbraco.Maps.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Skybrud.Umbraco.Maps {

    public class MapsPropertyValueConverter : PropertyValueConverterBase {

        public override bool IsConverter(IPublishedPropertyType propertyType) {
            return propertyType.EditorAlias == "Skybrud.Umbraco.Maps" || propertyType.EditorAlias.StartsWith("Skybrud.Umbraco.Maps.");
        }

        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview) {

            if (!(source is string)) return null;

            string strValue = source + "";

            switch (propertyType.EditorAlias) {

                case MapsConstants.Editors.Default:
                    return MapEditorModel.Deserialize(strValue);

                case MapsConstants.Editors.Geometry.Point:
                    return JsonUtils.ParseJsonObject(strValue, ParsePoint);

                case MapsConstants.Editors.Geometry.LineString:
                    return JsonUtils.ParseJsonObject(strValue, ParseLineString);

                case MapsConstants.Editors.Geometry.Polygon:
                    return JsonUtils.ParseJsonObject(strValue, ParsePolygon);

                case MapsConstants.Editors.Geometry.Rectangle:
                    return JsonUtils.ParseJsonObject(strValue, ParseRectangle);

            }

            return null;

        }
        
        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview) {
            return inter;
        }

        public override object ConvertIntermediateToXPath(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview) {
            return null;
        }

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType) {
            return PropertyCacheLevel.Snapshot;
        }

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType) {

            switch (propertyType.EditorAlias) {

                case MapsConstants.Editors.Default:
                    return typeof(MapEditorModel);

                case MapsConstants.Editors.Geometry.Point:
                    return typeof(IPoint);

                case MapsConstants.Editors.Geometry.LineString:
                    return typeof(ILineString);

                case MapsConstants.Editors.Geometry.Polygon:
                    return typeof(IPolygon);

            }

            return base.GetPropertyValueType(propertyType);

        }

        private IPoint ParsePoint(JObject obj) {

            string format = obj.GetString("format");

            switch (format) {

                case "polyline":
                    return GooglePolylineAlgoritm.Decode<IPoint>(obj.GetString("path"));

                default:
                    throw new Exception("Unknown geometry type: " + format);

            }

        }

        private ILineString ParseLineString(JObject obj) {

            string format = obj.GetString("format");

            switch (format) {

                case "polyline":
                    return GooglePolylineAlgoritm.Decode<ILineString>(obj.GetString("path"));

                default:
                    throw new Exception("Unknown geometry type: " + format);

            }

        }

        private IPolygon ParsePolygon(JObject obj) {

            string format = obj.GetString("format");

            switch (format) {

                case "polyline":
                    return GooglePolylineAlgoritm.Decode<IPolygon>(obj.GetString("path"));

                default:
                    throw new Exception("Unknown geometry type: " + format);

            }

        }

        private IRectangle ParseRectangle(JObject obj) {

            string format = obj.GetString("format");

            switch (format) {

                case "polyline":
                    return GooglePolylineAlgoritm.Decode<IRectangle>(obj.GetString("path"));

                default:
                    throw new Exception("Unknown geometry type: " + format);

            }

        }
 
    }

}