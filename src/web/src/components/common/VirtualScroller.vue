<template>
  <div 
    ref="containerRef"
    :class="['virtual-scroller', containerClass]"
    :style="containerStyle"
    @scroll="handleScroll"
  >
    <div :style="{ height: `${totalHeight}px` }" class="virtual-scroller__content">
      <div 
        v-for="item in visibleItems" 
        :key="items.indexOf(item)"
        :style="{ 
          position: 'absolute',
          top: `${items.indexOf(item) * itemHeight}px`,
          height: `${itemHeight}px`,
          width: '100%'
        }"
      >
        <slot :item="item" :index="items.indexOf(item)" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// Vue Composition API - v3.3.0
import { ref, computed, onMounted, watch, onUnmounted } from 'vue';

// ResizeObserver polyfill - v1.5.1
import ResizeObserver from 'resize-observer-polyfill';

// Internal imports
import { useVirtualScroll } from '@/composables/useVirtualScroll';

// Props definition
const props = defineProps({
  items: {
    type: Array,
    required: true,
    validator: (value: unknown[]) => Array.isArray(value) && value != null
  },
  itemHeight: {
    type: Number,
    required: true,
    validator: (value: number) => value > 0
  },
  buffer: {
    type: Number,
    default: 5,
    validator: (value: number) => value >= 0
  },
  containerClass: {
    type: String,
    default: ''
  }
});

// Emits definition
const emit = defineEmits<{
  (e: 'scroll', scrollTop: number, scrollInfo: { startIndex: number; endIndex: number }): void
  (e: 'resize', dimensions: { width: number; height: number }): void
  (e: 'visible-items-change', items: unknown[]): void
}>();

// Component state
const containerRef = ref<HTMLElement | null>(null);
const isScrolling = ref(false);
let resizeObserver: ResizeObserver | null = null;
let scrollDebounceTimer: number | null = null;

// Initialize virtual scroll composable
const {
  scrollTop,
  visibleStartIndex,
  visibleEndIndex,
  updateContainer
} = useVirtualScroll({
  itemHeight: props.itemHeight,
  buffer: props.buffer
});

// Computed properties
const containerStyle = computed(() => ({
  position: 'relative' as const,
  height: '100%',
  overflowY: 'auto' as const,
  willChange: isScrolling.value ? 'transform' : 'auto'
}));

const totalHeight = computed(() => 
  props.items.length * props.itemHeight
);

const visibleItems = computed(() => {
  const start = visibleStartIndex.value;
  const end = Math.min(visibleEndIndex.value, props.items.length);
  const items = props.items.slice(start, end + 1);
  emit('visible-items-change', items);
  return items;
});

// Methods
const handleScroll = (event: Event) => {
  if (scrollDebounceTimer) {
    window.clearTimeout(scrollDebounceTimer);
  }

  isScrolling.value = true;
  const target = event.target as HTMLElement;
  scrollTop.value = target.scrollTop;

  emit('scroll', scrollTop.value, {
    startIndex: visibleStartIndex.value,
    endIndex: visibleEndIndex.value
  });

  scrollDebounceTimer = window.setTimeout(() => {
    isScrolling.value = false;
  }, 150);
};

const scrollToIndex = (index: number, options: { smooth?: boolean } = {}) => {
  if (!containerRef.value) return;
  
  const targetScrollTop = index * props.itemHeight;
  containerRef.value.scrollTo({
    top: targetScrollTop,
    behavior: options.smooth ? 'smooth' : 'auto'
  });
};

const updateContainerSize = (entry: ResizeObserverEntry) => {
  const { width, height } = entry.contentRect;
  updateContainer(height);
  emit('resize', { width, height });
};

const cleanup = () => {
  if (resizeObserver) {
    resizeObserver.disconnect();
    resizeObserver = null;
  }
  if (scrollDebounceTimer) {
    window.clearTimeout(scrollDebounceTimer);
    scrollDebounceTimer = null;
  }
};

// Lifecycle hooks
onMounted(() => {
  if (!containerRef.value) return;

  resizeObserver = new ResizeObserver((entries) => {
    for (const entry of entries) {
      updateContainerSize(entry);
    }
  });

  resizeObserver.observe(containerRef.value);
});

onUnmounted(() => {
  cleanup();
});

// Watch for prop changes
watch(() => props.itemHeight, () => {
  if (containerRef.value) {
    updateContainer(containerRef.value.clientHeight);
  }
});

// Expose public methods
defineExpose({
  scrollToIndex,
  updateContainerSize
});
</script>

<style>
.virtual-scroller {
  position: relative;
  overflow-y: auto;
  -webkit-overflow-scrolling: touch;
}

.virtual-scroller__content {
  position: relative;
  width: 100%;
}
</style>