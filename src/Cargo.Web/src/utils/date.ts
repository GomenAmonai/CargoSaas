export const formatDateTime = (value?: string | null): string => {
  if (!value) return '—';

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '—';

  const now = new Date();
  const isSameDay = date.toDateString() === now.toDateString();

  const yesterday = new Date();
  yesterday.setDate(now.getDate() - 1);
  const isYesterday = date.toDateString() === yesterday.toDateString();

  const timeFormatter = new Intl.DateTimeFormat(undefined, {
    hour: '2-digit',
    minute: '2-digit',
  });

  const fullFormatter = new Intl.DateTimeFormat(undefined, {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });

  if (isSameDay) {
    return `Today, ${timeFormatter.format(date)}`;
  }

  if (isYesterday) {
    return `Yesterday, ${timeFormatter.format(date)}`;
  }

  return fullFormatter.format(date);
};

