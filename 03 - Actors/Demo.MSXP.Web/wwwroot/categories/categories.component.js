(function () {
    angular.module('categories')
        .component('categories', {
            templateUrl: 'categories/categories.template.html',
            controller: ['$http', CategoriesController]
        });

    function CategoriesController($http) {
        $http.defaults.cache = true;
        var ctrl = this;
        ctrl.categories = [];

        $http.get('api/catalog/categories')
            .then(function (response) {
                ctrl.categories = response.data;
            });
    };
})();