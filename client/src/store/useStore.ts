import { create } from 'zustand';
import type { IterationResult, OptimizationRun } from '../types';

interface AppState {
  currentRun: OptimizationRun | null;
  setCurrentRun: (run: OptimizationRun | null) => void;

  // Animation state
  iterationHistory: IterationResult[];
  currentIteration: number;
  isPlaying: boolean;
  playbackSpeed: number;
  setIterationHistory: (history: IterationResult[]) => void;
  setCurrentIteration: (iteration: number) => void;
  setIsPlaying: (playing: boolean) => void;
  setPlaybackSpeed: (speed: number) => void;
}

export const useStore = create<AppState>((set) => ({
  currentRun: null,
  setCurrentRun: (run) =>
    set({
      currentRun: run,
      iterationHistory: run?.iterationHistory ?? [],
      currentIteration: 0,
      isPlaying: false,
    }),

  iterationHistory: [],
  currentIteration: 0,
  isPlaying: false,
  playbackSpeed: 1,
  setIterationHistory: (history) => set({ iterationHistory: history }),
  setCurrentIteration: (iteration) => set({ currentIteration: iteration }),
  setIsPlaying: (playing) => set({ isPlaying: playing }),
  setPlaybackSpeed: (speed) => set({ playbackSpeed: speed }),
}));
