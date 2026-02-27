import { useEffect, useRef } from 'react';
import { useStore } from '../store/useStore';

export function useAnimation() {
  const { iterationHistory, currentIteration, isPlaying, playbackSpeed, setCurrentIteration, setIsPlaying } =
    useStore();
  const frameRef = useRef<number>(0);
  const lastTimeRef = useRef<number>(0);
  const historyLenRef = useRef(iterationHistory.length);

  useEffect(() => {
    historyLenRef.current = iterationHistory.length;
  }, [iterationHistory.length]);

  useEffect(() => {
    if (!isPlaying || iterationHistory.length === 0) return;

    const interval = Math.max(16, 200 / playbackSpeed);

    const animate = (timestamp: number) => {
      if (timestamp - lastTimeRef.current >= interval) {
        lastTimeRef.current = timestamp;
        setCurrentIteration((prev: number) => {
          const next = prev + 1;
          if (next >= historyLenRef.current) {
            setIsPlaying(false);
            return prev;
          }
          return next;
        });
      }
      frameRef.current = requestAnimationFrame(animate);
    };

    frameRef.current = requestAnimationFrame(animate);

    return () => {
      if (frameRef.current) cancelAnimationFrame(frameRef.current);
    };
  }, [isPlaying, playbackSpeed, setCurrentIteration, setIsPlaying, iterationHistory.length]);

  return {
    currentFrame: iterationHistory[currentIteration] ?? null,
    totalFrames: iterationHistory.length,
    progress: iterationHistory.length > 0 ? currentIteration / (iterationHistory.length - 1) : 0,
  };
}
