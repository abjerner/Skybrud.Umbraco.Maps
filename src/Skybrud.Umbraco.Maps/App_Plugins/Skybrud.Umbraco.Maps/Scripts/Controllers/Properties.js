﻿angular.module("umbraco").controller("Skybrud.Maps.Properties.Controller", function ($scope, formHelper) {

	$scope.hideIcon = typeof ($scope.model.data.icon) === "undefined" ? "true" : null;
	$scope.hideAlias = typeof ($scope.model.data.alias) === "undefined";
    $scope.nameLocked = typeof ($scope.model.data.name) === "undefined";
    
	$scope.data = angular.copy($scope.model.data);

    $scope.data.name = typeof ($scope.data.name) === "string" ? $scope.data.name : $scope.model.title;

    $scope.save = function () {

        if (formHelper.submitForm({ scope: $scope })) {

	        formHelper.resetForm({ scope: $scope });

	        $scope.model.submit($scope.data);

        }

	};

});