<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import apiClient from '@/services/api';
import type { CreateProductDto, ProductDto } from '@/types/dto';
import { toast } from 'vue-sonner';

const route = useRoute();
const router = useRouter();

const isEditMode = computed(() => route.params.id !== 'new');
const productId = computed(() => Number(route.params.id));
const loading = ref(false);

const form = ref<CreateProductDto>({
    name: '',
    description: '',
    price: 0,
    stock: 0,
    imageUrl: '',
    category: ''
});

const fetchProduct = async () => {
    if (!isEditMode.value) return;

    loading.value = true;
    try {
        const response = await apiClient.get<ProductDto>(`/v1/products/${productId.value}`);
        const product = response.data;
        form.value = {
            name: product.name,
            description: product.description,
            price: product.price,
            stock: product.stock,
            imageUrl: product.imageUrl || '',
            category: product.category || ''
        };
    } catch (error) {
        console.error('Error fetching product:', error);
        toast.error('Error al cargar el producto');
        router.push('/admin/products');
    } finally {
        loading.value = false;
    }
};

const handleSubmit = async () => {
    loading.value = true;
    try {
        if (isEditMode.value) {
            await apiClient.put(`/v1/products/${productId.value}`, form.value);
            toast.success('Producto actualizado');
        } else {
            await apiClient.post('/v1/products', form.value);
            toast.success('Producto creado');
        }
        router.push('/admin/products');
    } catch (error) {
        console.error('Error saving product:', error);
        toast.error('Error al guardar el producto');
    } finally {
        loading.value = false;
    }
};

onMounted(() => {
    fetchProduct();
});
</script>

<template>
    <div class="max-w-2xl mx-auto">
        <div class="md:flex md:items-center md:justify-between mb-6">
            <div class="min-w-0 flex-1">
                <h2
                    class="text-2xl font-bold leading-7 text-gray-900 dark:text-white sm:truncate sm:text-3xl sm:tracking-tight">
                    {{ isEditMode ? 'Editar Producto' : 'Nuevo Producto' }}
                </h2>
            </div>
        </div>

        <form @submit.prevent="handleSubmit" class="space-y-6 bg-white dark:bg-gray-800 p-6 rounded-lg shadow">
            <div>
                <label for="name"
                    class="block text-sm font-medium leading-6 text-gray-900 dark:text-white">Nombre</label>
                <div class="mt-2">
                    <input v-model="form.name" type="text" name="name" id="name" required
                        class="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6 dark:bg-gray-700 dark:text-white dark:ring-gray-600">
                </div>
            </div>

            <div>
                <label for="description"
                    class="block text-sm font-medium leading-6 text-gray-900 dark:text-white">Descripción</label>
                <div class="mt-2">
                    <textarea v-model="form.description" id="description" name="description" rows="3" required
                        class="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6 dark:bg-gray-700 dark:text-white dark:ring-gray-600"></textarea>
                </div>
            </div>

            <div class="grid grid-cols-1 gap-x-6 gap-y-8 sm:grid-cols-6">
                <div class="sm:col-span-3">
                    <label for="price"
                        class="block text-sm font-medium leading-6 text-gray-900 dark:text-white">Precio</label>
                    <div class="mt-2">
                        <input v-model.number="form.price" type="number" name="price" id="price" step="0.01" min="0"
                            required
                            class="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6 dark:bg-gray-700 dark:text-white dark:ring-gray-600">
                    </div>
                </div>

                <div class="sm:col-span-3">
                    <label for="stock"
                        class="block text-sm font-medium leading-6 text-gray-900 dark:text-white">Stock</label>
                    <div class="mt-2">
                        <input v-model.number="form.stock" type="number" name="stock" id="stock" min="0" required
                            class="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6 dark:bg-gray-700 dark:text-white dark:ring-gray-600">
                    </div>
                </div>
            </div>

            <div>
                <label for="category"
                    class="block text-sm font-medium leading-6 text-gray-900 dark:text-white">Categoría</label>
                <div class="mt-2">
                    <input v-model="form.category" type="text" name="category" id="category"
                        class="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6 dark:bg-gray-700 dark:text-white dark:ring-gray-600">
                </div>
            </div>

            <div>
                <label for="imageUrl" class="block text-sm font-medium leading-6 text-gray-900 dark:text-white">URL de
                    Imagen</label>
                <div class="mt-2">
                    <input v-model="form.imageUrl" type="text" name="imageUrl" id="imageUrl"
                        class="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6 dark:bg-gray-700 dark:text-white dark:ring-gray-600">
                </div>
            </div>

            <div class="flex items-center justify-end gap-x-6">
                <router-link to="/admin/products"
                    class="text-sm font-semibold leading-6 text-gray-900 dark:text-white">Cancelar</router-link>
                <button type="submit" :disabled="loading"
                    class="rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600 disabled:opacity-50">
                    {{ loading ? 'Guardando...' : 'Guardar' }}
                </button>
            </div>
        </form>
    </div>
</template>
