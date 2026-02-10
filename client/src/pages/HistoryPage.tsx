import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { runApi } from '../api/client';
import { useStore } from '../store/useStore';
import { useNavigate } from 'react-router-dom';
import { RunStatus } from '../types';
import type { OptimizationRun } from '../types';

export function HistoryPage() {
  const queryClient = useQueryClient();
  const { setCurrentRun } = useStore();
  const navigate = useNavigate();

  const { data: runs, isLoading } = useQuery({
    queryKey: ['runs'],
    queryFn: () => runApi.getAll(),
  });

  const deleteRun = useMutation({
    mutationFn: runApi.delete,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['runs'] }),
  });

  const loadRun = async (id: string) => {
    const run = await runApi.getById(id);
    setCurrentRun(run);
    navigate('/');
  };

  return (
    <div className="max-w-screen-xl mx-auto p-6">
      <h1 className="text-2xl font-bold mb-4">Run History</h1>

      {isLoading && <p className="text-gray-500">Loading...</p>}

      <div className="overflow-x-auto">
        <table className="w-full border-collapse">
          <thead>
            <tr className="bg-gray-100 text-left text-sm">
              <th className="p-3 border-b">Date</th>
              <th className="p-3 border-b">Status</th>
              <th className="p-3 border-b">Best Distance</th>
              <th className="p-3 border-b">Iterations</th>
              <th className="p-3 border-b">Time (ms)</th>
              <th className="p-3 border-b">Actions</th>
            </tr>
          </thead>
          <tbody>
            {runs?.map((run: OptimizationRun) => (
              <tr key={run.id} className="hover:bg-gray-50">
                <td className="p-3 border-b text-sm">{new Date(run.createdAt).toLocaleString()}</td>
                <td className="p-3 border-b">
                  <span
                    className={`px-2 py-1 rounded text-xs font-medium ${
                      run.status === RunStatus.Completed
                        ? 'bg-green-100 text-green-800'
                        : run.status === RunStatus.Failed
                          ? 'bg-red-100 text-red-800'
                          : 'bg-gray-100 text-gray-800'
                    }`}
                  >
                    {run.status}
                  </span>
                </td>
                <td className="p-3 border-b text-sm">{run.bestDistance?.toFixed(2) ?? '-'}</td>
                <td className="p-3 border-b text-sm">{run.totalIterations}</td>
                <td className="p-3 border-b text-sm">{run.executionTimeMs}</td>
                <td className="p-3 border-b">
                  <button
                    onClick={() => loadRun(run.id)}
                    className="text-blue-600 hover:text-blue-800 text-sm mr-3"
                  >
                    Replay
                  </button>
                  <button
                    onClick={() => deleteRun.mutate(run.id)}
                    className="text-red-600 hover:text-red-800 text-sm"
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {runs?.length === 0 && <p className="text-gray-500 mt-4">No runs yet. Go to Home to run an optimization.</p>}
    </div>
  );
}
