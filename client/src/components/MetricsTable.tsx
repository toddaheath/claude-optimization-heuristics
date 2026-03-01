import { memo, useMemo } from 'react';
import type { OptimizationRun, AlgorithmConfiguration } from '../types';
import { ALGORITHM_LABELS } from '../types';

interface Props {
  runs: OptimizationRun[];
  configs: AlgorithmConfiguration[];
}

export const MetricsTable = memo(function MetricsTable({ runs, configs }: Props) {
  const configMap = useMemo(() => new Map(configs.map((c) => [c.id, c])), [configs]);
  const bestIdx = runs.reduce(
    (min, r, i) =>
      r.bestDistance != null && (min === -1 || r.bestDistance < runs[min].bestDistance!)
        ? i
        : min,
    -1,
  );

  const formatMs = (ms: number) => (ms < 1000 ? `${ms} ms` : `${(ms / 1000).toFixed(2)} s`);

  const rows: { label: string; values: (string | null)[] }[] = [
    {
      label: 'Algorithm',
      values: runs.map((r) => {
        const cfg = configMap.get(r.algorithmConfigurationId);
        return cfg ? ALGORITHM_LABELS[cfg.algorithmType] : '–';
      }),
    },
    {
      label: 'Config Name',
      values: runs.map((r) => configMap.get(r.algorithmConfigurationId)?.name ?? '–'),
    },
    {
      label: 'Best Distance',
      values: runs.map((r) => (r.bestDistance != null ? r.bestDistance.toFixed(2) : '–')),
    },
    {
      label: 'Total Iterations',
      values: runs.map((r) => r.totalIterations.toLocaleString()),
    },
    {
      label: 'Execution Time',
      values: runs.map((r) => formatMs(r.executionTimeMs)),
    },
    {
      label: 'Improvement %',
      values: runs.map((r) => {
        const initial = r.iterationHistory?.[0]?.bestDistance;
        if (initial && r.bestDistance && initial > 0) {
          return `${(((initial - r.bestDistance) / initial) * 100).toFixed(2)}%`;
        }
        return '–';
      }),
    },
  ];

  return (
    <div className="bg-white p-4 rounded-lg border border-gray-300">
      <h3 className="text-sm font-semibold mb-2 text-gray-700">Metrics Comparison</h3>
      <div className="overflow-x-auto">
        <table className="w-full border-collapse text-sm">
          <thead>
            <tr className="bg-gray-50">
              <th className="p-2 border-b text-left text-gray-500 font-medium">Metric</th>
              {runs.map((_, i) => (
                <th key={i} className="p-2 border-b text-left font-medium text-gray-700">
                  Run {i + 1}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <tr key={row.label} className="hover:bg-gray-50">
                <td className="p-2 border-b text-gray-500">{row.label}</td>
                {row.values.map((val, i) => (
                  <td
                    key={i}
                    className={`p-2 border-b ${
                      row.label === 'Best Distance' && i === bestIdx
                        ? 'text-green-700 font-bold'
                        : 'text-gray-900'
                    }`}
                  >
                    {val}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
});
