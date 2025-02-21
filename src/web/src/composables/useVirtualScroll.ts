import { ref, computed, onMounted, onUnmounted, watch, nextTick } from 'vue' // ^3.3.0

// Constants
const RAF_TIMEOUT = 16

// Types
interface VirtualScrollOptions {
  itemHeight: number
  buffer?: number
  debounceMs?: number
}

interface VisibleRange {
  startIndex: number
  endIndex: number
  totalHeight: number
}

/**
 * Calculates the visible range of items based on scroll position and container dimensions
 * @param scrollTop Current scroll position
 * @param containerHeight Height of the viewport container
 * @param itemHeight Height of each item
 * @param totalItems Total number of items in the list
 * @param buffer Number of items to buffer above and below visible area
 * @returns Calculated visible range with start/end indices and total height
 */
const calculateVisibleRange = (
  scrollTop: number,
  containerHeight: number,
  itemHeight: number,
  totalItems: number,
  buffer: number
): VisibleRange => {
  // Calculate theoretical start index
  const startIndex = Math.floor(scrollTop / itemHeight)
  
  // Apply buffer and clamp to valid range
  const bufferedStart = Math.max(0, startIndex - buffer)
  
  // Calculate visible items
  const visibleItems = Math.ceil(containerHeight / itemHeight)
  const endIndex = Math.min(
    startIndex + visibleItems + buffer,
    totalItems - 1
  )
  
  // Calculate total scrollable height
  const totalHeight = totalItems * itemHeight

  return {
    startIndex: bufferedStart,
    endIndex,
    totalHeight
  }
}

/**
 * Vue composable that provides virtual scrolling functionality for efficient rendering
 * of large lists and tables.
 * @param options Configuration options for virtual scrolling
 * @returns Composable API for virtual scroll management
 */
export function useVirtualScroll(options: VirtualScrollOptions) {
  // Validate input parameters
  if (options.itemHeight <= 0) {
    throw new Error('itemHeight must be greater than 0')
  }
  
  const buffer = options.buffer ?? 5
  const debounceMs = options.debounceMs ?? RAF_TIMEOUT

  // Reactive state
  const scrollTop = ref(0)
  const containerHeight = ref(0)
  const isUpdating = ref(false)
  
  // Scroll handler with RAF debouncing
  let rafId: number | null = null
  const handleScroll = (event: Event) => {
    if (rafId) {
      cancelAnimationFrame(rafId)
    }
    
    rafId = requestAnimationFrame(() => {
      const target = event.target as HTMLElement
      scrollTop.value = target.scrollTop
      rafId = null
    })
  }

  // Resize observer for container
  let resizeObserver: ResizeObserver | null = null
  
  // Computed properties for visible range
  const visibleStartIndex = computed(() => {
    const range = calculateVisibleRange(
      scrollTop.value,
      containerHeight.value,
      options.itemHeight,
      Infinity, // This should be replaced with actual total items in consumer
      buffer
    )
    return range.startIndex
  })

  const visibleEndIndex = computed(() => {
    const range = calculateVisibleRange(
      scrollTop.value,
      containerHeight.value,
      options.itemHeight,
      Infinity, // This should be replaced with actual total items in consumer
      buffer
    )
    return range.endIndex
  })

  /**
   * Updates the container dimensions
   * @param height New container height
   */
  const updateContainer = async (height: number): Promise<void> => {
    if (height <= 0) return
    
    if (isUpdating.value) {
      await nextTick()
    }
    
    isUpdating.value = true
    
    try {
      containerHeight.value = height
      await nextTick()
    } finally {
      isUpdating.value = false
    }
  }

  // Lifecycle hooks
  onMounted(() => {
    resizeObserver = new ResizeObserver((entries) => {
      for (const entry of entries) {
        const height = entry.contentRect.height
        updateContainer(height)
      }
    })
  })

  onUnmounted(() => {
    if (rafId) {
      cancelAnimationFrame(rafId)
    }
    if (resizeObserver) {
      resizeObserver.disconnect()
    }
  })

  /**
   * Cleanup function to remove event listeners and observers
   */
  const cleanup = () => {
    if (rafId) {
      cancelAnimationFrame(rafId)
    }
    if (resizeObserver) {
      resizeObserver.disconnect()
    }
  }

  return {
    scrollTop,
    containerHeight,
    visibleStartIndex,
    visibleEndIndex,
    updateContainer,
    cleanup
  }
}

// Type exports for consumer usage
export type { VirtualScrollOptions, VisibleRange }