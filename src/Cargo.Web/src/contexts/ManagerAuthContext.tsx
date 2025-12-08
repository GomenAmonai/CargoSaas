import { useState, useEffect } from 'react';
import type { ReactNode } from 'react';
import { managerApi, managerTokenStorage } from '../api/manager';
import { tokenStorage } from '../api/client';
import { ManagerAuthContext, type ManagerAuthContextType, type ManagerUser } from './ManagerAuthContext';

interface ManagerAuthProviderProps {
  children: ReactNode;
}

export const ManagerAuthProvider = ({ children }: ManagerAuthProviderProps) => {
  const [user, setUser] = useState<ManagerUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Проверяем токен при монтировании
  useEffect(() => {
    const checkAuth = async () => {
      try {
        // Если нет Manager токена, но есть Client токен - используем его
        // (для Manager зашедших через Telegram WebApp)
        if (!managerTokenStorage.exists() && tokenStorage.exists()) {
          const clientToken = tokenStorage.get();
          if (clientToken) {
            // Временно сохраняем Client токен как Manager токен
            managerTokenStorage.set(clientToken);
          }
        }

        if (!managerTokenStorage.exists()) {
          setIsLoading(false);
          return;
        }

        // Проверяем токен через /auth/me
        const userData = await managerApi.auth.getCurrentUser();
        setUser({
          id: userData.id,
          firstName: userData.firstName,
          username: userData.username,
          photoUrl: userData.photoUrl,
          role: userData.role,
          tenantId: userData.tenantId,
        });
        setError(null);
      } catch (err) {
        console.error('Auth check failed:', err);
        managerTokenStorage.remove();
        setError('Authentication failed');
      } finally {
        setIsLoading(false);
      }
    };

    checkAuth();
  }, []);

  const logout = () => {
    setUser(null);
    managerApi.auth.logout();
  };

  const value: ManagerAuthContextType = {
    user,
    isAuthenticated: !!user,
    isLoading,
    error,
    logout,
  };

  return (
    <ManagerAuthContext.Provider value={value}>
      {children}
    </ManagerAuthContext.Provider>
  );
};
