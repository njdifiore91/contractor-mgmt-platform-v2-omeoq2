<template>
  <q-dialog
    v-model="isDialogOpen"
    :persistent="persistent"
    @hide="handleHide"
    @show="handleShow"
    :maximized="$q.screen.lt.sm"
    transition-show="scale"
    transition-hide="scale"
  >
    <q-card :style="{ width: $q.screen.lt.sm ? '100%' : width }">
      <!-- Header Section -->
      <q-card-section class="row items-center q-pb-none">
        <slot name="header">
          <div class="text-h6">{{ title }}</div>
        </slot>
        <q-space />
        <q-btn
          v-if="!persistent"
          icon="close"
          flat
          round
          dense
          @click="handleCancel"
          :aria-label="$q.lang.label.close"
        />
      </q-card-section>

      <!-- Content Section -->
      <q-card-section class="q-pt-md">
        <slot></slot>
      </q-card-section>

      <!-- Actions Section -->
      <q-card-actions align="right" class="q-pa-md">
        <slot name="actions">
          <q-btn
            :label="cancelText"
            color="grey-7"
            flat
            :disable="loading"
            @click="handleCancel"
            data-cy="dialog-cancel-btn"
          />
          <q-btn
            :label="confirmText"
            color="primary"
            :loading="loading"
            @click="handleConfirm"
            data-cy="dialog-confirm-btn"
          />
        </slot>
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script>
// vue@^3.2.0
import { ref, watch, nextTick } from 'vue'

// quasar@^2.0.0
import { QDialog, QCard, QCardSection, QCardActions, QBtn, useQuasar } from 'quasar'

export default {
  name: 'BaseDialog',
  
  components: {
    QDialog,
    QCard,
    QCardSection,
    QCardActions,
    QBtn
  },

  props: {
    modelValue: {
      type: Boolean,
      required: true,
      default: false
    },
    title: {
      type: String,
      required: false,
      default: ''
    },
    persistent: {
      type: Boolean,
      required: false,
      default: false
    },
    width: {
      type: String,
      required: false,
      default: '500px'
    },
    loading: {
      type: Boolean,
      required: false,
      default: false
    },
    confirmText: {
      type: String,
      required: false,
      default: 'Save'
    },
    cancelText: {
      type: String,
      required: false,
      default: 'Cancel'
    }
  },

  emits: ['update:modelValue', 'confirm', 'cancel'],

  setup(props, { emit }) {
    const $q = useQuasar()
    const isDialogOpen = ref(props.modelValue)
    const hasUnsavedChanges = ref(false)
    const focusableElements = ref(null)
    
    // Watch for external modelValue changes
    watch(() => props.modelValue, (newVal) => {
      isDialogOpen.value = newVal
    })

    // Watch for internal dialog state changes
    watch(isDialogOpen, (newVal) => {
      emit('update:modelValue', newVal)
    })

    const open = async () => {
      if (isDialogOpen.value) return
      
      // Reset state
      hasUnsavedChanges.value = false
      isDialogOpen.value = true
      
      // Set focus on first focusable element after render
      await nextTick()
      focusableElements.value = document.querySelectorAll(
        'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
      )
      if (focusableElements.value.length) {
        focusableElements.value[0].focus()
      }
    }

    const close = async () => {
      if (hasUnsavedChanges.value) {
        const shouldClose = await showConfirmationDialog()
        if (!shouldClose) return
      }
      
      // Cleanup and close
      focusableElements.value = null
      hasUnsavedChanges.value = false
      isDialogOpen.value = false
    }

    const handleConfirm = async () => {
      try {
        // Emit confirm event and wait for parent handling
        emit('confirm')
        
        // Close dialog if not persistent
        if (!props.persistent) {
          await close()
        }
      } catch (error) {
        $q.notify({
          type: 'negative',
          message: 'An error occurred while saving. Please try again.'
        })
      }
    }

    const handleCancel = async () => {
      await close()
      emit('cancel')
    }

    const handleShow = () => {
      document.body.style.overflow = 'hidden'
    }

    const handleHide = () => {
      document.body.style.overflow = ''
    }

    const showConfirmationDialog = () => {
      return new Promise((resolve) => {
        $q.dialog({
          title: 'Unsaved Changes',
          message: 'You have unsaved changes. Are you sure you want to close?',
          cancel: true,
          persistent: true
        }).onOk(() => {
          resolve(true)
        }).onCancel(() => {
          resolve(false)
        })
      })
    }

    // Expose public methods
    return {
      isDialogOpen,
      open,
      close,
      handleConfirm,
      handleCancel,
      handleShow,
      handleHide
    }
  }
}
</script>

<style lang="scss" scoped>
.q-dialog {
  :deep(.q-card) {
    max-height: 90vh;
    display: flex;
    flex-direction: column;
    
    .q-card__section {
      &:not(:first-of-type) {
        overflow-y: auto;
      }
    }
  }
}
</style>