import { useStore } from '../store/useStore';
import { useAnimation } from '../hooks/useAnimation';

export function CanvasControls() {
  const {
    isPlaying,
    setIsPlaying,
    playbackSpeed,
    setPlaybackSpeed,
    currentIteration,
    setCurrentIteration,
    iterationHistory,
  } = useStore();
  const { totalFrames } = useAnimation();

  if (totalFrames === 0) return null;

  return (
    <div className="flex items-center gap-4 p-3 bg-gray-50 rounded-lg">
      <button
        onClick={() => setIsPlaying(!isPlaying)}
        className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 font-medium"
      >
        {isPlaying ? 'Pause' : 'Play'}
      </button>

      <button
        onClick={() => {
          setCurrentIteration(0);
          setIsPlaying(false);
        }}
        className="px-3 py-2 bg-gray-200 rounded hover:bg-gray-300"
      >
        Reset
      </button>

      <input
        type="range"
        min={0}
        max={Math.max(0, iterationHistory.length - 1)}
        value={currentIteration}
        onChange={(e) => {
          setCurrentIteration(Number(e.target.value));
          setIsPlaying(false);
        }}
        className="flex-1"
      />

      <span className="text-sm text-gray-600 whitespace-nowrap">
        {currentIteration} / {totalFrames - 1}
      </span>

      <select
        value={playbackSpeed}
        onChange={(e) => setPlaybackSpeed(Number(e.target.value))}
        className="px-2 py-1 border rounded text-sm"
      >
        <option value={0.5}>0.5x</option>
        <option value={1}>1x</option>
        <option value={2}>2x</option>
        <option value={5}>5x</option>
        <option value={10}>10x</option>
      </select>
    </div>
  );
}
