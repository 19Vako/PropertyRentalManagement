import { ApplicationListWidget } from "@/widgets/application-list/ui/ApplicationListWidget";
import { OfflineBanner } from "@/widgets/offline-banner/ui/OfflineBanner";
import React from "react";
import { StyleSheet, View } from "react-native";

export default function ApplicationsScreen() {
  return (
    <View style={styles.container}>
      <OfflineBanner />

      <ApplicationListWidget />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: "#f8f9fa",
  },
});
