import { Property } from "@/entities/property/types";
import { Unit } from "@/entities/unit/types";
import { PropertyModal } from "@/features/manage-property/ui/PropertyModal";
import { UnitModal } from "@/features/manage-unit/ui/UnitModal";
import React, { useState } from "react";
import {
    Alert,
    FlatList,
    StyleSheet,
    Text,
    TouchableOpacity,
    View,
} from "react-native";

interface ManagerPropertyViewProps {
  properties: Property[];
  units: Unit[];
  deleteProperty: (id: number) => void;
  deleteUnit: (id: number) => void;
}

export const ManagerPropertyView: React.FC<ManagerPropertyViewProps> = ({
  properties,
  units,
  deleteProperty,
  deleteUnit,
}) => {
  const [propertyModal, setPropertyModal] = useState<{
    visible: boolean;
    property: Property | null;
  }>({ visible: false, property: null });

  const [unitModal, setUnitModal] = useState<{
    visible: boolean;
    propertyId: number;
    unit: Unit | null;
  }>({ visible: false, propertyId: 0, unit: null });

  const openAddProperty = () =>
    setPropertyModal({ visible: true, property: null });
  const openEditProperty = (property: Property) =>
    setPropertyModal({ visible: true, property });
  const closePropertyModal = () =>
    setPropertyModal((s) => ({ ...s, visible: false }));

  const openAddUnit = (propertyId: number) =>
    setUnitModal({ visible: true, propertyId, unit: null });
  const openEditUnit = (propertyId: number, unit: Unit) =>
    setUnitModal({ visible: true, propertyId, unit });
  const closeUnitModal = () => setUnitModal((s) => ({ ...s, visible: false }));

  const confirmDeleteProperty = (property: Property) => {
    Alert.alert("Delete Property", `Delete "${property.name}"?`, [
      { text: "Cancel", style: "cancel" },
      {
        text: "Delete",
        style: "destructive",
        onPress: () => deleteProperty(property.id),
      },
    ]);
  };

  const confirmDeleteUnit = (unit: Unit) => {
    Alert.alert("Delete Unit", `Delete unit #${unit.unitNumber}?`, [
      { text: "Cancel", style: "cancel" },
      {
        text: "Delete",
        style: "destructive",
        onPress: () => deleteUnit(unit.id),
      },
    ]);
  };

  return (
    <View style={styles.container}>
      <View style={styles.headerRow}>
        <Text style={styles.header}>Properties Catalog</Text>
        <TouchableOpacity style={styles.addBtn} onPress={openAddProperty}>
          <Text style={styles.addBtnText}>+ Add Property</Text>
        </TouchableOpacity>
      </View>

      <FlatList
        data={properties}
        keyExtractor={(item) => item.id.toString()}
        ListEmptyComponent={
          <Text style={styles.noUnits}>No properties found.</Text>
        }
        renderItem={({ item: property }) => {
          const propertyUnits = units.filter(
            (u) => String(u.propertyId) === String(property.id),
          );

          const fullAddress = `${property.streetAddress}, ${property.city}, ${property.region} ${property.postalCode}`;

          return (
            <View style={styles.card}>
              <View style={styles.cardHeader}>
                <View style={styles.cardHeaderText}>
                  <Text style={styles.propertyName}>{property.name}</Text>
                  <Text style={styles.propertyAddress}>{fullAddress}</Text>
                </View>
                <View style={styles.iconActions}>
                  <TouchableOpacity
                    style={[styles.smallBtn, styles.editBtn]}
                    onPress={() => openEditProperty(property)}
                  >
                    <Text style={styles.smallBtnText}>Edit</Text>
                  </TouchableOpacity>
                  <TouchableOpacity
                    style={[styles.smallBtn, styles.deleteBtn]}
                    onPress={() => confirmDeleteProperty(property)}
                  >
                    <Text style={styles.smallBtnText}>Delete</Text>
                  </TouchableOpacity>
                </View>
              </View>

              <Text style={styles.unitsHeader}>Units:</Text>
              {propertyUnits.length === 0 ? (
                <Text style={styles.noUnits}>
                  No units available for this property.
                </Text>
              ) : (
                propertyUnits.map((unit) => (
                  <View key={unit.id} style={styles.unitRow}>
                    <Text style={styles.unitText}>
                      Unit #{unit.unitNumber} | {unit.bedrooms} BR | $
                      {unit.monthlyRent}/mo
                    </Text>

                    <View style={styles.iconActions}>
                      <TouchableOpacity
                        style={[styles.smallBtn, styles.editBtn]}
                        onPress={() => openEditUnit(property.id, unit)}
                      >
                        <Text style={styles.smallBtnText}>Edit</Text>
                      </TouchableOpacity>
                      <TouchableOpacity
                        style={[styles.smallBtn, styles.deleteBtn]}
                        onPress={() => confirmDeleteUnit(unit)}
                      >
                        <Text style={styles.smallBtnText}>Delete</Text>
                      </TouchableOpacity>
                    </View>
                  </View>
                ))
              )}

              <TouchableOpacity
                style={styles.addUnitBtn}
                onPress={() => openAddUnit(property.id)}
              >
                <Text style={styles.addUnitBtnText}>+ Add Unit</Text>
              </TouchableOpacity>
            </View>
          );
        }}
      />

      <PropertyModal
        visible={propertyModal.visible}
        property={propertyModal.property}
        onClose={closePropertyModal}
      />

      <UnitModal
        visible={unitModal.visible}
        propertyId={unitModal.propertyId}
        unit={unitModal.unit}
        onClose={closeUnitModal}
      />
    </View>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1 },
  header: { fontSize: 22, fontWeight: "bold" },
  headerRow: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    marginBottom: 16,
  },
  addBtn: {
    backgroundColor: "#2563eb",
    paddingHorizontal: 14,
    paddingVertical: 8,
    borderRadius: 6,
  },
  addBtnText: { color: "#fff", fontSize: 14, fontWeight: "bold" },
  card: {
    backgroundColor: "#fff",
    padding: 16,
    borderRadius: 8,
    marginBottom: 16,
    borderWidth: 1,
    borderColor: "#e5e7eb",
  },
  cardHeader: {
    flexDirection: "row",
    alignItems: "flex-start",
    justifyContent: "space-between",
    gap: 10,
  },
  cardHeaderText: { flex: 1 },
  propertyName: { fontSize: 18, fontWeight: "bold" },
  propertyAddress: { color: "#6b7280", marginBottom: 10 },
  iconActions: { flexDirection: "row", gap: 8 },
  smallBtn: {
    paddingHorizontal: 10,
    paddingVertical: 5,
    borderRadius: 6,
    borderWidth: 1,
  },
  smallBtnText: { fontSize: 12, fontWeight: "600" },
  editBtn: { backgroundColor: "#eff6ff", borderColor: "#2563eb" },
  deleteBtn: { backgroundColor: "#fef2f2", borderColor: "#dc2626" },
  unitsHeader: { fontWeight: "600", marginTop: 8, marginBottom: 4 },
  noUnits: {
    color: "#9ca3af",
    fontStyle: "italic",
    textAlign: "center",
    marginVertical: 8,
  },
  unitRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    paddingVertical: 6,
    borderBottomWidth: 1,
    borderBottomColor: "#f3f4f6",
    gap: 10,
  },
  unitText: { flex: 1, fontSize: 14, color: "#374151" },
  addUnitBtn: {
    marginTop: 10,
    alignSelf: "flex-start",
    paddingHorizontal: 12,
    paddingVertical: 7,
    borderRadius: 6,
    borderWidth: 1,
    borderColor: "#2563eb",
    borderStyle: "dashed",
  },
  addUnitBtnText: { color: "#2563eb", fontSize: 13, fontWeight: "600" },
});
