import { Property } from "@/entities/property/types";
import { Unit } from "@/entities/unit/types";
import { useRouter } from "expo-router";
import React from "react";
import {
  FlatList,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from "react-native";

interface ApplicantUnitListViewProps {
  properties: Property[];
  units: Unit[];
}

export const ApplicantUnitListView: React.FC<ApplicantUnitListViewProps> = ({
  properties,
  units,
}) => {
  const router = useRouter();

  const handleApply = (unitId: number) => {
    router.push({
      pathname: "/application/new",
      params: { unitId: String(unitId) },
    });
  };

  const getPropertyById = (propertyId: number | string) =>
    properties.find((p) => String(p.id) === String(propertyId));

  return (
    <View style={styles.container}>
      <Text style={styles.header}>Available Properties & Units</Text>
      <FlatList
        data={units}
        keyExtractor={(item) => item.id.toString()}
        ListEmptyComponent={
          <Text style={styles.noUnits}>No available units found.</Text>
        }
        renderItem={({ item: unit }) => {
          const property = getPropertyById(unit.propertyId);
          const propertyName = property?.name || "Property";
          const location = property?.city;

          const rentFormatted = `$${unit.monthlyRent?.toLocaleString("en-US", {
            minimumFractionDigits: 2,
          })}`;

          return (
            <View style={styles.card}>
              <View style={styles.cardHeader}>
                <View style={styles.cardHeaderText}>
                  <Text style={styles.propertyName}>{propertyName}</Text>
                  <Text style={styles.propertyAddress}>{location}</Text>
                </View>
                <View style={styles.rentContainer}>
                  <Text style={styles.rentText}>{rentFormatted}</Text>
                  <Text style={styles.rentPeriod}>/month</Text>
                </View>
              </View>

              <View style={styles.detailsRow}>
                <Text style={styles.detailItem}>Unit #{unit.unitNumber}</Text>
              </View>

              <TouchableOpacity
                style={styles.applyBtn}
                onPress={() => handleApply(unit.id)}
              >
                <Text style={styles.applyBtnText}>Apply Now</Text>
              </TouchableOpacity>
            </View>
          );
        }}
      />
    </View>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1 },
  header: {
    fontSize: 22,
    fontWeight: "bold",
    marginBottom: 16,
    color: "#1f2937",
  },
  card: {
    backgroundColor: "#fff",
    borderRadius: 12,
    padding: 16,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: "#e5e7eb",
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 2,
    elevation: 2,
  },
  cardHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "flex-start",
    marginBottom: 12,
  },
  cardHeaderText: { flex: 1, marginRight: 12 },
  propertyName: {
    fontSize: 18,
    fontWeight: "bold",
    color: "#1f2937",
    marginBottom: 2,
  },
  propertyAddress: { fontSize: 13, color: "#6b7280" },
  rentContainer: { alignItems: "flex-end" },
  rentText: { fontSize: 18, fontWeight: "bold", color: "#2563eb" },
  rentPeriod: { fontSize: 11, color: "#6b7280" },
  detailsRow: {
    flexDirection: "row",
    alignItems: "center",
    flexWrap: "wrap",
    marginBottom: 14,
    paddingTop: 8,
    borderTopWidth: 1,
    borderTopColor: "#f3f4f6",
  },
  detailItem: { fontSize: 13, color: "#4b5563", fontWeight: "500" },
  detailDivider: { marginHorizontal: 8, color: "#9ca3af" },
  applyBtn: {
    backgroundColor: "#2563eb",
    borderRadius: 8,
    paddingVertical: 10,
    alignItems: "center",
  },
  applyBtnText: { color: "#fff", fontSize: 14, fontWeight: "bold" },
  noUnits: {
    color: "#9ca3af",
    fontStyle: "italic",
    textAlign: "center",
    marginVertical: 32,
    fontSize: 15,
  },
});
