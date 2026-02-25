import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { AuthUser, IterationResult, OptimizationRun } from '../types';

interface AppState {
  currentRun: OptimizationRun | null;
  setCurrentRun: (run: OptimizationRun | null) => void;

  selectedProblemId: string;
  setSelectedProblemId: (id: string) => void;

  // The randomized initial tour shown as grey underlay before/during optimization
  initialRoute: number[];
  setInitialRoute: (route: number[]) => void;

  // True while the background optimization is running (polling active)
  isRunning: boolean;
  setIsRunning: (running: boolean) => void;

  // Animation state
  iterationHistory: IterationResult[];
  currentIteration: number;
  isPlaying: boolean;
  playbackSpeed: number;
  setIterationHistory: (history: IterationResult[]) => void;
  setCurrentIteration: (iteration: number) => void;
  setIsPlaying: (playing: boolean) => void;
  setPlaybackSpeed: (speed: number) => void;

  // Auth state
  accessToken: string | null;
  refreshToken: string | null;
  refreshTokenExpiry: string | null;
  currentUser: AuthUser | null;
  setTokens: (tokens: { accessToken: string; refreshToken: string; refreshTokenExpiry: string }) => void;
  setCurrentUser: (user: AuthUser | null) => void;
  clearAuth: () => void;
}

export const useStore = create<AppState>()(
  persist(
    (set) => ({
      currentRun: null,
      setCurrentRun: (run) =>
        set((state) => ({
          currentRun: run,
          iterationHistory: run?.iterationHistory ?? [],
          currentIteration: 0,
          isPlaying: false,
          isRunning: false,
          selectedProblemId: run?.problemDefinitionId ?? state.selectedProblemId,
        })),

      selectedProblemId: '',
      setSelectedProblemId: (id) => set({ selectedProblemId: id }),

      initialRoute: [],
      setInitialRoute: (route) => set({ initialRoute: route }),

      isRunning: false,
      setIsRunning: (running) => set({ isRunning: running }),

      iterationHistory: [],
      currentIteration: 0,
      isPlaying: false,
      playbackSpeed: 1,
      setIterationHistory: (history) => set({ iterationHistory: history }),
      setCurrentIteration: (iteration) => set({ currentIteration: iteration }),
      setIsPlaying: (playing) => set({ isPlaying: playing }),
      setPlaybackSpeed: (speed) => set({ playbackSpeed: speed }),

      // Auth
      accessToken: null,
      refreshToken: null,
      refreshTokenExpiry: null,
      currentUser: null,
      setTokens: ({ accessToken, refreshToken, refreshTokenExpiry }) =>
        set({ accessToken, refreshToken, refreshTokenExpiry }),
      setCurrentUser: (user) => set({ currentUser: user }),
      clearAuth: () =>
        set({
          accessToken: null,
          refreshToken: null,
          refreshTokenExpiry: null,
          currentUser: null,
        }),
    }),
    {
      name: 'app-auth-storage',
      partialize: (state) => ({
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        refreshTokenExpiry: state.refreshTokenExpiry,
        currentUser: state.currentUser,
      }),
    },
  ),
);
