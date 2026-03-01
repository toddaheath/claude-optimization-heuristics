import { lazy, Suspense } from 'react';
import { BrowserRouter, Routes, Route, NavLink, useNavigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { HomePage } from './pages/HomePage';
import { HistoryPage } from './pages/HistoryPage';
import { ConfigurationsPage } from './pages/ConfigurationsPage';
import { ComparisonPage } from './pages/ComparisonPage';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { ProtectedRoute } from './components/ProtectedRoute';
import { useStore } from './store/useStore';
import { authApi } from './api/authApi';

const DocumentationPage = lazy(() =>
  import('./pages/DocumentationPage').then(m => ({ default: m.DocumentationPage }))
);

const queryClient = new QueryClient({
  defaultOptions: { queries: { staleTime: 30_000 } },
});

function Nav() {
  const navigate = useNavigate();
  const { currentUser, refreshToken, clearAuth } = useStore();

  const linkClass = ({ isActive }: { isActive: boolean }) =>
    `px-4 py-2 rounded-lg text-sm font-medium ${
      isActive ? 'bg-blue-600 text-white' : 'text-gray-600 hover:bg-gray-100'
    }`;

  async function handleLogout() {
    if (refreshToken) {
      authApi.revoke(refreshToken).catch(() => {
        // best-effort
      });
    }
    clearAuth();
    queryClient.clear();
    navigate('/login');
  }

  return (
    <nav className="flex items-center gap-2 p-4 border-b bg-white">
      <span className="font-bold text-lg mr-4">Optimization Heuristics</span>
      <NavLink to="/" className={linkClass}>
        Home
      </NavLink>
      <NavLink to="/history" className={linkClass}>
        History
      </NavLink>
      <NavLink to="/compare" className={linkClass}>
        Compare
      </NavLink>
      <NavLink to="/configurations" className={linkClass}>
        Configurations
      </NavLink>
      <NavLink to="/docs" className={linkClass}>
        Docs
      </NavLink>
      <div className="ml-auto flex items-center gap-3">
        {currentUser && (
          <span className="text-sm text-gray-500">{currentUser.displayName}</span>
        )}
        <button
          onClick={handleLogout}
          className="px-3 py-1.5 text-sm rounded-lg border border-gray-300 text-gray-600 hover:bg-gray-100"
        >
          Sign Out
        </button>
      </div>
    </nav>
  );
}

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
                </div>
              </ProtectedRoute>
            }
          />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
