import { zodResolver } from "@hookform/resolvers/zod";
import { useQueryClient } from "@tanstack/react-query";
import React, { useState } from "react";
import { Controller, useForm } from "react-hook-form";
import {
    ActivityIndicator,
    Alert,
    Modal,
    StyleSheet,
    Text,
    TextInput,
    TouchableOpacity,
    View,
} from "react-native";
import { reviewApi } from "../api/reviewApi";
import { ReviewFormValues, reviewSchema } from "../model/schema";

interface ReviewModalProps {
  visible: boolean;
  applicationId: number;
  onClose: () => void;
  onSuccess: () => void;
}

export const ReviewModal: React.FC<ReviewModalProps> = ({
  visible,
  applicationId,
  onClose,
  onSuccess,
}) => {
  const queryClient = useQueryClient();
  const [loading, setLoading] = useState(false);

  const {
    control,
    handleSubmit,
    watch,
    reset,
    formState: { errors },
  } = useForm<ReviewFormValues>({
    resolver: zodResolver(reviewSchema),
    defaultValues: { action: "approve", notes: "" },
  });

  const selectedAction = watch("action");

  const onSubmit = async (data: ReviewFormValues) => {
    try {
      setLoading(true);
      await reviewApi.submitReview(applicationId, data);

      if (__DEV__) {
        console.log(
          "Application reviewed successfully with action:",
          data.action,
        );
      }
      queryClient.invalidateQueries({ queryKey: ["applications"] });

      Alert.alert(
        "Success",
        `Application action '${data.action}' executed successfully.`,
      );
      reset();
      onSuccess();
      onClose();
    } catch (error: any) {
      if (__DEV__) console.error("Failed to review application:", error);
      const serverMessage =
        error?.response?.data?.message ||
        "Failed to process application review.";

      Alert.alert("Review Error", serverMessage);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal visible={visible} animationType="slide" transparent>
      <View style={styles.overlay}>
        <View style={styles.content}>
          <Text style={styles.title}>Review Application</Text>

          {/* Action Selection */}
          <Text style={styles.label}>Action:</Text>
          <Controller
            control={control}
            name="action"
            render={({ field: { value, onChange } }) => (
              <View style={styles.outcomeRow}>
                {(["approve", "reject", "return", "startreview"] as const).map((item) => (
                  <TouchableOpacity
                    key={item}
                    style={[
                      styles.outcomeBtn,
                      value === item && styles.outcomeBtnActive,
                    ]}
                    onPress={() => onChange(item)}
                  >
                    <Text
                      style={[
                        styles.outcomeText,
                        value === item && styles.outcomeTextActive,
                      ]}
                    >
                      {item}
                    </Text>
                  </TouchableOpacity>
                ))}
              </View>
            )}
          />

          {/* Notes Field */}
          <Text style={styles.label}>
            Notes {selectedAction === "return" || selectedAction === "reject" ? "*" : "(Optional)"}:
          </Text>
          <Controller
            control={control}
            name="notes"
            render={({ field: { onChange, value } }) => (
              <TextInput
                style={[styles.input, errors.notes && styles.inputError]}
                placeholder="Enter review notes..."
                multiline
                numberOfLines={4}
                onChangeText={onChange}
                value={value ?? ""}
              />
            )}
          />
          {errors.notes && (
            <Text style={styles.errorText}>{errors.notes.message}</Text>
          )}

          {/* Actions */}
          <View style={styles.buttonsRow}>
            <TouchableOpacity
              style={[styles.btn, styles.cancelBtn]}
              onPress={onClose}
              disabled={loading}
            >
              <Text style={styles.btnText}>Cancel</Text>
            </TouchableOpacity>
            <TouchableOpacity
              style={[styles.btn, styles.submitBtn]}
              onPress={handleSubmit(onSubmit)}
              disabled={loading}
            >
              {loading ? (
                <ActivityIndicator color="#fff" />
              ) : (
                <Text style={styles.btnText}>Submit Review</Text>
              )}
            </TouchableOpacity>
          </View>
        </View>
      </View>
    </Modal>
  );
};

const styles = StyleSheet.create({
  overlay: {
    flex: 1,
    backgroundColor: "rgba(0,0,0,0.5)",
    justifyContent: "center",
    padding: 20,
  },
  content: { backgroundColor: "#fff", borderRadius: 12, padding: 20 },
  title: {
    fontSize: 20,
    fontWeight: "bold",
    marginBottom: 16,
    textAlign: "center",
  },
  label: { fontWeight: "600", marginBottom: 6, marginTop: 12 },
  outcomeRow: { flexDirection: "row", flexWrap: "wrap", gap: 8, marginBottom: 10 },
  outcomeBtn: {
    paddingVertical: 8,
    paddingHorizontal: 12,
    borderWidth: 1,
    borderColor: "#d1d5db",
    borderRadius: 8,
    alignItems: "center",
  },
  outcomeBtnActive: { backgroundColor: "#2563eb", borderColor: "#2563eb" },
  outcomeText: { color: "#374151", fontSize: 12 },
  outcomeTextActive: { color: "#fff", fontWeight: "bold" },
  input: {
    borderWidth: 1,
    borderColor: "#d1d5db",
    borderRadius: 8,
    padding: 12,
    height: 100,
    textAlignVertical: "top",
    backgroundColor: "#f9fafb",
  },
  inputError: { borderColor: "#dc2626" },
  errorText: { color: "#dc2626", fontSize: 12, marginTop: 4 },
  buttonsRow: { flexDirection: "row", gap: 10, marginTop: 20 },
  btn: { flex: 1, padding: 14, borderRadius: 8, alignItems: "center" },
  cancelBtn: { backgroundColor: "#6b7280" },
  submitBtn: { backgroundColor: "#16a34a" },
  btnText: { color: "#fff", fontWeight: "bold"}},
);