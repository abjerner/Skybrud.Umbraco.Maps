using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json;
using Skybrud.Essentials.Json.Extensions;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Skybrud.Umbraco.Maps.Models {

    public class MapEditorModel {

        #region Properties

        public IPublishedElement[] Items { get; }

        #endregion

        #region Constructors

        public MapEditorModel(JObject obj, IContentTypeService contentTypeService, IPublishedContentTypeFactory publishedContentTypeFactory, ILogger logger, IDataTypeService dataTypeService, IPublishedModelFactory publishedModelFactory, PropertyEditorCollection propertyEditors) {

            List<IPublishedElement> items = new List<IPublishedElement>();

            foreach (JObject item in obj.GetObjectArray("items")) {

                // Get basic information from the item
                Guid key = item.GetGuid("key");
                string name = item.GetString("name");
                Guid contentTypeKey = item.GetGuid("contentType");

                // Get a reference to the content type
                IContentType contentType = contentTypeService.Get(contentTypeKey);
                if (contentType == null) {
                    logger.Error(typeof(MapEditorModel), "Content type with key " + contentTypeKey + " not found.");
                    continue;
                }

                // Convert the content type to it's published counterpart
                IPublishedContentType pct = publishedContentTypeFactory.CreateContentType(contentType);

                List<IPublishedProperty> properties = new List<IPublishedProperty>();

                foreach (JProperty prop in item.GetObject("properties").Properties()) {

                    // Get a reference to the property type
                    IPublishedPropertyType type = pct.GetPropertyType(prop.Name);
                    if (type == null) {
                        logger.Error(typeof(MapEditorModel), $"Property type for property with alias {prop.Name} not found.");
                        continue;
                    }

                    // Get a reference to the property editor
                    if (propertyEditors.TryGet(type.EditorAlias, out IDataEditor propEditor) == false) {
                        logger.Error(typeof(MapEditorModel), $"Property editor with alias {type.EditorAlias} not found.");
                        continue;
                    }


                    #region Borrowed from Doc Type Grid Editor

                    ContentPropertyData contentPropData = new ContentPropertyData(prop.Value, null);

                    object newValue = propEditor.GetValueEditor().FromEditor(contentPropData, prop.Value);

                    PropertyType propType2 = contentType.CompositionPropertyTypes.First(x => x.PropertyEditorAlias.InvariantEquals(type.DataType.EditorAlias));

                    Property prop2 = null;
                    try {
                        /* HACK: [LK:2016-04-01] When using the "Umbraco.Tags" property-editor, the converted DB value does
                             * not match the datatypes underlying db-column type. So it throws a "Type validation failed" exception.
                             * We feel that the Umbraco core isn't handling the Tags value correctly, as it should be the responsiblity
                             * of the "Umbraco.Tags" property-editor to handle the value conversion into the correct type.
                             * See: http://issues.umbraco.org/issue/U4-8279
                             */
                        prop2 = new Property(propType2);
                        prop2.SetValue(newValue);
                    } catch (Exception ex) {
                        logger.Error(typeof(MapEditorModel), ex, "Error creating Property object.");
                    }

                    if (prop2 != null) {
                        string newValue2 = propEditor.GetValueEditor().ConvertDbToString(propType2, newValue, dataTypeService);
                        properties.Add(new MapsPublishedProperty(type, prop.Name, newValue2));
                    }

                    #endregion

                }

                // Create the model based on our implementation of IPublishedElement
                IPublishedElement content = new MapsPublishedElement(key, name, pct, properties.ToArray());

                // Let the current model factory create a typed model to wrap our model
                if (publishedModelFactory != null) content = publishedModelFactory.CreateModel(content);

                items.Add(content);

            }

            Items = items.ToArray();

        }

        #endregion

        #region Static methods

        public static MapEditorModel Deserialize(string source) {
            JObject obj = JsonUtils.ParseJsonObject(source);
            return new MapEditorModel(
                obj,
                Current.Services.ContentTypeService,
                Current.PublishedContentTypeFactory,
                Current.Logger,
                Current.Services.DataTypeService,
                Current.Factory.GetInstance<IPublishedModelFactory>(),
                Current.PropertyEditors
            );
        }

        #endregion

    }

}