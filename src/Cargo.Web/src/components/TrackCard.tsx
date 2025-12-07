import type { Track } from '../api/client';
import { formatDateTime } from '../utils/date';

interface TrackCardProps {
  track: Track;
  onClick: () => void;
}

const getStatusConfig = (status: string) => {
  const normalized = status.toLowerCase();

  if (normalized === 'created' || normalized === 'accepted') {
    return {
      label: 'Created',
      badgeClass: 'bg-gray-500/20 text-gray-100',
      dotClass: 'bg-gray-300',
    };
  }

  if (
    normalized === 'intransit' ||
    normalized === 'customsprocessing' ||
    normalized === 'outfordelivery'
  ) {
    return {
      label: 'In transit',
      badgeClass: 'bg-amber-500/20 text-amber-200',
      dotClass: 'bg-amber-400',
    };
  }

  if (normalized === 'arrivedatdestination') {
    return {
      label: 'Arrived',
      badgeClass: 'bg-purple-500/20 text-purple-200',
      dotClass: 'bg-purple-400',
    };
  }

  if (normalized === 'delivered') {
    return {
      label: 'Delivered',
      badgeClass: 'bg-emerald-500/20 text-emerald-200',
      dotClass: 'bg-emerald-400',
    };
  }

  if (
    normalized === 'cancelled' ||
    normalized === 'returnedtosender' ||
    normalized === 'delayed'
  ) {
    return {
      label: 'Issue',
      badgeClass: 'bg-red-500/20 text-red-200',
      dotClass: 'bg-red-400',
    };
  }

  return {
    label: status,
    badgeClass: 'bg-tg-button/10 text-tg-button-text',
    dotClass: 'bg-tg-button',
  };
};

export const TrackCard = ({ track, onClick }: TrackCardProps) => {
  const statusConfig = getStatusConfig(track.status);
  const updatedAt = track.updatedAt || track.createdAt;

  return (
    <button
      className="w-full text-left bg-tg-secondary-bg rounded-2xl p-4 shadow-sm active:scale-[0.99] transition-transform duration-100"
      onClick={onClick}
    >
      <div className="flex items-center justify-between mb-2">
        <span className="text-sm font-mono text-tg-text break-all">
          {track.trackingNumber}
        </span>
        <span
          className={`inline-flex items-center gap-1 text-[10px] px-2 py-1 rounded-full ${statusConfig.badgeClass}`}
        >
          <span className={`w-2 h-2 rounded-full ${statusConfig.dotClass}`} />
          <span>{statusConfig.label}</span>
        </span>
      </div>

      {track.description && (
        <p className="text-xs text-tg-hint mb-1 line-clamp-2">
          {track.description}
        </p>
      )}

      <p className="text-[11px] text-tg-hint">
        Updated {formatDateTime(updatedAt)}
      </p>
    </button>
  );
};

