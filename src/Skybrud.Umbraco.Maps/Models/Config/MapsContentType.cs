using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Skybrud.Umbraco.Maps.Models.Config {

    public class MapsContentType {

        [JsonProperty("id")]
        public int Id { get; }

        [JsonProperty("key")]
        public Guid Key { get; }

        [JsonProperty("alias")]
        public string Alias { get; }

        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("icon")]
        public string Icon { get; }

        [JsonProperty("geometry")]
        public MapsContentTypeGeometryProperty Geometry { get; }

        [JsonProperty("propertyTypes")]
        public object PropertyTypes { get; }

        public MapsContentType(IContentType ct, ServiceContext services) {

            Id = ct.Id;
            Key = ct.Key;
            Name = ct.Name;
            Icon = ct.Icon;
            Alias = ct.Alias;

            foreach (PropertyType propertyType in ct.PropertyTypes) {
                if (propertyType.PropertyEditorAlias.StartsWith("Skybrud.Umbraco.Maps.Geometry.")) {
                    Geometry = new MapsContentTypeGeometryProperty(propertyType);
                    break;
                }
            }

            List<object> temp = new List<object>();
            
            foreach (PropertyType propertyType in ct.PropertyTypes) {

                IDataType dataType = services.DataTypeService.GetDataType(propertyType.DataTypeId);

                IDataEditor propertyEditor = dataType.Editor;

                temp.Add(new {
                    alias = propertyType.Alias,
                    name = propertyType.Name,
                    description = propertyType.Description,
                    dataType = new {
                        alias = propertyEditor.Alias,
                        view = propertyEditor.GetValueEditor().View,
                        config = dataType.Configuration
                    }
                });

            }

            PropertyTypes = temp;

        }

    }

}