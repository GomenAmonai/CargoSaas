import { useTelegram } from '../contexts/TelegramProvider';

const Home = () => {
  const { user, webApp } = useTelegram();

  const handleMyTracksClick = () => {
    // TODO: Navigate to tracks page
    webApp.showAlert('My Tracks feature coming soon!');
  };

  return (
    <div className="min-h-screen bg-tg-bg flex flex-col">
      {/* Header */}
      <div className="p-6 pb-8">
        <h1 className="text-3xl font-bold text-tg-text mb-2">
          Welcome{user?.first_name ? `, ${user.first_name}` : ''}! 👋
        </h1>
        <p className="text-tg-hint text-sm">
          Track your cargo shipments with ease
        </p>
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
        <div className="bg-tg-secondary-bg rounded-2xl p-6 shadow-sm">
          <h3 className="text-md font-semibold text-tg-text mb-2">
            About Cargo System
          </h3>
          <p className="text-tg-hint text-sm leading-relaxed">
            Track your packages in real-time, get instant notifications about status changes,
            and manage all your shipments in one place.
          </p>
        </div>

        {/* User Info (Debug) */}
        {user && (
          <div className="mt-4 p-4 bg-tg-secondary-bg rounded-xl">
            <p className="text-xs text-tg-hint">User ID: {user.id}</p>
            {user.username && (
              <p className="text-xs text-tg-hint">@{user.username}</p>
            )}
          </div>
        )}
      </div>

      {/* Footer */}
      <div className="p-6 text-center">
        <p className="text-xs text-tg-hint">
          Powered by Cargo System
        </p>
      </div>
    </div>
  );
};

export default Home;

