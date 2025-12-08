import { useNavigate } from 'react-router-dom';
import { useTelegram } from '../hooks/useTelegram';
import { useAuth } from '../hooks/useAuth';
import ClientLayout from '../components/ClientLayout';

const Home = () => {
  const { webApp } = useTelegram();
  const { user } = useAuth();
  const navigate = useNavigate();

  const handleMyTracksClick = () => {
    navigate('/tracks');
    if (webApp.HapticFeedback) {
      webApp.HapticFeedback.impactOccurred('light');
    }
  };

  const handleCopyClientCode = async () => {
    if (!user?.clientCode) return;

    try {
      await navigator.clipboard.writeText(user.clientCode);
      if (webApp.HapticFeedback) {
        webApp.HapticFeedback.notificationOccurred('success');
      }
      webApp.showAlert('Client code copied to clipboard');
    } catch {
      webApp.showAlert('Failed to copy client code');
    }
  };

  return (
    <ClientLayout>
      <div className="min-h-screen bg-tg-bg flex flex-col">
        {/* Header */}
        <div className="p-6 pb-8">
          <div className="flex items-start justify-between">
            <div className="flex-1">
              <h1 className="text-3xl font-bold text-tg-text mb-2">
                Welcome{user?.firstName ? `, ${user.firstName}` : ''}! 👋
              </h1>
              <p className="text-tg-hint text-sm">
                Track your cargo shipments with ease
              </p>
            </div>

            {/* User Avatar */}
            {user?.photoUrl && (
              <div className="ml-4">
                <img
                  src={user.photoUrl}
                  alt={user.firstName}
                  className="w-12 h-12 rounded-full border-2 border-tg-button"
                />
              </div>
            )}
          </div>
        </div>

        {/* Main Content */}
        <div className="flex-1 px-6">
        {/* Client Code Card */}
        {user?.clientCode && (
          <div className="bg-gradient-to-r from-blue-500 to-blue-600 rounded-2xl p-6 mb-4 shadow-lg text-white relative overflow-hidden">
            <div className="relative z-10">
              <p className="text-blue-100 text-[11px] font-medium uppercase tracking-wider mb-1">
                Your Client Code
              </p>
              <div className="flex items-center justify-between gap-3">
                <h2 className="text-2xl sm:text-3xl font-mono font-bold tracking-wider">
                  {user.clientCode}
                </h2>
                <button
                  onClick={handleCopyClientCode}
                  className="bg-white/15 hover:bg-white/25 active:bg-white/30 p-2 rounded-xl transition-all duration-150 active:scale-95"
                >
                  <span className="text-lg">📋</span>
                </button>
              </div>
              <p className="text-blue-100 text-xs mt-3 opacity-90">
                Share this code with your manager to link your shipments.
              </p>
            </div>
            <div className="absolute -right-6 -bottom-10 w-32 h-32 bg-white/10 rounded-full blur-2xl" />
          </div>
        )}

        {/* Quick Actions Card */}
        <div className="bg-tg-secondary-bg rounded-2xl p-6 mb-4 shadow-sm">
          <h2 className="text-lg font-semibold text-tg-text mb-4">
            Quick Actions
          </h2>

          {/* My Tracks Button */}
          <button
            onClick={handleMyTracksClick}
            className="w-full bg-tg-button text-tg-button-text rounded-xl py-4 px-6 font-semibold text-lg shadow-md active:scale-95 transition-transform duration-150 flex items-center justify-center gap-3"
          >
            <span className="text-2xl">📦</span>
            <span>My Tracks</span>
          </button>
        </div>

        {/* Info Card */}
        <div className="bg-tg-secondary-bg rounded-2xl p-6 mb-4 shadow-sm">
          <h3 className="text-md font-semibold text-tg-text mb-2">
            About Cargo System
          </h3>
          <p className="text-tg-hint text-sm leading-relaxed">
            Track your packages in real-time, get instant notifications about
            status changes, and manage all your shipments in one place.
          </p>
        </div>

        {/* User Info Card */}
        {user && (
          <div className="bg-tg-secondary-bg rounded-2xl p-6 shadow-sm">
            <h3 className="text-md font-semibold text-tg-text mb-3">
              Account Info
            </h3>

            <div className="space-y-2 mb-4">
              <div className="flex justify-between items-center">
                <span className="text-xs text-tg-hint">Name:</span>
                <span className="text-sm text-tg-text font-medium">
                  {user.firstName} {user.lastName || ''}
                </span>
              </div>

              {user.username && (
                <div className="flex justify-between items-center">
                  <span className="text-xs text-tg-hint">Username:</span>
                  <span className="text-sm text-tg-text">@{user.username}</span>
                </div>
              )}

              <div className="flex justify-between items-center">
                <span className="text-xs text-tg-hint">Role:</span>
                <span className="text-sm text-tg-text capitalize">
                  {user.role.toLowerCase()}
                </span>
              </div>

              <div className="flex justify-between items-center">
                <span className="text-xs text-tg-hint">Telegram ID:</span>
                <span className="text-xs text-tg-hint font-mono">
                  {user.telegramId}
                </span>
              </div>

              {user.clientCode && (
                <div className="flex justify-between items-center">
                  <span className="text-xs text-tg-hint">Client Code:</span>
                  <span className="text-xs text-tg-text font-mono">
                    {user.clientCode}
                  </span>
                </div>
              )}
            </div>
          </div>
        )}
      </div>

      {/* Footer */}
      <div className="p-6 text-center">
        <p className="text-xs text-tg-hint">
          Powered by Cargo System • v1.0.0
        </p>
      </div>
    </div>
    </ClientLayout>
  );
};

export default Home;

