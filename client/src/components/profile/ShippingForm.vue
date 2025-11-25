<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { Form } from 'vee-validate';
import { toTypedSchema } from '@vee-validate/zod';
import { z } from 'zod';
import apiClient from '@/services/api';
import BaseInput from '@/components/common/BaseInput.vue';
import BaseButton from '@/components/common/BaseButton.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import { toast } from 'vue-sonner';

const loading = ref(false);
const initialValues = ref({
    addressLine1: '',
    addressLine2: '',
    city: '',
    state: '',
    postalCode: '',
    country: ''
});

const schema = toTypedSchema(z.object({
    addressLine1: z.string().nonempty('La dirección es obligatoria.'),
    addressLine2: z.string().optional(),
    city: z.string().nonempty('La ciudad es obligatoria.'),
    state: z.string().nonempty('El estado/provincia es obligatorio.'),
    postalCode: z.string().nonempty('El código postal es obligatorio.'),
    country: z.string().nonempty('El país es obligatorio.')
}));

onMounted(async () => {
    loading.value = true;
    try {
        const response = await apiClient.get('/v1/shipping');
        if (response.data) {
            initialValues.value = response.data;
        }
    } catch (error) {
        console.error('Error loading shipping address:', error);
        // Don't show error toast if it's just 204 No Content (handled in controller)
        // But controller returns 204 if null, axios might treat it as success with empty data or null
    } finally {
        loading.value = false;
    }
});

const handleSubmit = async (values: any) => {
    loading.value = true;
    try {
        await apiClient.post('/v1/shipping', values);
        toast.success('Dirección de envío guardada correctamente.');
    } catch (error) {
        console.error('Error saving shipping address:', error);
        toast.error('Error al guardar la dirección.');
    } finally {
        loading.value = false;
    }
};
</script>

<template>
    <div class="bg-white dark:bg-gray-800 shadow overflow-hidden sm:rounded-lg">
        <div class="px-4 py-5 sm:px-6">
            <h3 class="text-lg leading-6 font-medium text-gray-900 dark:text-gray-100">
                Dirección de Envío
            </h3>
            <p class="mt-1 max-w-2xl text-sm text-gray-500 dark:text-gray-400">
                Esta dirección se utilizará para tus pedidos.
            </p>
        </div>
        <div class="border-t border-gray-200 dark:border-gray-700 px-4 py-5 sm:px-6">
            <div v-if="loading && !initialValues.addressLine1" class="flex justify-center py-8">
                <LoadingSpinner />
            </div>

            <Form v-else @submit="handleSubmit" :validation-schema="schema" :initial-values="initialValues"
                class="space-y-4">
                <BaseInput name="addressLine1" label="Dirección (Calle y número)" id="addressLine1" type="text"
                    placeholder="Ej. Av. Reforma 123" />
                <BaseInput name="addressLine2" label="Apartamento, suite, etc. (Opcional)" id="addressLine2" type="text"
                    placeholder="Ej. Depto 4B" />

                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <BaseInput name="city" label="Ciudad" id="city" type="text" />
                    <BaseInput name="state" label="Estado / Provincia" id="state" type="text" />
                </div>

                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <BaseInput name="postalCode" label="Código Postal" id="postalCode" type="text" />
                    <BaseInput name="country" label="País" id="country" type="text" />
                </div>

                <div class="flex justify-end pt-4">
                    <BaseButton type="submit" :disabled="loading" variant="primary">
                        <LoadingSpinner v-if="loading" class="h-5 w-5 mr-2" />
                        {{ loading ? 'Guardando...' : 'Guardar Dirección' }}
                    </BaseButton>
                </div>
            </Form>
        </div>
    </div>
</template>
