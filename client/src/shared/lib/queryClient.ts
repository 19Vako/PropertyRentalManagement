import AsyncStorage from "@react-native-async-storage/async-storage";
import NetInfo from "@react-native-community/netinfo";
import { createAsyncStoragePersister } from "@tanstack/query-async-storage-persister";
import { QueryClient, onlineManager } from "@tanstack/react-query";
import { Alert } from "react-native";
import { useSyncStore } from "./syncStore";

onlineManager.setEventListener((setOnline) => {
  return NetInfo.addEventListener((state) => {
    const online = Boolean(state.isConnected && state.isInternetReachable);
    setOnline(online);
    useSyncStore.getState().setIsOffline(!online);
  });
});

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5,
      gcTime: 1000 * 60 * 60 * 24 * 7,
      retry: 3,
      networkMode: "offlineFirst",
    },
    mutations: {
      networkMode: "offlineFirst",
      retry: 3,
    },
  },
});

queryClient.getMutationCache().subscribe((event) => {
  if (event?.type === "updated" && event.action.type === "error") {
    const error: any = event.action.error;
    const serverMessage = error?.response?.data?.message || error?.message;

    if (__DEV__) {
      console.error(
        "Mutation sync failed (Conflict / Validation error):",
        error,
      );
    }

    Alert.alert(
      "Sync Error",
      serverMessage ||
        "The queued change was rejected by the server (data may be outdated or the resource is already occupied).",
      [{ text: "OK" }],
    );
  }
});

queryClient.getMutationCache().subscribe(() => {
  const mutations = queryClient.getMutationCache().getAll();

  const pendingCount = mutations.filter(
    (mutation) =>
      mutation.state.status === "pending" || mutation.state.isPaused,
  ).length;

  useSyncStore.getState().setPendingMutationsCount(pendingCount);
});

export const asyncStoragePersister = createAsyncStoragePersister({
  storage: AsyncStorage,
  key: process.env.EXPO_PUBLIC_PROPERTY_RENTAL_OFFLINE_CACHE,
  throttleTime: 1000,
});
