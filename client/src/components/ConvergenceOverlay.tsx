import { useMemo } from 'react';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';
import type { OptimizationRun, AlgorithmConfiguration } from '../types';
import { ALGORITHM_LABELS } from '../types';

interface Props {
  runs: OptimizationRun[];
  configs: AlgorithmConfiguration[];
}

const COLORS = ['#3b82f6', '#ef4444', '#22c55e', '#f59e0b'];

export function ConvergenceOverlay({ runs, configs }: Props) {
  const configMap = new Map(configs.map((c) => [c.id, c]));

  const maxLen = Math.max(...runs.map((r) => r.iterationHistory?.length ?? 0));

  const data = useMemo(() => {
    if (maxLen === 0) return [];
    return Array.from({ length: maxLen }, (_, i) => {
      const point: Record<string, number> = { iteration: i + 1 };
      runs.forEach((r, ri) => {
        const h = r.iterationHistory?.[i];
        if (h) {
          point[`run${ri}`] = Math.round(h.bestDistance * 100) / 100;
        }
      });
      return point;
    });
  }, [runs, maxLen]);

  if (maxLen === 0) return null;

  return (
    <div className="bg-white p-4 rounded-lg border border-gray-300">
      <h3 className="text-sm font-semibold mb-2 text-gray-700">Convergence Comparison</h3>
      <ResponsiveContainer width="100%" height={300}>
        <LineChart data={data} margin={{ top: 4, right: 8, left: 0, bottom: 0 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
          <XAxis
            dataKey="iteration"
            tick={{ fontSize: 10 }}
            label={{ value: 'Iteration', position: 'insideBottomRight', offset: -4, fontSize: 10 }}
          />
          <YAxis tick={{ fontSize: 10 }} width={60} />
          <Tooltip
            formatter={(value: number | undefined, name: string | undefined) => [
              typeof value === 'number' ? value.toFixed(2) : '–',
              (() => {
                const idx = parseInt((name ?? '').replace('run', ''), 10);
                const cfg = configMap.get(runs[idx]?.algorithmConfigurationId);
                return cfg ? ALGORITHM_LABELS[cfg.algorithmType] : `Run ${idx + 1}`;
              })(),
            ]}
            labelFormatter={(label) => `Iteration ${label}`}
          />
          <Legend
            formatter={(value: string) => {
              const idx = parseInt(value.replace('run', ''), 10);
              const cfg = configMap.get(runs[idx]?.algorithmConfigurationId);
              return cfg ? ALGORITHM_LABELS[cfg.algorithmType] : `Run ${idx + 1}`;
            }}
            wrapperStyle={{ fontSize: 11 }}
          />
          {runs.map((_, i) => (
            <Line
              key={i}
              type="monotone"
              dataKey={`run${i}`}
              stroke={COLORS[i % COLORS.length]}
              strokeWidth={2}
              dot={false}
              isAnimationActive={false}
              connectNulls
            />
          ))}
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}
