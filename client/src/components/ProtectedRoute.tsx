import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useStore } from '../store/useStore';

interface ProtectedRouteProps {
  children: ReactNode;
}

export function ProtectedRoute({ children }: ProtectedRouteProps) {
  const { accessToken, refreshTokenExpiry } = useStore();
  const location = useLocation();

  const isAuthenticated =
    accessToken !== null &&
    refreshTokenExpiry !== null &&
    new Date(refreshTokenExpiry) > new Date();

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return <>{children}</>;
}
