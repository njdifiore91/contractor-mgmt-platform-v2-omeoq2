/**
 * @file Main application entry point
 * @version 1.0.0
 * Initializes Vue 3 application with Quasar Framework and enhanced security features
 */

// Vue and core dependencies
import { createApp } from 'vue'; // ^3.3.0
import { createPinia } from 'pinia'; // ^2.0.0
import { Quasar, Notify, Dialog, Loading, TouchPan, Meta } from 'quasar'; // ^2.12.0

// Quasar styles and extras
import '@quasar/extras/material-icons/material-icons.css'; // ^1.16.0
import '@quasar/extras/material-icons-outlined/material-icons-outlined.css';
import '@quasar/extras/material-icons-round/material-icons-round.css';
import 'quasar/dist/quasar.css';

// Internal imports
import App from './App';
import { router } from './router';
import { useAuthStore } from './stores/auth';

// Analytics
import VueGtag from 'vue-gtag'; // ^2.0.1

// Security configuration
const securityConfig = {
  csp: "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline';",
  xssProtection: '1; mode=block',
  frameOptions: 'DENY',
  contentTypeOptions: 'nosniff'
};

// Quasar configuration
const quasarConfig = {
  plugins: {
    Notify,
    Dialog,
    Loading,
    TouchPan,
    Meta
  },
  config: {
    brand: {
      primary: '#1976D2',
      secondary: '#26A69A',
      accent: '#9C27B0',
      dark: '#1D1D1D',
      positive: '#21BA45',
      negative: '#C10015',
      info: '#31CCEC',
      warning: '#F2C037'
    },
    notify: {
      position: 'top-right',
      timeout: 2500,
      progress: true,
      actions: [{ icon: 'close', color: 'white' }]
    },
    loading: {
      spinnerColor: 'primary',
      spinnerSize: 140,
      backgroundColor: 'rgba(255, 255, 255, 0.9)'
    },
    meta: {
      viewport: 'width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no'
    }
  }
};

/**
 * Configures security headers and CSP
 */
const configureSecurity = (): void => {
  // Set security headers
  const headers = new Headers();
  headers.set('Content-Security-Policy', securityConfig.csp);
  headers.set('X-XSS-Protection', securityConfig.xssProtection);
  headers.set('X-Frame-Options', securityConfig.frameOptions);
  headers.set('X-Content-Type-Options', securityConfig.contentTypeOptions);

  // Apply security meta tags
  const meta = document.createElement('meta');
  meta.httpEquiv = 'Content-Security-Policy';
  meta.content = securityConfig.csp;
  document.head.appendChild(meta);
};

/**
 * Sets up global error handling
 */
const setupErrorHandling = (): void => {
  window.onerror = (message, source, lineno, colno, error) => {
    console.error('Global error:', { message, source, lineno, colno, error });
    // Implement error reporting service integration here
  };

  window.onunhandledrejection = (event) => {
    console.error('Unhandled promise rejection:', event.reason);
    // Implement error reporting service integration here
  };
};

/**
 * Initializes the Vue application with enhanced security and performance features
 */
const initializeApp = async (): Promise<void> => {
  // Configure security
  configureSecurity();

  // Create Vue app instance
  const app = createApp(App);

  // Create and configure Pinia store
  const pinia = createPinia();
  app.use(pinia);

  // Initialize auth store
  const authStore = useAuthStore();

  // Configure Quasar Framework
  app.use(Quasar, quasarConfig);

  // Configure Vue Router
  app.use(router);

  // Configure analytics in production only
  if (process.env.NODE_ENV === 'production') {
    app.use(VueGtag, {
      config: { id: process.env.VUE_APP_GA_ID }
    });
  }

  // Setup error handling
  setupErrorHandling();

  // Mount application
  app.mount('#app');
};

// Initialize application
initializeApp().catch(error => {
  console.error('Application initialization failed:', error);
  // Implement fallback error handling here
});