import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { providePrimeNG } from 'primeng/config';
import { definePreset } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';

import { routes } from './app.routes';

// Deep teal/forest-green accent per docs/ARCHITECTURE.md §7 — starting
// proposal, not a locked brand decision.
const uvidikPreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: '#eef5f3',
      100: '#d4e7e1',
      200: '#aacfc3',
      300: '#7fb7a5',
      400: '#4f9c87',
      500: '#1f5d50',
      600: '#1a4f44',
      700: '#154039',
      800: '#10322d',
      900: '#0b2420',
      950: '#071512',
    },
  },
});

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    providePrimeNG({ theme: { preset: uvidikPreset, options: { darkModeSelector: false } } }),
  ],
};
