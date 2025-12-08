import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { managerApi, type ManagerTrack } from '../../api/manager';
import ManagerLayout from '../../components/manager/ManagerLayout';

const TrackList = () => {
  const [tracks, setTracks] = useState<ManagerTrack[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // Фильтры (простой useState)
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [clientCodeFilter, setClientCodeFilter] = useState('');
  
  const navigate = useNavigate();

  useEffect(() => {
    loadTracks();
  }, []);

  const loadTracks = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await managerApi.tracks.getAll();
      setTracks(data);
    } catch (err) {
      console.error('Failed to load tracks:', err);
      setError('Failed to load tracks');
    } finally {
      setIsLoading(false);
    }
  };

  const handleDelete = async (id: string, trackingNumber: string) => {
    if (!confirm(`Delete track ${trackingNumber}?`)) return;

    try {
      await managerApi.tracks.delete(id);
      setTracks(tracks.filter((t) => t.id !== id));
    } catch (err) {
      console.error('Failed to delete track:', err);
      alert('Failed to delete track');
    }
  };

  // Клиентская фильтрация (для MVP достаточно)
  const filteredTracks = tracks.filter((track) => {
    // Поиск
    if (searchQuery) {
      const query = searchQuery.toLowerCase();
      const matchesSearch =
        track.trackingNumber.toLowerCase().includes(query) ||
        track.clientCode.toLowerCase().includes(query) ||
        track.description?.toLowerCase().includes(query);
      if (!matchesSearch) return false;
    }

    // Фильтр по статусу
    if (statusFilter && track.status !== statusFilter) {
      return false;
    }

    // Фильтр по клиенту
    if (clientCodeFilter && track.clientCode !== clientCodeFilter) {
      return false;
    }

    return true;
  });

  // Уникальные значения для фильтров
  const uniqueStatuses = Array.from(new Set(tracks.map((t) => t.status)));
  const uniqueClientCodes = Array.from(new Set(tracks.map((t) => t.clientCode)));

  const getStatusColor = (status: string) => {
    const normalized = status.toLowerCase();
    if (normalized.includes('delivered')) return 'bg-green-100 text-green-700';
    if (normalized.includes('transit') || normalized.includes('customs'))
      return 'bg-amber-100 text-amber-700';
    if (normalized.includes('created') || normalized.includes('accepted'))
      return 'bg-gray-100 text-gray-700';
    if (normalized.includes('cancelled') || normalized.includes('delayed'))
      return 'bg-red-100 text-red-700';
    return 'bg-blue-100 text-blue-700';
  };

  if (isLoading) {
    return (
      <ManagerLayout>
        <div className="p-8">
          <div className="animate-pulse space-y-4">
            <div className="h-8 bg-gray-200 rounded w-1/4"></div>
            <div className="space-y-2">
              {[1, 2, 3, 4, 5].map((i) => (
                <div key={i} className="h-20 bg-gray-200 rounded"></div>
              ))}
            </div>
          </div>
        </div>
      </ManagerLayout>
    );
  }

  return (
    <ManagerLayout>
      <div className="p-8">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="text-3xl font-bold text-gray-900 mb-2">Tracks</h1>
            <p className="text-gray-600">
              {filteredTracks.length} of {tracks.length} tracks
            </p>
          </div>
          <button
            onClick={() => navigate('/manager/tracks/new')}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 transition-colors flex items-center gap-2"
          >
            <span>➕</span>
            <span>New Track</span>
          </button>
        </div>

        {/* Filters */}
        <div className="bg-white border border-gray-200 rounded-xl p-4 mb-6">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {/* Search */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Search
              </label>
              <input
                type="text"
                placeholder="Tracking number, client code..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            {/* Status Filter */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Status
              </label>
              <select
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">All Statuses</option>
                {uniqueStatuses.map((status) => (
                  <option key={status} value={status}>
                    {status}
                  </option>
                ))}
              </select>
            </div>

            {/* Client Filter */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Client
              </label>
              <select
                value={clientCodeFilter}
                onChange={(e) => setClientCodeFilter(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">All Clients</option>
                {uniqueClientCodes.map((code) => (
                  <option key={code} value={code}>
                    {code}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {/* Clear Filters */}
          {(searchQuery || statusFilter || clientCodeFilter) && (
            <button
              onClick={() => {
                setSearchQuery('');
                setStatusFilter('');
                setClientCodeFilter('');
              }}
              className="mt-3 text-sm text-blue-600 hover:text-blue-700 font-medium"
            >
              Clear Filters
            </button>
          )}
        </div>

        {/* Error */}
        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 mb-6 text-red-700">
            {error}
          </div>
        )}

        {/* Tracks List */}
        {filteredTracks.length === 0 ? (
          <div className="bg-white border border-gray-200 rounded-xl p-12 text-center">
            <p className="text-gray-500 mb-4">No tracks found</p>
            <button
              onClick={() => navigate('/manager/tracks/new')}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 transition-colors inline-flex items-center gap-2"
            >
              <span>➕</span>
              <span>Create First Track</span>
            </button>
          </div>
        ) : (
          <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
            {/* Table Header */}
            <div className="grid grid-cols-12 gap-4 px-6 py-3 bg-gray-50 border-b border-gray-200 text-sm font-medium text-gray-700">
              <div className="col-span-3">Tracking Number</div>
              <div className="col-span-2">Client</div>
              <div className="col-span-2">Status</div>
              <div className="col-span-2">Origin → Dest</div>
              <div className="col-span-2">Created</div>
              <div className="col-span-1 text-right">Actions</div>
            </div>

            {/* Table Body */}
            <div>
              {filteredTracks.map((track) => (
                <div
                  key={track.id}
                  className="grid grid-cols-12 gap-4 px-6 py-4 border-b border-gray-100 hover:bg-gray-50 transition-colors items-center"
                >
                  <div className="col-span-3">
                    <button
                      onClick={() => navigate(`/manager/tracks/${track.id}`)}
                      className="font-mono text-sm font-medium text-blue-600 hover:text-blue-700 text-left"
                    >
                      {track.trackingNumber}
                    </button>
                    {track.description && (
                      <p className="text-xs text-gray-500 mt-1 line-clamp-1">
                        {track.description}
                      </p>
                    )}
                  </div>

                  <div className="col-span-2">
                    <span className="text-sm font-mono text-gray-700">
                      {track.clientCode}
                    </span>
                  </div>

                  <div className="col-span-2">
                    <span
                      className={`inline-block px-2 py-1 text-xs font-medium rounded-full ${getStatusColor(
                        track.status
                      )}`}
                    >
                      {track.status}
                    </span>
                  </div>

                  <div className="col-span-2 text-sm text-gray-600">
                    {track.originCountry && track.destinationCountry ? (
                      <span>
                        {track.originCountry} → {track.destinationCountry}
                      </span>
                    ) : (
                      <span className="text-gray-400">—</span>
                    )}
                  </div>

                  <div className="col-span-2 text-sm text-gray-600">
                    {new Date(track.createdAt).toLocaleDateString()}
                  </div>

                  <div className="col-span-1 flex items-center justify-end gap-2">
                    <button
                      onClick={() => navigate(`/manager/tracks/${track.id}/edit`)}
                      className="text-blue-600 hover:text-blue-700 p-1"
                      title="Edit"
                    >
                      ✏️
                    </button>
                    <button
                      onClick={() => handleDelete(track.id, track.trackingNumber)}
                      className="text-red-600 hover:text-red-700 p-1"
                      title="Delete"
                    >
                      🗑️
                    </button>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </ManagerLayout>
  );
};

export default TrackList;
