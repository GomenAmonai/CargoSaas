import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { managerApi } from '../../api/manager';
import ManagerLayout from '../../components/manager/ManagerLayout';

const TrackForm = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEditMode = !!id;

  // Form state (простой useState для каждого поля)
  const [trackingNumber, setTrackingNumber] = useState('');
  const [clientCode, setClientCode] = useState('');
  const [status, setStatus] = useState('Created');
  const [description, setDescription] = useState('');
  const [weight, setWeight] = useState('');
  const [declaredValue, setDeclaredValue] = useState('');
  const [originCountry, setOriginCountry] = useState('');
  const [destinationCountry, setDestinationCountry] = useState('');
  const [shippedAt, setShippedAt] = useState('');
  const [estimatedDeliveryAt, setEstimatedDeliveryAt] = useState('');
  const [actualDeliveryAt, setActualDeliveryAt] = useState('');
  const [notes, setNotes] = useState('');

  const [isLoading, setIsLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Загружаем трек для редактирования
  useEffect(() => {
    if (isEditMode && id) {
      loadTrack(id);
    }
  }, [id, isEditMode]);

  const loadTrack = async (trackId: string) => {
    try {
      setIsLoading(true);
      const track = await managerApi.tracks.getById(trackId);
      
      setTrackingNumber(track.trackingNumber);
      setClientCode(track.clientCode);
      setStatus(track.status);
      setDescription(track.description || '');
      setWeight(track.weight?.toString() || '');
      setDeclaredValue(track.declaredValue?.toString() || '');
      setOriginCountry(track.originCountry || '');
      setDestinationCountry(track.destinationCountry || '');
      setShippedAt(track.shippedAt ? track.shippedAt.split('T')[0] : '');
      setEstimatedDeliveryAt(track.estimatedDeliveryAt ? track.estimatedDeliveryAt.split('T')[0] : '');
      setActualDeliveryAt(track.actualDeliveryAt ? track.actualDeliveryAt.split('T')[0] : '');
      setNotes(track.notes || '');
    } catch (err) {
      console.error('Failed to load track:', err);
      setError('Failed to load track');
    } finally {
      setIsLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Простая валидация
    if (!trackingNumber.trim()) {
      alert('Tracking number is required');
      return;
    }
    if (!clientCode.trim()) {
      alert('Client code is required');
      return;
    }

    try {
      setIsSaving(true);
      setError(null);

      const data = {
        trackingNumber: trackingNumber.trim(),
        clientCode: clientCode.trim(),
        status,
        description: description.trim() || undefined,
        weight: weight ? parseFloat(weight) : undefined,
        declaredValue: declaredValue ? parseFloat(declaredValue) : undefined,
        originCountry: originCountry.trim() || undefined,
        destinationCountry: destinationCountry.trim() || undefined,
        shippedAt: shippedAt || undefined,
        estimatedDeliveryAt: estimatedDeliveryAt || undefined,
        actualDeliveryAt: actualDeliveryAt || undefined,
        notes: notes.trim() || undefined,
      };

      if (isEditMode && id) {
        await managerApi.tracks.update(id, data);
      } else {
        await managerApi.tracks.create(data);
      }

      navigate('/manager/tracks');
    } catch (err: any) {
      console.error('Failed to save track:', err);
      const errorMessage = err.response?.data?.message || 'Failed to save track';
      setError(errorMessage);
    } finally {
      setIsSaving(false);
    }
  };

  const handleCancel = () => {
    navigate('/manager/tracks');
  };

  const statuses = [
    'Created',
    'Accepted',
    'InTransit',
    'CustomsProcessing',
    'ArrivedAtDestination',
    'OutForDelivery',
    'Delivered',
    'Delayed',
    'ReturnedToSender',
    'Cancelled',
  ];

  if (isLoading) {
    return (
      <ManagerLayout>
        <div className="p-8">
          <div className="animate-pulse space-y-4 max-w-2xl">
            <div className="h-8 bg-gray-200 rounded w-1/4"></div>
            <div className="space-y-3">
              {[1, 2, 3, 4, 5].map((i) => (
                <div key={i} className="h-12 bg-gray-200 rounded"></div>
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
        <div className="mb-6">
          <h1 className="text-3xl font-bold text-gray-900 mb-2">
            {isEditMode ? 'Edit Track' : 'Create New Track'}
          </h1>
          <p className="text-gray-600">
            {isEditMode ? 'Update track information' : 'Fill in the details to create a new track'}
          </p>
        </div>

        {/* Error */}
        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 mb-6 text-red-700">
            {error}
          </div>
        )}

        {/* Form */}
        <form onSubmit={handleSubmit} className="max-w-2xl">
          <div className="bg-white border border-gray-200 rounded-xl p-6 space-y-6">
            {/* Tracking Number */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Tracking Number <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={trackingNumber}
                onChange={(e) => setTrackingNumber(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="TR-001"
                required
              />
            </div>

            {/* Client Code */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Client Code <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={clientCode}
                onChange={(e) => setClientCode(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="CLT-12345678"
                required
              />
            </div>

            {/* Status */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Status
              </label>
              <select
                value={status}
                onChange={(e) => setStatus(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                {statuses.map((s) => (
                  <option key={s} value={s}>
                    {s}
                  </option>
                ))}
              </select>
            </div>

            {/* Description */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Description
              </label>
              <textarea
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                rows={3}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Brief description of the shipment..."
              />
            </div>

            {/* Weight & Value */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Weight (kg)
                </label>
                <input
                  type="number"
                  step="0.01"
                  value={weight}
                  onChange={(e) => setWeight(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="2.5"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Declared Value ($)
                </label>
                <input
                  type="number"
                  step="0.01"
                  value={declaredValue}
                  onChange={(e) => setDeclaredValue(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="100.00"
                />
              </div>
            </div>

            {/* Countries */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Origin Country
                </label>
                <input
                  type="text"
                  value={originCountry}
                  onChange={(e) => setOriginCountry(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="China"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Destination Country
                </label>
                <input
                  type="text"
                  value={destinationCountry}
                  onChange={(e) => setDestinationCountry(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Russia"
                />
              </div>
            </div>

            {/* Dates */}
            <div className="grid grid-cols-3 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Shipped Date
                </label>
                <input
                  type="date"
                  value={shippedAt}
                  onChange={(e) => setShippedAt(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Est. Delivery
                </label>
                <input
                  type="date"
                  value={estimatedDeliveryAt}
                  onChange={(e) => setEstimatedDeliveryAt(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              {isEditMode && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Actual Delivery
                  </label>
                  <input
                    type="date"
                    value={actualDeliveryAt}
                    onChange={(e) => setActualDeliveryAt(e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
              )}
            </div>

            {/* Notes */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Notes
              </label>
              <textarea
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                rows={3}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Additional notes or comments..."
              />
            </div>
          </div>

          {/* Actions */}
          <div className="flex items-center gap-3 mt-6">
            <button
              type="submit"
              disabled={isSaving}
              className="px-6 py-3 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 transition-colors disabled:bg-blue-400 disabled:cursor-not-allowed flex items-center gap-2"
            >
              {isSaving ? (
                <>
                  <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
                  <span>Saving...</span>
                </>
              ) : (
                <>
                  <span>💾</span>
                  <span>{isEditMode ? 'Update Track' : 'Create Track'}</span>
                </>
              )}
            </button>
            <button
              type="button"
              onClick={handleCancel}
              disabled={isSaving}
              className="px-6 py-3 bg-gray-100 text-gray-700 rounded-lg font-medium hover:bg-gray-200 transition-colors disabled:opacity-50"
            >
              Cancel
            </button>
          </div>
        </form>
      </div>
    </ManagerLayout>
  );
};

export default TrackForm;
