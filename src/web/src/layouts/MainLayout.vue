<template>
  <q-layout view="hHh lpR fFf" class="main-layout">
    <!-- Header -->
    <q-header elevated class="bg-primary text-white" height-hint="98">
      <q-toolbar>
        <q-btn
          flat
          dense
          round
          icon="menu"
          aria-label="Menu"
          @click="toggleDrawer"
        />

        <q-toolbar-title>
          {{ user?.firstName }} {{ user?.lastName }}
        </q-toolbar-title>

        <q-btn
          flat
          dense
          round
          icon="logout"
          aria-label="Logout"
          @click="handleLogout"
        />
      </q-toolbar>
    </q-header>

    <!-- Navigation Drawer -->
    <q-drawer
      v-model="isDrawerOpen"
      :mini="isMobileView"
      :breakpoint="500"
      bordered
      class="bg-grey-1"
      :width="250"
    >
      <q-scroll-area class="fit">
        <q-list padding>
          <template v-for="item in navigationItems" :key="item.path">
            <q-item
              v-if="checkPermission(item.permission)"
              clickable
              v-ripple
              :to="item.path"
              :active="$route.path === item.path"
            >
              <q-item-section avatar>
                <q-icon :name="item.icon" />
              </q-item-section>
              <q-item-section>
                {{ item.label }}
              </q-item-section>
            </q-item>
          </template>
        </q-list>
      </q-scroll-area>
    </q-drawer>

    <!-- Main Content -->
    <q-page-container>
      <router-view v-slot="{ Component }">
        <transition name="fade" mode="out-in">
          <component :is="Component" />
        </transition>
      </router-view>
    </q-page-container>
  </q-layout>
</template>

<script lang="ts">
import { defineComponent, ref, computed, onMounted, onBeforeUnmount } from 'vue';
import { useRouter } from 'vue-router';
import { useQuasar } from 'quasar';
import { useAuth } from '../composables/useAuth';
import { usePermissions } from '../composables/usePermissions';
import { PERMISSION_TYPES } from '../utils/permissions';

// Session management constants
const SESSION_CHECK_INTERVAL = 5 * 60 * 1000; // 5 minutes
const INACTIVITY_TIMEOUT = 30 * 60 * 1000; // 30 minutes

export default defineComponent({
  name: 'MainLayout',

  setup() {
    // Composables
    const router = useRouter();
    const $q = useQuasar();
    const { user, isAuthenticated, logout, refreshSession, validateSession } = useAuth();
    const { checkPermission, currentRoutePermissions } = usePermissions();

    // State
    const isDrawerOpen = ref(true);
    const sessionRefreshTimer = ref<number>(0);
    const inactivityTimer = ref<number>(0);

    // Computed
    const isMobileView = computed(() => $q.platform.is.mobile);
    const navigationItems = computed(() => [
      {
        label: 'Quick Links',
        path: '/admin/quick-links',
        icon: 'link',
        permission: PERMISSION_TYPES.EDIT_LINKS
      },
      {
        label: 'Codes',
        path: '/admin/codes',
        icon: 'code',
        permission: PERMISSION_TYPES.EDIT_CODES
      },
      {
        label: 'Users',
        path: '/admin/users',
        icon: 'people',
        permission: PERMISSION_TYPES.EDIT_USERS
      },
      {
        label: 'Equipment',
        path: '/equipment',
        icon: 'build',
        permission: PERMISSION_TYPES.EDIT_EQUIPMENT
      },
      {
        label: 'Customers',
        path: '/customers',
        icon: 'business',
        permission: PERMISSION_TYPES.MANAGE_CUSTOMERS
      },
      {
        label: 'Inspectors',
        path: '/inspectors',
        icon: 'assignment_ind',
        permission: PERMISSION_TYPES.MOBILIZE_INSPECTOR
      }
    ]);

    // Methods
    const handleLogout = async () => {
      try {
        clearInterval(sessionRefreshTimer.value);
        clearTimeout(inactivityTimer.value);
        await logout();
        router.push('/login');
        $q.notify({
          type: 'positive',
          message: 'Successfully logged out'
        });
      } catch (error) {
        console.error('Logout failed:', error);
        $q.notify({
          type: 'negative',
          message: 'Logout failed. Please try again.'
        });
      }
    };

    const toggleDrawer = () => {
      isDrawerOpen.value = !isDrawerOpen.value;
    };

    const setupSessionManagement = () => {
      // Periodic session refresh
      sessionRefreshTimer.value = window.setInterval(() => {
        refreshSession().catch(handleLogout);
      }, SESSION_CHECK_INTERVAL);

      // Inactivity monitoring
      const resetInactivityTimer = () => {
        clearTimeout(inactivityTimer.value);
        inactivityTimer.value = window.setTimeout(handleLogout, INACTIVITY_TIMEOUT);
      };

      // Activity event listeners
      ['mousedown', 'keydown', 'touchstart', 'mousemove'].forEach(event => {
        document.addEventListener(event, resetInactivityTimer);
      });

      // Initial timer setup
      resetInactivityTimer();
    };

    // Lifecycle hooks
    onMounted(async () => {
      if (isAuthenticated.value) {
        try {
          await validateSession();
          setupSessionManagement();
        } catch (error) {
          handleLogout();
        }
      } else {
        router.push('/login');
      }
    });

    onBeforeUnmount(() => {
      clearInterval(sessionRefreshTimer.value);
      clearTimeout(inactivityTimer.value);
      ['mousedown', 'keydown', 'touchstart', 'mousemove'].forEach(event => {
        document.removeEventListener(event, () => {});
      });
    });

    return {
      // State
      isDrawerOpen,
      user,
      
      // Computed
      navigationItems,
      isMobileView,
      
      // Methods
      handleLogout,
      toggleDrawer,
      checkPermission
    };
  }
});
</script>

<style lang="scss">
.main-layout {
  .q-drawer {
    background-color: #f5f5f5;
    
    .q-item {
      border-radius: 4px;
      margin: 0 4px;
      
      &.q-item--active {
        background-color: var(--q-primary);
        color: white;
      }
    }
  }

  .fade-enter-active,
  .fade-leave-active {
    transition: opacity 0.2s ease;
  }

  .fade-enter-from,
  .fade-leave-to {
    opacity: 0;
  }
}
</style>