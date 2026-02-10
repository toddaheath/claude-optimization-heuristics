import { useEffect, useRef } from 'react';
import { useStore } from '../store/useStore';

export function useAnimation() {
  const { iterationHistory, currentIteration, isPlaying, playbackSpeed, setCurrentIteration, setIsPlaying } =
    useStore();
  const frameRef = useRef<number>(0);
  const lastTimeRef = useRef<number>(0);

  useEffect(() => {
    if (!isPlaying || iterationHistory.length === 0) return;

    const interval = Math.max(16, 200 / playbackSpeed);

    const animate = (timestamp: number) => {
      if (timestamp - lastTimeRef.current >= interval) {
        lastTimeRef.current = timestamp;
        setCurrentIteration(currentIteration + 1);

        if (currentIteration + 1 >= iterationHistory.length) {
          setIsPlaying(false);
          return;
        }
      }
      frameRef.current = requestAnimationFrame(animate);
    };

    frameRef.current = requestAnimationFrame(animate);

    return () => {
      if (frameRef.current) cancelAnimationFrame(frameRef.current);
    };
  }, [isPlaying, currentIteration, iterationHistory.length, playbackSpeed, setCurrentIteration, setIsPlaying]);

  return {
    currentFrame: iterationHistory[currentIteration] ?? null,
    totalFrames: iterationHistory.length,
    progress: iterationHistory.length > 0 ? currentIteration / (iterationHistory.length - 1) : 0,
  };
}
