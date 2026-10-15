import { useApplication } from "@/entities/application/api/applicationApi";
import { getStatusName, isEditableStatus } from "@/entities/application/types";
import { useAuthStore } from "@/entities/user/model/store";
import { RentalApplicationWizard } from "@/features/rental-application-wizard/model/schema";
import { ReviewModal } from "@/features/review-application/ui/ReviewModal";
import { OfflineBanner } from "@/widgets/offline-banner/ui/OfflineBanner";
import { useLocalSearchParams, useRouter } from "expo-router";
import React, { useState } from "react";
import {
  ActivityIndicator,
  SafeAreaView,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from "react-native";

// --- BACKEND TYPES ---
export interface ApplicationActivity {
  actorUserId: string;
  applicationId: number;
  createdUtc: string;
  id: number;
  notes: string | null;
  type: number;
}

export interface ApplicationStatusObject {
  id: number;
  name: string;
}

export interface Property {
  id: number;
  name: string;
  city: string;
  region: string;
  streetAddress: string;
  postalCode: string;
  countryCode: string;
  description: string;
  isActive: boolean;
  managerUserId: string;
  createdUtc: string;
}

export interface Unit {
  id: number;
  unitNumber: string;
  monthlyRent: number;
  bathrooms: number;
  bedrooms: number;
  availableFrom: string;
  propertyId: number;
  property: Property;
  status: number;
  rowVersion: string;
  createdUtc: string;
}

export interface BackendApplication {
  id: number;
  applicantUserId: string;
  applicantMessage?: string | null;
  desiredMoveInDate: string;
  householdSize: number;
  monthlyIncome: number;
  statusId: number;
  status: ApplicationStatusObject;
  submittedUtc?: string | null;
  createdUtc: string;
  updatedUtc: string;
  unitId: number;
  unit: Unit;
  activities: ApplicationActivity[];
  rowVersion: string;
}

// --- READ-ONLY APPLICATION VIEW ---
const ReadOnlyApplicationView: React.FC<{
  application: BackendApplication;
}> = ({ application }) => {
  return (
    <View style={styles.sectionContainer}>
      {/* Property & Unit Information */}
      {application.unit && (
        <View style={styles.card}>
          <Text style={styles.cardTitle}>Property Information</Text>
          <Text style={styles.cardText}>
            <Text style={styles.bold}>Property:</Text>{" "}
            {application.unit.property?.name}
          </Text>
          <Text style={styles.cardText}>
            <Text style={styles.bold}>Address:</Text>{" "}
            {application.unit.property?.streetAddress},{" "}
            {application.unit.property?.city}
          </Text>
          <Text style={styles.cardText}>
            <Text style={styles.bold}>Unit Number:</Text>{" "}
            {application.unit.unitNumber}
          </Text>
          <Text style={styles.cardText}>
            <Text style={styles.bold}>Monthly Rent:</Text> $
            {application.unit.monthlyRent} / mo.
          </Text>
        </View>
      )}

      {/* Application Details */}
      <View style={styles.card}>
        <Text style={styles.cardTitle}>Application Details</Text>
        <Text style={styles.cardText}>
          <Text style={styles.bold}>Desired Move-In Date:</Text>{" "}
          {application.desiredMoveInDate}
        </Text>
        <Text style={styles.cardText}>
          <Text style={styles.bold}>Household Size:</Text>{" "}
          {application.householdSize} persons
        </Text>
        <Text style={styles.cardText}>
          <Text style={styles.bold}>Monthly Income:</Text> $
          {application.monthlyIncome}
        </Text>
        {application.applicantMessage && (
          <Text style={styles.cardText}>
            <Text style={styles.bold}>Message:</Text>{" "}
            {application.applicantMessage}
          </Text>
        )}
      </View>

      {/* Activity History (Mapped to avoid FlatList nesting issues) */}
      <View style={styles.card}>
        <Text style={styles.cardTitle}>Activity History</Text>
        {application.activities && application.activities.length > 0 ? (
          application.activities.map((activity) => (
            <View key={activity.id} style={styles.activityItem}>
              <View style={styles.activityHeader}>
                <Text style={styles.activityType}>Type: {activity.type}</Text>
                <Text style={styles.activityDate}>
                  {new Date(activity.createdUtc).toLocaleString()}
                </Text>
              </View>
              {activity.notes && (
                <Text style={styles.activityNotes}>{activity.notes}</Text>
              )}
            </View>
          ))
        ) : (
          <Text style={styles.emptyText}>No history available</Text>
        )}
      </View>
    </View>
  );
};

// --- MAIN SCREEN ---
export default function ApplicationDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();
  const roles = useAuthStore((state) => state.user?.roles);
  const isManager = Boolean(
    roles?.includes("PropertyManager") || roles?.includes("Property Manager"),
  );
  const [isReviewModalVisible, setIsReviewModalVisible] = useState(false);

  const { data, isLoading, error } = useApplication(id);
  const application = data as unknown as BackendApplication;

  if (isLoading) {
    return (
      <SafeAreaView style={styles.center}>
        <ActivityIndicator size="large" color="#2563eb" />
      </SafeAreaView>
    );
  }

  if (error || !application) {
    return (
      <SafeAreaView style={styles.center}>
        <Text style={styles.errorText}>Failed to load application.</Text>
        <TouchableOpacity style={styles.retryBtn} onPress={() => router.back()}>
          <Text style={styles.retryText}>Go back</Text>
        </TouchableOpacity>
      </SafeAreaView>
    );
  }

  const statusName =
    application.status?.name ?? getStatusName(application.statusId);
  const isEditable = isEditableStatus(application.statusId);
  const canReview = isManager && statusName === "Submitted";

  return (
    <SafeAreaView style={styles.container}>
      <OfflineBanner />

      <View style={styles.headerRow}>
        <TouchableOpacity
          style={styles.backButton}
          onPress={() => router.back()}
        >
          <Text style={styles.backButtonText}>← Back</Text>
        </TouchableOpacity>
      </View>

      {isEditable ? (
        <RentalApplicationWizard
          unitId={application.unitId}
          applicationId={application.id}
          initialStatus={application.statusId}
          initialData={{
            applicantInfo: {
              firstName: "",
              lastName: "",
              phone: "",
              email: "",
              currentAddress: "",
            },
            residences: [],
          }}
          onSuccess={() => router.replace("/(tabs)/applications")}
        />
      ) : (
        <ScrollView contentContainerStyle={styles.content}>
          <Text style={styles.title}>Application #{id}</Text>

          <View style={styles.statusPill}>
            <Text style={styles.statusPillText}>{statusName}</Text>
          </View>

          <ReadOnlyApplicationView application={application} />

          {canReview && (
            <TouchableOpacity
              style={styles.reviewButton}
              onPress={() => setIsReviewModalVisible(true)}
            >
              <Text style={styles.reviewButtonText}>Review Application</Text>
            </TouchableOpacity>
          )}
        </ScrollView>
      )}

      <ReviewModal
        applicationId={application.id}
        visible={isReviewModalVisible}
        onClose={() => setIsReviewModalVisible(false)}
        onSuccess={() => setIsReviewModalVisible(false)}
      />
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: "#f8f9fa" },
  headerRow: {
    paddingHorizontal: 16,
    paddingTop: 8,
    paddingBottom: 4,
  },
  backButton: {
    alignSelf: "flex-start",
    paddingVertical: 6,
    paddingRight: 12,
  },
  backButtonText: {
    color: "#2563eb",
    fontSize: 16,
    fontWeight: "600",
  },
  content: { padding: 16, paddingBottom: 32 },
  center: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    padding: 24,
    backgroundColor: "#f8f9fa",
  },
  title: {
    fontSize: 22,
    fontWeight: "bold",
    marginBottom: 12,
    color: "#1f2937",
  },
  statusPill: {
    alignSelf: "flex-start",
    backgroundColor: "#eff6ff",
    borderColor: "#2563eb",
    borderWidth: 1,
    borderRadius: 999,
    paddingHorizontal: 12,
    paddingVertical: 4,
    marginBottom: 16,
  },
  statusPillText: { color: "#2563eb", fontWeight: "600", fontSize: 13 },
  errorText: { color: "#dc2626", fontSize: 15, marginBottom: 12 },
  retryBtn: {
    backgroundColor: "#2563eb",
    paddingHorizontal: 20,
    paddingVertical: 10,
    borderRadius: 8,
  },
  retryText: { color: "#fff", fontWeight: "bold" },
  reviewButton: {
    backgroundColor: "#28a745",
    padding: 16,
    borderRadius: 8,
    alignItems: "center",
    marginTop: 16,
  },
  reviewButtonText: { color: "#fff", fontSize: 16, fontWeight: "bold" },
  sectionContainer: {
    gap: 12,
  },
  card: {
    backgroundColor: "#fff",
    borderRadius: 12,
    padding: 16,
    marginBottom: 12,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.1,
    shadowRadius: 2,
    elevation: 2,
  },
  cardTitle: {
    fontSize: 16,
    fontWeight: "bold",
    color: "#111827",
    marginBottom: 8,
  },
  cardText: {
    fontSize: 14,
    color: "#4b5563",
    marginBottom: 4,
  },
  bold: {
    fontWeight: "600",
    color: "#374151",
  },
  activityItem: {
    paddingVertical: 8,
    borderBottomWidth: 1,
    borderBottomColor: "#f3f4f6",
  },
  activityHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    marginBottom: 4,
  },
  activityType: {
    fontSize: 13,
    fontWeight: "600",
    color: "#2563eb",
  },
  activityDate: {
    fontSize: 12,
    color: "#9ca3af",
  },
  activityNotes: {
    fontSize: 13,
    color: "#4b5563",
  },
  emptyText: {
    fontSize: 13,
    color: "#9ca3af",
    fontStyle: "italic",
  },
});
