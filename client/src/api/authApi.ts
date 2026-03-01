import axios from 'axios';
import type { ApiResponse, AuthTokens, LoginRequest, RegisterRequest } from '../types';
import { unwrap } from './utils';

// Separate axios instance — not wrapped with the 401-refresh interceptor
// to prevent infinite loops on the auth endpoints themselves.
const authAxios = axios.create({ baseURL: `${import.meta.env.VITE_API_URL || ''}/api/v1` });

export const authApi = {
  register: (data: RegisterRequest) =>
    authAxios.post<ApiResponse<AuthTokens>>('/auth/register', data).then(unwrap),
  login: (data: LoginRequest) =>
    authAxios.post<ApiResponse<AuthTokens>>('/auth/login', data).then(unwrap),
  refresh: (refreshToken: string) =>
    authAxios.post<ApiResponse<AuthTokens>>('/auth/refresh', { refreshToken }).then(unwrap),
  revoke: (refreshToken: string) =>
    authAxios.post('/auth/revoke', { refreshToken }),
};
