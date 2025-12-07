import { createContext } from 'react';
import type WebApp from '@twa-dev/sdk';

export interface TelegramContextType {
  webApp: typeof WebApp;
  user: typeof WebApp.initDataUnsafe.user;
  isReady: boolean;
  isTelegramApp: boolean;
}

export const TelegramContext = createContext<TelegramContextType | undefined>(undefined);




