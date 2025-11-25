<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const router = useRouter();
const sidebarOpen = ref(false);

const navigation = [
    { name: 'Dashboard', href: '/admin', icon: 'HomeIcon' },
    { name: 'Productos', href: '/admin/products', icon: 'ShoppingBagIcon' },
    { name: 'Pedidos', href: '/admin/orders', icon: 'ClipboardDocumentListIcon' },
];

const logout = () => {
    authStore.logout();
    router.push('/login');
};
</script>

<template>
    <div class="min-h-screen bg-gray-100 dark:bg-gray-900">
        <!-- Sidebar for Desktop -->
        <div class="hidden md:fixed md:inset-y-0 md:flex md:w-64 md:flex-col">
            <div class="flex min-h-0 flex-1 flex-col bg-gray-800">
                <div class="flex h-16 flex-shrink-0 items-center bg-gray-900 px-4">
                    <h1 class="text-xl font-bold text-white">Admin Panel</h1>
                </div>
                <div class="flex flex-1 flex-col overflow-y-auto">
                    <nav class="flex-1 space-y-1 px-2 py-4">
                        <router-link v-for="item in navigation" :key="item.name" :to="item.href"
                            class="group flex items-center rounded-md px-2 py-2 text-sm font-medium text-gray-300 hover:bg-gray-700 hover:text-white"
                            active-class="bg-gray-900 text-white">
                            {{ item.name }}
                        </router-link>
                    </nav>
                </div>
                <div class="flex flex-shrink-0 bg-gray-700 p-4">
                    <button @click="logout" class="group block w-full flex-shrink-0">
                        <div class="flex items-center">
                            <div class="ml-3">
                                <p class="text-sm font-medium text-white group-hover:text-gray-200">Cerrar Sesión</p>
                            </div>
                        </div>
                    </button>
                </div>
            </div>
        </div>

        <!-- Mobile Header -->
        <div class="flex flex-1 flex-col md:pl-64">
            <div class="sticky top-0 z-10 bg-gray-100 pl-1 pt-1 sm:pl-3 sm:pt-3 md:hidden dark:bg-gray-900">
                <button type="button"
                    class="-ml-0.5 -mt-0.5 inline-flex h-12 w-12 items-center justify-center rounded-md text-gray-500 hover:text-gray-900 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-indigo-500"
                    @click="sidebarOpen = true">
                    <span class="sr-only">Open sidebar</span>
                    <!-- Icon Menu -->
                    <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"
                        aria-hidden="true">
                        <path stroke-linecap="round" stroke-linejoin="round"
                            d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25h16.5" />
                    </svg>
                </button>
            </div>

            <!-- Main Content -->
            <main class="flex-1">
                <div class="py-6">
                    <div class="mx-auto max-w-7xl px-4 sm:px-6 md:px-8">
                        <router-view></router-view>
                    </div>
                </div>
            </main>
        </div>
    </div>
</template>
