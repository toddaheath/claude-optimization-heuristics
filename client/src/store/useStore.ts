import { create } from 'zustand';
import type { IterationResult, OptimizationRun } from '../types';

interface AppState {
  currentRun: OptimizationRun | null;
  setCurrentRun: (run: OptimizationRun | null) => void;

  selectedProblemId: string;
  setSelectedProblemId: (id: string) => void;

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
    set((state) => ({
      currentRun: run,
      iterationHistory: run?.iterationHistory ?? [],
      currentIteration: 0,
      isPlaying: false,
      // Sync selectedProblemId from run (covers history replay)
      selectedProblemId: run?.problemDefinitionId ?? state.selectedProblemId,
    })),

  selectedProblemId: '',
  setSelectedProblemId: (id) => set({ selectedProblemId: id }),

  iterationHistory: [],
  currentIteration: 0,
  isPlaying: false,
  playbackSpeed: 1,
  setIterationHistory: (history) => set({ iterationHistory: history }),
  setCurrentIteration: (iteration) => set({ currentIteration: iteration }),
  setIsPlaying: (playing) => set({ isPlaying: playing }),
  setPlaybackSpeed: (speed) => set({ playbackSpeed: speed }),
}));
