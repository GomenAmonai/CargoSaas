import { TelegramProvider } from './contexts/TelegramProvider';
import { AuthProvider } from './contexts/AuthProvider';
import { useTelegram } from './hooks/useTelegram';
import { AuthGuard } from './components/AuthGuard';
import Home from './pages/Home';
import './index.css';

// Компонент для проверки готовности Telegram SDK
const AppContent = () => {
  const { isReady } = useTelegram(); // isTelegramApp временно не используется для debug

  // Показываем загрузку пока SDK инициализируется
  if (!isReady) {
    return (
      <div className="min-h-screen bg-tg-bg flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-tg-button mx-auto mb-4"></div>
          <p className="text-tg-hint">Loading...</p>
        </div>
      </div>
    );
  }

  // Проверяем что приложение открыто в Telegram
  // ВРЕМЕННО ОТКЛЮЧЕНО ДЛЯ DEBUG
  // if (!isTelegramApp) {
  //   return (
  //     <div className="min-h-screen bg-gray-100 flex items-center justify-center p-6">
  //       <div className="bg-white rounded-2xl p-8 shadow-lg max-w-md text-center">
  //         <div className="text-6xl mb-4">📱</div>
  //         <h1 className="text-2xl font-bold text-gray-800 mb-3">
  //           Please Open in Telegram
  //         </h1>
  //         <p className="text-gray-600 mb-6">
  //           This application is designed to work inside Telegram.
  //           Please open it through your Telegram bot.
  //         </p>
  //         <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
  //           <p className="text-sm text-blue-800">
  //             <strong>How to open:</strong><br />
  //             1. Open Telegram<br />
  //             2. Find your bot<br />
  //             3. Send /start<br />
  //             4. Click "Open App" button
  //           </p>
  //         </div>
  //       </div>
  //     </div>
  //   );
  // }

  // Всё хорошо - показываем приложение с AuthGuard
  return (
    <AuthProvider>
      <AuthGuard>
        <Home />
      </AuthGuard>
    </AuthProvider>
  );
};

function App() {
  return (
    <TelegramProvider>
      <AppContent />
    </TelegramProvider>
  );
}

export default App;
