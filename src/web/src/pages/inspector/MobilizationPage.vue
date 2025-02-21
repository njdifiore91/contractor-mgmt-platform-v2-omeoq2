<template>
  <QPage class="q-pa-md">
    <!-- Page Header -->
    <div class="row items-center q-mb-lg">
      <div class="text-h5">Inspector Mobilization</div>
      <q-space />
      <QBtn
        flat
        icon="arrow_back"
        label="Back to Inspectors"
        @click="router.push('/inspectors')"
      />
    </div>

    <!-- Error Alert -->
    <QBanner
      v-if="error"
      class="bg-negative text-white q-mb-md"
      rounded
    >
      {{ error }}
      <template v-slot:action>
        <QBtn flat label="Dismiss" @click="error = null" />
      </template>
    </QBanner>

    <!-- Mobilization Form -->
    <MobilizationForm
      v-if="selectedInspector"
      v-model="mobilizationData"
      :loading="isLoading"
      @submit="handleMobilization"
    />

    <!-- No Inspector Selected Warning -->
    <QCard v-else class="q-pa-md">
      <div class="text-h6">No Inspector Selected</div>
      <div class="q-mt-sm">Please select an inspector from the inspector list first.</div>
      <QBtn
        class="q-mt-md"
        color="primary"
        label="Go to Inspector List"
        @click="router.push('/inspectors')"
      />
    </QCard>
  </QPage>
</template>

<script setup lang="ts">
// Vue imports - v3.3.0
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'

// Quasar imports - v2.12.0
import { QPage, QBanner, QBtn, QCard, useQuasar, useConfirm } from 'quasar'

// Internal imports
import MobilizationForm from '@/components/inspector/MobilizationForm.vue'
import { useInspectorStore } from '@/stores/inspector'
import { useEmailDialog } from '@/composables/useEmailDialog'
import type { InspectorMobilization } from '@/types/inspector'

// Initialize composables
const router = useRouter()
const $q = useQuasar()
const { selectedInspector, mobilizeInspector } = useInspectorStore()
const { sendEmailWithAttachments } = useEmailDialog()

// Component state
const isLoading = ref(false)
const error = ref<string | null>(null)
const mobilizationData = ref<InspectorMobilization | null>(null)

// Initialize page
onMounted(() => {
  if (!selectedInspector.value) {
    router.push('/inspectors')
    return
  }

  // Initialize mobilization data with inspector details
  mobilizationData.value = {
    employeeName: `${selectedInspector.value.firstName} ${selectedInspector.value.lastName}`,
    primaryEmail: selectedInspector.value.email,
    phone: selectedInspector.value.phone,
    dateOfBirth: selectedInspector.value.dateOfBirth,
    mobDate: new Date(),
    hireType: selectedInspector.value.hireType,
    hirePack: '',
    training: false,
    ispTrans: false,
    drugKit: false,
    daPool: false,
    project: '',
    customer: null,
    contract: '',
    department: selectedInspector.value.department,
    function: selectedInspector.value.function,
    type: '',
    location: selectedInspector.value.projectLocation,
    classification: selectedInspector.value.classification,
    certRequired: selectedInspector.value.certificationRequired,
    certsRequired: selectedInspector.value.requiredCertifications,
    addressType: 'HOME',
    shipOpt: 'STANDARD',
    projectContact: '',
    invoiceContact: ''
  }
})

// Handle mobilization submission
const handleMobilization = async (data: InspectorMobilization) => {
  try {
    // Show confirmation dialog
    const shouldProceed = await useConfirm({
      title: 'Confirm Mobilization',
      message: `Are you sure you want to mobilize ${data.employeeName}?`,
      ok: 'Yes, Mobilize',
      cancel: 'Cancel'
    })

    if (!shouldProceed) return

    isLoading.value = true
    error.value = null

    if (!selectedInspector.value) {
      throw new Error('No inspector selected')
    }

    // Attempt to mobilize inspector
    const result = await mobilizeInspector(selectedInspector.value.id, data)

    if (!result.success) {
      throw new Error(result.error)
    }

    // Send mobilization email notification
    await sendEmailWithAttachments({
      to: data.primaryEmail,
      subject: `Mobilization Confirmation - ${data.project}`,
      body: `Dear ${data.employeeName},\n\nYou have been mobilized to ${data.project} starting ${data.mobDate}.\n\nPlease review the attached documents and contact your project manager with any questions.`,
      templateName: 'inspector-mobilization',
      templateData: {
        inspectorName: data.employeeName,
        projectName: data.project,
        customerName: data.customer?.name,
        mobilizationDate: data.mobDate
      }
    })

    // Show success notification
    $q.notify({
      type: 'positive',
      message: `Successfully mobilized ${data.employeeName}`,
      position: 'top'
    })

    // Navigate back to inspector list
    router.push('/inspectors')

  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to mobilize inspector'
    
    $q.notify({
      type: 'negative',
      message: error.value,
      position: 'top'
    })
  } finally {
    isLoading.value = false
  }
}
</script>

<style lang="scss" scoped>
.q-page {
  max-width: 1200px;
  margin: 0 auto;
}
</style>