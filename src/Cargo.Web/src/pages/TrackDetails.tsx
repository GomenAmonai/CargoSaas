import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { api, type Track } from '../api/client';
import { useTelegram } from '../hooks/useTelegram';
import { formatDateTime } from '../utils/date';
import ClientLayout from '../components/ClientLayout';

const TrackDetails = () => {
  const { id } = useParams<{ id: string }>();
  const { webApp } = useTelegram();
  const navigate = useNavigate();
  const [track, setTrack] = useState<Track | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const handleBack = () => {
      navigate(-1);
    };

    webApp.BackButton.show();
    webApp.BackButton.onClick(handleBack);

    const loadTrack = async () => {
      if (!id) return;
      try {
        setIsLoading(true);
        setError(null);
        const data = await api.tracks.getById(id);
        setTrack(data);
      } catch (err) {
        console.error('Failed to load track', err);
        setError('Failed to load track');
        webApp.showAlert('Failed to load track. Please try again.');
      } finally {
        setIsLoading(false);
      }
    };

    loadTrack();

    return () => {
      webApp.BackButton.hide();
      webApp.BackButton.offClick(handleBack);
    };
  }, [id, webApp, navigate]);

  if (isLoading) {
    return (
      <ClientLayout>
        <div className="min-h-screen bg-tg-bg flex items-center justify-center">
          <div className="text-center">
            <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-tg-button mx-auto mb-3" />
            <p className="text-tg-hint text-sm">Loading track...</p>
          </div>
        </div>
      </ClientLayout>
    );
  }

  if (error || !track) {
    return (
      <ClientLayout>
        <div className="min-h-screen bg-tg-bg flex items-center justify-center px-6">
          <p className="text-tg-hint text-sm text-center">
            {error || 'Track not found'}
          </p>
        </div>
      </ClientLayout>
    );
  }

  return (
    <ClientLayout>
      <div className="min-h-screen bg-tg-bg flex flex-col">
      <div className="p-6 pb-4">
        <p className="text-xs text-tg-hint mb-1">Tracking Number</p>
        <h1 className="text-2xl font-bold text-tg-text break-all">
          {track.trackingNumber}
        </h1>
      </div>

      <div className="flex-1 px-4 pb-6 space-y-4">
        <div className="bg-tg-secondary-bg rounded-2xl p-4 shadow-sm">
          <div className="flex items-center justify-between mb-2">
            <span className="text-xs text-tg-hint">Status</span>
            <span className="text-xs px-2 py-1 rounded-full bg-tg-button/10 text-tg-button-text">
              {track.status}
            </span>
          </div>
          {track.description && (
            <div className="mt-2">
              <p className="text-xs text-tg-hint mb-1">Description</p>
              <p className="text-sm text-tg-text">{track.description}</p>
            </div>
          )}
        </div>

        <div className="bg-tg-secondary-bg rounded-2xl p-4 shadow-sm">
          <h2 className="text-sm font-semibold text-tg-text mb-3">
            Shipment Info
          </h2>
          <div className="space-y-2 text-xs text-tg-hint">
            <div className="flex justify-between">
              <span>Client Code</span>
              <span className="font-mono text-tg-text">
                {track.clientCode}
              </span>
            </div>
            <div className="flex justify-between">
              <span>Origin</span>
              <span className="text-tg-text">
                {track.originCountry || '—'}
              </span>
            </div>
            <div className="flex justify-between">
              <span>Destination</span>
              <span className="text-tg-text">
                {track.destinationCountry || '—'}
              </span>
            </div>
            <div className="flex justify-between">
              <span>Weight</span>
              <span className="text-tg-text">
                {track.weight != null ? `${track.weight} kg` : '—'}
              </span>
            </div>
          </div>
        </div>

        <div className="bg-tg-secondary-bg rounded-2xl p-4 shadow-sm">
          <h2 className="text-sm font-semibold text-tg-text mb-3">
            Timeline
          </h2>
          <div className="space-y-2 text-xs text-tg-hint">
            <div className="flex justify-between">
              <span>Created</span>
              <span className="text-tg-text">
                {formatDateTime(track.createdAt)}
              </span>
            </div>
            {track.shippedAt && (
              <div className="flex justify-between">
                <span>Shipped</span>
                <span className="text-tg-text">
                  {formatDateTime(track.shippedAt)}
                </span>
              </div>
            )}
            {track.estimatedDeliveryAt && (
              <div className="flex justify-between">
                <span>ETA</span>
                <span className="text-tg-text">
                  {formatDateTime(track.estimatedDeliveryAt)}
                </span>
              </div>
            )}
            {track.actualDeliveryAt && (
              <div className="flex justify-between">
                <span>Delivered</span>
                <span className="text-tg-text">
                  {formatDateTime(track.actualDeliveryAt)}
                </span>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
    </ClientLayout>
  );
};

export default TrackDetails;
