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
  const { currentRun, iterationHistory, currentIteration, selectedProblemId } = useStore();
  const { currentFrame, totalFrames } = useAnimation();

  // Show cities as soon as a problem is selected — not just after a run completes.
  const problemId = selectedProblemId || currentRun?.problemDefinitionId;
  const { data: problem } = useQuery({
    queryKey: ['problem', problemId],
    queryFn: () => problemApi.getById(problemId!),
    enabled: !!problemId,
  });

  const cities = problem?.cities ?? [];

  // Route is "complete" when the run finished and we're on the final animation frame
  const isComplete =
    currentRun?.status === RunStatus.Completed &&
    totalFrames > 0 &&
    currentIteration >= totalFrames - 1;

  // Improvement % relative to the naive sequential tour's first-iteration distance
  const initialDistance = iterationHistory[0]?.bestDistance;
  const currentDistance = currentFrame?.bestDistance;
  const improvementPct =
    initialDistance && currentDistance && initialDistance > 0
      ? (((initialDistance - currentDistance) / initialDistance) * 100).toFixed(1)
      : null;

  return (
    <div className="flex gap-6 p-6 max-w-screen-xl mx-auto">
      <div className="w-80 shrink-0">
        <ConfigurationPanel />
      </div>

      <div className="flex-1 space-y-4">
        {/* Header row */}
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-bold">TSP Visualization</h1>

          <div className="flex items-center gap-3 text-sm text-gray-600">
            {currentRun && (
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
            )}
            {currentFrame && (
              <>
                <span>
                  Iter <strong>{currentFrame.iteration + 1}</strong>/{iterationHistory.length}
                </span>
                <span>
                  Dist <strong>{currentFrame.bestDistance.toFixed(2)}</strong>
                </span>
                {improvementPct !== null && (
                  <span className="text-green-700 font-semibold">▼ {improvementPct}%</span>
                )}
                {currentRun?.executionTimeMs != null && currentRun.executionTimeMs > 0 && (
                  <span className="text-gray-400">{currentRun.executionTimeMs}ms</span>
                )}
              </>
            )}
            {!currentFrame && cities.length > 0 && (
              <span className="text-gray-400 text-xs italic">
                {cities.length} cities · sequential tour
              </span>
            )}
          </div>
        </div>

        <TspCanvas
          cities={cities}
          currentFrame={currentFrame}
          isComplete={isComplete}
          width={700}
          height={500}
        />

        <CanvasControls />

        <ConvergenceChart history={iterationHistory} currentIteration={currentIteration} />
      </div>
    </div>
  );
}
