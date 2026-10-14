import { OfflineBanner } from "@/widgets/offline-banner/ui/OfflineBanner";
import { PropertyList } from "@/widgets/property-list/ui/PropertyList";
import React from "react";
import { StyleSheet, View } from "react-native";

export default function PropertiesScreen() {
  return (
    <View style={styles.container}>
      <OfflineBanner />

      <PropertyList />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: "#f8f9fa",
  },
});
