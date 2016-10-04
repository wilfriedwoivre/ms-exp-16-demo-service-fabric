(function () {
    angular.module('storeApp')
        .config(['$locationProvider', '$routeProvider',
            function ($locationProvider, $routeProvider) {
                $locationProvider.hashPrefix('!');
                $routeProvider
                    .when('/categories', { template: '<categories></categories>' })
                    .when('/categories/:id', { template: '<products></products>' })
                    .otherwise('/categories');
            }]);
})();