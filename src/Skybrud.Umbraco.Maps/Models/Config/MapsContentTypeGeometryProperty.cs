using Newtonsoft.Json;
using Skybrud.Umbraco.Maps.Constants;
using Umbraco.Core.Models;

namespace Skybrud.Umbraco.Maps.Models.Config {

    public class MapsContentTypeGeometryProperty {
        
        [JsonProperty("type")]
        public string Type { get; }

        [JsonProperty("alias")]
        public string Alias { get; }

        [JsonProperty("config")]
        public object Config { get; }

        public MapsContentTypeGeometryProperty(PropertyType propertyType) {

            switch (propertyType.PropertyEditorAlias) {

                case MapsConstants.Editors.Geometry.Point:
                    Type = "point";
                    break;

                case MapsConstants.Editors.Geometry.LineString:
                    Type = "lineString";
                    break;

                case MapsConstants.Editors.Geometry.Polygon:
                    Type = "polygon";
                    break;

                case MapsConstants.Editors.Geometry.Rectangle:
                    Type = "rectangle";
                    break;

            }

            Alias = propertyType.Alias;

            Config = new { };

        }

    }

}