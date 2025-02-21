/**
 * @file Vue Router configuration with enhanced security and performance features
 * @version 1.0.0
 */

import { createRouter, createWebHistory, Router, RouteLocationNormalized } from 'vue-router'; // ^4.0.0
import { adminRoutes, customerRoutes, equipmentRoutes, inspectorRoutes } from './routes';
import { authGuard, permissionGuard } from './guards';

// Performance monitoring constants
const ROUTE_TRANSITION_THRESHOLD = 300; // milliseconds
const PERFORMANCE_SAMPLE_RATE = 0.1; // 10% of transitions

/**
 * Validates route configuration and parameters
 */
const validateRoute = (to: RouteLocationNormalized, from: RouteLocationNormalized): boolean => {
  // Ensure route exists
  if (!to.matched.length) {
    console.error(`Invalid route: ${to.fullPath}`);
    return false;
  }

  // Validate route parameters if required
  if (to.meta.validateParams && to.params) {
    for (const [key, value] of Object.entries(to.params)) {
      if (!value || (typeof value === 'string' && !value.trim())) {
        console.error(`Invalid parameter ${key} for route ${to.fullPath}`);
        return false;
      }
    }
  }

  // Verify required meta configuration
  if (to.meta.requiresAuth === undefined) {
    console.warn(`Missing authentication configuration for route ${to.fullPath}`);
  }

  return true;
};

/**
 * Creates and configures the Vue Router instance
 */
const createAppRouter = (): Router => {
  const router = createRouter({
    history: createWebHistory(),
    routes: [
      {
        path: '/',
        redirect: '/customers'
      },
      ...adminRoutes,
      ...customerRoutes,
      ...equipmentRoutes,
      ...inspectorRoutes,
      {
        path: '/:pathMatch(.*)*',
        name: 'notFound',
        component: () => import('../pages/NotFoundPage.vue')
      }
    ],
    scrollBehavior(to, from, savedPosition) {
      if (savedPosition) {
        return savedPosition;
      }
      return { top: 0 };
    }
  });

  // Route validation guard
  router.beforeEach((to, from, next) => {
    if (!validateRoute(to, from)) {
      next({ name: 'notFound' });
      return;
    }
    next();
  });

  // Authentication and permission guards
  router.beforeEach(authGuard);
  router.beforeEach(permissionGuard);

  // Performance monitoring
  router.beforeEach((to, from, next) => {
    if (Math.random() < PERFORMANCE_SAMPLE_RATE) {
      const start = performance.now();
      to.meta.transitionStart = start;
    }
    next();
  });

  router.afterEach((to) => {
    if (to.meta.transitionStart) {
      const duration = performance.now() - (to.meta.transitionStart as number);
      if (duration > ROUTE_TRANSITION_THRESHOLD) {
        console.warn(`Slow route transition to ${to.fullPath}: ${duration.toFixed(2)}ms`);
      }
      // Clean up
      delete to.meta.transitionStart;
    }
  });

  // Error boundary
  router.onError((error) => {
    console.error('Router error:', error);
    // Implement error reporting service integration here
  });

  // Mobile optimization
  router.beforeEach((to, from, next) => {
    if (to.meta.mobileOptimized) {
      // Set mobile-specific meta tags
      document.querySelector('meta[name="viewport"]')?.setAttribute(
        'content',
        'width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no'
      );
    }
    next();
  });

  // Route cleanup
  router.afterEach((to) => {
    // Clean up any resources from previous route
    if (to.meta.cleanup) {
      (to.meta.cleanup as Function)();
    }
  });

  return router;
};

// Create and export router instance
export const router = createAppRouter();