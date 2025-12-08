import axios from 'axios';
import { tokenStorage as clientTokenStorage } from './client';

const API_URL = import.meta.env.VITE_API_URL || 'https://cargosaas-production.up.railway.app/api';

// Manager API client
const managerClient = axios.create({
  baseURL: `${API_URL}/manager`,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Token storage
const MANAGER_TOKEN_KEY = 'manager_auth_token';

export const managerTokenStorage = {
  get: () => localStorage.getItem(MANAGER_TOKEN_KEY),
  set: (token: string) => localStorage.setItem(MANAGER_TOKEN_KEY, token),
  remove: () => localStorage.removeItem(MANAGER_TOKEN_KEY),
  exists: () => !!localStorage.getItem(MANAGER_TOKEN_KEY),
};

// Request interceptor - добавляем JWT token
managerClient.interceptors.request.use(
  (config) => {
    // Сначала пробуем Manager токен, если нет - используем Client токен
    // (для Manager зашедших через Telegram WebApp)
    const token = managerTokenStorage.get() || clientTokenStorage.get();
    if (token) {
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor - обработка ошибок
managerClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Неавторизован - очищаем токен и редиректим на логин
      managerTokenStorage.remove();
      window.location.href = '/manager/login';
    }
    return Promise.reject(error);
  }
);

// Types
export interface TelegramLoginData {
  id: number;
  first_name: string;
  last_name?: string;
  username?: string;
  photo_url?: string;
  auth_date: number;
  hash: string;
}

export interface ManagerAuthResponse {
  token: string;
  userId: string;
  tenantId: string;
  firstName: string;
  username?: string;
  photoUrl?: string;
  role: string;
  isNewUser: boolean;
}

export interface ManagerTrack {
  id: string;
  trackingNumber: string;
  clientCode: string;
  status: string;
  description?: string;
  weight?: number;
  declaredValue?: number;
  originCountry?: string;
  destinationCountry?: string;
  shippedAt?: string;
  estimatedDeliveryAt?: string;
  actualDeliveryAt?: string;
  notes?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateTrackRequest {
  trackingNumber: string;
  clientCode: string;
  status: string;
  description?: string;
  weight?: number;
  declaredValue?: number;
  originCountry?: string;
  destinationCountry?: string;
  shippedAt?: string;
  estimatedDeliveryAt?: string;
  notes?: string;
}

export interface UpdateTrackRequest extends CreateTrackRequest {
  actualDeliveryAt?: string;
}

export interface DashboardStatistics {
  totalTracks: number;
  activeClients: number;
  tracksByStatus: Record<string, number>;
  tracksInTransit: number;
  tracksDeliveredThisWeek: number;
  tracksCreatedToday: number;
  delayedTracks: number;
  recentTracks: Array<{
    id: string;
    trackingNumber: string;
    clientCode: string;
    status: string;
    createdAt: string;
  }>;
}

// API methods
export const managerApi = {
  // Authentication
  auth: {
    loginWithTelegram: async (data: TelegramLoginData): Promise<ManagerAuthResponse> => {
      const response = await managerClient.post<ManagerAuthResponse>('/auth/telegram', data);
      if (response.data.token) {
        managerTokenStorage.set(response.data.token);
      }
      return response.data;
    },
    
    getCurrentUser: async () => {
      const response = await managerClient.get('/auth/me');
      return response.data;
    },
    
    logout: () => {
      managerTokenStorage.remove();
      window.location.href = '/manager/login';
    },
  },

  // Tracks CRUD
  tracks: {
    getAll: async (filters?: {
      search?: string;
      clientCode?: string;
      status?: string;
    }): Promise<ManagerTrack[]> => {
      const params = new URLSearchParams();
      if (filters?.search) params.append('search', filters.search);
      if (filters?.clientCode) params.append('clientCode', filters.clientCode);
      if (filters?.status) params.append('status', filters.status);
      
      const response = await managerClient.get<ManagerTrack[]>('/tracks', { params });
      return response.data;
    },

    getById: async (id: string): Promise<ManagerTrack> => {
      const response = await managerClient.get<ManagerTrack>(`/tracks/${id}`);
      return response.data;
    },

    create: async (data: CreateTrackRequest): Promise<ManagerTrack> => {
      const response = await managerClient.post<ManagerTrack>('/tracks', data);
      return response.data;
    },

    update: async (id: string, data: UpdateTrackRequest): Promise<ManagerTrack> => {
      const response = await managerClient.put<ManagerTrack>(`/tracks/${id}`, data);
      return response.data;
    },

    delete: async (id: string): Promise<void> => {
      await managerClient.delete(`/tracks/${id}`);
    },
  },

  // Statistics
  statistics: {
    getDashboard: async (): Promise<DashboardStatistics> => {
      const response = await managerClient.get<DashboardStatistics>('/statistics/dashboard');
      return response.data;
    },
  },

  // Import (будет использовать существующий endpoint)
  import: {
    uploadExcel: async (file: File): Promise<{
      successCount: number;
      failedCount: number;
      errors: string[];
    }> => {
      const formData = new FormData();
      formData.append('file', file);
      
      const response = await axios.post(
        `${API_URL}/import/tracks`,
        formData,
        {
          headers: {
            'Content-Type': 'multipart/form-data',
            'Authorization': `Bearer ${managerTokenStorage.get()}`,
          },
        }
      );
      return response.data;
    },
  },
};
