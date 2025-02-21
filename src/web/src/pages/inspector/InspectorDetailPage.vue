<template>
  <div class="inspector-detail">
    <!-- Loading state -->
    <div v-if="loading" class="inspector-detail__loading">
      <QSpinner size="3em" color="primary" />
      <span>Loading inspector details...</span>
    </div>

    <!-- Error state -->
    <div v-else-if="error" class="inspector-detail__error" role="alert">
      {{ error }}
    </div>

    <!-- Content -->
    <div v-else class="inspector-detail__content">
      <!-- Inspector header -->
      <QCard class="inspector-detail__header">
        <div class="inspector-detail__title">
          <h1>{{ inspectorFullName }}</h1>
          <div class="inspector-detail__status" :class="statusClass">
            {{ inspector?.status }}
          </div>
        </div>
        <div class="inspector-detail__actions">
          <QBtn
            v-if="canMobilize"
            color="primary"
            label="Mobilize"
            :disable="isMobilized"
            @click="handleMobilize"
          />
          <QBtn
            v-if="canDemobilize"
            color="negative"
            label="Demobilize"
            :disable="!isMobilized"
            @click="handleDemobilize"
          />
        </div>
      </QCard>

      <!-- Inspector details tabs -->
      <QCard class="inspector-detail__tabs">
        <QTabs
          v-model="activeTab"
          class="text-primary"
          active-color="primary"
          indicator-color="primary"
        >
          <QTab name="personal" label="Personal Info" />
          <QTab 
            v-if="canEditDrugTests" 
            name="drugTests" 
            label="Drug Tests" 
          />
          <QTab 
            v-if="canManageEquipment" 
            name="equipment" 
            label="Equipment" 
          />
          <QTab name="mobilization" label="Mobilization History" />
        </QTabs>

        <QTabPanels v-model="activeTab" animated>
          <!-- Personal Info Panel -->
          <QTabPanel name="personal">
            <div class="inspector-detail__personal">
              <div class="info-row">
                <span class="label">Inspector ID:</span>
                <span>{{ inspector?.inspectorId }}</span>
              </div>
              <div class="info-row">
                <span class="label">Email:</span>
                <span>{{ inspector?.email }}</span>
              </div>
              <div class="info-row">
                <span class="label">Phone:</span>
                <span>{{ inspector?.phone }}</span>
              </div>
              <div class="info-row">
                <span class="label">State:</span>
                <span>{{ inspector?.state }}</span>
              </div>
              <div class="info-row">
                <span class="label">Classification:</span>
                <span>{{ inspector?.classification }}</span>
              </div>
              <div class="info-row">
                <span class="label">Specialties:</span>
                <span>{{ inspector?.specialties?.join(', ') }}</span>
              </div>
            </div>
          </QTabPanel>

          <!-- Drug Tests Panel -->
          <QTabPanel name="drugTests" v-if="canEditDrugTests">
            <div class="inspector-detail__drug-tests">
              <QBtn
                color="primary"
                label="Add Drug Test"
                @click="showDrugTestForm = true"
              />
              
              <div class="drug-tests-list">
                <div v-for="test in sortedDrugTests" :key="test.id" class="drug-test-item">
                  <div class="test-date">{{ formatDate(test.testDate) }}</div>
                  <div class="test-type">{{ test.testType }}</div>
                  <div class="test-result" :class="test.result.toLowerCase()">
                    {{ test.result }}
                  </div>
                </div>
              </div>

              <!-- Drug Test Form Dialog -->
              <QDialog v-model="showDrugTestForm">
                <DrugTestForm
                  :inspector-id="inspector?.id"
                  @submit="handleDrugTestSubmit"
                  @cancel="showDrugTestForm = false"
                />
              </QDialog>
            </div>
          </QTabPanel>

          <!-- Equipment Panel -->
          <QTabPanel name="equipment" v-if="canManageEquipment">
            <div class="inspector-detail__equipment">
              <QBtn
                color="primary"
                label="Assign Equipment"
                @click="showEquipmentForm = true"
              />

              <div class="equipment-list">
                <div v-for="item in inspector?.equipmentAssignments" :key="item.id" class="equipment-item">
                  <div class="equipment-info">
                    <span class="model">{{ item.model }}</span>
                    <span class="serial">{{ item.serialNumber }}</span>
                  </div>
                  <div class="assignment-dates">
                    <div>Out: {{ formatDate(item.outDate) }}</div>
                    <div v-if="item.returnedDate">
                      Returned: {{ formatDate(item.returnedDate) }}
                    </div>
                  </div>
                  <div class="equipment-status" :class="item.status.toLowerCase()">
                    {{ item.status }}
                  </div>
                </div>
              </div>
            </div>
          </QTabPanel>

          <!-- Mobilization History Panel -->
          <QTabPanel name="mobilization">
            <div class="inspector-detail__mobilization">
              <div v-for="record in mobilizationHistory" :key="record.id" class="mobilization-record">
                <div class="mob-date">{{ formatDate(record.mobDate) }}</div>
                <div class="mob-details">
                  <div>Project: {{ record.project }}</div>
                  <div>Customer: {{ record.customer?.name }}</div>
                  <div>Location: {{ record.location }}</div>
                </div>
                <div class="mob-status" :class="record.status.toLowerCase()">
                  {{ record.status }}
                </div>
              </div>
            </div>
          </QTabPanel>
        </QTabPanels>
      </QCard>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { QCard, QTabs, QTab, QTabPanels, QTabPanel, QBtn, QSpinner, QDialog } from 'quasar'; // ^2.0.0
import { DrugTestForm } from '../../components/inspector/DrugTestForm';
import { usePermissions } from '../../composables/usePermissions';
import { Inspector, InspectorStatus, DrugTest } from '../../types/inspector';
import { getInspectorById, manageDrugTest, mobilizeInspector, demobilizeInspector } from '../../services/api/inspector';

// Component state
const route = useRoute();
const inspector = ref<Inspector | null>(null);
const loading = ref(true);
const error = ref<string | null>(null);
const activeTab = ref('personal');
const showDrugTestForm = ref(false);
const showEquipmentForm = ref(false);

// Permissions
const { checkPermission } = usePermissions();
const canEditDrugTests = computed(() => checkPermission('manage_drug_tests'));
const canManageEquipment = computed(() => checkPermission('edit_equipment'));
const canMobilize = computed(() => checkPermission('mobilize_inspector'));

// Computed properties
const inspectorFullName = computed(() => {
  if (!inspector.value) return '';
  return `${inspector.value.firstName} ${inspector.value.lastName}`;
});

const isMobilized = computed(() => {
  return inspector.value?.status === InspectorStatus.MOBILIZED;
});

const statusClass = computed(() => {
  return inspector.value?.status?.toLowerCase() || '';
});

const sortedDrugTests = computed(() => {
  return [...(inspector.value?.drugTests || [])].sort((a, b) => 
    new Date(b.testDate).getTime() - new Date(a.testDate).getTime()
  );
});

const mobilizationHistory = computed(() => {
  return inspector.value?.mobilizationHistory || [];
});

// Methods
const loadInspectorDetails = async () => {
  try {
    loading.value = true;
    error.value = null;
    const inspectorId = parseInt(route.params.id as string);
    inspector.value = await getInspectorById(inspectorId);
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to load inspector details';
  } finally {
    loading.value = false;
  }
};

const handleDrugTestSubmit = async (drugTest: DrugTest) => {
  try {
    if (!inspector.value?.id) return;
    await manageDrugTest(inspector.value.id, drugTest);
    await loadInspectorDetails();
    showDrugTestForm.value = false;
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to save drug test';
  }
};

const handleMobilize = async () => {
  try {
    if (!inspector.value?.id) return;
    const result = await mobilizeInspector(inspector.value.id, {
      employeeName: inspectorFullName.value,
      primaryEmail: inspector.value.email,
      phone: inspector.value.phone,
      dateOfBirth: inspector.value.dateOfBirth,
      mobDate: new Date(),
      // Additional mobilization details would be collected via form
    });
    if (result.success) {
      await loadInspectorDetails();
    }
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to mobilize inspector';
  }
};

const handleDemobilize = async () => {
  try {
    if (!inspector.value?.id) return;
    await demobilizeInspector(inspector.value.id, {
      demobDate: new Date(),
      demobReason: 'PROJECT_COMPLETE',
      note: ''
      // Additional demobilization details would be collected via form
    });
    await loadInspectorDetails();
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to demobilize inspector';
  }
};

const formatDate = (date: Date | string) => {
  return new Date(date).toLocaleDateString();
};

// Lifecycle hooks
onMounted(loadInspectorDetails);
</script>

<style lang="scss" scoped>
.inspector-detail {
  padding: 1rem;

  &__loading,
  &__error {
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: 200px;
    gap: 1rem;
  }

  &__error {
    color: var(--q-negative);
  }

  &__header {
    margin-bottom: 1rem;
    padding: 1rem;
  }

  &__title {
    display: flex;
    align-items: center;
    gap: 1rem;
    margin-bottom: 1rem;

    h1 {
      margin: 0;
      font-size: 1.5rem;
    }
  }

  &__status {
    padding: 0.25rem 0.5rem;
    border-radius: 4px;
    font-size: 0.875rem;
    text-transform: uppercase;

    &.active { background-color: var(--q-positive); color: white; }
    &.mobilized { background-color: var(--q-info); color: white; }
    &.inactive { background-color: var(--q-negative); color: white; }
  }

  &__actions {
    display: flex;
    gap: 1rem;
  }

  &__tabs {
    padding: 1rem;
  }

  &__personal {
    .info-row {
      display: flex;
      padding: 0.5rem 0;
      border-bottom: 1px solid rgba(0, 0, 0, 0.12);

      .label {
        width: 150px;
        font-weight: 500;
      }
    }
  }

  &__drug-tests,
  &__equipment {
    .drug-test-item,
    .equipment-item {
      display: flex;
      align-items: center;
      padding: 0.75rem;
      margin: 0.5rem 0;
      border: 1px solid rgba(0, 0, 0, 0.12);
      border-radius: 4px;
      gap: 1rem;
    }
  }

  &__mobilization {
    .mobilization-record {
      display: flex;
      align-items: center;
      padding: 0.75rem;
      margin: 0.5rem 0;
      border: 1px solid rgba(0, 0, 0, 0.12);
      border-radius: 4px;
      gap: 1rem;
    }
  }
}
</style>