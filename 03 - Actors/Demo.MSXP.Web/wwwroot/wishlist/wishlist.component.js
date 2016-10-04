(function () {
    angular.module('wishlist')
        .component('wishlist', {
            templateUrl: 'wishlist/wishlist.template.html',
            controller: ['$http', WishListController]
        });

    function WishListController($http) {
        var ctrl = this;
        ctrl.wishes = [];


        loadWishes();

        function loadWishes() {
            $http.get('api/user/wishlist')
                .then(function(response) {
                    for (var i = 0; i < response.data.length; i++) {

                        var wish = {
                            name: response.data[i].name,
                            items: []
                        };

                        for (var j = 0; j < response.data[i].items.length; j++) {
                            var id = response.data[i].items[j].productId;

                            (function(item) {
                                $http.get(`api/catalog/products/${id}`)
                                    .then(function(responseProduct) {
                                        item.items.push(responseProduct.data);
                                    });
                            })(wish);


                        }

                        ctrl.wishes.push(wish);

                    }

                });
        }
    }
})();


/*
 *  
 */