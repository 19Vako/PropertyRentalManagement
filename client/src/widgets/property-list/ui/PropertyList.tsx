import { useProperties } from "@/entities/property/model/useProperties";
import { useAvailableUnits, useUnits } from "@/entities/unit/model/useUnits";
import { useAuthStore } from "@/entities/user/model/store";
import { useManageProperty } from "@/features/manage-property/model/useManageProperty";
import { useManageUnit } from "@/features/manage-unit/model/useManageUnit";
import React from "react";
import { StyleSheet, Text, View } from "react-native";
import { ApplicantUnitListView } from "./ApplicantUnitListView";
import { ManagerPropertyView } from "./ManagerPropertyView";

export const PropertyList: React.FC = () => {
  const user = useAuthStore((state) => state.user);
  const isManager =
    user?.roles?.includes("Property Manager") ||
    user?.roles?.includes("PropertyManager");

  const { data: properties, isLoading: loadingProperties } = useProperties();
  const { data: allUnits, isLoading: loadingAllUnits } = useUnits({
    enabled: isManager,
  });
  const { data: availableUnits, isLoading: loadingAvailableUnits } =
    useAvailableUnits({ enabled: !isManager });

  const { deleteProperty } = useManageProperty();
  const { deleteUnit } = useManageUnit();

  const units = isManager ? allUnits : availableUnits;
  const loadingUnits = isManager ? loadingAllUnits : loadingAvailableUnits;

  if (loadingProperties || loadingUnits) {
    return <Text style={styles.centerText}>Loading properties...</Text>;
  }

  const propertyList = properties ?? [];
  const unitList = units ?? [];

  return (
    <View style={styles.container}>
      {isManager ? (
        <ManagerPropertyView
          properties={propertyList}
          units={unitList}
          deleteProperty={deleteProperty}
          deleteUnit={deleteUnit}
        />
      ) : (
        <ApplicantUnitListView properties={propertyList} units={unitList} />
      )}
    </View>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, padding: 16, backgroundColor: "#f9fafb" },
  centerText: { textAlign: "center", marginTop: 20, color: "#6b7280" },
});
