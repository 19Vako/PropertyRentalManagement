import React from "react";
import { StyleSheet, Text, View } from "react-native";
import { ApplicationWizardValues } from "../model/schema";

interface SummarySectionProps {
  values: ApplicationWizardValues;
}

export const SummarySection: React.FC<SummarySectionProps> = ({ values }) => {
  const { desiredMoveInDate, householdSize, monthlyIncome, applicantMessage } =
    values;

  return (
    <View>
      <Text style={styles.sectionTitle}>Summary & Review</Text>
      <View style={styles.summaryCard}>
        <Text style={styles.summaryHeader}>Rental Application Details</Text>
        <Text style={styles.textItem}>
          Desired Move-in Date: {desiredMoveInDate}
        </Text>
        <Text style={styles.textItem}>Household Size: {householdSize}</Text>
        <Text style={styles.textItem}>Monthly Income: ${monthlyIncome}</Text>
        <Text style={styles.textItem}>
          Message: {applicantMessage || "No message provided"}
        </Text>
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  sectionTitle: { fontSize: 20, fontWeight: "bold", marginBottom: 16 },
  summaryCard: {
    padding: 16,
    backgroundColor: "#f3f4f6",
    borderRadius: 8,
    marginBottom: 12,
    gap: 8,
  },
  summaryHeader: {
    fontWeight: "bold",
    fontSize: 16,
    marginBottom: 4,
    color: "#1f2937",
  },
  textItem: { fontSize: 14, color: "#4b5563" },
});
