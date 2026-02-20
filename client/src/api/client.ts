import axios from 'axios';
import type {
  ApiResponse,
  ProblemDefinition,
  AlgorithmConfiguration,
  OptimizationRun,
  RunProgressResponse,
  City,
  AlgorithmType,
} from '../types';

const api = axios.create({ baseURL: `${import.meta.env.VITE_API_URL || ''}/api/v1` });

function unwrap<T>(response: { data: ApiResponse<T> }): T {
  if (!response.data.success) {
    throw new Error(response.data.errors.join(', '));
  }
  return response.data.data!;
}

export const problemApi = {
  getAll: () => api.get<ApiResponse<ProblemDefinition[]>>('/problem-definitions').then(unwrap),
  getById: (id: string) => api.get<ApiResponse<ProblemDefinition>>(`/problem-definitions/${id}`).then(unwrap),
  create: (data: { name: string; description?: string; cities: City[] }) =>
    api.post<ApiResponse<ProblemDefinition>>('/problem-definitions', data).then(unwrap),
  delete: (id: string) => api.delete(`/problem-definitions/${id}`),
};

export const configApi = {
  getAll: () => api.get<ApiResponse<AlgorithmConfiguration[]>>('/algorithm-configurations').then(unwrap),
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
    api.get<ApiResponse<OptimizationRun[]>>('/optimization-runs', { params: { page, pageSize } }).then(unwrap),
  getById: (id: string) => api.get<ApiResponse<OptimizationRun>>(`/optimization-runs/${id}`).then(unwrap),
  delete: (id: string) => api.delete(`/optimization-runs/${id}`),
};
