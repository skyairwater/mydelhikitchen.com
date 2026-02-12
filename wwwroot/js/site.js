const CartManager = {
    cart: JSON.parse(localStorage.getItem('ecommerce_cart')) || {},

    save() {
        localStorage.setItem('ecommerce_cart', JSON.stringify(this.cart));
        this.updateBadge();
        $(document).trigger('cartUpdated', [this.cart]);
    },

    addItem(product) {
        if (!this.cart[product.id]) {
            this.cart[product.id] = {
                id: product.id,
                name: product.name,
                category: product.category,
                price: product.price,
                qty: 0
            };
        }
        this.cart[product.id].qty++;
        this.save();
    },

    removeItem(id) {
        if (this.cart[id]) {
            this.cart[id].qty--;
            if (this.cart[id].qty <= 0) {
                delete this.cart[id];
            }
            this.save();
        }
    },

    getCart() {
        return this.cart;
    },

    getTotal() {
        let total = 0;
        for (let id in this.cart) {
            total += this.cart[id].qty * this.cart[id].price;
        }
        return total;
    },

    getItemCount() {
        let count = 0;
        for (let id in this.cart) {
            count += this.cart[id].qty;
        }
        return count;
    },

    updateBadge() {
        const count = this.getItemCount();
        const badge = $('#cart-badge');
        if (count > 0) {
            badge.text(count).show();
        } else {
            badge.hide();
        }
    },

    renderCartModal() {
        const container = $('#cart-items-container');
        container.empty();
        const cart = this.getCart();

        if (Object.keys(cart).length === 0) {
            container.append('<div class="text-center py-4 text-muted">Your cart is empty</div>');
            $('#cart-modal-footer').hide();
            return;
        }

        $('#cart-modal-footer').show();
        let html = '<ul class="list-group list-group-flush">';
        for (let id in cart) {
            const item = cart[id];
            html += `
                <li class="list-group-item px-0">
                    <div class="d-flex justify-content-between align-items-start">
                        <div class="me-auto">
                            <div class="small text-uppercase text-muted">${item.category}</div>
                            <div class="fw-bold">${item.name}</div>
                            <div class="small">${item.qty} x $${item.price.toFixed(2)}</div>
                        </div>
                        <div class="text-end">
                            <div class="fw-bold">$${(item.qty * item.price).toFixed(2)}</div>
                            <button class="btn btn-sm text-danger p-0 mt-1 remove-from-cart" data-id="${id}">Remove</button>
                        </div>
                    </div>
                </li>`;
        }
        html += '</ul>';
        container.append(html);
        $('#cart-modal-total').text('$' + this.getTotal().toFixed(2));
    }
};

$(document).ready(function () {
    CartManager.updateBadge();

    $('#cart-modal').on('show.bs.modal', function () {
        CartManager.renderCartModal();
    });

    $(document).on('click', '.remove-from-cart', function () {
        const id = $(this).data('id');
        CartManager.removeItem(id);
        CartManager.renderCartModal();
    });
});
