import { BrowserRouter, Routes, Route, NavLink } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { HomePage } from './pages/HomePage';
import { HistoryPage } from './pages/HistoryPage';
import { ConfigurationsPage } from './pages/ConfigurationsPage';
import { DocumentationPage } from './pages/DocumentationPage';

const queryClient = new QueryClient({
  defaultOptions: { queries: { staleTime: 30_000 } },
});

function Nav() {
  const linkClass = ({ isActive }: { isActive: boolean }) =>
    `px-4 py-2 rounded-lg text-sm font-medium ${
      isActive ? 'bg-blue-600 text-white' : 'text-gray-600 hover:bg-gray-100'
    }`;

  return (
    <nav className="flex items-center gap-2 p-4 border-b bg-white">
      <span className="font-bold text-lg mr-4">Optimization Heuristics</span>
      <NavLink to="/" className={linkClass}>
        Home
      </NavLink>
      <NavLink to="/history" className={linkClass}>
        History
      </NavLink>
      <NavLink to="/configurations" className={linkClass}>
        Configurations
      </NavLink>
      <NavLink to="/docs" className={linkClass}>
        Docs
      </NavLink>
    </nav>
  );
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <div className="min-h-screen bg-gray-100">
          <Nav />
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/history" element={<HistoryPage />} />
            <Route path="/configurations" element={<ConfigurationsPage />} />
            <Route path="/docs" element={<DocumentationPage />} />
          </Routes>
        </div>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
