require('@rushstack/eslint-patch/modern-module-resolution') // @rushstack/eslint-patch@^1.3.0

module.exports = {
  root: true,
  extends: [
    'plugin:vue/vue3-essential', // eslint-plugin-vue@^9.11.0
    'eslint:recommended',
    '@vue/eslint-config-typescript', // @vue/eslint-config-typescript@^11.0.0
    '@vue/eslint-config-prettier', // @vue/eslint-config-prettier@^7.1.0
    'plugin:cypress/recommended' // eslint-plugin-cypress@^2.13.0
  ],
  parserOptions: {
    ecmaVersion: 'latest'
  },
  env: {
    browser: true,
    es2021: true,
    node: true,
    'vue/setup-compiler-macros': true
  },
  rules: {
    // Vue specific rules
    'vue/multi-word-component-names': 'error', // Enforce multi-word component names for better readability
    'vue/component-name-in-template-casing': ['error', 'PascalCase'], // Enforce PascalCase for component names in templates
    'vue/script-setup-uses-vars': 'error', // Prevent unused variables in <script setup>

    // TypeScript rules
    '@typescript-eslint/no-explicit-any': 'warn', // Warn on usage of 'any' type
    '@typescript-eslint/explicit-function-return-type': 'off', // Don't require explicit return types
    '@typescript-eslint/no-unused-vars': ['error', {
      'argsIgnorePattern': '^_' // Allow unused variables that start with underscore
    }],

    // Prettier integration
    'prettier/prettier': ['error', {
      'singleQuote': true, // Use single quotes
      'semi': false, // No semicolons
      'tabWidth': 2, // 2 spaces indentation
      'printWidth': 100 // Max line length
    }]
  },
  overrides: [
    // Cypress E2E testing files configuration
    {
      files: ['cypress/e2e/**/*.{cy,spec}.{js,ts,jsx,tsx}'],
      extends: ['plugin:cypress/recommended']
    },
    // Jest unit testing files configuration
    {
      files: [
        '**/__tests__/*.{j,t}s?(x)',
        '**/tests/unit/**/*.spec.{j,t}s?(x)'
      ],
      env: {
        jest: true
      }
    }
  ]
}