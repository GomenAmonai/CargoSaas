import { TelegramProvider } from './contexts/TelegramProvider';
import { AuthProvider } from './contexts/AuthProvider';
import { useTelegram } from './hooks/useTelegram';
import { AuthGuard } from './components/AuthGuard';
import Home from './pages/Home';
import TrackList from './pages/TrackList';
import TrackDetails from './pages/TrackDetails';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
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

  // Всё хорошо - показываем приложение с AuthGuard и роутингом
  return (
    <AuthProvider>
      <AuthGuard>
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/tracks" element={<TrackList />} />
            <Route path="/tracks/:id" element={<TrackDetails />} />
          </Routes>
        </BrowserRouter>
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

