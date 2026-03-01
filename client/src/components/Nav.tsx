import { NavLink, useNavigate } from 'react-router-dom';
import { useQueryClient } from '@tanstack/react-query';
import { useStore } from '../store/useStore';
import { authApi } from '../api/authApi';

export function Nav() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { currentUser, refreshToken, clearAuth } = useStore();

  const linkClass = ({ isActive }: { isActive: boolean }) =>
    `px-4 py-2 rounded-lg text-sm font-medium ${
      isActive ? 'bg-blue-600 text-white' : 'text-gray-600 hover:bg-gray-100'
    }`;

  function handleLogout() {
    if (refreshToken) {
      authApi.revoke(refreshToken).catch(() => {
        // best-effort
      });
    }
    clearAuth();
    queryClient.clear();
    void navigate('/login');
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
