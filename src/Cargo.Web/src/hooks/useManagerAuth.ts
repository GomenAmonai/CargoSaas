import { useContext } from 'react';
import { ManagerAuthContext, type ManagerAuthContextType } from '../contexts/ManagerAuthContext';

/**
 * Hook для использования Manager Auth Context
 * Вынесен в отдельный файл для соответствия ESLint правилу react-refresh/only-export-components
 */
export const useManagerAuth = (): ManagerAuthContextType => {
  const context = useContext(ManagerAuthContext);
  if (!context) {
    throw new Error('useManagerAuth must be used within ManagerAuthProvider');
  }
  return context;
};
