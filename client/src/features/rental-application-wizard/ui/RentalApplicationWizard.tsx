import {
  ApplicationStatus,
  isEditableStatus,
} from "@/entities/application/types";
import { zodResolver } from "@hookform/resolvers/zod";
import * as Crypto from "expo-crypto";
import React, { useState } from "react";
import { useForm } from "react-hook-form";
import {
  Alert,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from "react-native";
import { applicationApi } from "../api/applicationApi";
import {
  applicationWizardSchema,
  ApplicationWizardValues,
} from "../model/schema";
import { ApplicantInfoSection } from "./ApplicantInfoSection";
import { SummarySection } from "./SummarySection";

interface RentalApplicationWizardProps {
  unitId: number;  
  applicationId?: number; 
  initialStatus?: ApplicationStatus;
  initialData?: Partial<ApplicationWizardValues>;
  onSuccess?: () => void;
}

type WizardIntent = "continue" | "back" | "submit";

export const RentalApplicationWizard: React.FC<
  RentalApplicationWizardProps
> = ({
  unitId,
  applicationId,
  initialStatus = "Draft",
  initialData,
  onSuccess,
}) => {
  const [step, setStep] = useState(0); // 0: Details, 1: Summary
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [intent, setIntent] = useState<WizardIntent>("continue");
  const [isFormValid, setIsFormValid] = useState(
    !!initialData?.desiredMoveInDate,
  );

  const isReadOnly = !isEditableStatus(initialStatus);

  const { control, handleSubmit, trigger, getValues } =
    useForm<ApplicationWizardValues>({
      resolver: zodResolver(applicationWizardSchema),
      defaultValues: initialData || {
        desiredMoveInDate: new Date().toISOString().split("T")[0],
        householdSize: 1,
        monthlyIncome: 0,
        applicantMessage: "",
      },
    });

  const handleContinue = async () => {
    setIntent("continue");
    const isValid = await trigger([
      "desiredMoveInDate",
      "householdSize",
      "monthlyIncome",
    ]);

    if (!isValid) {
      Alert.alert(
        "Incomplete Data",
        "Please fill in all required fields correctly before continuing.",
      );
      return;
    }

    setIsFormValid(true);
    setStep(1);
  };

  const handleBack = () => {
    setIntent("back");
    setStep(0);
  };

  const onSubmit = async (data: ApplicationWizardValues) => {
    if (intent !== "submit") return;
    try {
      setIsSubmitting(true);
      const idempotencyKey = Crypto.randomUUID();

      const payload = {
        unitId: Number(unitId),
        ...data,
      };

      if (applicationId) {
        await applicationApi.update(applicationId, payload, idempotencyKey);
        if (__DEV__) console.log("Application updated successfully");
      } else {
        await applicationApi.create(payload, idempotencyKey);
        if (__DEV__) console.log("Application created successfully");
      }

      Alert.alert("Success", "Application submitted successfully.");
      onSuccess?.();
    } catch (error: any) {
      if (__DEV__)
        console.error("Failed to submit application:", error?.response?.data);

      const errorData = error?.response?.data;
      let errorMessage = "Failed to submit application.";

      if (typeof errorData === "string") {
        errorMessage = errorData;
      } else if (errorData?.message) {
        errorMessage = errorData.message;
      } else if (errorData?.errors) {
        const validationErrors = Object.values(errorData.errors).flat();
        if (validationErrors.length > 0) {
          errorMessage = validationErrors.join("\n");
        }
      }

      Alert.alert("Error", errorMessage);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleSubmitPress = () => {
    setIntent("submit");
    handleSubmit(onSubmit)();
  };

  return (
    <View style={styles.container}>
      <Text style={styles.stepIndicator}>
        Step {step + 1} of 2 (
        {step === 0 ? "Application Details" : "Summary & Review"})
      </Text>

      <ScrollView contentContainerStyle={styles.scrollContent}>
        {step === 0 && (
          <ApplicantInfoSection control={control} readOnly={isReadOnly} />
        )}

        {step === 1 && <SummarySection values={getValues()} />}
      </ScrollView>

      <View style={styles.footerNav}>
        {step > 0 && (
          <TouchableOpacity style={styles.navBtnBack} onPress={handleBack}>
            <Text style={styles.navBtnText}>Back</Text>
          </TouchableOpacity>
        )}

        {step === 0 ? (
          <TouchableOpacity
            style={styles.navBtnContinue}
            onPress={handleContinue}
          >
            <Text style={styles.navBtnText}>Continue</Text>
          </TouchableOpacity>
        ) : (
          !isReadOnly &&
          isFormValid && (
            <TouchableOpacity
              style={styles.navBtnSubmit}
              onPress={handleSubmitPress}
              disabled={isSubmitting}
            >
              <Text style={styles.navBtnText}>
                {isSubmitting ? "Submitting..." : "Submit Application"}
              </Text>
            </TouchableOpacity>
          )
        )}
      </View>

      {step === 1 && !isReadOnly && !isFormValid && (
        <Text style={styles.hintText}>
          Complete application details to enable submission.
        </Text>
      )}
    </View>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: "#fff", padding: 16 },
  stepIndicator: {
    fontSize: 14,
    color: "#6b7280",
    marginBottom: 12,
    textAlign: "center",
  },
  scrollContent: { paddingBottom: 20 },
  hintText: {
    fontSize: 13,
    color: "#6b7280",
    textAlign: "center",
    marginTop: 8,
  },
  footerNav: {
    flexDirection: "row",
    justifyContent: "space-between",
    paddingVertical: 12,
    borderTopWidth: 1,
    borderTopColor: "#e5e7eb",
    gap: 10,
  },
  navBtnBack: {
    flex: 1,
    backgroundColor: "#6b7280",
    padding: 14,
    borderRadius: 8,
    alignItems: "center",
  },
  navBtnContinue: {
    flex: 1,
    backgroundColor: "#2563eb",
    padding: 14,
    borderRadius: 8,
    alignItems: "center",
  },
  navBtnSubmit: {
    flex: 1,
    backgroundColor: "#16a34a",
    padding: 14,
    borderRadius: 8,
    alignItems: "center",
  },
  navBtnText: { color: "#fff", fontWeight: "bold", fontSize: 16 },
});
