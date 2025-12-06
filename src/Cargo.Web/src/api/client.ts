import axios from 'axios';
import WebApp from '@twa-dev/sdk';

const API_URL = import.meta.env.VITE_API_URL || 'https://cargosaas-production.up.railway.app/api';

// Token storage keys
const TOKEN_KEY = 'cargo_auth_token';

// Создаем экземпляр Axios с базовой конфигурацией
const apiClient = axios.create({
  baseURL: API_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor для добавления JWT токена к каждому запросу
apiClient.interceptors.request.use(
  (config) => {
    // 1. Проверяем наличие JWT токена в localStorage
    const token = localStorage.getItem(TOKEN_KEY);
    
    if (token) {
      // Добавляем Authorization header с JWT
      config.headers['Authorization'] = `Bearer ${token}`;
    } else if (WebApp.initData) {
      // Fallback: если нет токена, отправляем initData (для первичной авторизации)
      config.headers['X-Telegram-Init-Data'] = WebApp.initData;
    }

    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Interceptor для обработки ответов
apiClient.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    // Обработка ошибок
    if (error.response) {
      // Сервер ответил с кодом ошибки
      console.error('API Error:', error.response.status, error.response.data);
      
      if (error.response.status === 401) {
        // Неавторизован - показываем уведомление через Telegram
        WebApp.showAlert('Authorization failed. Please restart the app.');
      }
    } else if (error.request) {
      // Запрос был отправлен, но ответа не получено
      console.error('Network Error:', error.request);
      WebApp.showAlert('Network error. Please check your connection.');
    } else {
      // Что-то пошло не так при настройке запроса
      console.error('Error:', error.message);
    }

    return Promise.reject(error);
  }
);

export default apiClient;

// Token management utilities
export const tokenStorage = {
  get: () => localStorage.getItem(TOKEN_KEY),
  set: (token: string) => localStorage.setItem(TOKEN_KEY, token),
  remove: () => localStorage.removeItem(TOKEN_KEY),
  exists: () => !!localStorage.getItem(TOKEN_KEY),
};

// API Types
export interface AuthResponse {
  token: string;
  user: {
    id: string;
    telegramId: number;
    firstName: string;
    lastName?: string;
    username?: string;
    photoUrl?: string;
    role: string;
    tenantId?: string;
  };
}

export interface Track {
  id: string;
  trackingNumber: string;
  clientCode: string;
  status: string;
  weight?: number;
  description?: string;
  originCountry?: string;
  destinationCountry?: string;
  shippedAt?: string;
  estimatedDeliveryAt?: string;
  actualDeliveryAt?: string;
  createdAt: string;
  updatedAt?: string;
}

// API функции
export const api = {
  // Аутентификация через Telegram
  auth: {
    login: async (initData: string): Promise<AuthResponse> => {
      const response = await apiClient.post<AuthResponse>('client/auth', { initData });
      
      // Автоматически сохраняем токен
      if (response.data.token) {
        tokenStorage.set(response.data.token);
      }
      
      return response.data;
    },
    
    logout: () => {
      tokenStorage.remove();
      // Можно добавить редирект или показать Telegram уведомление
      WebApp.showAlert('You have been logged out');
    },
  },

  // Треки
  tracks: {
    getAll: async (): Promise<Track[]> => {
      const response = await apiClient.get<Track[]>('tracks');
      return response.data;
    },
    
    getById: async (id: string): Promise<Track> => {
      const response = await apiClient.get<Track>(`tracks/${id}`);
      return response.data;
    },
  },
};

