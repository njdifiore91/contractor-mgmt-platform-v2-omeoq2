<template>
  <div class="base-search q-pa-md">
    <!-- Main search input -->
    <QInput
      v-model="searchTerm"
      :placeholder="placeholder"
      outlined
      dense
      clearable
      class="q-mb-md"
      :error="!!errorMessage"
      :error-message="errorMessage"
      @keyup.enter="handleSearch"
      :disable="isLoading"
    >
      <template v-slot:append>
        <QBtn
          icon="search"
          flat
          round
          dense
          :loading="isLoading"
          @click="handleSearch"
          :disable="!isValidSearch"
        />
      </template>
    </QInput>

    <!-- Geographic search section -->
    <div v-if="enableGeographicSearch" class="geographic-search q-mt-md row q-col-gutter-md">
      <div class="col-xs-12 col-sm-6">
        <QInput
          v-model="zipCode"
          label="Zip Code"
          outlined
          dense
          mask="#####"
          :rules="[
            val => !val || validateZipCode(val, 'US').isValid || 'Invalid zip code format'
          ]"
          :disable="isLoading"
        />
      </div>
      <div class="col-xs-12 col-sm-6">
        <QInput
          v-model.number="searchRadius"
          label="Search Radius (miles)"
          type="number"
          outlined
          dense
          :rules="[
            val => val >= 1 || 'Minimum radius is 1 mile',
            val => val <= 500 || 'Maximum radius is 500 miles'
          ]"
          :disable="isLoading"
        />
      </div>
    </div>

    <!-- Error message display -->
    <div v-if="errorMessage" class="text-negative q-mt-sm">
      {{ errorMessage }}
    </div>
  </div>
</template>

<script lang="ts">
// Vue 3.3.0
import { defineComponent, ref, computed } from 'vue';

// Quasar 2.12.0
import { QInput, QBtn } from 'quasar';

// @vueuse/core 10.0.0
import { useDebounce } from '@vueuse/core';

// Internal imports
import { ListField } from '@/types/common';
import { validateZipCode } from '@/utils/validation';

export default defineComponent({
  name: 'BaseSearch',

  components: {
    QInput,
    QBtn
  },

  props: {
    searchFields: {
      type: Array as () => Array<ListField>,
      required: true,
      validator: (value: ListField[]) => value.length > 0
    },
    placeholder: {
      type: String,
      default: 'Search...'
    },
    enableGeographicSearch: {
      type: Boolean,
      default: false
    },
    minSearchLength: {
      type: Number,
      default: 3
    }
  },

  emits: ['search'],

  setup(props, { emit }) {
    // Reactive state
    const searchTerm = ref('');
    const zipCode = ref('');
    const searchRadius = ref(50);
    const isLoading = ref(false);
    const errorMessage = ref('');

    // Computed properties
    const isValidSearch = computed(() => {
      if (!props.enableGeographicSearch) {
        return searchTerm.value.length >= props.minSearchLength;
      }

      // For geographic search, either main search or zip+radius must be valid
      return (
        searchTerm.value.length >= props.minSearchLength ||
        (zipCode.value.length === 5 && searchRadius.value >= 1 && searchRadius.value <= 500)
      );
    });

    // Methods
    const validateSearchParams = (): boolean => {
      errorMessage.value = '';

      if (!searchTerm.value && !props.enableGeographicSearch) {
        errorMessage.value = `Please enter at least ${props.minSearchLength} characters`;
        return false;
      }

      if (props.enableGeographicSearch && zipCode.value) {
        const zipValidation = validateZipCode(zipCode.value, 'US');
        if (!zipValidation.isValid) {
          errorMessage.value = zipValidation.errorMessage;
          return false;
        }

        if (searchRadius.value < 1 || searchRadius.value > 500) {
          errorMessage.value = 'Search radius must be between 1 and 500 miles';
          return false;
        }
      }

      return true;
    };

    // Debounced search handler
    const debouncedSearch = useDebounce(() => {
      if (!validateSearchParams()) {
        return;
      }

      isLoading.value = true;

      const searchParams = {
        term: searchTerm.value,
        ...(props.enableGeographicSearch && zipCode.value ? {
          zipCode: zipCode.value,
          radius: searchRadius.value
        } : {})
      };

      emit('search', searchParams);
      isLoading.value = false;
    }, 300);

    const handleSearch = () => {
      if (isValidSearch.value) {
        debouncedSearch();
      }
    };

    return {
      // State
      searchTerm,
      zipCode,
      searchRadius,
      isLoading,
      errorMessage,
      
      // Computed
      isValidSearch,
      
      // Methods
      handleSearch,
      validateSearchParams,
      validateZipCode
    };
  }
});
</script>

<style lang="scss" scoped>
.base-search {
  .geographic-search {
    border-top: 1px solid $grey-4;
    padding-top: 1rem;
  }
}
</style>