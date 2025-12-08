import { useEffect, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { api, type Track } from '../api/client';
import { useTelegram } from '../hooks/useTelegram';
import { TrackCard } from '../components/TrackCard';
import ClientLayout from '../components/ClientLayout';

const TrackList = () => {
  const { webApp } = useTelegram();
  const navigate = useNavigate();
  const [tracks, setTracks] = useState<Track[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadTracks = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await api.tracks.getAll();
      setTracks(data);
    } catch (err) {
      console.error('Failed to load tracks', err);
      setError('Failed to load tracks');
      webApp.showAlert('Failed to load tracks. Please try again.');
    } finally {
      setIsLoading(false);
    }
  }, [webApp]);

  useEffect(() => {
    const handleBack = () => {
      navigate(-1);
    };

    webApp.BackButton.show();
    webApp.BackButton.onClick(handleBack);

    loadTracks();

    return () => {
      webApp.BackButton.hide();
      webApp.BackButton.offClick(handleBack);
    };
  }, [webApp, loadTracks, navigate]);

  const handleRefresh = () => {
    loadTracks();
  };

  return (
    <ClientLayout>
      <div className="min-h-screen bg-tg-bg flex flex-col">
        <div className="p-6 pb-4 flex items-center justify-between">
          <h1 className="text-2xl font-bold text-tg-text">My Tracks</h1>
          <button
            onClick={handleRefresh}
            className="text-sm text-tg-link font-medium"
          >
            Refresh
          </button>
        </div>

      <div className="flex-1 px-4 pb-4">
        {isLoading && (
          <div className="space-y-3">
            {[1, 2, 3].map((i) => (
              <div
                key={i}
                className="bg-tg-secondary-bg rounded-2xl p-4 animate-pulse"
              >
                <div className="flex items-center justify-between mb-2">
                  <div className="h-4 bg-gray-300/40 rounded w-1/3" />
                  <div className="h-4 bg-gray-300/40 rounded w-16" />
                </div>
                <div className="h-3 bg-gray-300/40 rounded w-2/3 mb-2" />
                <div className="h-3 bg-gray-300/40 rounded w-1/2" />
              </div>
            ))}
          </div>
        )}

        {!isLoading && error && (
          <div className="text-center text-tg-hint text-sm mt-8">
            {error}
          </div>
        )}

        {!isLoading && !error && tracks.length === 0 && (
          <div className="text-center text-tg-hint text-sm mt-8">
            You don&apos;t have any tracks yet.
          </div>
        )}

        {!isLoading && !error && tracks.length > 0 && (
          <div className="space-y-3">
            {tracks.map((track) => (
              <TrackCard
                key={track.id}
                track={track}
                onClick={() => {
                  navigate(`/tracks/${track.id}`);
                  if (webApp.HapticFeedback) {
                    webApp.HapticFeedback.impactOccurred('light');
                  }
                }}
              />
            ))}
          </div>
        )}
      </div>
    </div>
    </ClientLayout>
  );
};

export default TrackList;
