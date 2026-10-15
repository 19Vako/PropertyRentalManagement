import { useWithdrawApplication } from "@/entities/application/api/applicationApi";
import { useFilteredApplications } from "@/entities/application/model/useFilteredApplications";
import {
  APPLICATION_STATUSES,
  Application,
  ApplicationStatus,
  TERMINAL_STATUSES,
  getStatusName,
} from "@/entities/application/types";
import { useAuthStore } from "@/entities/user/model/store";
import { OfflineBanner } from "@/widgets/offline-banner/ui/OfflineBanner";
import { useRouter } from "expo-router";
import React, { useState } from "react";
import {
  ActivityIndicator,
  FlatList,
  Modal,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from "react-native";

const STATUS_COLORS: Record<string, string> = {
  Draft: "#6b7280",
  Submitted: "#2563eb",
  Returned: "#d97706",
  Approved: "#16a34a",
  Denied: "#dc2626",
  Withdrawn: "#9ca3af",
};

export const ApplicationListWidget: React.FC = () => {
  const router = useRouter();
  const user = useAuthStore((state) => state.user);
  const isManager =
    user?.roles?.includes("PropertyManager") ||
    user?.roles?.includes("Property Manager");
  const [selectedStatus, setStatus] = useState<string | null>(null);
  const [selectedPropertyId, setPropertyId] = useState<number | null>(null);

  const resetFilters = () => {
    setStatus(null);
    setPropertyId(null);
  };

  const {
    data: applications,
    isLoading,
    error,
  } = useFilteredApplications({
    status: (selectedStatus as ApplicationStatus) ?? undefined,
  });

  const withdrawMutation = useWithdrawApplication();

  const [statusPickerOpen, setStatusPickerOpen] = useState(false);
  const [propertyPickerOpen, setPropertyPickerOpen] = useState(false);

  const applicationList = (applications ?? []) as Application[];

  const handleOpen = (id: string) =>
    router.push({ pathname: "/application/[id]", params: { id } });

  const handleWithdraw = (id: string) =>
    withdrawMutation.mutate(id, {
      onError: (err: any) => {
        if (__DEV__) console.error("Failed to withdraw application:", err);
      },
    });

  const canWithdraw = (item: Application) => {
    const statusName = getStatusName(item.status);
    return (
      !isManager &&
      !TERMINAL_STATUSES.includes(statusName) &&
      (item.applicantInfo?.email
        ? item.applicantInfo.email === user?.email
        : true)
    );
  };

  if (isLoading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color="#2563eb" />
      </View>
    );
  }
  if (error) {
    return (
      <View style={styles.center}>
        <Text style={styles.errorText}>Error loading applications.</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <OfflineBanner />

      <Text style={styles.header}>Applications List</Text>

      <View style={styles.filterRow}>
        <TouchableOpacity
          style={styles.filterBtn}
          onPress={() => setStatusPickerOpen(true)}
        >
          <Text style={styles.filterBtnText}>
            {selectedStatus ? `Status: ${selectedStatus}` : "Status: All"}
          </Text>
        </TouchableOpacity>

        <TouchableOpacity style={styles.resetBtn} onPress={resetFilters}>
          <Text style={styles.resetText}>Reset</Text>
        </TouchableOpacity>
      </View>

      <FlatList
        data={applicationList}
        keyExtractor={(item) => item.id}
        ListEmptyComponent={
          <Text style={styles.emptyText}>No applications found.</Text>
        }
        renderItem={({ item }) => {
          const statusName = getStatusName(item.status);
          return (
            <TouchableOpacity
              style={styles.card}
              activeOpacity={0.7}
              onPress={() => handleOpen(item.id)}
            >
              <View style={styles.cardHeader}>
                <Text style={styles.cardTitle}>
                  Unit #{item.unitNumber || item.unitId}
                </Text>
                <View
                  style={[
                    styles.statusBadge,
                    { backgroundColor: STATUS_COLORS[statusName] ?? "#6b7280" },
                  ]}
                />
              </View>

              <Text style={styles.cardMeta}>Applicantion:{item.id}</Text>

              {canWithdraw(item) && (
                <TouchableOpacity
                  style={styles.withdrawBtn}
                  onPress={() => handleWithdraw(item.id)}
                  disabled={withdrawMutation.isPending}
                >
                  <Text style={styles.withdrawText}>
                    {withdrawMutation.isPending ? "Withdrawing…" : "Withdraw"}
                  </Text>
                </TouchableOpacity>
              )}
            </TouchableOpacity>
          );
        }}
      />

      <Modal visible={statusPickerOpen} transparent animationType="fade">
        <View style={styles.modalOverlay}>
          <View style={styles.modalContent}>
            <Text style={styles.modalTitle}>Filter by status</Text>
            <TouchableOpacity
              style={styles.option}
              onPress={() => {
                setStatus(null);
                setStatusPickerOpen(false);
              }}
            >
              <Text
                style={[
                  styles.optionText,
                  !selectedStatus && styles.optionActive,
                ]}
              >
                All Statuses
              </Text>
            </TouchableOpacity>
            {APPLICATION_STATUSES.map((status) => (
              <TouchableOpacity
                key={status}
                style={styles.option}
                onPress={() => {
                  setStatus(status);
                  setStatusPickerOpen(false);
                }}
              >
                <Text
                  style={[
                    styles.optionText,
                    selectedStatus === status && styles.optionActive,
                  ]}
                >
                  {status}
                </Text>
              </TouchableOpacity>
            ))}
            <TouchableOpacity
              style={styles.modalCancel}
              onPress={() => setStatusPickerOpen(false)}
            >
              <Text style={styles.modalCancelText}>Cancel</Text>
            </TouchableOpacity>
          </View>
        </View>
      </Modal>

      <Modal visible={propertyPickerOpen} transparent animationType="fade">
        <View style={styles.modalOverlay}>
          <View style={styles.modalContent}>
            <Text style={styles.modalTitle}>Filter by property</Text>
            <TouchableOpacity
              style={styles.option}
              onPress={() => {
                setPropertyId(null);
                setPropertyPickerOpen(false);
              }}
            >
              <Text
                style={[
                  styles.optionText,
                  selectedPropertyId === null && styles.optionActive,
                ]}
              >
                All Properties
              </Text>
            </TouchableOpacity>

            <TouchableOpacity
              style={styles.modalCancel}
              onPress={() => setPropertyPickerOpen(false)}
            >
              <Text style={styles.modalCancelText}>Cancel</Text>
            </TouchableOpacity>
          </View>
        </View>
      </Modal>
    </View>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: "#f9fafb" },
  center: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    padding: 24,
    backgroundColor: "#f9fafb",
  },
  header: { fontSize: 22, fontWeight: "bold", padding: 16 },
  filterRow: {
    flexDirection: "row",
    paddingHorizontal: 16,
    marginBottom: 12,
    gap: 8,
    alignItems: "center",
  },
  filterBtn: {
    flex: 1,
    paddingVertical: 8,
    paddingHorizontal: 10,
    borderWidth: 1,
    borderColor: "#d1d5db",
    borderRadius: 6,
    backgroundColor: "#fff",
  },
  filterBtnText: { fontSize: 13, color: "#374151" },
  resetBtn: { padding: 8 },
  resetText: { color: "#2563eb", fontWeight: "600", fontSize: 13 },
  emptyText: {
    textAlign: "center",
    color: "#9ca3af",
    marginTop: 40,
    fontStyle: "italic",
  },
  errorText: { color: "#dc2626", fontSize: 15 },
  card: {
    backgroundColor: "#fff",
    marginHorizontal: 16,
    marginBottom: 10,
    padding: 14,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: "#e5e7eb",
  },
  cardHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: 6,
    gap: 8,
  },
  cardTitle: { fontWeight: "bold", fontSize: 16, flex: 1 },
  statusBadge: {
    width: 12,
    height: 12,
    borderRadius: 6,
  },
  cardMeta: { fontSize: 13, color: "#6b7280", marginBottom: 4 },
  roleTag: { color: "#059669", fontSize: 12, marginTop: 4, fontWeight: "600" },
  withdrawBtn: {
    marginTop: 10,
    alignSelf: "flex-start",
    paddingHorizontal: 14,
    paddingVertical: 7,
    borderRadius: 6,
    borderWidth: 1,
    borderColor: "#dc2626",
    backgroundColor: "#fef2f2",
  },
  withdrawText: { color: "#dc2626", fontSize: 13, fontWeight: "600" },
  modalOverlay: {
    flex: 1,
    backgroundColor: "rgba(0,0,0,0.5)",
    justifyContent: "center",
    padding: 24,
  },
  modalContent: {
    backgroundColor: "#fff",
    borderRadius: 12,
    padding: 16,
  },
  modalTitle: {
    fontSize: 17,
    fontWeight: "bold",
    marginBottom: 12,
    textAlign: "center",
  },
  option: {
    paddingVertical: 12,
    borderBottomWidth: 1,
    borderBottomColor: "#f3f4f6",
  },
  optionText: { fontSize: 15, color: "#374151" },
  optionActive: { color: "#2563eb", fontWeight: "bold" },
  modalCancel: { marginTop: 12, alignItems: "center", paddingVertical: 8 },
  modalCancelText: { color: "#6b7280", fontWeight: "600" },
});
