<template>
  <div class="inspector-search">
    <!-- Base search component with geographic search enabled -->
    <BaseSearch
      ref="baseSearchRef"
      :search-fields="searchFields"
      :enable-geographic-search="true"
      placeholder="Search inspectors by name or ID..."
      @search="handleBaseSearch"
    />

    <!-- Advanced filters section -->
    <div class="filters-section q-mt-md">
      <div class="row q-col-gutter-md">
        <!-- Specialties filter -->
        <div class="col-xs-12 col-sm-6">
          <QSelect
            v-model="selectedSpecialtiesModel"
            :options="specialtiesOptions"
            label="Specialties"
            multiple
            outlined
            dense
            use-chips
            @update:model-value="updateFilters('specialties', $event)"
          />
        </div>

        <!-- Certifications filter -->
        <div class="col-xs-12 col-sm-6">
          <QSelect
            v-model="selectedCertificationsModel"
            :options="certificationsOptions"
            label="Certifications"
            multiple
            outlined
            dense
            use-chips
            @update:model-value="updateFilters('certifications', $event)"
          />
        </div>

        <!-- Status filter -->
        <div class="col-xs-12 col-sm-6">
          <QSelect
            v-model="selectedStatus"
            :options="Object.values(InspectorStatus)"
            label="Status"
            outlined
            dense
            emit-value
            map-options
            @update:model-value="updateFilters('status', $event)"
          />
        </div>

        <!-- Available only toggle -->
        <div class="col-xs-12 col-sm-6 q-pt-md">
          <QToggle
            v-model="availableOnly"
            label="Show Available Inspectors Only"
            @update:model-value="updateFilters('availableOnly', $event)"
          />
        </div>
      </div>
    </div>

    <!-- Loading state -->
    <div v-if="isLoading" class="q-pa-md text-center">
      <QSpinner color="primary" size="2em" />
      <div class="q-mt-sm">Searching for inspectors...</div>
    </div>

    <!-- Error state -->
    <div v-if="error" class="q-pa-md text-negative">
      {{ error }}
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, computed } from 'vue';
import { useDebounce } from '@vueuse/core'; // ^10.0.0
import { QSelect, QToggle, QSpinner } from 'quasar'; // ^2.12.0
import BaseSearch from '../common/BaseSearch.vue';
import { searchInspectors } from '@/services/api/inspector';
import { InspectorStatus, type InspectorSearch, type Inspector } from '@/types/inspector';
import type { ListField } from '@/types/common';

export default defineComponent({
  name: 'InspectorSearch',

  components: {
    BaseSearch,
    QSelect,
    QToggle,
    QSpinner
  },

  props: {
    selectedSpecialties: {
      type: Array as () => Array<string>,
      default: () => []
    },
    selectedCertifications: {
      type: Array as () => Array<string>,
      default: () => []
    },
    autoSearch: {
      type: Boolean,
      default: true
    }
  },

  emits: ['searchResults', 'searchError'],

  setup(props, { emit }) {
    // Refs
    const baseSearchRef = ref<InstanceType<typeof BaseSearch> | null>(null);
    const isLoading = ref(false);
    const error = ref<string | null>(null);
    const selectedSpecialtiesModel = ref(props.selectedSpecialties);
    const selectedCertificationsModel = ref(props.selectedCertifications);
    const selectedStatus = ref<InspectorStatus | null>(null);
    const availableOnly = ref(false);

    // Search fields configuration
    const searchFields = ref<Array<ListField>>([
      { field: 'status', label: 'Status', sortable: true, visible: true, width: '100px', formatter: null, align: 'left' },
      { field: 'firstName', label: 'First Name', sortable: true, visible: true, width: '120px', formatter: null, align: 'left' },
      { field: 'lastName', label: 'Last Name', sortable: true, visible: true, width: '120px', formatter: null, align: 'left' },
      { field: 'state', label: 'State', sortable: true, visible: true, width: '80px', formatter: null, align: 'left' },
      { field: 'title', label: 'Title', sortable: true, visible: true, width: '150px', formatter: null, align: 'left' },
      { field: 'specialties', label: 'Specialties', sortable: true, visible: true, width: '200px', formatter: null, align: 'left' },
      { field: 'hasIssues', label: 'Issues', sortable: true, visible: true, width: '100px', formatter: null, align: 'center' },
      { field: 'needsApproval', label: 'Approval Needed', sortable: true, visible: true, width: '130px', formatter: null, align: 'center' },
      { field: 'email', label: 'Email', sortable: true, visible: true, width: '200px', formatter: null, align: 'left' },
      { field: 'inspectorId', label: 'Inspector ID', sortable: true, visible: true, width: '120px', formatter: null, align: 'left' }
    ]);

    // Mock options for demo - in production these would come from an API
    const specialtiesOptions = ref([
      'Welding',
      'Electrical',
      'Mechanical',
      'Civil',
      'Structural'
    ]);

    const certificationsOptions = ref([
      'AWS',
      'API',
      'ASME',
      'NACE',
      'PE'
    ]);

    // Debounced search handler
    const debouncedSearch = useDebounce(async (searchParams: InspectorSearch) => {
      try {
        isLoading.value = true;
        error.value = null;
        const results = await searchInspectors(searchParams);
        emit('searchResults', results);
      } catch (err) {
        const errorMessage = err instanceof Error ? err.message : 'An error occurred during search';
        error.value = errorMessage;
        emit('searchError', errorMessage);
      } finally {
        isLoading.value = false;
      }
    }, 300);

    // Methods
    const handleBaseSearch = async (baseParams: { term: string; zipCode?: string; radius?: number }) => {
      const searchParams: InspectorSearch = {
        zipCode: baseParams.zipCode || '',
        radius: baseParams.radius || 50,
        specialties: selectedSpecialtiesModel.value,
        certifications: selectedCertificationsModel.value,
        availableOnly: availableOnly.value,
        status: selectedStatus.value || InspectorStatus.ACTIVE,
        locations: [],
        classifications: [],
        hasValidDrugTest: true
      };

      await debouncedSearch(searchParams);
    };

    const updateFilters = (filterType: string, value: any) => {
      switch (filterType) {
        case 'specialties':
          selectedSpecialtiesModel.value = value;
          break;
        case 'certifications':
          selectedCertificationsModel.value = value;
          break;
        case 'status':
          selectedStatus.value = value;
          break;
        case 'availableOnly':
          availableOnly.value = value;
          break;
      }

      if (props.autoSearch && baseSearchRef.value) {
        baseSearchRef.value.handleSearch();
      }
    };

    return {
      // Refs
      baseSearchRef,
      isLoading,
      error,
      selectedSpecialtiesModel,
      selectedCertificationsModel,
      selectedStatus,
      availableOnly,
      searchFields,
      specialtiesOptions,
      certificationsOptions,

      // Enums
      InspectorStatus,

      // Methods
      handleBaseSearch,
      updateFilters
    };
  }
});
</script>

<style lang="scss" scoped>
.inspector-search {
  .filters-section {
    border-top: 1px solid $grey-4;
    padding-top: 1rem;
  }
}
</style>