// @quasar/extras v1.16.0
// quasar v2.12.0
// vue v3.3.0

import { ComponentCustomProperties, App } from 'vue'

// Module augmentation for Vue to include Quasar types
declare module 'vue' {
  interface ComponentCustomProperties {
    $q: Quasar.QuasarInstance
  }
}

// Module declaration for Quasar components, plugins, directives and utilities
declare module 'quasar' {
  export interface QuasarLanguage {
    isoName: string
    nativeName: string
    label: Record<string, string>
  }

  export interface QuasarIconSet {
    name: string
    type: 'material-icons' | 'material-icons-outlined' | 'fontawesome'
  }

  // Global Quasar configuration options
  export interface QuasarPluginOptions {
    config?: {
      brand?: {
        primary?: string
        secondary?: string
        accent?: string
        dark?: string
        positive?: string
        negative?: string
        info?: string
        warning?: string
      }
      notify?: object
      loading?: object
      loadingBar?: object
      // Add other config options
    }
    lang?: QuasarLanguage
    iconSet?: QuasarIconSet
    animations?: string[]
  }

  // Form validation rules interface
  export interface QuasarFormValidationRules {
    required?: boolean | string
    email?: boolean | string
    minLength?: number | [number, string]
    maxLength?: number | [number, string]
    pattern?: RegExp | [RegExp, string]
    numeric?: boolean | string
    // Add custom validation rules
    dateFormat?: boolean | string
    phoneFormat?: boolean | string
    zipCode?: boolean | string
  }

  // Table component with virtual scroll
  export interface QTableProps {
    virtualScroll?: boolean
    virtualScrollSliceSize?: number
    virtualScrollItemSize?: number
    rows?: Array<any>
    columns?: Array<{
      name: string
      label: string
      field: string | ((row: any) => any)
      required?: boolean
      align?: 'left' | 'right' | 'center'
      sortable?: boolean
      sort?: (a: any, b: any) => number
      format?: (val: any) => any
    }>
    loading?: boolean
    filter?: string
    selected?: Array<any>
  }

  // Layout component for mobile optimization
  export interface QLayoutProps {
    view?: string
    container?: boolean
    mobileBreakpoint?: number | string
    mobileView?: boolean
    leftDrawerWidth?: number
    leftDrawerBehavior?: 'default' | 'desktop' | 'mobile'
    rightDrawerWidth?: number
    rightDrawerBehavior?: 'default' | 'desktop' | 'mobile'
  }

  // Dialog component
  export interface QDialogProps {
    modelValue?: boolean
    persistent?: boolean
    maximized?: boolean
    transitionShow?: string
    transitionHide?: string
    fullWidth?: boolean
    fullHeight?: boolean
    position?: 'standard' | 'top' | 'right' | 'bottom' | 'left'
    seamless?: boolean
  }

  // Form component
  export interface QFormProps {
    autofocus?: boolean
    noErrorFocus?: boolean
    noResetFocus?: boolean
    greedy?: boolean
    validationRules?: QuasarFormValidationRules
  }

  // Form methods
  export interface QFormMethods {
    validate: () => Promise<boolean>
    reset: () => void
    resetValidation: () => void
    focus: () => void
  }

  // Quasar namespace for global utilities
  export namespace Quasar {
    interface QuasarInstance {
      plugins: Record<string, any>
      config: QuasarPluginOptions
      lang: QuasarLanguage
      iconSet: QuasarIconSet
      platform: {
        is: {
          mobile: boolean
          desktop: boolean
          android: boolean
          ios: boolean
        }
      }
      screen: {
        width: number
        height: number
        sizes: {
          sm: boolean
          md: boolean
          lg: boolean
          xl: boolean
        }
      }
    }

    // Plugin installation
    export function install(app: App, options?: QuasarPluginOptions): void
  }

  // Component exports
  export const QTable: (props: QTableProps) => JSX.Element
  export const QLayout: (props: QLayoutProps) => JSX.Element
  export const QDialog: (props: QDialogProps) => JSX.Element
  export const QForm: (props: QFormProps & QFormMethods) => JSX.Element
}

// Additional type declarations for form components used across modules
declare module 'quasar' {
  // Admin module components
  export interface QAdminFields {
    quickLinks: {
      label: string
      link: string
      order?: number
    }
    codeTypes: {
      typeName: string
      codes: Array<{
        code: string
        description: string
        expireable: boolean
      }>
    }
    users: {
      first: string
      last: string
      email: string
      confirmed: boolean
      roles: string[]
      emails: string[]
    }
  }

  // Customer module components
  export interface QCustomerFields {
    customer: {
      name: string
      code: string
      contacts: Array<any>
      contracts: Array<any>
    }
    contact: {
      first: string
      middle?: string
      last: string
      suffix?: string
      nickname?: string
      deceased?: boolean
      inactive?: boolean
      rating?: number
      jobTitle?: string
      birthday?: string
    }
  }

  // Equipment module components
  export interface QEquipmentFields {
    equipment: {
      model: string
      serialNumber: string
      description: string
      condition: string
      out?: boolean
      returnedOn?: string
      returnedCondition?: string
    }
  }

  // Inspector module components
  export interface QInspectorFields {
    drugTest: {
      testDate: string
      testType: string
      frequency: string
      result: string
      comment?: string
    }
    mobilization: {
      employeeName: string
      primaryEmail: string
      phone: string
      dateOfBirth: string
      mobDate: string
      hireType: string
      projectDetails: {
        customer: string
        contract: string
        department: string
        location: string
      }
    }
    demobilization: {
      demobReason: string
      demobDate: string
      note?: string
    }
  }
}