import { memo, useMemo } from 'react';
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
import type { IterationResult } from '../types';

interface Props {
  history: IterationResult[];
  currentIteration: number;
}

export const ConvergenceChart = memo(function ConvergenceChart({ history, currentIteration }: Props) {
  const { hasCurrentDistance, data } = useMemo(() => {
    if (history.length === 0) return { hasCurrentDistance: false, data: [] as { iteration: number; best: number; current?: number }[] };

    const visible = history.slice(0, currentIteration + 1);

    // Only show the current-distance line when data is present and non-zero
    // (old runs stored before this field was added will have currentDistance === 0)
    const hasCurrentDistance = visible.some((h) => h.currentDistance > 0);

    const data = visible.map((h) => ({
      iteration: h.iteration + 1, // 1-based for display
      best: Math.round(h.bestDistance * 100) / 100,
      ...(hasCurrentDistance
        ? { current: Math.round(h.currentDistance * 100) / 100 }
        : {}),
    }));

    return { hasCurrentDistance, data };
  }, [history, currentIteration]);

  if (history.length === 0) return null;

  return (
    <div className="bg-white p-4 rounded-lg border border-gray-300">
      <h3 className="text-sm font-semibold mb-2 text-gray-700">Convergence</h3>
      <ResponsiveContainer width="100%" height={200}>
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
              typeof value === 'number' ? value.toFixed(2) : '—',
              name === 'best' ? 'Best distance' : 'Current distance',
            ]}
            labelFormatter={(label) => `Iteration ${label}`}
          />
          {hasCurrentDistance && <Legend
            formatter={(value) => (value === 'best' ? 'Best so far' : 'Current iteration')}
            wrapperStyle={{ fontSize: 11 }}
          />}

          {/* Current iteration distance — noisy grey line behind the best line */}
          {hasCurrentDistance && (
            <Line
              type="monotone"
              dataKey="current"
              stroke="#9ca3af"
              strokeWidth={1}
              strokeDasharray="4 2"
              dot={false}
              isAnimationActive={false}
            />
          )}

          {/* Best-so-far distance — solid blue, always on top */}
          <Line
            type="monotone"
            dataKey="best"
            stroke="#3b82f6"
            strokeWidth={2}
            dot={false}
            isAnimationActive={false}
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
});
