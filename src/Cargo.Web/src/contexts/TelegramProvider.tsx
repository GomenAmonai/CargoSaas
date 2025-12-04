import { createContext, useContext, useEffect, useState } from 'react';
import type { ReactNode } from 'react';
import WebApp from '@twa-dev/sdk';

interface TelegramContextType {
  webApp: typeof WebApp;
  user: typeof WebApp.initDataUnsafe.user;
  isReady: boolean;
  isTelegramApp: boolean;
}

const TelegramContext = createContext<TelegramContextType | undefined>(undefined);

export const useTelegram = () => {
  const context = useContext(TelegramContext);
  if (!context) {
    throw new Error('useTelegram must be used within TelegramProvider');
  }
  return context;
};

interface TelegramProviderProps {
  children: ReactNode;
}

export const TelegramProvider = ({ children }: TelegramProviderProps) => {
  const [isReady, setIsReady] = useState(false);
  const [isTelegramApp, setIsTelegramApp] = useState(false);

  useEffect(() => {
    // Проверяем, что мы внутри Telegram WebApp
    const checkTelegramEnvironment = () => {
      try {
        // Telegram WebApp SDK автоматически инициализируется
        WebApp.ready();
        
        // Проверяем что initData существует
        if (WebApp.initData && WebApp.initData.length > 0) {
          setIsTelegramApp(true);
          
          // Расширяем WebApp на весь экран
          WebApp.expand();
          
          // Применяем тему Telegram
          document.documentElement.style.setProperty('--tg-theme-bg-color', WebApp.backgroundColor);
          document.documentElement.style.setProperty('--tg-theme-text-color', WebApp.themeParams.text_color || '#000000');
          document.documentElement.style.setProperty('--tg-theme-hint-color', WebApp.themeParams.hint_color || '#999999');
          document.documentElement.style.setProperty('--tg-theme-link-color', WebApp.themeParams.link_color || '#2481cc');
          document.documentElement.style.setProperty('--tg-theme-button-color', WebApp.themeParams.button_color || '#2481cc');
          document.documentElement.style.setProperty('--tg-theme-button-text-color', WebApp.themeParams.button_text_color || '#ffffff');
          document.documentElement.style.setProperty('--tg-theme-secondary-bg-color', WebApp.themeParams.secondary_bg_color || '#f4f4f5');
        } else {
          setIsTelegramApp(false);
        }
      } catch (error) {
        console.error('Telegram WebApp initialization error:', error);
        setIsTelegramApp(false);
      } finally {
        setIsReady(true);
      }
    };

    checkTelegramEnvironment();
  }, []);

  const value: TelegramContextType = {
    webApp: WebApp,
    user: WebApp.initDataUnsafe.user,
    isReady,
    isTelegramApp,
  };

  return (
    <TelegramContext.Provider value={value}>
      {children}
    </TelegramContext.Provider>
  );
};

