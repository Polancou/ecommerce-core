<script setup lang="ts">
import { ref, onMounted } from 'vue';
import apiClient from '@/services/api';
import type { ProductDto } from '@/types/dto';
import { toast } from 'vue-sonner';

const products = ref<ProductDto[]>([]);
const loading = ref(false);

const fetchProducts = async () => {
    loading.value = true;
    try {
        const response = await apiClient.get<ProductDto[]>('/v1/products');
        products.value = response.data;
    } catch (error) {
        console.error('Error fetching products:', error);
        toast.error('Error al cargar productos');
    } finally {
        loading.value = false;
    }
};

const deleteProduct = async (id: number) => {
    if (!confirm('¿Estás seguro de eliminar este producto?')) return;

    try {
        await apiClient.delete(`/v1/products/${id}`);
        toast.success('Producto eliminado');
        await fetchProducts();
    } catch (error) {
        console.error('Error deleting product:', error);
        toast.error('Error al eliminar el producto');
    }
};

onMounted(() => {
    fetchProducts();
});
</script>

<template>
    <div>
        <div class="sm:flex sm:items-center">
            <div class="sm:flex-auto">
                <h1 class="text-2xl font-semibold text-gray-900 dark:text-white">Productos</h1>
                <p class="mt-2 text-sm text-gray-700 dark:text-gray-300">Lista de todos los productos disponibles en la
                    tienda.</p>
            </div>
            <div class="mt-4 sm:ml-16 sm:mt-0 sm:flex-none">
                <router-link to="/admin/products/new"
                    class="block rounded-md bg-indigo-600 px-3 py-2 text-center text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600">
                    Agregar Producto
                </router-link>
            </div>
        </div>

        <div class="mt-8 flow-root">
            <div class="-mx-4 -my-2 overflow-x-auto sm:-mx-6 lg:-mx-8">
                <div class="inline-block min-w-full py-2 align-middle sm:px-6 lg:px-8">
                    <div class="overflow-hidden shadow ring-1 ring-black ring-opacity-5 sm:rounded-lg">
                        <table class="min-w-full divide-y divide-gray-300 dark:divide-gray-700">
                            <thead class="bg-gray-50 dark:bg-gray-800">
                                <tr>
                                    <th scope="col"
                                        class="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 dark:text-white sm:pl-6">
                                        Nombre</th>
                                    <th scope="col"
                                        class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 dark:text-white">
                                        Precio</th>
                                    <th scope="col"
                                        class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 dark:text-white">
                                        Stock</th>
                                    <th scope="col"
                                        class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 dark:text-white">
                                        Categoría</th>
                                    <th scope="col" class="relative py-3.5 pl-3 pr-4 sm:pr-6">
                                        <span class="sr-only">Acciones</span>
                                    </th>
                                </tr>
                            </thead>
                            <tbody class="divide-y divide-gray-200 bg-white dark:divide-gray-700 dark:bg-gray-900">
                                <tr v-if="loading">
                                    <td colspan="5" class="text-center py-4 text-gray-500">Cargando...</td>
                                </tr>
                                <tr v-else-if="products.length === 0">
                                    <td colspan="5" class="text-center py-4 text-gray-500">No hay productos registrados.
                                    </td>
                                </tr>
                                <tr v-for="product in products" :key="product.id">
                                    <td
                                        class="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 dark:text-white sm:pl-6">
                                        {{ product.name }}</td>
                                    <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500 dark:text-gray-300">${{
                                        product.price }}</td>
                                    <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500 dark:text-gray-300">{{
                                        product.stock }}</td>
                                    <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500 dark:text-gray-300">{{
                                        product.category || 'N/A' }}</td>
                                    <td
                                        class="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                                        <router-link :to="`/admin/products/${product.id}`"
                                            class="text-indigo-600 hover:text-indigo-900 dark:text-indigo-400 dark:hover:text-indigo-300 mr-4">Editar</router-link>
                                        <button @click="deleteProduct(product.id)"
                                            class="text-red-600 hover:text-red-900 dark:text-red-400 dark:hover:text-red-300">Eliminar</button>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
