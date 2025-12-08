import { TelegramProvider } from './contexts/TelegramProvider';
import { AuthProvider } from './contexts/AuthProvider';
import { ManagerAuthProvider } from './contexts/ManagerAuthContext.tsx';
import { useTelegram } from './hooks/useTelegram';
import { AuthGuard } from './components/AuthGuard';
import ManagerRoute from './components/manager/ManagerRoute';
import Home from './pages/Home';
import TrackList from './pages/TrackList';
import TrackDetails from './pages/TrackDetails';
import ManagerLogin from './pages/manager/ManagerLogin';
import Dashboard from './pages/manager/Dashboard';
import ManagerTrackList from './pages/manager/TrackList';
import TrackForm from './pages/manager/TrackForm';
import ImportExcel from './pages/manager/ImportExcel';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import './index.css';

// Компонент для проверки готовности Telegram SDK (для Telegram клиентов)
const TelegramAppContent = () => {
  const { isReady } = useTelegram();

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

  return (
    <AuthProvider>
      <AuthGuard>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/tracks" element={<TrackList />} />
          <Route path="/tracks/:id" element={<TrackDetails />} />
        </Routes>
      </AuthGuard>
    </AuthProvider>
  );
};

function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Manager Routes (без Telegram SDK) */}
        <Route path="/manager/login" element={<ManagerLogin />} />
        <Route
          path="/manager/*"
          element={
            <ManagerAuthProvider>
              <ManagerRoute>
                <Routes>
                  <Route path="dashboard" element={<Dashboard />} />
                  <Route path="tracks" element={<ManagerTrackList />} />
                  <Route path="tracks/new" element={<TrackForm />} />
                  <Route path="tracks/:id/edit" element={<TrackForm />} />
                  <Route path="import" element={<ImportExcel />} />
                </Routes>
              </ManagerRoute>
            </ManagerAuthProvider>
          }
        />

        {/* Telegram Client Routes (с Telegram SDK) */}
        <Route
          path="/*"
          element={
            <TelegramProvider>
              <TelegramAppContent />
            </TelegramProvider>
          }
        />
      </Routes>
    </BrowserRouter>
  );
}

export default App;

