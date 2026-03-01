export interface JwtPayload {
  sub: string;
  email: string;
  displayName?: string;
}

export function decodeJwtPayload(token: string): JwtPayload {
  try {
    const base64 = token.split('.')[1];
    const json = atob(base64);
    return JSON.parse(json) as JwtPayload;
  } catch {
    throw new Error('Invalid JWT token format');
  }
}
