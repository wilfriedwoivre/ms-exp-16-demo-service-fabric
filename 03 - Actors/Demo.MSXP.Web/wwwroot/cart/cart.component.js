(function () {
    angular.module('cart')
        .component('cart', {
            templateUrl: 'cart/cart.template.html',
            controller: ['$http', 'Cart', CartController]
        });

    function CartController($http, Cart) {
        var ctrl = this;
        ctrl.cart = [];
        ctrl.totalPrice = 0;
        ctrl.wishlistname = "";
        ctrl.wishlistexpiration = 0;
        ctrl.placeWishlist = placeWishlist;
        ctrl.orderId = null;

        loadCart();

        function loadCart() {
            for (let itemId in Cart.items) {
                if (Cart.items.hasOwnProperty(itemId)) {
                    var item = { id: itemId, qty: Cart.items[itemId], product: null };
                    ctrl.cart.push(item);
                    $http.get(`api/catalog/products/${itemId}`)
                        .then(function (response) {
                            const cartItem = ctrl.cart.filter(function (i) { return i.id === response.data.id })[0];
                            cartItem.product = response.data;
                            ctrl.totalPrice += cartItem.qty * cartItem.product.price;
                        });
                }
            }
        }

        function placeWishlist() {
            var wishlist = {
                items: [],
                name: ctrl.wishlistname,
                expiration: ctrl.wishlistexpiration
            };

            for (let index in ctrl.cart) {
                wishlist.items.push({
                    productId: ctrl.cart[index].id,
                    quantity: ctrl.cart[index].qty
                });
            }

            $http.post('api/user/wishlist', wishlist)
                .then(function (response) {
                    ctrl.orderId = response.data;
                    Cart.clear();
                });
        }

    };
})();