(function () {
    angular.module('core.cart')
        .factory('Cart', ['$rootScope', CartFactory]);

    function CartFactory($rootScope) {
        var cart = {
            items: {},
            count: 0
        };

        function updateCartCount() {
            var qty = 0;
            for (let id in cart.items) {
                if (cart.items.hasOwnProperty(id)) {
                    qty += cart.items[id];
                }
            }
            cart.count = qty;

            $rootScope.$broadcast('cart:updated');
        }

        cart.addItem = function (id, qty) {
            if (!cart.items.hasOwnProperty(id)
                || cart.items[id] === null
                || cart.items[id] === Number.NaN) {
                cart.items[id] = 0;
            }
            cart.items[id] += qty;
            updateCartCount();
        };

        cart.removeItem = function (id, qty) {
            if (cart.items.hasOwnProperty(id)) {
                cart.items[id] -= qty;
                if (cart.items <= 0)
                    delete cart[id];
            }
            updateCartCount();
        };

        cart.clear = function () {
            cart.items = [];
            updateCartCount();
        };

        return cart;
    }
})();