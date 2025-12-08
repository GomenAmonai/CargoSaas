import { useState, type ReactNode } from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { useTelegram } from '../hooks/useTelegram';

interface ClientLayoutProps {
  children: ReactNode;
}

const ClientLayout = ({ children }: ClientLayoutProps) => {
  const { user, logout } = useAuth();
  const { webApp } = useTelegram();
  const navigate = useNavigate();
  const [sidebarOpen, setSidebarOpen] = useState(false);

  const handleLogout = () => {
    webApp.showConfirm('Are you sure you want to log out?', (confirmed) => {
      if (confirmed) {
        logout();
        navigate('/');
      }
    });
  };

  const navItems = [
    { path: '/', label: 'Home', icon: '🏠' },
    { path: '/tracks', label: 'My Tracks', icon: '📦' },
  ];

  return (
    <div className="min-h-screen bg-tg-bg flex">
      {/* Mobile Sidebar Overlay */}
      {sidebarOpen && (
        <div
          className="fixed inset-0 bg-black/50 z-40 lg:hidden"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* Sidebar */}
      <aside
        className={`fixed lg:static inset-y-0 left-0 z-50 w-64 bg-tg-secondary-bg border-r border-tg-button/10 flex flex-col transform transition-transform duration-300 ease-in-out ${
          sidebarOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'
        }`}
      >
        {/* Logo */}
        <div className="p-6 border-b border-tg-button/10">
          <h1 className="text-xl font-bold text-tg-text">
            📦 Cargo Tracker
          </h1>
          <p className="text-xs text-tg-hint mt-1">Track your shipments</p>
        </div>

        {/* Navigation */}
        <nav className="flex-1 p-4 space-y-1">
          {navItems.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              className={({ isActive }) =>
                `flex items-center gap-3 px-4 py-3 rounded-lg transition-colors ${
                  isActive
                    ? 'bg-tg-button text-tg-button-text font-medium'
                    : 'text-tg-text hover:bg-tg-button/10'
                }`
              }
              onClick={() => {
                setSidebarOpen(false);
                if (webApp.HapticFeedback) {
                  webApp.HapticFeedback.impactOccurred('light');
                }
              }}
            >
              <span className="text-xl">{item.icon}</span>
              <span>{item.label}</span>
            </NavLink>
          ))}
        </nav>

        {/* User Info */}
        {user && (
          <div className="p-4 border-t border-tg-button/10">
            <div className="flex items-center gap-3 mb-3">
              {user.photoUrl ? (
                <img
                  src={user.photoUrl}
                  alt={user.firstName}
                  className="w-10 h-10 rounded-full"
                />
              ) : (
                <div className="w-10 h-10 rounded-full bg-tg-button/20 flex items-center justify-center">
                  <span className="text-tg-button-text font-semibold">
                    {user.firstName?.[0] || 'U'}
                  </span>
                </div>
              )}
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-tg-text truncate">
                  {user.firstName}
                </p>
                {user.clientCode && (
                  <p className="text-xs text-tg-hint font-mono truncate">
                    {user.clientCode}
                  </p>
                )}
              </div>
            </div>
            <button
              onClick={handleLogout}
              className="w-full px-4 py-2 text-sm text-red-500 hover:bg-red-500/10 rounded-lg transition-colors flex items-center justify-center gap-2"
            >
              <span>🚪</span>
              <span>Logout</span>
            </button>
          </div>
        )}
      </aside>

      {/* Main Content */}
      <main className="flex-1 overflow-auto w-full lg:w-auto">
        {/* Mobile Menu Button */}
        <button
          onClick={() => setSidebarOpen(!sidebarOpen)}
          className="lg:hidden fixed top-4 left-4 z-30 bg-tg-button text-tg-button-text p-2 rounded-lg shadow-lg"
        >
          <span className="text-xl">☰</span>
        </button>
        {children}
      </main>
    </div>
  );
};

export default ClientLayout;
