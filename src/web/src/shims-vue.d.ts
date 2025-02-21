/* Vue 3 TypeScript Declaration File
 * Version: 3.0.0
 * Purpose: Provides TypeScript type definitions for Vue single-file components (.vue files)
 */

// Import the DefineComponent type from Vue 3
import type { DefineComponent } from 'vue' // ^3.0.0

/* Declaration for Vue Single-File Components (.vue files)
 * This enables TypeScript to understand .vue files as modules that export Vue components
 * Generic parameters:
 * - {}: Props type (empty object as default)
 * - {}: RawBindings type (empty object as default)
 * - any: DataType (any as default for maximum flexibility)
 */
declare module '*.vue' {
  const component: DefineComponent<{}, {}, any>
  export default component
}