import { useSyncStore } from "@/shared/lib/syncStore";
import React from "react";
import { StyleSheet, Text, View } from "react-native";

export const OfflineBanner: React.FC = () => {
  const isOffline = useSyncStore((state) => state.isOffline);
  const pendingCount = useSyncStore((state) => state.pendingMutationsCount);

  if (!isOffline && pendingCount === 0) {
    return null;
  }

  return (
    <View
      style={[styles.banner, isOffline ? styles.offlineBg : styles.syncingBg]}
    >
      <Text style={styles.text}>
        {isOffline
          ? `Offline mode. Pending changes: ${pendingCount}`
          : `Syncing data... Remaining: ${pendingCount}`}
      </Text>
    </View>
  );
};

const styles = StyleSheet.create({
  banner: {
    width: "100%",
    paddingVertical: 8,
    paddingHorizontal: 16,
    alignItems: "center",
    justifyContent: "center",
    zIndex: 9999,
  },
  offlineBg: {
    backgroundColor: "#b91c1c",
  },
  syncingBg: {
    backgroundColor: "#d97706",
  },
  text: {
    color: "#ffffff",
    fontSize: 14,
    fontWeight: "600",
    textAlign: "center",
  },
});
