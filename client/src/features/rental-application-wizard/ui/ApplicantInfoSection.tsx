import React from "react";
import { Control, Controller } from "react-hook-form";
import { StyleSheet, Text, TextInput, View } from "react-native";
import { ApplicationWizardValues } from "../model/schema";

interface ApplicantInfoSectionProps {
  control: Control<ApplicationWizardValues>;
  readOnly: boolean;
}

export const ApplicantInfoSection: React.FC<ApplicantInfoSectionProps> = ({
  control,
  readOnly,
}) => {
  return (
    <View>
      <Text style={styles.sectionTitle}>Application Details</Text>

      <Controller
        control={control}
        name="desiredMoveInDate"
        render={({ field: { onChange, value } }) => (
          <View style={styles.inputContainer}>
            <Text style={styles.label}>Desired Move-in Date (YYYY-MM-DD)</Text>
            <TextInput
              style={styles.input}
              placeholder="YYYY-MM-DD"
              onChangeText={onChange}
              value={value}
              editable={!readOnly}
            />
          </View>
        )}
      />

      <Controller
        control={control}
        name="householdSize"
        render={({ field: { onChange, value } }) => (
          <View style={styles.inputContainer}>
            <Text style={styles.label}>Household Size</Text>
            <TextInput
              style={styles.input}
              placeholder="1"
              // Явно преобразуем введенный текст в число для Zod
              onChangeText={(text) => onChange(text === "" ? 0 : Number(text))}
              value={value !== undefined && value !== null ? String(value) : ""}
              keyboardType="numeric"
              editable={!readOnly}
            />
          </View>
        )}
      />

      <Controller
        control={control}
        name="monthlyIncome"
        render={({ field: { onChange, value } }) => (
          <View style={styles.inputContainer}>
            <Text style={styles.label}>Monthly Income ($)</Text>
            <TextInput
              style={styles.input}
              placeholder="0.00"
              // Явно преобразуем введенный текст в число для Zod
              onChangeText={(text) => onChange(text === "" ? 0 : Number(text))}
              value={value !== undefined && value !== null ? String(value) : ""}
              keyboardType="numeric"
              editable={!readOnly}
            />
          </View>
        )}
      />

      <Controller
        control={control}
        name="applicantMessage"
        render={({ field: { onChange, value } }) => (
          <View style={styles.inputContainer}>
            <Text style={styles.label}>Applicant Message (Optional)</Text>
            <TextInput
              style={[styles.input, styles.textArea]}
              placeholder="Tell us a bit about yourself..."
              onChangeText={onChange}
              value={value || ""}
              multiline
              numberOfLines={4}
              editable={!readOnly}
            />
          </View>
        )}
      />
    </View>
  );
};

const styles = StyleSheet.create({
  sectionTitle: { fontSize: 20, fontWeight: "bold", marginBottom: 16 },
  inputContainer: { marginBottom: 14 },
  label: { fontSize: 13, fontWeight: "600", color: "#374151", marginBottom: 6 },
  input: {
    borderWidth: 1,
    borderColor: "#d1d5db",
    padding: 12,
    borderRadius: 8,
    backgroundColor: "#f9fafb",
    fontSize: 15,
  },
  textArea: { height: 100, textAlignVertical: "top" },
});
