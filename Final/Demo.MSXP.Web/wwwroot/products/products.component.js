(function () {
    angular.module('products')
        .component('products', {
            templateUrl: 'products/products.template.html',
            controller: ['$routeParams', '$http', 'Cart', ProductsController]
        });

    function ProductsController($routeParams, $http, Cart) {
        $http.defaults.cache = true;

        var ctrl = this;
        ctrl.category = {};
        ctrl.products = [];

        $http.get(`api/catalog/categories/${$routeParams.id}`)
            .then(function (response) {
                ctrl.category = response.data;
            });

        $http.get(`api/catalog/categories/${$routeParams.id}/products`)
            .then(function(response) {
                var productIds = response.data;
                productIds.forEach(function(id) {
                    $http.get(`api/catalog/products/${id}`)
                        .then(function(response2) {
                            ctrl.products.push(response2.data);
                        });
                });
            });

        ctrl.addToCart = function (id) {
            Cart.addItem(id, 1);
        }
    };
})();