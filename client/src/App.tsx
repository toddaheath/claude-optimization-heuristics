import { lazy, Suspense } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { HomePage } from './pages/HomePage';
import { HistoryPage } from './pages/HistoryPage';
import { ConfigurationsPage } from './pages/ConfigurationsPage';
import { ComparisonPage } from './pages/ComparisonPage';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { ProtectedRoute } from './components/ProtectedRoute';
import { ErrorBoundary, RouteErrorFallback } from './components/ErrorBoundary';
import { Nav } from './components/Nav';

const DocumentationPage = lazy(() =>
  import('./pages/DocumentationPage').then(m => ({ default: m.DocumentationPage }))
);

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      retry: 2,
      retryDelay: (attempt) => Math.min(1000 * 2 ** attempt, 10_000),
    },
  },
});

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route
            path="/*"
            element={
              <ProtectedRoute>
                <div className="min-h-screen bg-gray-100">
                  <Nav />
                  <ErrorBoundary fallback={<RouteErrorFallback />}>
                    <Routes>
                      <Route path="/" element={<HomePage />} />
                      <Route path="/history" element={<HistoryPage />} />
                      <Route path="/compare" element={<ComparisonPage />} />
                      <Route path="/configurations" element={<ConfigurationsPage />} />
                      <Route path="/docs" element={
                        <Suspense fallback={<div className="p-8 text-center text-gray-500">Loading...</div>}>
                          <DocumentationPage />
                        </Suspense>
                      } />
                    </Routes>
                  </ErrorBoundary>
                </div>
              </ProtectedRoute>
            }
          />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
