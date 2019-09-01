angular.module("umbraco").controller("Skybrud.Maps.Controller", function ($scope, $http, $element, editorService) {

    $scope.model.config = {
        allowedTypes: $scope.model.config && Array.isArray($scope.model.config.allowedTypes) ? $scope.model.config.allowedTypes : [],
        apiKey: $scope.model.config ? $scope.model.config.apiKey : ""
    };


    // https://stackoverflow.com/a/2117523
    function uuidv4() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }


    if (!$scope.model.config.apiKey) {
        $scope.error = "No API key specified.";
        return;
    }





    // Define an alias for the editor (eg. used for callbacks)
    var alias = $scope.alias = ("skybrudsocial_" + Math.random()).replace(".", "");


    var shapes = {};
    $scope.contentTypes = [];
    var contentTypes = {};

    $scope.current = null;

    var map;



    if (!$scope.model.value) {
        $scope.model.value = {
            items: []
        };
    }

    function initPoint(item, position, isNew, editable) {

        isNew = isNew === true;
        editable = editable === true;

        if (!position) {
            position = { lat: 0, lng: 0 };
        } else if (typeof (position) === "string") {
            position = google.maps.geometry.encoding.decodePath(position)[0];
        }

        var marker = new google.maps.Marker({
	        icon: {
		        url: "/App_Plugins/Skybrud.Umbraco.Maps/Markers/" + (isNew ? "active" : "inactive") + ".png"
	        },
            position: position,
            map: map
        });

        shapes[item.key] = marker;

    }

    function initPolyline(item, path, isNew, editable) {

        isNew = isNew === true;
        editable = editable === true;

        if (!path) {
            path = [];
        } else if (typeof (path) === "string") {
            path = google.maps.geometry.encoding.decodePath(path);
        }

        var p = new google.maps.Polyline({
            strokeColor: isNew ? "red" : "#000",
            strokeOpacity: 1.0,
            strokeWeight: 3,
            editable: editable,
            path: path
        });

        p.setMap(map);

        shapes[item.key] = p;

    }

    function initPolygon(item, path, isNew, editable) {

        isNew = isNew === true;
        editable = editable === true;

        if (!path) {
            path = [];
        } else if (typeof (path) === "string") {
            path = google.maps.geometry.encoding.decodePath(path);
        }

        var p = new google.maps.Polygon({
            strokeColor: isNew ? "red" : "#000",
            strokeOpacity: 1.0,
            strokeWeight: 3,
            fillColor: isNew ? "red" : "#000",
            fillOpacity: 0.25,
            editable: editable,
            path: path
        });

        p.setMap(map);

        shapes[item.key] = p;

    }

    function init() {

        // Get data about the allowed data types
        $http.get("/umbraco/backoffice/api/Maps/GetContentTypes?ids=" + $scope.model.config.allowedTypes.join(",")).then(function (r) {
            $scope.contentTypes = r.data;
            $scope.contentTypes.forEach(function (e) {
                contentTypes[e.key] = e;
            });
            init2();
        });

    }

    function init2() {

        var e = $element[0].querySelector(".map");

        map = new google.maps.Map(e, {
            zoom: 12,
            center: { lat: 55.706975, lng: 12.429038 },
            streetViewControl: false,
            fullscreenControl: false,
            mapTypeId: "terrain"
        });

        google.maps.event.addListener(map, "click", addPoint);

        if ($scope.model.value.items) {

            $scope.model.value.items.forEach(function (item) {

                var ct = contentTypes[item.contentType];

                switch (ct.geometry.type) {

                    case "point":
                        initPoint(item, item.properties[ct.geometry.alias].position);
                        break;

                    case "lineString":
                    case "polyline":
                        initPolyline(item, item.properties[ct.geometry.alias].path);
                        break;

                    case "polygon":
                        initPolygon(item, item.properties[ct.geometry.alias].path);
                        break;

                }

            });

        }

    }

    window[alias] = function () {
        init();
    };

    function addPoint(event) {

        if ($scope.current === null) return;

        // Get the content type
        const ct = contentTypes[$scope.current.contentType];
        if (!ct) return;

        const shape = shapes[$scope.current.key];

        switch (ct.geometry.type) {

            case "point":
                $scope.current.$valid = true;
                shape.setOptions({
                    position: event.latLng
                });
                break;

            case "lineString": {
                const path = shape.getPath();
                path.push(event.latLng);
                $scope.current.$valid = path.length >= 2;
                break;
            }

            case "polygon": {
                const path = shape.getPath();
                path.push(event.latLng);
                $scope.current.$valid = path.length >= 3;
                break;
            }

        }

    }

    function drawLineString(contentType) {

        var item = {
            $new: true,
            key: uuidv4(),
            name: "",
            contentType: contentType.key,
            properties: {}
        };

        item.properties[contentType.geometry.alias] = null;

        var polyline = new google.maps.Polyline({
            strokeColor: "red",
            strokeOpacity: 1.0,
            strokeWeight: 3,
            editable: true,
            path: [],
            map: map
        });

        shapes[item.key] = polyline;

        //google.maps.event.addListener(p.getPath(), "insert_at", hest);
        //google.maps.event.addListener(p.getPath(), "remove_at", hest);
        //google.maps.event.addListener(p.getPath(), "set_at", hest);

        $scope.current = item;

    }


    function drawPolygon(contentType) {

        var item = {
            $new: true,
            key: uuidv4(),
            name: "",
            contentType: contentType.key,
            properties: {}
        };

        item.properties[contentType.geometry.alias] = null;

        var polygon = new google.maps.Polygon({
            strokeColor: "red",
            strokeOpacity: 1.0,
            strokeWeight: 3,
            editable: true,
            path: [],
            map: map
        });

        shapes[item.key] = polygon;

        var path = polygon.getPath();

        function updated() {
            item.$valid = path.length >= 3;
            $scope.$apply();
        }

        google.maps.event.addListener(path, "insert_at", updated);
        google.maps.event.addListener(path, "remove_at", updated);
        google.maps.event.addListener(path, "set_at", updated);

        $scope.current = item;

    }

    function drawPoint(contentType) {

        var item = {
            $new: true,
            key: uuidv4(),
            contentType: contentType.key,
            properties: {}
        };

        item.properties[contentType.geometry.alias] = null;

        var marker = new google.maps.Marker({
            //position: myLatLng,
            map: map,
            animation: google.maps.Animation.BOUNCE,
            draggable: true,
            icon: {
	            url: "/App_Plugins/Skybrud.Umbraco.Maps/Markers/active.png"
            }
        });

        shapes[item.key] = marker;

        $scope.current = item;

    }

    function formatPath(shape) {

        var path = shape.getPath();

        return google.maps.geometry.encoding.encodePath(path);

    }

    function finishShape() {

        var item = $scope.current;
        var isNew = item.$new === true;

        var shape = shapes[item.key];

        var ct = contentTypes[item.contentType];

        item.properties[ct.geometry.alias] = {
            format: ct.geometry.type,
            path: formatPath(shape)
        };

        if (item.$new) {
            delete (item.$new);
            $scope.model.value.items.push(item);
        }

        shape.setOptions({
            editable: false,
            strokeColor: "#000"
        });

        $scope.current = null;

        if (isNew) $scope.editContent(item);

    }

    function finishPoint() {

        var item = $scope.current;
        var isNew = item.$new === true;

        var shape = shapes[item.key];

        var ct = contentTypes[item.contentType];

        item.properties[ct.geometry.alias] = {
            format: "polyline",
            position: google.maps.geometry.encoding.encodePath([shape.position])
        };

        if (item.$new) {
            delete (item.$new);
            $scope.model.value.items.push(item);
        }

        shape.setOptions({
	        icon: {
		        url: "/App_Plugins/Skybrud.Umbraco.Maps/Markers/inactive.png"
	        },
            draggable: false,
            animation: google.maps.Animation.NONE
        });

        $scope.current = null;

        if (isNew) $scope.editContent(item);

    }

    $scope.ct = function (item) {
        return contentTypes[item.contentType];
    };

    $scope.draw = function (contentType) {
        if (contentType.geometry.type === "point") {
            drawPoint(contentType);
        } else if (contentType.geometry.type === "lineString") {
            drawLineString(contentType);
        } else if (contentType.geometry.type === "polygon") {
            drawPolygon(contentType);
        }
    };

    $scope.confirm = function () {

        // Get the current item
        var item = $scope.current;
        if (!item) return;

        // Get a reference to the content type
        var ct = contentTypes[item.contentType];

        switch (ct.geometry.type) {

            case "point":
                finishPoint();
                break;

            case "lineString":
            case "polygon":
                finishShape();
                break;

            default:
                console.log("Unsupported geometry type: " + ct.geometry.type);
                break;

        }

    };

    $scope.cancel = function () {

        if (!$scope.current) return;

        var item = $scope.current;
        $scope.current = null;

        var shape = shapes[item.key];

        // Get a reference to the content type
        var ct = contentTypes[item.contentType];

        if (item.$new) {
            shape.setMap(null);
            delete shapes[item.key];
            $scope.current = null;
            return;
        }

        switch (ct.geometry.type) {

            case "point":

                shape.setOptions({
                    draggable: false,
                    animation: google.maps.Animation.NONE,
                    position: google.maps.geometry.encoding.decodePath(item.properties[ct.geometry.alias].position)[0],
                    icon: {
	                    url: "/App_Plugins/Skybrud.Umbraco.Maps/Markers/inactive.png"
                    }
                });

                break;

            case "lineString":
            case "polygon":

                shape.setOptions({
                    editable: false,
                    strokeColor: "#000"
                });

                shape.setPath(google.maps.geometry.encoding.decodePath(item.properties[ct.geometry.alias].path));

                break;

        }

    };

    $scope.editContent = function (item) {

        var properties = [];

        var data = {
            properties: properties
        };

        var ct = contentTypes[item.contentType];

        var nameAlias = null;

        ct.propertyTypes.forEach(function (p) {

            if (p.alias === ct.geometry.alias) return;

            //if (p.alias === "elementName") {
            // nameAlias = p.alias;
            // data.name = item.properties[p.alias] ? item.properties[p.alias] : "";
            // return;
            //}

            var pp = {
                alias: p.alias,
                label: p.name,
                description: p.description,
                value: item.properties[p.alias],
                view: p.dataType.view
            };

            properties.push(pp);

        });

        editorService.open({
            view: "/App_Plugins/Skybrud.Umbraco.Maps/Views/Overlays/Properties.html",
            data: data,
            size: "small",
            title: "Edit element",
            submit: function (model) {
                if (nameAlias) {
                    item.properties[nameAlias] = model.data.name || "";
                }
                angular.forEach(model.properties, function (p) {
                    item.properties[p.alias] = p.value;
                });
                editorService.close();
            },
            close: function () {
                editorService.close();
            }
        });

    };

    $scope.editGeometry = function (item) {

        if ($scope.current) {
	        $scope.cancel();
        }

        $scope.current = item;

        var shape = shapes[item.key];

        var ct = contentTypes[item.contentType];

        if (ct.geometry.type === "point") {
	        $scope.current.$valid = true;
            shape.setOptions({
                draggable: true,
                animation: google.maps.Animation.BOUNCE,
                icon: {
	                url: "/App_Plugins/Skybrud.Umbraco.Maps/Markers/active.png"
                }
            });
        } else {
            shape.setOptions({
                editable: true,
                strokeColor: "red"
            });
        }

    };

    $scope.delete = function (index) {

        var item = $scope.model.value.items[index];

        $scope.model.value.items.splice(index, 1);

        if (shapes[item.key]) {
            shapes[item.key].setMap(null);
            delete shapes[item.key];
        }

    };

    var script = document.createElement("script");
    script.src = "https://maps.googleapis.com/maps/api/js?key=" + $scope.model.config.apiKey + "&callback=" + alias + "&libraries=geometry";
    document.body.appendChild(script);


    $scope.fullscreen = false;

    var p = $element[0].parentNode;

    $scope.toggleFullscreen = function () {

        if ($scope.fullscreen) {

            p.appendChild($element[0]);
            $element[0].classList.remove("fullscreen");
            $scope.fullscreen = false;

        } else {
            var body = document.getElementsByTagName("body")[0];
            body.appendChild($element[0]);
            $element[0].classList.add("fullscreen");
            $scope.fullscreen = true;
        }


    };


});