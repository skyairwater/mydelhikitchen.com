const CartManager = {
    cart: {},

    init() {
        try {
            const saved = localStorage.getItem('ecommerce_cart');
            this.cart = saved ? JSON.parse(saved) : {};
        } catch (e) {
            console.error('Failed to parse cart', e);
            this.cart = {};
        }
        this.updateBadge();
    },

    save() {
        try {
            localStorage.setItem('ecommerce_cart', JSON.stringify(this.cart));
        } catch (e) {
            console.error('Failed to save cart', e);
        }
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

    clear() {
        this.cart = {};
        localStorage.removeItem('ecommerce_cart'); // Explicit removal
        this.updateBadge();
        $(document).trigger('cartUpdated', [this.cart]);
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
        let html = '<div class="d-flex justify-content-between align-items-center mb-2 px-1">';
        html += '<span class="small text-muted">Items</span>';
        html += '<button class="btn btn-link btn-sm text-danger p-0 text-decoration-none" id="empty-cart-btn" data-bs-toggle="modal" data-bs-target="#confirm-empty-cart-modal"><i class="bi bi-trash3 me-1"></i>Empty Cart</button>';
        html += '</div>';
        html += '<ul class="list-group list-group-flush">';
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

window.CartManager = CartManager; // Ensure global access

$(document).ready(function () {
    CartManager.init();

    $('#cart-modal').on('show.bs.modal', function () {
        CartManager.renderCartModal();
    });

    $(document).on('click', '.remove-from-cart', function () {
        const id = $(this).data('id');
        CartManager.removeItem(id);
        CartManager.renderCartModal();
    });

    $(document).on('click', '#confirm-clear-cart', function () {
        CartManager.clear();
        CartManager.renderCartModal();
        bootstrap.Modal.getInstance(document.getElementById('confirm-empty-cart-modal')).hide();
        // Also close the cart modal if it was open on top
        const cartModal = bootstrap.Modal.getInstance(document.getElementById('cart-modal'));
        if (cartModal) cartModal.hide();
    });

    // Theme Management
    const themeManager = {
        init() {
            const currentTheme = localStorage.getItem('theme') || 'light';
            this.setTheme(currentTheme);

            $('#theme-toggle').click(() => {
                const newTheme = document.documentElement.getAttribute('data-bs-theme') === 'dark' ? 'light' : 'dark';
                this.setTheme(newTheme);
            });
        },
        setTheme(theme) {
            document.documentElement.setAttribute('data-bs-theme', theme);
            localStorage.setItem('theme', theme);

            if (theme === 'dark') {
                $('#theme-icon-light').hide();
                $('#theme-icon-dark').show();
            } else {
                $('#theme-icon-light').show();
                $('#theme-icon-dark').hide();
            }
        }
    };

    themeManager.init();
});
