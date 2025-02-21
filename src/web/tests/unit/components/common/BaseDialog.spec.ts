// vitest@^0.29.0
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'

// @vue/test-utils@^2.3.0
import { mount, VueWrapper } from '@vue/test-utils'

// quasar@^2.0.0
import { Quasar, QDialog } from 'quasar'

// Internal imports
import BaseDialog from '@/components/common/BaseDialog.vue'

let wrapper: VueWrapper<any>

// Mock Quasar plugin and notifications
const mockNotify = vi.fn()
const mockDialog = vi.fn()
vi.mock('quasar', () => ({
  useQuasar: () => ({
    notify: mockNotify,
    dialog: mockDialog,
    screen: {
      lt: {
        sm: false
      }
    }
  })
}))

const defaultProps = {
  modelValue: false,
  title: 'Test Dialog',
  persistent: false,
  width: '500px',
  loading: false,
  confirmText: 'Save',
  cancelText: 'Cancel'
}

const createWrapper = (props = {}, slots = {}) => {
  return mount(BaseDialog, {
    props: { ...defaultProps, ...props },
    slots,
    global: {
      plugins: [Quasar],
      stubs: {
        QDialog: {
          template: '<div><slot></slot></div>',
          emits: ['update:modelValue', 'hide', 'show']
        }
      }
    }
  })
}

describe('BaseDialog', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    if (wrapper) {
      wrapper.unmount()
    }
  })

  afterEach(() => {
    wrapper?.unmount()
    vi.clearAllMocks()
  })

  it('should mount with default props', () => {
    wrapper = createWrapper()
    expect(wrapper.exists()).toBe(true)
    expect(wrapper.props()).toEqual(defaultProps)
  })

  it('should show/hide based on modelValue prop', async () => {
    wrapper = createWrapper()
    expect(wrapper.vm.isDialogOpen).toBe(false)
    
    await wrapper.setProps({ modelValue: true })
    expect(wrapper.vm.isDialogOpen).toBe(true)
    
    await wrapper.setProps({ modelValue: false })
    expect(wrapper.vm.isDialogOpen).toBe(false)
  })

  it('should emit update:modelValue when closed', async () => {
    wrapper = createWrapper({ modelValue: true })
    await wrapper.vm.close()
    
    expect(wrapper.emitted('update:modelValue')).toBeTruthy()
    expect(wrapper.emitted('update:modelValue')[0]).toEqual([false])
  })

  it('should handle confirm action', async () => {
    wrapper = createWrapper({ modelValue: true })
    await wrapper.find('[data-cy="dialog-confirm-btn"]').trigger('click')
    
    expect(wrapper.emitted('confirm')).toBeTruthy()
    expect(wrapper.emitted('update:modelValue')).toBeTruthy()
    expect(wrapper.emitted('update:modelValue')[0]).toEqual([false])
  })

  it('should handle cancel action', async () => {
    wrapper = createWrapper({ modelValue: true })
    await wrapper.find('[data-cy="dialog-cancel-btn"]').trigger('click')
    
    expect(wrapper.emitted('cancel')).toBeTruthy()
    expect(wrapper.emitted('update:modelValue')).toBeTruthy()
    expect(wrapper.emitted('update:modelValue')[0]).toEqual([false])
  })

  it('should respect persistent prop', async () => {
    wrapper = createWrapper({ modelValue: true, persistent: true })
    await wrapper.find('[data-cy="dialog-confirm-btn"]').trigger('click')
    
    expect(wrapper.emitted('confirm')).toBeTruthy()
    expect(wrapper.emitted('update:modelValue')).toBeFalsy()
  })

  it('should handle loading state', async () => {
    wrapper = createWrapper({ loading: true })
    
    const cancelBtn = wrapper.find('[data-cy="dialog-cancel-btn"]')
    const confirmBtn = wrapper.find('[data-cy="dialog-confirm-btn"]')
    
    expect(cancelBtn.attributes('disable')).toBeDefined()
    expect(confirmBtn.attributes('loading')).toBeDefined()
  })

  it('should render custom title', () => {
    const customTitle = 'Custom Dialog Title'
    wrapper = createWrapper({ title: customTitle })
    
    expect(wrapper.find('.text-h6').text()).toBe(customTitle)
  })

  it('should apply custom width', () => {
    const customWidth = '800px'
    wrapper = createWrapper({ width: customWidth })
    
    expect(wrapper.find('.q-card').attributes('style')).toContain(`width: ${customWidth}`)
  })

  it('should render slot content', () => {
    const slots = {
      default: '<div class="test-content">Test Content</div>',
      header: '<div class="test-header">Custom Header</div>',
      actions: '<div class="test-actions">Custom Actions</div>'
    }
    
    wrapper = createWrapper({}, slots)
    
    expect(wrapper.find('.test-content').exists()).toBe(true)
    expect(wrapper.find('.test-header').exists()).toBe(true)
    expect(wrapper.find('.test-actions').exists()).toBe(true)
  })

  it('should handle error during confirm action', async () => {
    wrapper = createWrapper()
    const error = new Error('Save failed')
    
    wrapper.vm.$emit = vi.fn().mockRejectedValueOnce(error)
    await wrapper.find('[data-cy="dialog-confirm-btn"]').trigger('click')
    
    expect(mockNotify).toHaveBeenCalledWith({
      type: 'negative',
      message: 'An error occurred while saving. Please try again.'
    })
  })

  it('should handle unsaved changes confirmation', async () => {
    wrapper = createWrapper({ modelValue: true })
    wrapper.vm.hasUnsavedChanges = true
    
    mockDialog.mockImplementationOnce(() => ({
      onOk: (fn: Function) => fn(),
      onCancel: vi.fn()
    }))
    
    await wrapper.vm.close()
    
    expect(mockDialog).toHaveBeenCalledWith({
      title: 'Unsaved Changes',
      message: 'You have unsaved changes. Are you sure you want to close?',
      cancel: true,
      persistent: true
    })
  })

  it('should manage body overflow on show/hide', async () => {
    wrapper = createWrapper()
    
    await wrapper.vm.handleShow()
    expect(document.body.style.overflow).toBe('hidden')
    
    await wrapper.vm.handleHide()
    expect(document.body.style.overflow).toBe('')
  })

  it('should handle responsive layout', async () => {
    vi.mock('quasar', () => ({
      useQuasar: () => ({
        screen: {
          lt: {
            sm: true
          }
        }
      })
    }))
    
    wrapper = createWrapper()
    expect(wrapper.find('.q-card').attributes('style')).toContain('width: 100%')
  })
})