import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { managerApi, managerTokenStorage, type TelegramLoginData } from '../../api/manager';

const ManagerLogin = () => {
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    // Проверяем есть ли уже токен
    if (managerTokenStorage.exists()) {
      navigate('/manager/dashboard', { replace: true });
      return;
    }

    // Инициализируем Telegram Login Widget
    const script = document.createElement('script');
    script.src = 'https://telegram.org/js/telegram-widget.js?22';
    // Bot username из переменной окружения или дефолтное значение
    const botUsername = import.meta.env.VITE_TELEGRAM_BOT_USERNAME || 'YOUR_BOT_USERNAME';
    script.setAttribute('data-telegram-login', botUsername);
    script.setAttribute('data-size', 'large');
    script.setAttribute('data-radius', '10');
    script.setAttribute('data-userpic', 'true');
    script.setAttribute('data-request-access', 'write');
    script.setAttribute('data-onauth', 'onTelegramAuth(user)');
    script.async = true;

    const container = document.getElementById('telegram-login-container');
    if (container) {
      container.appendChild(script);
    }

    // Глобальная функция для обработки callback от Telegram
    interface WindowWithTelegramAuth extends Window {
      onTelegramAuth?: (user: TelegramLoginData) => Promise<void>;
    }
    
    const windowWithAuth = window as WindowWithTelegramAuth;
    
    windowWithAuth.onTelegramAuth = async (user: TelegramLoginData) => {
      try {
        setIsLoading(true);
        setError(null);

        // Отправляем данные на бэкенд
        await managerApi.auth.loginWithTelegram(user);

        // Редирект на dashboard
        navigate('/manager/dashboard', { replace: true });
      } catch (err) {
        const error = err as { response?: { data?: { message?: string } } };
        const errorMessage = error.response?.data?.message || 'Login failed. Please try again.';
        setError(errorMessage);
        setIsLoading(false);
      }
    };

    return () => {
      // Cleanup
      delete windowWithAuth.onTelegramAuth;
    };
  }, [navigate]);

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-gray-100 flex items-center justify-center p-4">
      <div className="max-w-md w-full">
        {/* Card */}
        <div className="bg-white rounded-2xl shadow-xl p-8">
          {/* Header */}
          <div className="text-center mb-8">
            <div className="text-5xl mb-4">📦</div>
            <h1 className="text-3xl font-bold text-gray-900 mb-2">
              Cargo Manager
            </h1>
            <p className="text-gray-600">
              Admin Dashboard
            </p>
          </div>

          {/* Info */}
          <div className="bg-blue-50 border border-blue-200 rounded-xl p-4 mb-6">
            <p className="text-sm text-blue-800">
              <strong>👋 Welcome, Manager!</strong>
              <br />
              Log in with your Telegram account to access the admin dashboard.
            </p>
          </div>

          {/* Loading State */}
          {isLoading && (
            <div className="text-center py-4">
              <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin mx-auto mb-2"></div>
              <p className="text-gray-600 text-sm">Authenticating...</p>
            </div>
          )}

          {/* Error */}
          {error && (
            <div className="bg-red-50 border border-red-200 rounded-lg p-4 mb-6 text-red-700 text-sm">
              {error}
            </div>
          )}

          {/* Telegram Login Widget Container */}
          {!isLoading && (
            <div className="flex flex-col items-center">
              <div
                id="telegram-login-container"
                className="mb-6"
              ></div>
              
              <p className="text-xs text-gray-500 text-center">
                By logging in, you agree to access the admin dashboard
              </p>
            </div>
          )}

          {/* Divider */}
          <div className="relative my-8">
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-gray-200"></div>
            </div>
            <div className="relative flex justify-center text-sm">
              <span className="px-2 bg-white text-gray-500">OR</span>
            </div>
          </div>

          {/* Back to Client App */}
          <button
            onClick={() => navigate('/')}
            className="w-full px-4 py-3 bg-gray-100 text-gray-700 rounded-lg font-medium hover:bg-gray-200 transition-colors text-center"
          >
            ← Back to Client App
          </button>
        </div>

        {/* Footer */}
        <p className="text-center text-sm text-gray-500 mt-6">
          Cargo Management System • v1.0.0
        </p>
      </div>
    </div>
  );
};

export default ManagerLogin;
