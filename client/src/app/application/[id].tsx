import { useApplication } from "@/entities/application/api/applicationApi";
import {
  Application,
  getStatusName,
  isEditableStatus,
} from "@/entities/application/types";
import { useAuthStore } from "@/entities/user/model/store";
import { ApplicationWizardValues } from "@/features/rental-application-wizard/model/schema";
import { ApplicantInfoSection } from "@/features/rental-application-wizard/ui/ApplicantInfoSection";
import { RentalApplicationWizard } from "@/features/rental-application-wizard/ui/RentalApplicationWizard";
import { ResidenceHistorySection } from "@/features/rental-application-wizard/ui/ResidenceHistorySection";
import { SummarySection } from "@/features/rental-application-wizard/ui/SummarySection";
import { ReviewModal } from "@/features/review-application/ui/ReviewModal";
import { ApplicationHistory } from "@/widgets/application-history/ui/ApplicationHistory";
import { OfflineBanner } from "@/widgets/offline-banner/ui/OfflineBanner";
import { useLocalSearchParams, useRouter } from "expo-router";
import React, { useState } from "react";
import { useForm } from "react-hook-form";
import {
  ActivityIndicator,
  SafeAreaView,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from "react-native";

const toFormValues = (application: Application): ApplicationWizardValues => ({
  applicantInfo: {
    firstName: application.applicantInfo?.firstName ?? "",
    lastName: application.applicantInfo?.lastName ?? "",
    phone: application.applicantInfo?.phone ?? "",
    email: application.applicantInfo?.email ?? "",
    currentAddress: application.applicantInfo?.currentAddress ?? "",
  },
  residences: application.residences ?? [],
});

const ReadOnlyApplicationView: React.FC<{ application: Application }> = ({
  application,
}) => {
  const values = toFormValues(application);
  const { control } = useForm<ApplicationWizardValues>({
    defaultValues: values,
  });

  return (
    <View>
      <ApplicantInfoSection control={control} readOnly />
      <ResidenceHistorySection
        control={control}
        readOnly
        onAdd={() => {}}
        onEdit={() => {}}
      />
      <SummarySection values={values} />
    </View>
  );
};

export default function ApplicationDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();
  const roles = useAuthStore((state) => state.user?.roles);
  const isManager = Boolean(
    roles?.includes("PropertyManager") || roles?.includes("Property Manager"),
  );
  const [isReviewModalVisible, setIsReviewModalVisible] = useState(false);

  const { data: application, isLoading, error } = useApplication(id ?? "");

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

  const statusName = getStatusName(application.status);
  const isEditable = isEditableStatus(application.status);
  const canReview = isManager && statusName === "Submitted";
  const history = application.statusHistory ?? application.history ?? [];

  return (
    <SafeAreaView style={styles.container}>
      <OfflineBanner />

      <View style={styles.headerRow}>
        <TouchableOpacity
          style={styles.backButton}
          onPress={() => router.back()}
        >
          <Text style={styles.backButtonText}>← Назад</Text>
        </TouchableOpacity>
      </View>

      {isEditable ? (
        <RentalApplicationWizard
          unitId={application.unitId}
          applicationId={application.id}
          initialStatus={application.status}
          initialData={toFormValues(application)}
          onSuccess={() => router.replace("/(tabs)/applications")}
        />
      ) : (
        <ScrollView contentContainerStyle={styles.content}>
          <Text style={styles.title}>Application #{id}</Text>

          <View style={styles.statusPill}>
            <Text style={styles.statusPillText}>{statusName}</Text>
          </View>

          <ReadOnlyApplicationView application={application} />

          <ApplicationHistory history={history} />

          {canReview && (
            <TouchableOpacity
              style={styles.reviewButton}
              onPress={() => setIsReviewModalVisible(true)}
            >
              <Text style={styles.reviewButtonText}>
                Вынести решение (Review)
              </Text>
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
  title: { fontSize: 22, fontWeight: "bold", marginBottom: 12 },
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
});
