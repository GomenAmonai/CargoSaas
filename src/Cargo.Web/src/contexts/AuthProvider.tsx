import { useEffect, useState, useCallback } from 'react';
import type { ReactNode } from 'react';
import WebApp from '@twa-dev/sdk';
import { api, tokenStorage } from '../api/client';
import type { AuthResponse } from '../api/client';
import { AuthContext } from './AuthContext';
import type { User, AuthContextType } from './AuthContext';

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Функция для логина
  const login = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);

      // Проверяем наличие initData
      if (!WebApp.initData || WebApp.initData.length === 0) {
        throw new Error('No Telegram initData available');
      }

      console.log('🔐 Attempting login with Telegram initData...');
      console.log('📍 API URL:', import.meta.env.VITE_API_URL || 'using default');
      console.log('📦 initData length:', WebApp.initData?.length || 0);

      // Отправляем запрос на бэкенд
      const response: AuthResponse = await api.auth.login(WebApp.initData);

      console.log('✅ Login successful!', response);

      // Сохраняем данные пользователя (преобразуем плоскую структуру в user объект)
      setUser({
        id: response.userId,
        telegramId: 0, // TODO: добавить telegramId в AuthResponse если нужно
        firstName: response.firstName,
        username: response.username,
        photoUrl: response.photoUrl,
        role: response.role,
        tenantId: response.tenantId,
      });
      setIsLoading(false);

      // Показываем уведомление об успешном входе (если поддерживается)
      if (WebApp.isVersionAtLeast && WebApp.isVersionAtLeast('6.1')) {
        WebApp.showPopup({
          title: 'Welcome! 👋',
          message: `Hello, ${response.firstName}! You are now logged in.`,
        });
      } else {
        console.log('✅ Login successful! Welcome,', response.firstName);
      }

    } catch (err) {
      console.error('❌ Login error:', err);
      
      const error = err as { response?: { data?: { message?: string }; status?: number }; message?: string; config?: { url?: string } };
      console.error('📍 Error details:', {
        status: error.response?.status,
        url: error.config?.url,
        message: error.response?.data?.message || error.message
      });
      
      const errorMessage = error.response?.data?.message || error.message || 'Authentication failed';
      setError(errorMessage);
      setIsLoading(false);

      // Показываем ошибку пользователю (если поддерживается)
      if (WebApp.showAlert && WebApp.isVersionAtLeast && WebApp.isVersionAtLeast('6.1')) {
        WebApp.showAlert(`Login failed: ${errorMessage}`);
      } else {
        console.error('❌ Login failed:', errorMessage);
      }
    }
  }, []);

  // Функция для логаута
  const logout = useCallback(() => {
    setUser(null);
    setError(null);  // Очищаем ошибку при logout
    api.auth.logout();
    
    // Опционально: можно закрыть WebApp
    // WebApp.close();
  }, []);

  // Автоматический логин при монтировании компонента
  useEffect(() => {
    const initAuth = async () => {
      try {
        if (tokenStorage.exists()) {
          console.log('🔑 Found existing token, session restored');
          setIsLoading(false);
        } else {
          console.log('🔐 No token found, initiating automatic login...');
          await login();
        }
      } catch (err) {
        console.error('Auth initialization error:', err);
        setIsLoading(false);
      }
    };

    initAuth();
  }, [login]);

  const value: AuthContextType = {
    user,
    // isAuthenticated если есть токен (даже если user еще не загружен из /me endpoint)
    // ИЛИ если user уже загружен (при успешном login)
    isAuthenticated: tokenStorage.exists() || !!user,
    isLoading,
    error,
    login,
    logout,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};

