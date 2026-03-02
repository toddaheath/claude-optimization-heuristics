import axios from 'axios';
import type { AxiosError, InternalAxiosRequestConfig } from 'axios';
import type {
  ApiResponse,
  PaginatedResponse,
  ProblemDefinition,
  AlgorithmConfiguration,
  OptimizationRun,
  RunProgressResponse,
  City,
  AlgorithmType,
} from '../types';
import { authApi } from './authApi';
import { unwrap } from './utils';
import { useStore } from '../store/useStore';
import { decodeJwtPayload } from '../utils/jwt';

interface RetryableRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

const api = axios.create({ baseURL: `${import.meta.env.VITE_API_URL || ''}/api/v1` });

// Token storage trade-off: Tokens are stored in Zustand (persisted to localStorage) for
// SPA convenience. Moving to httpOnly cookies would improve XSS resilience but requires
// backend changes (cookie-based auth flow, CSRF protection). Acceptable for current scope.
api.interceptors.request.use((config) => {
  const { accessToken } = useStore.getState();
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`;
  }
  return config;
});

// --- Response interceptor: refresh on 401 ---
let isRefreshing = false;
let failedQueue: { resolve: (token: string) => void; reject: (err: unknown) => void }[] = [];

function processQueue(error: unknown, token: string | null) {
  failedQueue.forEach(({ resolve, reject }) => {
    if (error) {
      reject(error);
    } else {
      resolve(token!);
    }
  });
  failedQueue = [];
}

api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as RetryableRequestConfig | undefined;

    if (error.response?.status !== 401 || !originalRequest || originalRequest._retry) {
      return Promise.reject(new Error(error.message));
    }

    if (isRefreshing) {
      return new Promise<string>((resolve, reject) => {
        failedQueue.push({ resolve, reject });
      }).then((token) => {
        originalRequest.headers.Authorization = `Bearer ${token}`;
        return api(originalRequest);
      });
    }

    originalRequest._retry = true;
    isRefreshing = true;

    const { refreshToken, setTokens, setCurrentUser, clearAuth } = useStore.getState();

    if (!refreshToken) {
      clearAuth();
      window.location.href = '/login';
      return Promise.reject(new Error(error.message));
    }

    try {
      const tokens = await authApi.refresh(refreshToken);
      setTokens(tokens);

      // Decode basic user info from the new access token (sub, email, displayName)
      const payload = decodeJwtPayload(tokens.accessToken);
      setCurrentUser({
        id: payload.sub,
        email: payload.email,
        displayName: payload.displayName ?? '',
      });

      processQueue(null, tokens.accessToken);
      originalRequest.headers.Authorization = `Bearer ${tokens.accessToken}`;
      return api(originalRequest);
    } catch (refreshError) {
      processQueue(refreshError, null);
      clearAuth();
      window.location.href = '/login';
      return Promise.reject(new Error(refreshError instanceof Error ? refreshError.message : 'Token refresh failed'));
    } finally {
      isRefreshing = false;
    }
  },
);

export const problemApi = {
  getAll: () =>
    api.get<ApiResponse<PaginatedResponse<ProblemDefinition>>>('/problem-definitions', { params: { pageSize: 100 } })
      .then(unwrap).then((r) => r.items),
  getById: (id: string) => api.get<ApiResponse<ProblemDefinition>>(`/problem-definitions/${id}`).then(unwrap),
  create: (data: { name: string; description?: string; cities: City[] }) =>
    api.post<ApiResponse<ProblemDefinition>>('/problem-definitions', data).then(unwrap),
  delete: (id: string) => api.delete(`/problem-definitions/${id}`),
};

export const configApi = {
  getAll: () =>
    api.get<ApiResponse<PaginatedResponse<AlgorithmConfiguration>>>('/algorithm-configurations', { params: { pageSize: 100 } })
      .then(unwrap).then((r) => r.items),
  getById: (id: string) =>
    api.get<ApiResponse<AlgorithmConfiguration>>(`/algorithm-configurations/${id}`).then(unwrap),
  create: (data: {
    name: string;
    description?: string;
    algorithmType: AlgorithmType;
    parameters: Record<string, number>;
    maxIterations: number;
  }) => api.post<ApiResponse<AlgorithmConfiguration>>('/algorithm-configurations', data).then(unwrap),
  update: (
    id: string,
    data: {
      name: string;
      description?: string;
      algorithmType: AlgorithmType;
      parameters: Record<string, number>;
      maxIterations: number;
    },
  ) => api.put<ApiResponse<AlgorithmConfiguration>>(`/algorithm-configurations/${id}`, data).then(unwrap),
  delete: (id: string) => api.delete(`/algorithm-configurations/${id}`),
};

export const runApi = {
  run: (data: { algorithmConfigurationId: string; problemDefinitionId: string }) =>
    api.post<ApiResponse<OptimizationRun>>('/optimization-runs', data).then(unwrap),
  getProgress: (id: string) =>
    api.get<ApiResponse<RunProgressResponse>>(`/optimization-runs/${id}/progress`).then(unwrap),
  getAll: (page = 1, pageSize = 20) =>
    api.get<ApiResponse<PaginatedResponse<OptimizationRun>>>('/optimization-runs', { params: { page, pageSize } }).then(unwrap),
  getById: (id: string) => api.get<ApiResponse<OptimizationRun>>(`/optimization-runs/${id}`).then(unwrap),
  delete: (id: string) => api.delete(`/optimization-runs/${id}`),
};
