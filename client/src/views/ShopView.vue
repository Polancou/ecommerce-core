<template>
  <div class="container mx-auto px-4 py-8">
    <h1 class="text-3xl font-bold mb-8 text-gray-900 dark:text-white">Tienda</h1>
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
      <div v-for="product in products" :key="product.id" class="bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden transition-transform hover:scale-105">
        <img :src="product.imageUrl || 'https://placehold.co/600x400'" :alt="product.name" class="w-full h-48 object-cover">
        <div class="p-4">
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-2">{{ product.name }}</h2>
          <p class="text-gray-600 dark:text-gray-300 mb-4 line-clamp-2 text-sm">{{ product.description }}</p>
          <div class="flex items-center justify-between mt-4">
            <span class="text-2xl font-bold text-gray-900 dark:text-white">${{ product.price }}</span>
            <button @click="cartStore.addItem(product)" 
              class="bg-indigo-600 text-white px-4 py-2 rounded hover:bg-indigo-700 transition-colors focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2">
              Agregar
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useCartStore } from '@/stores/cart';
import type { ProductDto } from '@/types/dto';

const cartStore = useCartStore();

const products = ref<ProductDto[]>([
  {
    id: 1,
    name: 'Smartphone X',
    description: 'Último modelo con cámara de alta resolución y batería de larga duración.',
    price: 999,
    stock: 10,
    imageUrl: 'https://placehold.co/600x400?text=Smartphone'
  },
  {
    id: 2,
    name: 'Laptop Pro',
    description: 'Potente laptop para profesionales creativos.',
    price: 1499,
    stock: 5,
    imageUrl: 'https://placehold.co/600x400?text=Laptop'
  },
  {
    id: 3,
    name: 'Auriculares Noise Cancelling',
    description: 'Sumérgete en tu música sin distracciones.',
    price: 299,
    stock: 20,
    imageUrl: 'https://placehold.co/600x400?text=Headphones'
  },
  {
    id: 4,
    name: 'Smartwatch Series 5',
    description: 'Monitorea tu salud y notificaciones en tu muñeca.',
    price: 399,
    stock: 15,
    imageUrl: 'https://placehold.co/600x400?text=Watch'
  },
  {
    id: 5,
    name: 'Cámara DSLR',
    description: 'Captura momentos increíbles con calidad profesional.',
    price: 899,
    stock: 8,
    imageUrl: 'https://placehold.co/600x400?text=Camera'
  },
  {
    id: 6,
    name: 'Tablet Air',
    description: 'Ligera y potente, perfecta para el entretenimiento y trabajo.',
    price: 599,
    stock: 12,
    imageUrl: 'https://placehold.co/600x400?text=Tablet'
  }
]);
</script>
