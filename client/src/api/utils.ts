import type { ApiResponse } from '../types';

export function unwrap<T>(response: { data: ApiResponse<T> }): T {
  if (!response.data.success) {
    throw new Error(response.data.errors.join(', '));
  }
  return response.data.data!;
}
