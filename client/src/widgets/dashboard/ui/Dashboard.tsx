import { useApplications } from "@/entities/application/model/useApplications";
import {
  APPLICATION_STATUSES,
  Application,
  getStatusName,
} from "@/entities/application/types";
import { useProperties } from "@/entities/property/model/useProperties";
import { useUnits } from "@/entities/unit/model/useUnits";
import { useAuthStore } from "@/entities/user/model/store";
import { useRouter } from "expo-router";
import React from "react";
import {
  ActivityIndicator,
  FlatList,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from "react-native";

const isManagerRole = (roles?: string[]) =>
  Boolean(
    roles?.includes("PropertyManager") || roles?.includes("Property Manager"),
  );

export const Dashboard: React.FC = () => {
  const roles = useAuthStore((state) => state.user?.roles);

  if (!roles) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color="#2563eb" />
      </View>
    );
  }

  return isManagerRole(roles) ? <ManagerDashboard /> : <ApplicantDashboard />;
};

const ManagerDashboard: React.FC = () => {
  const {
    data: properties,
    isLoading: loadingProperties,
    error: propertiesError,
  } = useProperties();

  const {
    data: units,
    isLoading: loadingUnits,
    error: unitsError,
  } = useUnits();
  const {
    data: applications,
    isLoading: loadingApplications,
    error: applicationsError,
  } = useApplications();

  const isLoading = loadingProperties || loadingUnits || loadingApplications;
  const error = propertiesError || unitsError || applicationsError;

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
        <Text style={styles.errorText}>Failed to load dashboard data.</Text>
      </View>
    );
  }

  const propertyList = properties ?? [];
  const unitList = units ?? [];
  const rawApplications = applications as any;
  const applicationList = (
    Array.isArray(rawApplications)
      ? rawApplications
      : (rawApplications?.applications ?? rawApplications?.items ?? [])
  ) as Application[];

  const statusCounts = APPLICATION_STATUSES.reduce<Record<string, number>>(
    (acc, status) => {
      acc[status] = applicationList.filter(
        (app) => getStatusName(app.status) === status,
      ).length;
      return acc;
    },
    {},
  );

  return (
    <ScrollView contentContainerStyle={styles.scrollContent}>
      <Text style={styles.title}>Manager Overview</Text>

      <View style={styles.statsRow}>
        <StatCard label="Properties" value={propertyList.length} />
        <StatCard label="Units" value={unitList.length} />
        <StatCard label="Applications" value={applicationList.length} />
      </View>

      <Text style={styles.sectionTitle}>Applications by status</Text>
      {applicationList.length === 0 ? (
        <Text style={styles.emptyText}>
          No applications have been submitted yet.
        </Text>
      ) : (
        <View style={styles.statusList}>
          {APPLICATION_STATUSES.map((status) => (
            <View key={status} style={styles.statusRow}>
              <Text style={styles.statusLabel}>{status}</Text>
              <Text style={styles.statusValue}>{statusCounts[status]}</Text>
            </View>
          ))}
        </View>
      )}
    </ScrollView>
  );
};

const ApplicantDashboard: React.FC = () => {
  const router = useRouter();
  const { data: applications, isLoading, error } = useApplications();

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
        <Text style={styles.errorText}>Failed to load your applications.</Text>
      </View>
    );
  }

  const rawApplications = applications as any;
  const applicationList = (
    Array.isArray(rawApplications)
      ? rawApplications
      : (rawApplications?.applications ?? rawApplications?.items ?? [])
  ) as Application[];

  const handleBrowse = () => router.push("/(tabs)/properties");

  if (applicationList.length === 0) {
    return (
      <View style={styles.center}>
        <Text style={styles.emptyTitle}>No applications yet</Text>
        <Text style={styles.emptyText}>
          Browse available properties and submit your first application.
        </Text>
        <TouchableOpacity style={styles.ctaButton} onPress={handleBrowse}>
          <Text style={styles.ctaText}>Browse Properties</Text>
        </TouchableOpacity>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <Text style={styles.title}>My Applications</Text>
      <FlatList
        data={applicationList}
        keyExtractor={(item) => item.id}
        renderItem={({ item }) => (
          <View style={styles.card}>
            <View style={styles.cardHeader}>
              <Text style={styles.cardTitle}>
                Unit #{item.unitNumber || item.unitId}
              </Text>
            </View>
            <Text style={styles.cardMeta}>
              Applicant:{" "}
              {item.applicantName || item.applicantInfo?.firstName || "—"}
            </Text>
          </View>
        )}
        ListFooterComponent={
          <TouchableOpacity style={styles.ctaButton} onPress={handleBrowse}>
            <Text style={styles.ctaText}>Apply for Another Property</Text>
          </TouchableOpacity>
        }
      />
    </View>
  );
};

interface StatCardProps {
  label: string;
  value: number;
}

const StatCard: React.FC<StatCardProps> = ({ label, value }) => (
  <View style={styles.statCard}>
    <Text style={styles.statValue}>{value}</Text>
    <Text style={styles.statLabel}>{label}</Text>
  </View>
);

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: "#f9fafb",
    padding: 16,
  },
  center: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    padding: 24,
    backgroundColor: "#f9fafb",
  },
  scrollContent: {
    padding: 16,
    paddingBottom: 32,
  },
  title: {
    fontSize: 22,
    fontWeight: "bold",
    color: "#1f2937",
    marginBottom: 16,
  },
  sectionTitle: {
    fontSize: 18,
    fontWeight: "600",
    color: "#1f2937",
    marginTop: 8,
    marginBottom: 12,
  },
  statsRow: {
    flexDirection: "row",
    gap: 12,
    marginBottom: 24,
  },
  statCard: {
    flex: 1,
    backgroundColor: "#fff",
    borderRadius: 8,
    borderWidth: 1,
    borderColor: "#e5e7eb",
    paddingVertical: 16,
    paddingHorizontal: 12,
    alignItems: "center",
  },
  statValue: {
    fontSize: 24,
    fontWeight: "bold",
    color: "#1f2937",
  },
  statLabel: {
    fontSize: 13,
    color: "#6b7280",
    marginTop: 4,
  },
  statusList: {
    backgroundColor: "#fff",
    borderRadius: 8,
    borderWidth: 1,
    borderColor: "#e5e7eb",
    paddingHorizontal: 16,
    paddingVertical: 4,
  },
  statusRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    paddingVertical: 12,
    borderBottomWidth: 1,
    borderBottomColor: "#f3f4f6",
  },
  statusLabel: {
    fontSize: 15,
    color: "#374151",
  },
  statusValue: {
    fontSize: 15,
    fontWeight: "bold",
    color: "#2563eb",
  },
  card: {
    backgroundColor: "#fff",
    borderRadius: 8,
    borderWidth: 1,
    borderColor: "#e5e7eb",
    padding: 14,
    marginBottom: 12,
  },
  cardHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: 6,
  },
  cardTitle: {
    fontSize: 16,
    fontWeight: "bold",
    color: "#1f2937",
  },
  cardMeta: {
    fontSize: 13,
    color: "#6b7280",
    marginTop: 4,
  },
  ctaButton: {
    backgroundColor: "#2563eb",
    borderRadius: 8,
    paddingVertical: 14,
    paddingHorizontal: 24,
    alignItems: "center",
    marginTop: 16,
  },
  ctaText: {
    color: "#fff",
    fontSize: 16,
    fontWeight: "bold",
  },
  errorText: {
    color: "#dc2626",
    textAlign: "center",
    fontSize: 15,
  },
  emptyTitle: {
    fontSize: 18,
    fontWeight: "bold",
    color: "#1f2937",
    marginBottom: 8,
    textAlign: "center",
  },
  emptyText: {
    color: "#6b7280",
    fontSize: 14,
    textAlign: "center",
    marginBottom: 8,
  },
});
