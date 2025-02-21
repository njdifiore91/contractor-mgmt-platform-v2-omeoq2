// cypress: ^12.0.0
// @vue/cli-plugin-e2e-cypress: ^5.0.0

import { defineConfig } from 'cypress';
import { startDevServer } from '@cypress/vite-dev-server';

/**
 * Main Cypress plugin configuration function that sets up the testing environment
 * and configures all necessary plugins and settings for comprehensive E2E testing.
 * 
 * @param on - Cypress plugin event handler
 * @param config - Cypress configuration object
 * @returns Enhanced Cypress configuration
 */
export default function pluginConfig(
  on: Cypress.PluginEvents,
  config: Cypress.PluginConfigOptions
): Cypress.PluginConfigOptions {
  // Register TypeScript preprocessor
  on('file:preprocessor', require('@cypress/webpack-preprocessor')({
    webpackOptions: {
      resolve: {
        extensions: ['.ts', '.js']
      },
      module: {
        rules: [
          {
            test: /\.ts$/,
            exclude: [/node_modules/],
            use: [
              {
                loader: 'ts-loader',
                options: { transpileOnly: true }
              }
            ]
          }
        ]
      }
    }
  }));

  // Configure Vue CLI integration
  on('dev-server:start', (options) => {
    return startDevServer({ options });
  });

  // Configure test environment settings
  const environment = process.env.NODE_ENV || 'development';
  config.env = {
    ...config.env,
    apiUrl: process.env.API_URL || 'http://localhost:5000',
    coverage: process.env.COVERAGE === 'true',
    codeCoverage: {
      url: process.env.COVERAGE_URL || 'http://localhost:5000/__coverage__'
    }
  };

  // Configure test reporting
  config.reporter = 'cypress-multi-reporters';
  config.reporterOptions = {
    reporterEnabled: 'spec, mocha-junit-reporter',
    mochaJunitReporterReporterOptions: {
      mochaFile: 'tests/e2e/results/[hash].xml',
      toConsole: true
    }
  };

  // Configure browser launch options
  on('before:browser:launch', (browser, launchOptions) => {
    if (browser.name === 'chrome' && browser.isHeadless) {
      launchOptions.args.push('--disable-gpu');
      launchOptions.args.push('--no-sandbox');
      launchOptions.args.push('--disable-dev-shm-usage');
    }
    return launchOptions;
  });

  // Configure network handling
  on('task', {
    clearDownloads: () => {
      return null;
    },
    log(message) {
      console.log(message);
      return null;
    }
  });

  // Set up test retry mechanism
  config.retries = {
    runMode: 2,
    openMode: 0
  };

  // Configure viewport and timeouts
  config.viewportWidth = 1280;
  config.viewportHeight = 800;
  config.defaultCommandTimeout = 5000;
  config.watchForFileChanges = false;

  // Configure test file patterns
  config.testFiles = '**/*.spec.ts';
  config.supportFile = 'tests/e2e/support/index.ts';
  config.videosFolder = 'tests/e2e/videos';
  config.screenshotsFolder = 'tests/e2e/screenshots';
  config.baseUrl = process.env.BASE_URL || 'http://localhost:8080';

  // Set up cleanup handlers
  on('after:run', async () => {
    // Cleanup after test run
    console.log('Test run completed, performing cleanup...');
  });

  on('after:screenshot', async (details) => {
    // Handle screenshot processing
    return details;
  });

  // Return enhanced configuration
  return config;
}