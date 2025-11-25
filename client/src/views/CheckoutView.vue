<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { loadStripe } from '@stripe/stripe-js';
import { useDark } from '@vueuse/core';
import apiClient from '@/services/api';
import { useCartStore } from '@/stores/cart';
import { toast } from 'vue-sonner';
import { useRouter } from 'vue-router';

import { type Stripe, type StripeElements, type StripePaymentElement } from '@stripe/stripe-js';

const publishableKey = import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY;

const stripePromise = loadStripe(publishableKey);
const cartStore = useCartStore();
const router = useRouter();

const loading = ref(false);
const clientSecret = ref('');
const stripe = ref<Stripe | null>(null);
const elements = ref<StripeElements | null>(null);
const paymentElement = ref<StripePaymentElement | null>(null);

const isDark = useDark();

onMounted(async () => {
    if (cartStore.items.length === 0) {
        toast.error("El carrito está vacío");
        router.push({ name: 'shop' });
        return;
    }

    try {
        loading.value = true;
        // 1. Create PaymentIntent on backend
        const { data } = await apiClient.post<{ clientSecret: string }>('/v1/payments/create-intent');
        clientSecret.value = data.clientSecret;

        // 2. Initialize Stripe Elements
        stripe.value = await stripePromise;
        if (!stripe.value) return;

        const theme = isDark.value ? 'night' : 'stripe';
        const appearance: { theme: 'night' | 'stripe', labels: 'floating' } = { theme, labels: 'floating' };
        elements.value = stripe.value.elements({ clientSecret: clientSecret.value, appearance });

        paymentElement.value = elements.value.create('payment');
        paymentElement.value.mount('#payment-element');
    } catch (error) {
        console.error("Error initializing payment:", error);
        toast.error("Error al iniciar el pago");
    } finally {
        loading.value = false;
    }
});

// Watch for theme changes and update Stripe Elements
watch(isDark, (newVal) => {
    if (elements.value) {
        elements.value.update({ appearance: { theme: newVal ? 'night' : 'stripe', labels: 'floating' } });
    }
});

const handleSubmit = async () => {
    if (!stripe.value || !elements.value) return;

    loading.value = true;

    const { error } = await stripe.value.confirmPayment({
        elements: elements.value,
        confirmParams: {
            return_url: window.location.origin + '/payment-success', // Create this route later
        },
    });

    if (error) {
        toast.error(error.message || 'Error desconocido');
    } else {
        // Successful payment will redirect to return_url
    }
    loading.value = false;
};
</script>

<template>
    <div class="container mx-auto px-4 py-8 max-w-lg">
        <h1 class="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Checkout</h1>

        <div class="bg-white dark:bg-gray-800 p-6 rounded-lg shadow-lg transition-colors duration-300">
            <div class="mb-6">
                <h2 class="text-lg font-semibold text-gray-700 dark:text-gray-300 mb-2">Resumen del Pedido</h2>
                <div class="flex justify-between text-gray-600 dark:text-gray-400">
                    <span>Total a Pagar:</span>
                    <span class="text-gray-900 dark:text-white font-bold text-xl">${{ cartStore.totalPrice.toFixed(2)
                        }}</span>
                </div>
            </div>

            <form id="payment-form" @submit.prevent="handleSubmit">
                <div id="payment-element" class="mb-6 min-h-[200px]">
                    <!-- Stripe Element will be inserted here -->
                </div>

                <button id="submit" :disabled="loading || !stripe || !elements"
                    class="w-full bg-indigo-600 hover:bg-indigo-700 text-white font-bold py-3 px-4 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed">
                    <span v-if="loading">Procesando...</span>
                    <span v-else>Pagar Ahora</span>
                </button>
            </form>
        </div>
    </div>
</template>
