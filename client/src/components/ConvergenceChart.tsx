import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import type { IterationResult } from '../types';

interface Props {
  history: IterationResult[];
  currentIteration: number;
}

export function ConvergenceChart({ history, currentIteration }: Props) {
  if (history.length === 0) return null;

  const data = history.slice(0, currentIteration + 1).map((h) => ({
    iteration: h.iteration,
    distance: Math.round(h.bestDistance * 100) / 100,
  }));

  return (
    <div className="bg-white p-4 rounded-lg border border-gray-300">
      <h3 className="text-sm font-semibold mb-2 text-gray-700">Convergence</h3>
      <ResponsiveContainer width="100%" height={200}>
        <LineChart data={data}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="iteration" tick={{ fontSize: 10 }} />
          <YAxis tick={{ fontSize: 10 }} />
          <Tooltip />
          <Line type="monotone" dataKey="distance" stroke="#3b82f6" dot={false} strokeWidth={2} />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}
