// Vite v4.3.0
import { defineConfig } from 'vite';
// Vue Plugin v4.2.0
import vue from '@vitejs/plugin-vue';
// Quasar Plugin v1.4.0
import quasar from '@quasar/vite-plugin';
// Built-in Node.js path module
import path from 'path';
// Compression Plugin v0.5.1
import compression from 'vite-plugin-compression';

export default defineConfig({
  plugins: [
    vue({
      reactivityTransform: true,
      template: {
        compilerOptions: {
          // Enable production optimizations
          whitespace: 'condense',
          comments: false
        }
      }
    }),
    quasar({
      sassVariables: 'src/quasar-variables.sass',
      framework: {
        config: {
          // Enable virtual scrolling optimizations
          virtualScroll: true,
          loading: {
            delay: 400,
            spinner: 'QSpinnerGears'
          }
        }
      }
    }),
    compression({
      algorithm: 'gzip',
      ext: '.gz',
      threshold: 10240, // Only compress files > 10KB
      deleteOriginFile: false
    })
  ],

  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@components': path.resolve(__dirname, './src/components'),
      '@views': path.resolve(__dirname, './src/views')
    }
  },

  build: {
    target: 'es2015',
    minify: 'terser',
    sourcemap: true,
    chunkSizeWarningLimit: 2000,
    terserOptions: {
      compress: {
        drop_console: true,
        drop_debugger: true,
        pure_funcs: ['console.log', 'console.info'],
        passes: 2
      }
    },
    rollupOptions: {
      output: {
        manualChunks: {
          // Core vendor chunks
          'vendor-core': ['vue', 'quasar'],
          // Virtual scrolling specific chunks
          'virtual-scroll': ['virtual-scroll-grid'],
          // Feature-specific chunks
          'components': ['./src/components/index.ts'],
          'admin': [
            './src/views/admin/QuickLinks.vue',
            './src/views/admin/CodeTypes.vue',
            './src/views/admin/Users.vue'
          ],
          'customers': [
            './src/views/customers/CustomerList.vue',
            './src/views/customers/CustomerDetail.vue'
          ],
          'inspectors': [
            './src/views/inspectors/InspectorList.vue',
            './src/views/inspectors/InspectorDetail.vue'
          ]
        },
        // Optimize chunk distribution
        chunkFileNames: 'assets/js/[name]-[hash].js',
        entryFileNames: 'assets/js/[name]-[hash].js',
        assetFileNames: 'assets/[ext]/[name]-[hash].[ext]'
      }
    },
    cssCodeSplit: true,
    assetsInlineLimit: 4096
  },

  server: {
    port: 3000,
    cors: true,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/api/, '')
      }
    },
    headers: {
      'Access-Control-Allow-Origin': '*',
      'Cache-Control': 'no-cache',
      'Content-Security-Policy': "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob:; font-src 'self' data:;"
    },
    hmr: {
      overlay: true,
      clientPort: 3000
    },
    watch: {
      usePolling: true,
      interval: 1000
    }
  },

  optimizeDeps: {
    include: [
      'vue',
      'quasar',
      'virtual-scroll-grid',
      '@vueuse/core'
    ],
    exclude: ['@quasar/extras'],
    esbuildOptions: {
      target: 'es2015'
    }
  },

  css: {
    preprocessorOptions: {
      scss: {
        additionalData: "@import '@/styles/variables.scss';"
      }
    },
    postcss: {
      plugins: [
        require('autoprefixer'),
        require('cssnano')({
          preset: ['default', {
            discardComments: { removeAll: true }
          }]
        })
      ]
    }
  },

  // Environment specific configurations
  define: {
    'process.env': {
      NODE_ENV: JSON.stringify(process.env.NODE_ENV),
      BASE_URL: JSON.stringify(process.env.BASE_URL || '/')
    }
  }
});