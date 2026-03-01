import { useState, useEffect, useRef } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { runApi, configApi } from '../api/client';
import { useStore } from '../store/useStore';
import { useNavigate } from 'react-router-dom';
import { RunStatus, ALGORITHM_LABELS } from '../types';
import type { OptimizationRun, AlgorithmConfiguration } from '../types';

interface RunDetailsModalProps {
  run: OptimizationRun;
  config?: AlgorithmConfiguration;
  onClose: () => void;
}

function RunDetailsModal({ run, config, onClose }: RunDetailsModalProps) {
  const closeRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    closeRef.current?.focus();
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [onClose]);

  const initialDistance = run.iterationHistory?.[0]?.bestDistance;
  const finalDistance = run.bestDistance;
  const improvementPct =
    initialDistance && finalDistance && initialDistance > 0
      ? (((initialDistance - finalDistance) / initialDistance) * 100).toFixed(2)
      : null;

  const formatMs = (ms: number) => {
    if (ms < 1000) return `${ms} ms`;
    return `${(ms / 1000).toFixed(2)} s`;
  };

  const row = (label: string, value: React.ReactNode) => (
    <div className="flex items-start py-2 border-b last:border-0">
      <span className="w-48 text-sm text-gray-500 shrink-0">{label}</span>
      <span className="text-sm font-medium text-gray-900">{value}</span>
    </div>
  );

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40" onClick={onClose}>
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="run-details-title"
        className="bg-white rounded-xl shadow-2xl w-full max-w-lg mx-4 p-6 space-y-4"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between">
          <h2 id="run-details-title" className="text-lg font-bold">Run Details</h2>
          <button ref={closeRef} onClick={onClose} className="text-gray-400 hover:text-gray-600 text-xl leading-none">&times;</button>
        </div>

        <div className="divide-y divide-gray-100">
          {row('Run ID', <span className="font-mono text-xs text-gray-600 break-all">{run.id}</span>)}
          {row('Status',
            <span className={`px-2 py-0.5 rounded text-xs font-semibold ${
              run.status === RunStatus.Completed ? 'bg-green-100 text-green-800' :
              run.status === RunStatus.Failed    ? 'bg-red-100 text-red-800' :
                                                   'bg-gray-100 text-gray-700'
            }`}>{run.status}</span>
          )}
          {row('Algorithm', config
            ? ALGORITHM_LABELS[config.algorithmType] ?? config.algorithmType
            : <span className="text-gray-400 italic">–</span>
          )}
          {row('Configuration', config
            ? config.name
            : <span className="font-mono text-xs text-gray-500">{run.algorithmConfigurationId}</span>
          )}
          {row('Max Iterations', config?.maxIterations?.toLocaleString() ?? '–')}
          {row('Completed Iterations', run.totalIterations.toLocaleString())}
          {row('Initial Tour Distance',
            initialDistance != null
              ? initialDistance.toFixed(4)
              : <span className="text-gray-400 italic">–</span>
          )}
          {row('Optimized Distance',
            finalDistance != null
              ? finalDistance.toFixed(4)
              : <span className="text-gray-400 italic">–</span>
          )}
          {row('Improvement',
            improvementPct != null
              ? <span className="text-green-700 font-bold">▼ {improvementPct}%</span>
              : <span className="text-gray-400 italic">–</span>
          )}
          {row('Execution Time', formatMs(run.executionTimeMs))}
          {row('Created', new Date(run.createdAt).toLocaleString())}
          {row('Last Updated', new Date(run.updatedAt).toLocaleString())}
          {run.status === RunStatus.Failed && row('Error',
            <span className="text-red-600 text-xs break-words">
              {run.errorMessage ?? 'Unknown error'}
            </span>
          )}
        </div>

        {config && Object.keys(config.parameters).length > 0 && (
          <div>
            <p className="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-1">Parameters</p>
            <div className="bg-gray-50 rounded-lg p-3 text-xs font-mono space-y-1">
              {Object.entries(config.parameters).map(([k, v]) => (
                <div key={k} className="flex justify-between">
                  <span className="text-gray-500">{k}</span>
                  <span className="text-gray-800">{v}</span>
                </div>
              ))}
            </div>
          </div>
        )}

        <button
          onClick={onClose}
          className="w-full py-2 bg-gray-100 hover:bg-gray-200 rounded-lg text-sm font-medium"
        >
          Close
        </button>
      </div>
    </div>
  );
}

export function HistoryPage() {
  const queryClient = useQueryClient();
  const { setCurrentRun } = useStore();
  const navigate = useNavigate();

  const [detailsRun, setDetailsRun] = useState<OptimizationRun | null>(null);
  const [detailsLoading, setDetailsLoading] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [confirmingDeleteId, setConfirmingDeleteId] = useState<string | null>(null);
  const pageSize = 20;

  const { data: runs, isLoading } = useQuery({
    queryKey: ['runs', page],
    queryFn: () => runApi.getAll(page, pageSize),
  });

  const { data: configs } = useQuery({
    queryKey: ['configs'],
    queryFn: () => configApi.getAll(),
  });

  const deleteRun = useMutation({
    mutationFn: runApi.delete,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['runs'] }),
  });

  const loadRun = async (id: string) => {
    try {
      setError(null);
      const run = await runApi.getById(id);
      setCurrentRun(run);
      navigate('/');
    } catch {
      setError('Failed to load run. Please try again.');
    }
  };

  const openDetails = async (run: OptimizationRun) => {
    try {
      setError(null);
      setDetailsLoading(run.id);
      const full = await runApi.getById(run.id);
      setDetailsRun(full);
    } catch {
      setError('Failed to load run details. Please try again.');
    } finally {
      setDetailsLoading(null);
    }
  };

  const configMap = new Map<string, AlgorithmConfiguration>(
    configs?.map((c: AlgorithmConfiguration) => [c.id, c]) ?? []
  );

  return (
    <div className="max-w-screen-xl mx-auto p-6">
      <h1 className="text-2xl font-bold mb-4">Run History</h1>

      {error && (
        <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
          {error}
        </div>
      )}

      {isLoading && <p className="text-gray-500">Loading...</p>}

      <div className="overflow-x-auto">
        <table className="w-full border-collapse">
          <thead>
            <tr className="bg-gray-100 text-left text-sm">
              <th className="p-3 border-b">Date</th>
              <th className="p-3 border-b">Status</th>
              <th className="p-3 border-b">Algorithm</th>
              <th className="p-3 border-b">Best Distance</th>
              <th className="p-3 border-b">Iterations</th>
              <th className="p-3 border-b">Time</th>
              <th className="p-3 border-b">Actions</th>
            </tr>
          </thead>
          <tbody>
            {runs?.map((run: OptimizationRun) => {
              const cfg = configMap.get(run.algorithmConfigurationId);
              return (
                <tr key={run.id} className="hover:bg-gray-50">
                  <td className="p-3 border-b text-sm">{new Date(run.createdAt).toLocaleString()}</td>
                  <td className="p-3 border-b">
                    <span className={`px-2 py-1 rounded text-xs font-medium ${
                      run.status === RunStatus.Completed ? 'bg-green-100 text-green-800' :
                      run.status === RunStatus.Failed    ? 'bg-red-100 text-red-800' :
                      run.status === RunStatus.Running   ? 'bg-orange-100 text-orange-800' :
                                                           'bg-gray-100 text-gray-800'
                    }`}>
                      {run.status}
                    </span>
                  </td>
                  <td className="p-3 border-b text-sm text-gray-600">
                    {cfg ? ALGORITHM_LABELS[cfg.algorithmType] : '–'}
                  </td>
                  <td className="p-3 border-b text-sm">{run.bestDistance?.toFixed(2) ?? '–'}</td>
                  <td className="p-3 border-b text-sm">{run.totalIterations.toLocaleString()}</td>
                  <td className="p-3 border-b text-sm">
                    {run.executionTimeMs < 1000
                      ? `${run.executionTimeMs} ms`
                      : `${(run.executionTimeMs / 1000).toFixed(1)} s`}
                  </td>
                  <td className="p-3 border-b space-x-3">
                    <button onClick={() => openDetails(run)} disabled={detailsLoading === run.id} className="text-gray-600 hover:text-gray-900 text-sm disabled:opacity-50">{detailsLoading === run.id ? 'Loading…' : 'Details'}</button>
                    <button onClick={() => loadRun(run.id)} className="text-blue-600 hover:text-blue-800 text-sm">Replay</button>
                    {confirmingDeleteId === run.id ? (
                      <button
                        onClick={() => { deleteRun.mutate(run.id); setConfirmingDeleteId(null); }}
                        className="text-red-700 font-semibold hover:underline text-xs"
                      >
                        Confirm?
                      </button>
                    ) : (
                      <button
                        onClick={() => setConfirmingDeleteId(run.id)}
                        className="text-red-500 hover:underline text-xs"
                      >
                        Delete
                      </button>
                    )}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      {runs && runs.length > 0 && (
        <div className="flex items-center justify-between mt-4">
          <button
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page <= 1}
            className="px-4 py-2 bg-gray-100 hover:bg-gray-200 rounded-lg text-sm font-medium disabled:opacity-50"
          >
            Previous
          </button>
          <span className="text-sm text-gray-600">Page {page}</span>
          <button
            onClick={() => setPage((p) => p + 1)}
            disabled={runs.length < pageSize}
            className="px-4 py-2 bg-gray-100 hover:bg-gray-200 rounded-lg text-sm font-medium disabled:opacity-50"
          >
            Next
          </button>
        </div>
      )}

      {runs?.length === 0 && page === 1 && (
        <p className="text-gray-500 mt-4">No runs yet. Go to Home to run an optimization.</p>
      )}

      {detailsRun && (
        <RunDetailsModal
          run={detailsRun}
          config={configMap.get(detailsRun.algorithmConfigurationId)}
          onClose={() => setDetailsRun(null)}
        />
      )}
    </div>
  );
}
