import { create } from "zustand";

interface SyncState {
  pendingMutationsCount: number;
  isOffline: boolean;
  setPendingMutationsCount: (count: number) => void;
  setIsOffline: (isOffline: boolean) => void;
}

export const useSyncStore = create<SyncState>((set) => ({
  pendingMutationsCount: 0,
  isOffline: false,
  setPendingMutationsCount: (count) => set({ pendingMutationsCount: count }),
  setIsOffline: (isOffline) => set({ isOffline }),
}));
