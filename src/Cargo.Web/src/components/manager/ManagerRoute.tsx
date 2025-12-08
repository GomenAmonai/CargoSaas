import { useEffect } from 'react';
import type { ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { useManagerAuth } from '../../hooks/useManagerAuth';

interface ManagerRouteProps {
  children: ReactNode;
}

/**
 * Protected route для менеджеров
 * Проверяет авторизацию и роль пользователя
 */
const ManagerRoute = ({ children }: ManagerRouteProps) => {
  const { user, isAuthenticated, isLoading } = useManagerAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      // Не авторизован - редирект на логин
      navigate('/manager/login', { replace: true });
    } else if (!isLoading && user && user.role !== 'Manager' && user.role !== 'SystemAdmin') {
      // Авторизован, но не менеджер - редирект на главную
      alert('Access denied. This area is for managers only.');
      navigate('/', { replace: true });
    }
  }, [isAuthenticated, isLoading, user, navigate]);

  // Показываем загрузку пока проверяем авторизацию
  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="w-12 h-12 border-4 border-blue-600 border-t-transparent rounded-full animate-spin mx-auto mb-4"></div>
          <p className="text-gray-600">Loading...</p>
        </div>
      </div>
    );
  }

  // Если не авторизован или не менеджер - не показываем контент (редирект в useEffect)
  if (!isAuthenticated || !user || (user.role !== 'Manager' && user.role !== 'SystemAdmin')) {
    return null;
  }

  // Все проверки пройдены - показываем контент
  return <>{children}</>;
};

export default ManagerRoute;
