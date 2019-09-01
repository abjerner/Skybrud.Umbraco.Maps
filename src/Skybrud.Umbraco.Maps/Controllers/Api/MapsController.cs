using System;
using System.Collections.Generic;
using Skybrud.Essentials.Strings;
using Skybrud.Umbraco.Maps.Models.Config;
using Skybrud.WebApi.Json;
using Umbraco.Web.WebApi;

namespace Skybrud.Umbraco.Maps.Controllers.Api {

    [JsonOnlyConfiguration]
    public class MapsController : UmbracoAuthorizedApiController {

        public object GetContentTypes(string ids) {

            List<object> temp = new List<object>();

            foreach (string id in StringUtils.ParseStringArray(ids)) {

                if (Guid.TryParse(id, out Guid guid)) {

                    var ct = Services.ContentTypeService.Get(guid);

                    if (ct == null) continue;

                    temp.Add(new MapsContentType(ct, Services));

                } else if (int.TryParse(id, out int numeric)) {

                    var ct = Services.ContentTypeService.Get(numeric);

                    if (ct == null) continue;

                    temp.Add(new MapsContentType(ct, Services));

                }

            }

            return temp;

        }

    }

}