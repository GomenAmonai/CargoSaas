import type { ReactNode } from 'react';
import { useAuth } from '../contexts/AuthProvider';

interface AuthGuardProps {
  children: ReactNode;
}

// Компонент для отображения UI во время загрузки и ошибок авторизации
export const AuthGuard = ({ children }: AuthGuardProps) => {
  const { isLoading, error, isAuthenticated, login } = useAuth();

  // Показываем спиннер во время загрузки
  if (isLoading) {
    return (
      <div className="min-h-screen bg-tg-bg flex items-center justify-center p-6">
        <div className="text-center">
          <div className="relative w-16 h-16 mx-auto mb-4">
            {/* Анимированный спиннер */}
            <div className="absolute top-0 left-0 w-full h-full">
              <div className="w-16 h-16 border-4 border-tg-button/30 border-t-tg-button rounded-full animate-spin"></div>
            </div>
          </div>
          
          <h2 className="text-lg font-semibold text-tg-text mb-2">
            Authenticating...
          </h2>
          <p className="text-sm text-tg-hint">
            Please wait while we log you in
          </p>
        </div>
      </div>
    );
  }

  // Показываем экран ошибки с кнопкой повтора
  if (error && !isAuthenticated) {
    return (
      <div className="min-h-screen bg-tg-bg flex items-center justify-center p-6">
        <div className="bg-tg-secondary-bg rounded-2xl p-8 max-w-md text-center shadow-lg">
          {/* Иконка ошибки */}
          <div className="text-6xl mb-4">⚠️</div>
          
          <h2 className="text-xl font-bold text-tg-text mb-3">
            Authentication Failed
          </h2>
          
          <p className="text-sm text-tg-hint mb-2">
            We couldn't log you in. Please try again.
          </p>
          
          {/* Детали ошибки */}
          <div className="bg-red-50 border border-red-200 rounded-lg p-3 mb-6">
            <p className="text-xs text-red-700 font-mono break-words">
              {error}
            </p>
          </div>
          
          {/* Кнопка повтора */}
          <button
            onClick={login}
            className="w-full bg-tg-button text-tg-button-text font-semibold py-3 px-6 rounded-xl 
                     hover:opacity-90 active:scale-95 transition-all duration-150
                     focus:outline-none focus:ring-2 focus:ring-tg-button focus:ring-offset-2"
          >
            🔄 Retry Login
          </button>
          
          {/* Дополнительная информация */}
          <p className="text-xs text-tg-hint mt-4">
            If the problem persists, try restarting the app
          </p>
        </div>
      </div>
    );
  }

  // Если всё ОК - показываем детей (основное приложение)
  return <>{children}</>;
};

