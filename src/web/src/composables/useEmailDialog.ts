// vue@^3.2.0
import { ref } from 'vue'

// quasar@^2.0.0
import { useQuasar } from 'quasar'

// Internal imports
import { sendEmail, sendTemplatedEmail } from '@/services/email'
import type BaseDialog from '@/components/common/BaseDialog.vue'

interface EmailDialogOptions {
  to?: string
  subject?: string
  body?: string
  attachments?: File[]
  templateName?: string
  templateData?: Record<string, unknown>
}

interface EmailDialogState {
  to: string
  subject: string
  body: string
  attachments: File[]
  templateName?: string
  templateData?: Record<string, unknown>
  isValid: boolean
  isDirty: boolean
}

export function useEmailDialog() {
  const $q = useQuasar()
  const dialogRef = ref<typeof BaseDialog | null>(null)
  
  // Form state
  const state = ref<EmailDialogState>({
    to: '',
    subject: '',
    body: '',
    attachments: [],
    isValid: false,
    isDirty: false
  })

  // Loading and error states
  const isLoading = ref(false)
  const hasError = ref(false)

  // Validation rules
  const emailRegex = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/

  const validateForm = (): boolean => {
    if (!state.value.to || !emailRegex.test(state.value.to)) {
      $q.notify({
        type: 'negative',
        message: 'Please enter a valid email address'
      })
      return false
    }

    if (!state.value.subject?.trim()) {
      $q.notify({
        type: 'negative',
        message: 'Subject is required'
      })
      return false
    }

    if (!state.value.body?.trim()) {
      $q.notify({
        type: 'negative',
        message: 'Email body is required'
      })
      return false
    }

    return true
  }

  const resetState = () => {
    state.value = {
      to: '',
      subject: '',
      body: '',
      attachments: [],
      isValid: false,
      isDirty: false
    }
    isLoading.value = false
    hasError.value = false
  }

  const openEmailDialog = async (options: EmailDialogOptions = {}) => {
    try {
      resetState()
      
      // Set initial values if provided
      if (options.to) state.value.to = options.to
      if (options.subject) state.value.subject = options.subject
      if (options.body) state.value.body = options.body
      if (options.attachments) state.value.attachments = options.attachments
      if (options.templateName) state.value.templateName = options.templateName
      if (options.templateData) state.value.templateData = options.templateData

      await dialogRef.value?.open()
    } catch (error) {
      console.error('Error opening email dialog:', error)
      $q.notify({
        type: 'negative',
        message: 'Failed to open email dialog'
      })
    }
  }

  const sendEmailWithAttachments = async (): Promise<boolean> => {
    try {
      if (!validateForm()) return false

      isLoading.value = true
      hasError.value = false

      const success = await sendEmail(
        state.value.to,
        state.value.subject,
        state.value.body,
        state.value.attachments
      )

      if (success) {
        $q.notify({
          type: 'positive',
          message: 'Email sent successfully'
        })
        await dialogRef.value?.close()
        resetState()
      }

      return success
    } catch (error) {
      console.error('Error sending email:', error)
      hasError.value = true
      $q.notify({
        type: 'negative',
        message: 'Failed to send email. Please try again.'
      })
      return false
    } finally {
      isLoading.value = false
    }
  }

  const sendTemplatedEmailWithAttachments = async (): Promise<boolean> => {
    try {
      if (!validateForm()) return false
      if (!state.value.templateName) {
        $q.notify({
          type: 'negative',
          message: 'Template name is required'
        })
        return false
      }

      isLoading.value = true
      hasError.value = false

      const success = await sendTemplatedEmail(
        state.value.to,
        state.value.templateName,
        state.value.templateData || {},
        state.value.attachments
      )

      if (success) {
        $q.notify({
          type: 'positive',
          message: 'Templated email sent successfully'
        })
        await dialogRef.value?.close()
        resetState()
      }

      return success
    } catch (error) {
      console.error('Error sending templated email:', error)
      hasError.value = true
      $q.notify({
        type: 'negative',
        message: 'Failed to send templated email. Please try again.'
      })
      return false
    } finally {
      isLoading.value = false
    }
  }

  return {
    // State
    state,
    isLoading,
    hasError,
    dialogRef,

    // Methods
    openEmailDialog,
    sendEmailWithAttachments,
    sendTemplatedEmailWithAttachments,
    validateForm,
    resetState
  }
}

export type EmailDialogComposable = ReturnType<typeof useEmailDialog>