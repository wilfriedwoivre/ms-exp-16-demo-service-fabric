(function () {
    angular.module('storeApp')
        .config(['$locationProvider', '$routeProvider',
            function ($locationProvider, $routeProvider) {
                $locationProvider.hashPrefix('!');
                $routeProvider
                    .when('/categories')
                    .otherwise('/categories');
            }]);
})();