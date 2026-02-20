import { useEffect } from 'react';
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
  } = useStore();
  const { totalFrames } = useAnimation();

  const lastFrame = Math.max(0, totalFrames - 1);

  const stepBack = () => {
    setCurrentIteration(Math.max(0, currentIteration - 1));
    setIsPlaying(false);
  };

  const stepForward = () => {
    setCurrentIteration(Math.min(lastFrame, currentIteration + 1));
    setIsPlaying(false);
  };

  const reset = () => {
    setCurrentIteration(0);
    setIsPlaying(false);
  };

  // Keyboard shortcuts
  useEffect(() => {
    if (totalFrames === 0) return;

    const handler = (e: KeyboardEvent) => {
      // Ignore when typing in an input or select
      const tag = (e.target as HTMLElement).tagName;
      if (tag === 'INPUT' || tag === 'SELECT' || tag === 'TEXTAREA') return;

      if (e.key === ' ') {
        e.preventDefault();
        setIsPlaying(!isPlaying);
      } else if (e.key === 'ArrowRight') {
        e.preventDefault();
        stepForward();
      } else if (e.key === 'ArrowLeft') {
        e.preventDefault();
        stepBack();
      } else if (e.key === 'Home') {
        e.preventDefault();
        reset();
      } else if (e.key === 'End') {
        e.preventDefault();
        setCurrentIteration(lastFrame);
        setIsPlaying(false);
      }
    };

    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [totalFrames, isPlaying, currentIteration]);

  if (totalFrames === 0) return null;

  return (
    <div className="space-y-2">
      <div className="flex items-center gap-2 p-3 bg-gray-50 rounded-lg">
        {/* Reset */}
        <button
          onClick={reset}
          title="Reset (Home)"
          className="px-3 py-2 bg-gray-200 rounded hover:bg-gray-300 text-sm"
        >
          ⏮
        </button>

        {/* Step back */}
        <button
          onClick={stepBack}
          disabled={currentIteration === 0}
          title="Step back (←)"
          className="px-3 py-2 bg-gray-200 rounded hover:bg-gray-300 text-sm disabled:opacity-40"
        >
          ◀
        </button>

        {/* Play / Pause */}
        <button
          onClick={() => setIsPlaying(!isPlaying)}
          title="Play / Pause (Space)"
          className="px-5 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 font-medium text-sm min-w-[80px]"
        >
          {isPlaying ? '⏸ Pause' : '▶ Play'}
        </button>

        {/* Step forward */}
        <button
          onClick={stepForward}
          disabled={currentIteration >= lastFrame}
          title="Step forward (→)"
          className="px-3 py-2 bg-gray-200 rounded hover:bg-gray-300 text-sm disabled:opacity-40"
        >
          ▶
        </button>

        {/* Scrubber */}
        <input
          type="range"
          min={0}
          max={lastFrame}
          value={currentIteration}
          onChange={(e) => {
            setCurrentIteration(Number(e.target.value));
            setIsPlaying(false);
          }}
          className="flex-1"
        />

        {/* Frame counter */}
        <span className="text-sm text-gray-600 whitespace-nowrap tabular-nums">
          {currentIteration + 1} / {totalFrames}
        </span>

        {/* Speed */}
        <select
          value={playbackSpeed}
          onChange={(e) => setPlaybackSpeed(Number(e.target.value))}
          className="px-2 py-1 border rounded text-sm bg-white"
        >
          <option value={0.5}>0.5×</option>
          <option value={1}>1×</option>
          <option value={2}>2×</option>
          <option value={5}>5×</option>
          <option value={10}>10×</option>
        </select>
      </div>

      <p className="text-xs text-gray-400 px-1">
        Keyboard: <kbd className="bg-gray-100 px-1 rounded">Space</kbd> play/pause ·{' '}
        <kbd className="bg-gray-100 px-1 rounded">←</kbd>
        <kbd className="bg-gray-100 px-1 rounded">→</kbd> step ·{' '}
        <kbd className="bg-gray-100 px-1 rounded">Home</kbd> reset ·{' '}
        <kbd className="bg-gray-100 px-1 rounded">End</kbd> last frame
      </p>
    </div>
  );
}
