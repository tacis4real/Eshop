(function () {
    'use strict';

    const TOKEN_KEY = 'jwt_token';

    function getToken() {
        return localStorage.getItem(TOKEN_KEY);
    }

    function authHeaders() {
        return {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + getToken()
        };
    }

    // Redirect to login if no token
    if (!getToken()) {
        window.location.href = '/login';
        return;
    }

    document.getElementById('logout-btn').addEventListener('click', function () {
        localStorage.removeItem(TOKEN_KEY);
        window.location.href = '/login';
    });

    // ── Products ──────────────────────────────────────────────────────────────

    async function loadProducts() {
        const container = document.getElementById('products-list');
        try {
            const res = await fetch('/api/products');
            if (!res.ok) throw new Error('Failed to load products');
            const products = await res.json();

            if (!products.length) {
                container.innerHTML = '<div class="col-12 text-muted">No products available.</div>';
                return;
            }

            container.innerHTML = products.map(p => `
                <div class="col-12">
                    <div class="card h-100">
                        <div class="card-body d-flex justify-content-between align-items-start">
                            <div>
                                <h6 class="card-title mb-1">${escHtml(p.name)}</h6>
                                <p class="card-text text-muted small mb-1">${escHtml(p.description)}</p>
                                <span class="badge bg-secondary">$${p.price.toFixed(2)}</span>
                                <span class="text-muted small ms-2">Stock: ${p.stockQuantity}</span>
                            </div>
                            <button class="btn btn-sm btn-primary ms-3 flex-shrink-0"
                                    onclick="addToCart(${p.id})"
                                    ${p.stockQuantity < 1 ? 'disabled' : ''}>
                                Add to Cart
                            </button>
                        </div>
                    </div>
                </div>`).join('');
        } catch (e) {
            container.innerHTML = `<div class="col-12 text-danger">Error loading products.</div>`;
        }
    }

    window.addToCart = async function (productId) {
        try {
            const res = await fetch('/api/cart/items', {
                method: 'POST',
                headers: authHeaders(),
                body: JSON.stringify({ productId, quantity: 1 })
            });
            if (handleUnauthorized(res)) return;
            if (!res.ok) {
                const err = await res.json().catch(() => ({}));
                alert(err.message || 'Could not add item to cart.');
                return;
            }
            await loadCart();
        } catch {
            alert('Network error while adding to cart.');
        }
    };

    // ── Cart ──────────────────────────────────────────────────────────────────

    async function loadCart() {
        const section = document.getElementById('cart-section');
        try {
            const res = await fetch('/api/cart', { headers: authHeaders() });
            if (handleUnauthorized(res)) return;
            if (!res.ok) throw new Error('Failed to load cart');
            const cart = await res.json();
            renderCart(cart);
        } catch {
            section.innerHTML = '<p class="text-danger">Error loading cart.</p>';
        }
    }

    function renderCart(cart) {
        const section = document.getElementById('cart-section');
        if (!cart.items || cart.items.length === 0) {
            section.innerHTML = '<p class="text-muted">Your cart is empty.</p>';
            return;
        }

        const rows = cart.items.map(item => `
            <tr>
                <td>${escHtml(item.productName)}</td>
                <td>$${item.unitPrice.toFixed(2)}</td>
                <td>
                    <input type="number" class="form-control form-control-sm" style="width:70px;"
                           id="qty-${item.id}" value="${item.quantity}" min="1" max="999" />
                </td>
                <td>$${item.lineTotal.toFixed(2)}</td>
                <td class="text-nowrap">
                    <button class="btn btn-sm btn-outline-secondary me-1"
                            onclick="updateItem(${item.id})">Update</button>
                    <button class="btn btn-sm btn-outline-danger"
                            onclick="removeItem(${item.id})">Remove</button>
                </td>
            </tr>`).join('');

        section.innerHTML = `
            <table class="table table-sm align-middle">
                <thead class="table-light">
                    <tr>
                        <th>Product</th><th>Price</th><th>Qty</th><th>Total</th><th></th>
                    </tr>
                </thead>
                <tbody>${rows}</tbody>
            </table>
            <div class="text-end fw-bold fs-5">Cart Total: $${cart.total.toFixed(2)}</div>`;
    }

    window.updateItem = async function (itemId) {
        const qtyInput = document.getElementById('qty-' + itemId);
        const quantity = parseInt(qtyInput ? qtyInput.value : '1', 10);
        if (isNaN(quantity) || quantity < 1 || quantity > 999) {
            alert('Quantity must be between 1 and 999.');
            return;
        }
        try {
            const res = await fetch('/api/cart/items/' + itemId, {
                method: 'PUT',
                headers: authHeaders(),
                body: JSON.stringify({ quantity })
            });
            if (handleUnauthorized(res)) return;
            if (!res.ok) {
                const err = await res.json().catch(() => ({}));
                alert(err.message || 'Could not update item.');
                return;
            }
            const cart = await res.json();
            renderCart(cart);
        } catch {
            alert('Network error while updating cart.');
        }
    };

    window.removeItem = async function (itemId) {
        try {
            const res = await fetch('/api/cart/items/' + itemId, {
                method: 'DELETE',
                headers: authHeaders()
            });
            if (handleUnauthorized(res)) return;
            if (!res.ok) {
                const err = await res.json().catch(() => ({}));
                alert(err.message || 'Could not remove item.');
                return;
            }
            const cart = await res.json();
            renderCart(cart);
        } catch {
            alert('Network error while removing from cart.');
        }
    };

    // ── Helpers ───────────────────────────────────────────────────────────────

    function handleUnauthorized(res) {
        if (res.status === 401) {
            localStorage.removeItem(TOKEN_KEY);
            window.location.href = '/login';
            return true;
        }
        return false;
    }

    function escHtml(str) {
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    // ── Init ──────────────────────────────────────────────────────────────────
    loadProducts();
    loadCart();
}());
