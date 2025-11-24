import { defineStore } from 'pinia';
import type { CartItem, ProductDto } from '@/types/dto';
import { toast } from 'vue-sonner';

export const useCartStore = defineStore('cart', {
    state: () => ({
        items: [] as CartItem[],
        isOpen: false,
    }),
    persist: true,
    getters: {
        totalItems: (state) => state.items.reduce((sum, item) => sum + item.quantity, 0),
        totalPrice: (state) => state.items.reduce((sum, item) => sum + (item.price * item.quantity), 0),
    },
    actions: {
        addItem(product: ProductDto) {
            const existingItem = this.items.find(item => item.productId === product.id);
            if (existingItem) {
                existingItem.quantity++;
                toast.success(`Cantidad actualizada para ${product.name}`);
            } else {
                this.items.push({
                    productId: product.id,
                    name: product.name,
                    price: product.price,
                    quantity: 1,
                    imageUrl: product.imageUrl
                });
                toast.success(`${product.name} agregado al carrito`);
            }
        },
        removeItem(productId: number) {
            const index = this.items.findIndex(item => item.productId === productId);
            if (index !== -1) {
                this.items.splice(index, 1);
                toast.info('Producto eliminado del carrito');
            }
        },
        updateQuantity(productId: number, quantity: number) {
            const item = this.items.find(item => item.productId === productId);
            if (item) {
                if (quantity <= 0) {
                    this.removeItem(productId);
                } else {
                    item.quantity = quantity;
                }
            }
        },
        clearCart() {
            this.items = [];
            toast.info('Carrito vaciado');
        },
        toggleCart() {
            this.isOpen = !this.isOpen;
        },
        openCart() {
            this.isOpen = true;
        },
        closeCart() {
            this.isOpen = false;
        }
    }
});
