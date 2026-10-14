import React from "react";
import { FlatList, StyleSheet, Text, View } from "react-native";
import { StatusHistoryItem } from "../types";

interface ApplicationHistoryProps {
  history: StatusHistoryItem[];
}

export const ApplicationHistory: React.FC<ApplicationHistoryProps> = ({
  history,
}) => {
  if (!history || history.length === 0) {
    return (
      <View style={styles.container}>
        <Text style={styles.title}>Status History</Text>
        <Text style={styles.emptyText}>No history records available.</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <Text style={styles.title}>Status History & Audit Log</Text>
      <FlatList
        data={history}
        keyExtractor={(item) => item.id || Math.random().toString()}
        renderItem={({ item }) => (
          <View style={styles.card}>
            <View style={styles.row}>
              <Text style={styles.statusLabel}>Status: {item.status}</Text>
              <Text style={styles.dateText}>
                {new Date(item.changedAt).toLocaleString()}
              </Text>
            </View>
            {item.outcome && (
              <Text style={styles.outcomeText}>Outcome: {item.outcome}</Text>
            )}
            <Text style={styles.userText}>By: {item.changedBy}</Text>
            {item.comment ? (
              <Text style={styles.commentText}>
                Comment: &quot;{item.comment}&quot;
              </Text>
            ) : null}
          </View>
        )}
      />
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    marginTop: 16,
    backgroundColor: "#fff",
    padding: 16,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: "#e5e7eb",
  },
  title: { fontSize: 18, fontWeight: "bold", marginBottom: 12 },
  emptyText: { color: "#9ca3af", fontStyle: "italic" },
  card: {
    padding: 12,
    backgroundColor: "#f9fafb",
    borderRadius: 8,
    marginBottom: 10,
    borderWidth: 1,
    borderColor: "#f3f4f6",
  },
  row: {
    flexDirection: "row",
    justifyContent: "space-between",
    marginBottom: 4,
  },
  statusLabel: { fontWeight: "bold", color: "#1f2937" },
  dateText: { fontSize: 12, color: "#6b7280" },
  outcomeText: { fontWeight: "600", color: "#2563eb", marginBottom: 2 },
  userText: { fontSize: 12, color: "#4b5563", marginBottom: 4 },
  commentText: { fontStyle: "italic", color: "#374151", fontSize: 13 },
});
