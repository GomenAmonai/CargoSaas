import { createContext, useContext, useState, useEffect } from 'react';
import type { ReactNode } from 'react';
import { managerApi, managerTokenStorage } from '../api/manager';

interface ManagerUser {
  id: string;
  firstName: string;
  username?: string;
  photoUrl?: string;
  role: string;
  tenantId: string;
}

interface ManagerAuthContextType {
  user: ManagerUser | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  logout: () => void;
}

const ManagerAuthContext = createContext<ManagerAuthContextType | undefined>(undefined);

export const useManagerAuth = () => {
  const context = useContext(ManagerAuthContext);
  if (!context) {
    throw new Error('useManagerAuth must be used within ManagerAuthProvider');
  }
  return context;
};

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
