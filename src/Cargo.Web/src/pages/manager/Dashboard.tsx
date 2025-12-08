import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { managerApi, type DashboardStatistics } from '../../api/manager';
import ManagerLayout from '../../components/manager/ManagerLayout';

const Dashboard = () => {
  const [stats, setStats] = useState<DashboardStatistics | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    loadStatistics();
  }, []);

  const loadStatistics = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await managerApi.statistics.getDashboard();
      setStats(data);
    } catch (err) {
      console.error('Failed to load statistics:', err);
      setError('Failed to load statistics');
    } finally {
      setIsLoading(false);
    }
  };

  if (isLoading) {
    return (
      <ManagerLayout>
        <div className="p-8">
          <div className="animate-pulse space-y-4">
            <div className="h-8 bg-gray-200 rounded w-1/4"></div>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
              {[1, 2, 3, 4].map((i) => (
                <div key={i} className="h-32 bg-gray-200 rounded-xl"></div>
              ))}
            </div>
          </div>
        </div>
      </ManagerLayout>
    );
  }

  if (error || !stats) {
    return (
      <ManagerLayout>
        <div className="p-8">
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-red-700">
            {error || 'Failed to load statistics'}
          </div>
        </div>
      </ManagerLayout>
    );
  }

  const statCards = [
    {
      label: 'Total Tracks',
      value: stats.totalTracks,
      icon: '📦',
      color: 'blue',
    },
    {
      label: 'In Transit',
      value: stats.tracksInTransit,
      icon: '🚚',
      color: 'amber',
    },
    {
      label: 'Delivered This Week',
      value: stats.tracksDeliveredThisWeek,
      icon: '✅',
      color: 'green',
    },
    {
      label: 'Active Clients',
      value: stats.activeClients,
      icon: '👥',
      color: 'purple',
    },
  ];

  const getColorClasses = (color: string) => {
    const colors: Record<string, string> = {
      blue: 'bg-blue-50 border-blue-200 text-blue-700',
      amber: 'bg-amber-50 border-amber-200 text-amber-700',
      green: 'bg-green-50 border-green-200 text-green-700',
      purple: 'bg-purple-50 border-purple-200 text-purple-700',
      red: 'bg-red-50 border-red-200 text-red-700',
      gray: 'bg-gray-50 border-gray-200 text-gray-700',
    };
    return colors[color] || colors.gray;
  };

  return (
    <ManagerLayout>
      <div className="p-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Dashboard</h1>
          <p className="text-gray-600">Overview of your cargo operations</p>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
          {statCards.map((card) => (
            <div
              key={card.label}
              className={`border rounded-xl p-6 ${getColorClasses(card.color)}`}
            >
              <div className="flex items-center justify-between mb-2">
                <span className="text-2xl">{card.icon}</span>
                <span className="text-3xl font-bold">{card.value}</span>
              </div>
              <p className="text-sm font-medium opacity-80">{card.label}</p>
            </div>
          ))}
        </div>

        {/* Additional Stats Row */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
          <div className="bg-white border border-gray-200 rounded-xl p-6">
            <div className="flex items-center gap-3 mb-2">
              <span className="text-2xl">📅</span>
              <h3 className="text-lg font-semibold text-gray-900">
                Created Today
              </h3>
            </div>
            <p className="text-3xl font-bold text-gray-700">
              {stats.tracksCreatedToday}
            </p>
          </div>

          <div className="bg-white border border-gray-200 rounded-xl p-6">
            <div className="flex items-center gap-3 mb-2">
              <span className="text-2xl">⚠️</span>
              <h3 className="text-lg font-semibold text-gray-900">Delayed</h3>
            </div>
            <p className="text-3xl font-bold text-gray-700">
              {stats.delayedTracks}
            </p>
          </div>

          <div className="bg-white border border-gray-200 rounded-xl p-6">
            <div className="flex items-center gap-3 mb-2">
              <span className="text-2xl">📊</span>
              <h3 className="text-lg font-semibold text-gray-900">
                Completion Rate
              </h3>
            </div>
            <p className="text-3xl font-bold text-gray-700">
              {stats.totalTracks > 0
                ? Math.round(
                    ((stats.tracksByStatus['Delivered'] || 0) /
                      stats.totalTracks) *
                      100
                  )
                : 0}
              %
            </p>
          </div>
        </div>

        {/* Recent Tracks */}
        <div className="bg-white border border-gray-200 rounded-xl p-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-xl font-bold text-gray-900">Recent Tracks</h2>
            <button
              onClick={() => navigate('/manager/tracks')}
              className="text-sm text-blue-600 hover:text-blue-700 font-medium"
            >
              View All →
            </button>
          </div>

          {stats.recentTracks.length === 0 ? (
            <p className="text-gray-500 text-center py-8">No tracks yet</p>
          ) : (
            <div className="space-y-2">
              {stats.recentTracks.map((track) => (
                <button
                  key={track.id}
                  onClick={() => navigate(`/manager/tracks/${track.id}`)}
                  className="w-full flex items-center justify-between p-4 hover:bg-gray-50 rounded-lg transition-colors text-left"
                >
                  <div className="flex-1">
                    <p className="font-mono text-sm font-medium text-gray-900">
                      {track.trackingNumber}
                    </p>
                    <p className="text-xs text-gray-500">
                      Client: {track.clientCode}
                    </p>
                  </div>
                  <div className="flex items-center gap-3">
                    <span className="text-xs px-2 py-1 bg-gray-100 text-gray-700 rounded-full">
                      {track.status}
                    </span>
                    <span className="text-gray-400">→</span>
                  </div>
                </button>
              ))}
            </div>
          )}
        </div>

        {/* Tracks by Status */}
        {Object.keys(stats.tracksByStatus).length > 0 && (
          <div className="bg-white border border-gray-200 rounded-xl p-6 mt-4">
            <h2 className="text-xl font-bold text-gray-900 mb-4">
              Tracks by Status
            </h2>
            <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
              {Object.entries(stats.tracksByStatus).map(([status, count]) => (
                <div
                  key={status}
                  className="text-center p-4 bg-gray-50 rounded-lg"
                >
                  <p className="text-2xl font-bold text-gray-900">{count}</p>
                  <p className="text-xs text-gray-600 mt-1">{status}</p>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </ManagerLayout>
  );
};

export default Dashboard;
