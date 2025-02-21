<template>
  <BaseForm
    ref="formRef"
    :fields="formFields"
    v-model="formData"
    :loading="isLoading"
    @submit="handleSubmit"
  >
    <template #default>
      <QCard class="q-pa-md">
        <!-- Employee Information Section -->
        <div class="text-h6 q-mb-md">Employee Information</div>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-md-6">
            <QInput
              v-model="formData.employeeName"
              label="Employee Name"
              :rules="[val => !!val || 'Employee name is required']"
              outlined
              dense
            />
          </div>
          <div class="col-12 col-md-6">
            <QInput
              v-model="formData.primaryEmail"
              label="Primary Email"
              type="email"
              :rules="[
                val => !!val || 'Email is required',
                val => /^[^@]+@[^@]+\.[^@]+$/.test(val) || 'Invalid email format'
              ]"
              outlined
              dense
            />
          </div>
          <div class="col-12 col-md-6">
            <QInput
              v-model="formData.phone"
              label="Phone"
              mask="(###) ###-####"
              outlined
              dense
            />
          </div>
          <div class="col-12 col-md-6">
            <QDate
              v-model="formData.dateOfBirth"
              label="Date of Birth"
              outlined
              dense
            />
          </div>
        </div>

        <!-- Hire Information Section -->
        <div class="text-h6 q-mt-lg q-mb-md">Hire Information</div>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-md-6">
            <QDate
              v-model="formData.mobDate"
              label="Mobilization Date"
              :rules="[val => !!val || 'Mobilization date is required']"
              outlined
              dense
            />
          </div>
          <div class="col-12 col-md-6">
            <QSelect
              v-model="formData.hireType"
              :options="hireTypeOptions"
              label="Hire Type"
              outlined
              dense
            />
          </div>
          <div class="col-12 col-md-6">
            <QSelect
              v-model="formData.hirePack"
              :options="hirePackOptions"
              label="Hire Pack"
              outlined
              dense
            />
          </div>
        </div>

        <!-- Project Assignment Section -->
        <div class="text-h6 q-mt-lg q-mb-md">Project Assignment</div>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-md-6">
            <QSelect
              v-model="formData.project"
              :options="projectOptions"
              label="Project"
              :rules="[val => !!val || 'Project is required']"
              outlined
              dense
            />
          </div>
          <div class="col-12 col-md-6">
            <QSelect
              v-model="formData.customer"
              :options="customerOptions"
              label="Customer"
              :rules="[val => !!val || 'Customer is required']"
              outlined
              dense
            />
          </div>
          <div class="col-12 col-md-6">
            <QSelect
              v-model="formData.contract"
              :options="contractOptions"
              label="Contract"
              outlined
              dense
            />
          </div>
        </div>

        <!-- Classification Section -->
        <div class="text-h6 q-mt-lg q-mb-md">Classification</div>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-md-6">
            <QSelect
              v-model="formData.department"
              :options="departmentOptions"
              label="Department"
              outlined
              dense
            />
          </div>
          <div class="col-12 col-md-6">
            <QSelect
              v-model="formData.function"
              :options="functionOptions"
              label="Function"
              outlined
              dense
            />
          </div>
          <div class="col-12 col-md-6">
            <QSelect
              v-model="formData.type"
              :options="typeOptions"
              label="Type"
              outlined
              dense
            />
          </div>
          <div class="col-12 col-md-6">
            <QInput
              v-model="formData.location"
              label="Location"
              outlined
              dense
            />
          </div>
        </div>

        <!-- Certification Section -->
        <div class="text-h6 q-mt-lg q-mb-md">Certifications</div>
        <div class="row q-col-gutter-md">
          <div class="col-12">
            <QSelect
              v-model="formData.certsRequired"
              :options="certificationOptions"
              label="Certifications Required"
              multiple
              outlined
              dense
            />
          </div>
        </div>

        <!-- Contact Information Section -->
        <div class="text-h6 q-mt-lg q-mb-md">Contact Information</div>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-md-6">
            <QSelect
              v-model="formData.projectContact"
              :options="contactOptions"
              label="Project Contact"
              outlined
              dense
            />
          </div>
          <div class="col-12 col-md-6">
            <QSelect
              v-model="formData.invoiceContact"
              :options="contactOptions"
              label="Invoice Contact"
              outlined
              dense
            />
          </div>
        </div>
      </QCard>
    </template>
  </BaseForm>
</template>

<script setup lang="ts">
// Vue imports - v3.3.0
import { ref, computed, onMounted } from 'vue'

// Quasar imports - v2.12.0
import { QCard, QSelect, QDate, QInput, QBtn } from 'quasar'

// Internal imports
import BaseForm from '@/components/common/BaseForm.vue'
import { useEmailDialog } from '@/composables/useEmailDialog'
import { sendMobilizationNotification } from '@/services/email'

// Props and emits
const props = defineProps<{
  inspectorId: number
}>()

const emit = defineEmits<{
  (e: 'submit', value: any): void
  (e: 'success', value: any): void
  (e: 'error', value: string): void
}>()

// Component state
const formRef = ref<InstanceType<typeof BaseForm> | null>(null)
const isLoading = ref(false)
const formData = ref({
  employeeName: '',
  primaryEmail: '',
  phone: '',
  dateOfBirth: '',
  mobDate: '',
  hireType: '',
  hirePack: '',
  training: false,
  ispTrans: false,
  drugKit: false,
  daPool: false,
  project: '',
  customer: '',
  contract: '',
  department: '',
  function: '',
  type: '',
  location: '',
  classification: '',
  certRequired: false,
  certsRequired: [],
  addressType: '',
  shipOpt: '',
  projectContact: '',
  invoiceContact: ''
})

// Email dialog composable
const { openEmailDialog } = useEmailDialog()

// Form validation and submission
const handleSubmit = async () => {
  try {
    isLoading.value = true
    
    // Validate form
    const isValid = await formRef.value?.validate()
    if (!isValid) return

    // Emit submit event with form data
    emit('submit', formData.value)

    // Send mobilization notification
    await sendMobilizationNotification(
      formData.value.primaryEmail,
      formData.value.project,
      formData.value.customer,
      new Date(formData.value.mobDate)
    )

    // Open email dialog for additional notifications
    await openEmailDialog({
      to: formData.value.primaryEmail,
      subject: `Mobilization Confirmation - ${formData.value.project}`,
      templateName: 'inspector-mobilization',
      templateData: {
        inspectorName: formData.value.employeeName,
        projectName: formData.value.project,
        mobilizationDate: formData.value.mobDate
      }
    })

    emit('success', formData.value)
  } catch (error) {
    console.error('Mobilization error:', error)
    emit('error', error instanceof Error ? error.message : 'Mobilization failed')
  } finally {
    isLoading.value = false
  }
}

// Form options (these would typically come from API/store)
const hireTypeOptions = ['Full-Time', 'Contract', 'Temporary']
const hirePackOptions = ['Standard', 'Extended', 'Custom']
const projectOptions = []
const customerOptions = []
const contractOptions = []
const departmentOptions = []
const functionOptions = []
const typeOptions = []
const certificationOptions = []
const contactOptions = []

// Initialize form
onMounted(async () => {
  // Here you would typically fetch any required data
  // such as projects, customers, etc.
})
</script>

<style lang="scss" scoped>
.base-form {
  &__field {
    margin-bottom: 1rem;
  }
}
</style>