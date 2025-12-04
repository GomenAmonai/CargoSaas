import axios from 'axios';
import WebApp from '@twa-dev/sdk';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

// Создаем экземпляр Axios с базовой конфигурацией
const apiClient = axios.create({
  baseURL: API_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor для добавления Telegram initData к каждому запросу
apiClient.interceptors.request.use(
  (config) => {
    // Добавляем Telegram initData для авторизации
    if (WebApp.initData) {
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

// API функции
export const api = {
  // Аутентификация через Telegram
  auth: {
    login: async (initData: string) => {
      const response = await apiClient.post('/client/auth', { initData });
      return response.data;
    },
  },

  // Треки
  tracks: {
    getAll: async () => {
      const response = await apiClient.get('/tracks');
      return response.data;
    },
    
    getById: async (id: string) => {
      const response = await apiClient.get(`/tracks/${id}`);
      return response.data;
    },
  },
};

