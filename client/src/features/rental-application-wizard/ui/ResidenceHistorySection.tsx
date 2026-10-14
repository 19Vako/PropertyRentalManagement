import React from "react";
import { Control, useFieldArray } from "react-hook-form";
import { StyleSheet, Text, TouchableOpacity, View } from "react-native";
import { ApplicationWizardValues } from "../model/schema";

interface ResidenceHistorySectionProps {
  control: Control<ApplicationWizardValues>;
  /** When true, add/edit/remove actions are hidden. */
  readOnly: boolean;
  onAdd: () => void;
  onEdit: (index: number) => void;
}

export const ResidenceHistorySection: React.FC<ResidenceHistorySectionProps> = ({
  control,
  readOnly,
  onAdd,
  onEdit,
}) => {
  const { fields, remove } = useFieldArray({ control, name: "residences" });

  return (
    <View>
      <Text style={styles.sectionTitle}>Residence History</Text>
      {!readOnly && (
        <TouchableOpacity style={styles.addBtn} onPress={onAdd}>
          <Text style={styles.addBtnText}>+ Add Residence</Text>
        </TouchableOpacity>
      )}

      {fields.map((field, index) => (
        <View key={field.id} style={styles.residenceCard}>
          <Text style={styles.resAddress}>{field.address}</Text>
          <Text>
            Landlord: {field.landlordName} ({field.landlordPhone})
          </Text>
          <Text>
            From: {field.moveInDate} To: {field.moveOutDate}
          </Text>
          {!readOnly && (
            <View style={styles.resActions}>
              <TouchableOpacity onPress={() => onEdit(index)}>
                <Text style={styles.actionEdit}>Edit</Text>
              </TouchableOpacity>
              <TouchableOpacity onPress={() => remove(index)}>
                <Text style={styles.actionDelete}>Remove</Text>
              </TouchableOpacity>
            </View>
          )}
        </View>
      ))}
    </View>
  );
};

const styles = StyleSheet.create({
  sectionTitle: { fontSize: 20, fontWeight: "bold", marginBottom: 16 },
  addBtn: {
    backgroundColor: "#e5e7eb",
    padding: 10,
    borderRadius: 8,
    alignItems: "center",
    marginBottom: 12,
  },
  addBtnText: { fontWeight: "600", color: "#374151" },
  residenceCard: {
    padding: 12,
    borderWidth: 1,
    borderColor: "#e5e7eb",
    borderRadius: 8,
    marginBottom: 10,
    backgroundColor: "#f9fafb",
  },
  resAddress: { fontWeight: "bold", fontSize: 16 },
  resActions: { flexDirection: "row", gap: 15, marginTop: 8 },
  actionEdit: { color: "#2563eb", fontWeight: "600" },
  actionDelete: { color: "#dc2626", fontWeight: "600" },
});
