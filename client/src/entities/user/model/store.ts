import { queryClient } from "@/shared/lib/queryClient";
import * as SecureStore from "expo-secure-store";
import { create } from "zustand";
import { User, UserState } from "../types";

export const useAuthStore = create<UserState>((set) => ({
  token: null,
  user: null,
  isLoadingSession: true,

  setSession: async (token: string, user: User) => {
    if (__DEV__) {
      console.log("Setting session for user:", user.email);
    }
    try {
      await SecureStore.setItemAsync(
        process.env.EXPO_PUBLIC_TOKEN_STORAGE_KEY!,
        token,
      );
      await SecureStore.setItemAsync(
        process.env.EXPO_PUBLIC_USER_DATA_STORAGE_KEY!,
        JSON.stringify(user),
      );
      set({ token, user, isLoadingSession: false });
    } catch (error) {
      if (__DEV__) console.error(error);
    }
  },

  
  loadSession: async () => {
    try {
      set({ isLoadingSession: true });
      const token = await SecureStore.getItemAsync(
        process.env.EXPO_PUBLIC_TOKEN_STORAGE_KEY!,
      );
      const userJson = await SecureStore.getItemAsync(
        process.env.EXPO_PUBLIC_USER_DATA_STORAGE_KEY!,
      );

      if (token && userJson) {
        const user: User = JSON.parse(userJson);
        set({ token, user, isLoadingSession: false });
      } else {
        set({ token: null, user: null, isLoadingSession: false });
      }
    } catch (error) {
      if (__DEV__) console.error(error);
      set({ token: null, user: null, isLoadingSession: false });
    }
  },

  clearSession: async () => {
    try {
      await SecureStore.deleteItemAsync(
        process.env.EXPO_PUBLIC_TOKEN_STORAGE_KEY!,
      );
      await SecureStore.deleteItemAsync(
        process.env.EXPO_PUBLIC_USER_DATA_STORAGE_KEY!,
      );

      set({ token: null, user: null, isLoadingSession: false });

      queryClient.clear();
    } catch (error) {
      if (__DEV__) console.error(error);
    }
  },
}));
