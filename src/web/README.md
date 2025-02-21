# Inspector Management Web Application

A comprehensive web application built with Vue.js and Quasar Framework for tracking service provider data, communication, and notes with focus on user management, permissions, and equipment tracking.

## Technology Stack

### Core Technologies
- Vue.js 3.3.0
- Quasar Framework 2.12.0
- TypeScript 5.0.x

### State Management & Routing
- Pinia 2.1.0
- Vue Router 4.2.0

### Testing
- Vitest (Unit Testing)
- Cypress 12.10.0 (E2E Testing)

### Build Tools
- Vite 4.3.0
- ESBuild

## Project Structure

```
src/
├── components/          # Reusable UI components
│   ├── virtual-scroll/  # Virtual scrolling implementation
│   ├── dialogs/        # CRUD operation dialogs
│   └── common/         # Shared components
├── pages/              # Page components for all sections
│   ├── admin/          # Admin management pages
│   ├── customers/      # Customer management pages
│   ├── equipment/      # Equipment tracking pages
│   └── inspectors/     # Inspector management pages
├── stores/             # Pinia state management stores
├── composables/        # Shared composition functions
├── services/          # API and external service integrations
├── utils/             # Utility functions and helpers
├── types/             # TypeScript type definitions
└── layouts/           # Page layouts and navigation
```

## Getting Started

### Prerequisites
- Node.js 16.0.0 or higher
- npm 8.0.0 or higher
- Git

### Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd inspector-management-web
```

2. Install dependencies:
```bash
npm install
```

3. Create environment file:
```bash
cp .env.example .env
```

4. Start development server:
```bash
npm run dev
```

### Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build production bundle
- `npm run preview` - Preview production build
- `npm run test:unit` - Run unit tests
- `npm run test:e2e` - Run end-to-end tests
- `npm run lint` - Lint and fix files
- `npm run format` - Format code with Prettier

## Development Guidelines

### Component Architecture

Follow atomic design principles:
- Atoms: Basic UI components
- Molecules: Combinations of atoms
- Organisms: Complex UI components
- Templates: Page layouts
- Pages: Complete views

### Virtual Scrolling Implementation

Use the virtual scrolling component for efficient list rendering:

```vue
<virtual-scroll
  :items="items"
  :item-size="50"
  :buffer-size="10"
>
  <template #default="{ item }">
    <list-item :data="item" />
  </template>
</virtual-scroll>
```

### State Management

Use Pinia stores with composition API:
- Create modular stores for each feature
- Implement actions for async operations
- Use getters for computed state
- Follow strict typing with TypeScript

### Form Handling

Use Vee-Validate for form validation:
- Create reusable form components
- Implement validation schemas
- Handle async validation
- Support custom validation rules

## Required Pages Implementation

### Admin Section
- Quick Links Management
- Code Types and Codes Management
- User Management

### Customer Section
- Customer List with Search
- Customer Details
- Contract Management
- Contact Management

### Equipment Section
- Equipment List by Company
- Equipment Assignment
- Equipment Return Tracking

### Inspector Section
- Inspector Search and List
- Drug Test Management
- Mobilization Management
- Demobilization Tracking
- OneDrive Integration
- Class Change Management

## Testing Strategy

### Unit Testing
- Component testing with Vitest
- Store testing with @pinia/testing
- Service mocking with Vitest

### E2E Testing
- Critical user flows with Cypress
- API mocking with Cypress interceptors
- Visual regression testing

## Building and Deployment

### Production Build
```bash
npm run build
```

The build process:
1. Type checking with TypeScript
2. Code bundling with Vite
3. Asset optimization
4. Compression

### Deployment
- Azure deployment configuration
- Environment variable management
- Build artifact handling

## Environment Configuration

Configure the following environment variables:
```
VITE_API_BASE_URL=
VITE_AUTH_DOMAIN=
VITE_ONEDRIVE_CLIENT_ID=
VITE_ANALYTICS_ID=
```

## Browser Support

- Chrome (latest 2 versions)
- Firefox (latest 2 versions)
- Edge (latest 2 versions)
- Safari (latest 2 versions)

## Contributing

1. Follow the coding standards
2. Write comprehensive tests
3. Update documentation
4. Submit pull requests

## License

Private - All rights reserved