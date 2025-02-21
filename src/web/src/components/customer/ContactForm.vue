<template>
  <BaseForm
    ref="formRef"
    :fields="formFields"
    :model-value="formData"
    @submit="handleSubmit"
  >
    <QCard class="contact-form">
      <QCardSection>
        <h2 class="text-h6">{{ t('contact.form.personalInfo') }}</h2>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-md-4">
            <QInput
              v-model="formData.firstName"
              :label="t('contact.form.firstName')"
              required
              :error="!!errors.firstName"
              :error-message="errors.firstName"
            />
          </div>
          <div class="col-12 col-md-4">
            <QInput
              v-model="formData.middleName"
              :label="t('contact.form.middleName')"
            />
          </div>
          <div class="col-12 col-md-4">
            <QInput
              v-model="formData.lastName"
              :label="t('contact.form.lastName')"
              required
              :error="!!errors.lastName"
              :error-message="errors.lastName"
            />
          </div>
          <div class="col-12 col-md-4">
            <QInput
              v-model="formData.suffix"
              :label="t('contact.form.suffix')"
            />
          </div>
          <div class="col-12 col-md-4">
            <QInput
              v-model="formData.nickname"
              :label="t('contact.form.nickname')"
            />
          </div>
          <div class="col-12 col-md-4">
            <QInput
              v-model="formData.jobTitle"
              :label="t('contact.form.jobTitle')"
              required
              :error="!!errors.jobTitle"
              :error-message="errors.jobTitle"
            />
          </div>
          <div class="col-12 col-md-4">
            <QDate
              v-model="formData.birthday"
              :label="t('contact.form.birthday')"
              mask="YYYY-MM-DD"
            />
          </div>
          <div class="col-12 col-md-4">
            <QRating
              v-model="formData.rating"
              :max="5"
              :label="t('contact.form.rating')"
            />
          </div>
          <div class="col-12 col-md-4">
            <QCheckbox
              v-model="formData.isInactive"
              :label="t('contact.form.inactive')"
            />
          </div>
          <div class="col-12 col-md-4">
            <QCheckbox
              v-model="formData.isDeceased"
              :label="t('contact.form.deceased')"
            />
          </div>
        </div>
      </QCardSection>

      <QCardSection>
        <h2 class="text-h6">{{ t('contact.form.addresses') }}</h2>
        <div v-for="(address, index) in formData.addresses" :key="index" class="address-entry q-mb-md">
          <div class="row q-col-gutter-md">
            <div class="col-12 col-md-4">
              <QInput
                v-model="address.type"
                :label="t('contact.form.addressType')"
                required
              />
            </div>
            <div class="col-12">
              <QInput
                v-model="address.line1"
                :label="t('contact.form.addressLine1')"
                required
              />
            </div>
            <div class="col-12">
              <QInput
                v-model="address.line2"
                :label="t('contact.form.addressLine2')"
              />
            </div>
            <div class="col-12">
              <QInput
                v-model="address.line3"
                :label="t('contact.form.addressLine3')"
              />
            </div>
            <div class="col-12 col-md-4">
              <QInput
                v-model="address.city"
                :label="t('contact.form.city')"
                required
              />
            </div>
            <div class="col-12 col-md-4">
              <QInput
                v-model="address.state"
                :label="t('contact.form.state')"
                required
              />
            </div>
            <div class="col-12 col-md-4">
              <QInput
                v-model="address.zip"
                :label="t('contact.form.zip')"
                required
              />
            </div>
            <div class="col-12 col-md-4">
              <QInput
                v-model="address.country"
                :label="t('contact.form.country')"
                required
              />
            </div>
          </div>
          <QBtn
            flat
            color="negative"
            :label="t('common.remove')"
            @click="removeAddress(index)"
          />
        </div>
        <QBtn
          color="primary"
          :label="t('contact.form.addAddress')"
          @click="addAddress"
        />
      </QCardSection>

      <QCardSection>
        <h2 class="text-h6">{{ t('contact.form.emails') }}</h2>
        <div v-for="(email, index) in formData.emails" :key="index" class="email-entry q-mb-md">
          <div class="row q-col-gutter-md">
            <div class="col-12 col-md-4">
              <QCheckbox
                v-model="email.primary"
                :label="t('contact.form.primaryEmail')"
              />
            </div>
            <div class="col-12 col-md-8">
              <QInput
                v-model="email.email"
                :label="t('contact.form.emailAddress')"
                type="email"
                required
              />
            </div>
          </div>
          <QBtn
            flat
            color="negative"
            :label="t('common.remove')"
            @click="removeEmail(index)"
          />
        </div>
        <QBtn
          color="primary"
          :label="t('contact.form.addEmail')"
          @click="addEmail"
        />
      </QCardSection>

      <QCardSection>
        <h2 class="text-h6">{{ t('contact.form.notes') }}</h2>
        <div v-for="(note, index) in formData.notes" :key="index" class="note-entry q-mb-md">
          <QInput
            v-model="note.note"
            type="textarea"
            :label="t('contact.form.noteText')"
            autogrow
          />
          <QBtn
            flat
            color="negative"
            :label="t('common.remove')"
            @click="removeNote(index)"
          />
        </div>
        <QBtn
          color="primary"
          :label="t('contact.form.addNote')"
          @click="addNote"
        />
      </QCardSection>
    </QCard>
  </BaseForm>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { QCard, QCardSection, QInput, QCheckbox, QDate, QRating, QBtn, QIcon } from 'quasar'; // v2.12.0
import { useI18n } from 'vue-i18n'; // v9.2.0
import BaseForm from '@/components/common/BaseForm.vue';
import type { Contact } from '@/types/customer';

const props = defineProps<{
  modelValue: Contact;
  customerId: number;
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', value: Contact): void;
  (e: 'submit', value: Contact): void;
  (e: 'error', value: string[]): void;
}>();

const { t } = useI18n();
const formRef = ref<InstanceType<typeof BaseForm> | null>(null);
const errors = ref<Record<string, string>>({});
const formData = ref<Contact>({
  id: 0,
  firstName: '',
  middleName: '',
  lastName: '',
  suffix: '',
  nickname: '',
  isDeceased: false,
  isInactive: false,
  rating: 0,
  jobTitle: '',
  birthday: null,
  dateCreated: new Date(),
  customerId: props.customerId,
  addresses: [],
  emails: [],
  phoneNumbers: [],
  notes: []
});

onMounted(() => {
  formData.value = { ...props.modelValue };
});

const addAddress = () => {
  formData.value.addresses.push({
    type: '',
    line1: '',
    line2: '',
    line3: '',
    city: '',
    state: '',
    zip: '',
    country: ''
  });
};

const removeAddress = (index: number) => {
  formData.value.addresses.splice(index, 1);
};

const addEmail = () => {
  formData.value.emails.push({
    primary: false,
    email: ''
  });
};

const removeEmail = (index: number) => {
  formData.value.emails.splice(index, 1);
};

const addNote = () => {
  formData.value.notes.push({
    note: '',
    dateCreated: new Date(),
    userId: 0 // Will be set by backend
  });
};

const removeNote = (index: number) => {
  formData.value.notes.splice(index, 1);
};

const validateAddress = (address: any): boolean => {
  return !!(
    address.line1 &&
    address.city &&
    address.state &&
    address.zip &&
    address.country
  );
};

const handleSubmit = async () => {
  errors.value = {};
  let hasErrors = false;

  // Validate required fields
  if (!formData.value.firstName) {
    errors.value.firstName = t('validation.required');
    hasErrors = true;
  }
  if (!formData.value.lastName) {
    errors.value.lastName = t('validation.required');
    hasErrors = true;
  }
  if (!formData.value.jobTitle) {
    errors.value.jobTitle = t('validation.required');
    hasErrors = true;
  }

  // Validate addresses
  for (const address of formData.value.addresses) {
    if (!validateAddress(address)) {
      hasErrors = true;
      emit('error', [t('validation.addressIncomplete')]);
      break;
    }
  }

  // Validate emails
  for (const email of formData.value.emails) {
    if (!email.email) {
      hasErrors = true;
      emit('error', [t('validation.emailRequired')]);
      break;
    }
  }

  if (hasErrors) {
    return;
  }

  emit('submit', formData.value);
};
</script>

<style lang="scss" scoped>
.contact-form {
  max-width: 1200px;
  margin: 0 auto;
}

.address-entry,
.email-entry,
.note-entry {
  border: 1px solid #ddd;
  border-radius: 4px;
  padding: 1rem;
  margin-bottom: 1rem;
  position: relative;
}

.text-h6 {
  margin-bottom: 1rem;
}
</style>