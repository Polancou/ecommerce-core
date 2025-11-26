<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useCartStore } from '@/stores/cart';
import type { ProductDto } from '@/types/dto';
import { ShoppingCartIcon, MagnifyingGlassIcon, ChevronLeftIcon, ChevronRightIcon } from '@heroicons/vue/24/solid';
import apiClient from '@/services/api';
import { toast } from 'vue-sonner';
import { getImageUrl } from '@/utils/image';

const cartStore = useCartStore();
const products = ref<ProductDto[]>([]);
const loading = ref(true);

// Pagination & Search state
const searchTerm = ref('');
const currentPage = ref(1);
const totalPages = ref(1);
const totalCount = ref(0);
const pageSize = 9; // 3x3 grid

const fetchProducts = async () => {
  loading.value = true;
  try {
    const response = await apiClient.get<{ items: ProductDto[], totalPages: number, totalCount: number }>('/v1/products', {
      params: {
        search: searchTerm.value,
        page: currentPage.value,
        pageSize: pageSize
      }
    });

    products.value = response.data.items;
    totalPages.value = response.data.totalPages;
    totalCount.value = response.data.totalCount;
  } catch (error) {
    console.error('Error fetching products:', error);
    toast.error('Error al cargar los productos');
  } finally {
    loading.value = false;
  }
};

// Debounce search
let searchTimeout: ReturnType<typeof setTimeout>;
watch(searchTerm, () => {
  clearTimeout(searchTimeout);
  searchTimeout = setTimeout(() => {
    currentPage.value = 1; // Reset to first page on search
    fetchProducts();
  }, 300);
});

const changePage = (page: number) => {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page;
    fetchProducts();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
};

onMounted(() => {
  fetchProducts();
});
</script>

<template>
  <div class="container mx-auto px-4 py-8">
    <div class="flex flex-col md:flex-row md:items-center justify-between mb-8 gap-4">
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white">Tienda</h1>

      <!-- Search Bar -->
      <div class="w-full md:w-96 relative">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          <MagnifyingGlassIcon class="h-5 w-5 text-gray-400" aria-hidden="true" />
        </div>
        <input v-model="searchTerm" type="text"
          class="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md leading-5 bg-white dark:bg-gray-700 dark:border-gray-600 dark:text-white placeholder-gray-500 focus:outline-none focus:placeholder-gray-400 focus:ring-1 focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm transition duration-150 ease-in-out"
          placeholder="Buscar productos..." />
      </div>
    </div>

    <div v-if="loading" class="flex justify-center items-center h-64">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
    </div>

    <div v-else-if="products.length === 0" class="text-center py-12">
      <p class="text-gray-500 dark:text-gray-400 text-lg">No se encontraron productos.</p>
    </div>

    <div v-else>
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
        <div v-for="product in products" :key="product.id"
          class="bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden transition-transform hover:scale-105">
          <img :src="getImageUrl(product.imageUrl)" :alt="product.name" class="w-full h-48 object-cover">
          <div class="p-4">
            <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-2">{{ product.name }}</h2>
            <p class="text-gray-600 dark:text-gray-300 mb-4 line-clamp-2 text-sm">{{ product.description }}</p>
            <div class="flex items-center justify-between mt-4">
              <span class="text-2xl font-bold text-gray-900 dark:text-white">${{ product.price }}</span>
              <button @click="cartStore.addItem(product)"
                class="bg-indigo-600 text-white px-4 py-2 rounded hover:bg-indigo-700 transition-colors focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 flex items-center gap-2">
                <ShoppingCartIcon class="h-5 w-5" />
                Agregar
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Pagination -->
      <div class="flex items-center justify-between border-t border-gray-200 dark:border-gray-700 px-4 py-3 sm:px-6">
        <div class="flex flex-1 justify-between sm:hidden">
          <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1"
            class="relative inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed dark:bg-gray-800 dark:border-gray-600 dark:text-gray-300">
            Anterior
          </button>
          <button @click="changePage(currentPage + 1)" :disabled="currentPage === totalPages"
            class="relative ml-3 inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed dark:bg-gray-800 dark:border-gray-600 dark:text-gray-300">
            Siguiente
          </button>
        </div>
        <div class="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
          <div>
            <p class="text-sm text-gray-700 dark:text-gray-300">
              Mostrando <span class="font-medium">{{ (currentPage - 1) * pageSize + 1 }}</span> a <span
                class="font-medium">{{ Math.min(currentPage * pageSize, totalCount) }}</span> de <span
                class="font-medium">{{ totalCount }}</span> resultados
            </p>
          </div>
          <div>
            <nav class="isolate inline-flex -space-x-px rounded-md shadow-sm" aria-label="Pagination">
              <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1"
                class="relative inline-flex items-center rounded-l-md px-2 py-2 text-gray-400 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 focus:z-20 focus:outline-offset-0 disabled:opacity-50 disabled:cursor-not-allowed dark:ring-gray-600 dark:hover:bg-gray-700">
                <span class="sr-only">Anterior</span>
                <ChevronLeftIcon class="h-5 w-5" aria-hidden="true" />
              </button>

              <!-- Page Numbers (Simplified) -->
              <button v-for="page in totalPages" :key="page" @click="changePage(page)" :class="[
                page === currentPage
                  ? 'z-10 bg-indigo-600 text-white focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600'
                  : 'text-gray-900 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 focus:outline-offset-0 dark:text-gray-300 dark:ring-gray-600 dark:hover:bg-gray-700',
                'relative inline-flex items-center px-4 py-2 text-sm font-semibold'
              ]">
                {{ page }}
              </button>

              <button @click="changePage(currentPage + 1)" :disabled="currentPage === totalPages"
                class="relative inline-flex items-center rounded-r-md px-2 py-2 text-gray-400 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 focus:z-20 focus:outline-offset-0 disabled:opacity-50 disabled:cursor-not-allowed dark:ring-gray-600 dark:hover:bg-gray-700">
                <span class="sr-only">Siguiente</span>
                <ChevronRightIcon class="h-5 w-5" aria-hidden="true" />
              </button>
            </nav>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
