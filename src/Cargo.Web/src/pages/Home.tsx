import { useTelegram } from '../contexts/TelegramProvider';
import { useAuth } from '../contexts/AuthProvider';

const Home = () => {
  const { webApp } = useTelegram();
  const { user, logout } = useAuth();

  const handleMyTracksClick = () => {
    // TODO: Navigate to tracks page
    webApp.showAlert('My Tracks feature coming soon!');
  };

  const handleLogout = () => {
    webApp.showConfirm('Are you sure you want to log out?', (confirmed) => {
      if (confirmed) {
        logout();
      }
    });
  };

  return (
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
            Track your packages in real-time, get instant notifications about status changes,
            and manage all your shipments in one place.
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
                <span className="text-xs text-tg-hint font-mono">{user.telegramId}</span>
              </div>
            </div>

            {/* Logout Button */}
            <button
              onClick={handleLogout}
              className="w-full bg-red-500 text-white rounded-lg py-2 px-4 text-sm font-medium 
                       hover:bg-red-600 active:scale-95 transition-all duration-150"
            >
              🚪 Logout
            </button>
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
  );
};

export default Home;

