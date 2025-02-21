/**
 * @file Application route definitions with enhanced security and mobile optimization
 * @version 1.0.0
 */

import { RouteRecordRaw } from 'vue-router'; // ^4.0.0
import { authGuard, permissionGuard } from './guards';
import MainLayout from '../layouts/MainLayout.vue';
import AuthLayout from '../layouts/AuthLayout.vue';

// Auth routes configuration
export const authRoutes: RouteRecordRaw[] = [{
  path: '/auth',
  component: AuthLayout,
  meta: {
    rateLimit: true,
    mobileOptimized: true
  },
  children: [
    {
      path: 'login',
      name: 'login',
      component: () => import('../pages/auth/LoginPage.vue')
    },
    {
      path: 'reset-password',
      name: 'resetPassword',
      component: () => import('../pages/auth/ResetPasswordPage.vue')
    },
    {
      path: 'confirm-email',
      name: 'confirmEmail',
      component: () => import('../pages/auth/ConfirmEmailPage.vue')
    }
  ]
}];

// Admin routes configuration
export const adminRoutes: RouteRecordRaw[] = [{
  path: '/admin',
  component: MainLayout,
  meta: {
    requiresAuth: true,
    mobileOptimized: true,
    errorBoundary: true
  },
  children: [
    {
      path: 'quick-links',
      name: 'quickLinks',
      component: () => import('../pages/admin/QuickLinksPage.vue'),
      meta: {
        permissions: ['Edit Links'],
        mobileBreakpoints: { xs: 'compact', sm: 'default' }
      }
    },
    {
      path: 'codes',
      name: 'codes',
      component: () => import('../pages/admin/CodeTypesPage.vue'),
      meta: {
        permissions: ['Edit Codes'],
        mobileBreakpoints: { xs: 'compact', sm: 'default' }
      }
    },
    {
      path: 'users',
      name: 'users',
      component: () => import('../pages/admin/UsersPage.vue'),
      meta: {
        permissions: ['Edit Users'],
        mobileBreakpoints: { xs: 'compact', sm: 'default' }
      }
    }
  ]
}];

// Customer routes configuration
export const customerRoutes: RouteRecordRaw[] = [{
  path: '/customers',
  component: MainLayout,
  meta: {
    requiresAuth: true,
    mobileOptimized: true,
    errorBoundary: true
  },
  children: [
    {
      path: '',
      name: 'customerList',
      component: () => import('../pages/customer/CustomerListPage.vue'),
      meta: {
        mobileBreakpoints: { xs: 'compact', sm: 'default' }
      }
    },
    {
      path: ':id',
      name: 'customerDetail',
      component: () => import('../pages/customer/CustomerDetailPage.vue'),
      meta: {
        validateParams: true,
        mobileBreakpoints: { xs: 'compact', sm: 'default' }
      }
    }
  ]
}];

// Equipment routes configuration
export const equipmentRoutes: RouteRecordRaw[] = [{
  path: '/equipment',
  component: MainLayout,
  meta: {
    requiresAuth: true,
    permissions: ['Edit Equipment'],
    mobileOptimized: true,
    errorBoundary: true
  },
  children: [
    {
      path: '',
      name: 'equipmentList',
      component: () => import('../pages/equipment/EquipmentListPage.vue'),
      meta: {
        mobileBreakpoints: { xs: 'compact', sm: 'default' }
      }
    }
  ]
}];

// Inspector routes configuration
export const inspectorRoutes: RouteRecordRaw[] = [{
  path: '/inspectors',
  component: MainLayout,
  meta: {
    requiresAuth: true,
    mobileOptimized: true,
    errorBoundary: true
  },
  children: [
    {
      path: '',
      name: 'inspectorList',
      component: () => import('../pages/inspector/InspectorListPage.vue'),
      meta: {
        mobileBreakpoints: { xs: 'compact', sm: 'default' }
      }
    },
    {
      path: ':id',
      name: 'inspectorDetail',
      component: () => import('../pages/inspector/InspectorDetailPage.vue'),
      meta: {
        validateParams: true,
        mobileBreakpoints: { xs: 'compact', sm: 'default' }
      }
    }
  ]
}];

// Root routes array combining all route configurations
const routes: RouteRecordRaw[] = [
  {
    path: '/',
    redirect: '/customers'
  },
  ...authRoutes,
  ...adminRoutes,
  ...customerRoutes,
  ...equipmentRoutes,
  ...inspectorRoutes,
  {
    path: '/:pathMatch(.*)*',
    name: 'notFound',
    component: () => import('../pages/NotFoundPage.vue')
  }
];

// Apply global navigation guards
routes.forEach(route => {
  if (route.meta?.requiresAuth) {
    const originalBeforeEnter = route.beforeEnter;
    route.beforeEnter = async (to, from, next) => {
      // Execute auth guard first
      const authResult = await authGuard(to, from, next);
      if (authResult === false) return;

      // Execute permission guard if auth passes
      const permResult = await permissionGuard(to, from, next);
      if (permResult === false) return;

      // Execute original beforeEnter if it exists
      if (originalBeforeEnter) {
        return originalBeforeEnter(to, from, next);
      }

      return next();
    };
  }
});

export default routes;