import { useQuery } from '@tanstack/react-query';
import { useStore } from '../store/useStore';
import { useAnimation } from '../hooks/useAnimation';
import { problemApi } from '../api/client';
import { ConfigurationPanel } from '../components/ConfigurationPanel';
import { TspCanvas } from '../components/TspCanvas';
import { CanvasControls } from '../components/CanvasControls';
import { ConvergenceChart } from '../components/ConvergenceChart';
import { RunStatus } from '../types';

export function HomePage() {
  const { currentRun, iterationHistory, currentIteration } = useStore();
  const { currentFrame } = useAnimation();

  const { data: problem } = useQuery({
    queryKey: ['problem', currentRun?.problemDefinitionId],
    queryFn: () => problemApi.getById(currentRun!.problemDefinitionId),
    enabled: !!currentRun?.problemDefinitionId,
  });

  return (
    <div className="flex gap-6 p-6 max-w-screen-xl mx-auto">
      <div className="w-80 shrink-0">
        <ConfigurationPanel />
      </div>

      <div className="flex-1 space-y-4">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-bold">TSP Visualization</h1>
          {currentRun && (
            <div className="text-sm text-gray-600">
              <span
                className={`px-2 py-1 rounded text-xs font-medium ${
                  currentRun.status === RunStatus.Completed
                    ? 'bg-green-100 text-green-800'
                    : currentRun.status === RunStatus.Running
                      ? 'bg-yellow-100 text-yellow-800'
                      : 'bg-gray-100 text-gray-800'
                }`}
              >
                {currentRun.status}
              </span>
              {currentRun.bestDistance != null && (
                <span className="ml-2">Best: {currentRun.bestDistance.toFixed(2)}</span>
              )}
              {currentRun.executionTimeMs > 0 && <span className="ml-2">({currentRun.executionTimeMs}ms)</span>}
            </div>
          )}
        </div>

        <TspCanvas cities={problem?.cities ?? []} currentFrame={currentFrame} width={700} height={500} />

        <CanvasControls />

        <ConvergenceChart history={iterationHistory} currentIteration={currentIteration} />
      </div>
    </div>
  );
}
