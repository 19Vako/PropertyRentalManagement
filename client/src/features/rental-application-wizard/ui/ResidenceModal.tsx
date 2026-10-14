import { zodResolver } from "@hookform/resolvers/zod";
import React from "react";
import { Controller, useForm } from "react-hook-form";
import {
    Modal,
    ScrollView,
    StyleSheet,
    Text,
    TextInput,
    TouchableOpacity,
    View,
} from "react-native";
import { residenceSchema, ResidenceValues } from "../model/schema";

interface ResidenceModalProps {
  visible: boolean;
  onClose: () => void;
  onSave: (data: ResidenceValues) => void;
  initialData?: ResidenceValues | null;
}

export const ResidenceModal: React.FC<ResidenceModalProps> = ({
  visible,
  onClose,
  onSave,
  initialData,
}) => {
  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ResidenceValues>({
    resolver: zodResolver(residenceSchema),
    defaultValues: initialData || {
      address: "",
      landlordName: "",
      landlordPhone: "",
      moveInDate: "",
      moveOutDate: "",
    },
  });

  React.useEffect(() => {
    if (initialData) {
      reset(initialData);
    } else {
      reset({
        address: "",
        landlordName: "",
        landlordPhone: "",
        moveInDate: "",
        moveOutDate: "",
      });
    }
  }, [initialData, reset]);

  const handleFormSubmit = (data: ResidenceValues) => {
    onSave(data);
    onClose();
  };

  return (
    <Modal visible={visible} animationType="slide" transparent>
      <View style={styles.modalOverlay}>
        <View style={styles.modalContent}>
          <Text style={styles.modalTitle}>
            {initialData ? "Edit Residence" : "Add Residence"}
          </Text>
          <ScrollView>
            <Controller
              control={control}
              name="address"
              render={({ field: { onChange, value } }) => (
                <View style={styles.inputContainer}>
                  <TextInput
                    style={styles.input}
                    placeholder="Previous Address"
                    onChangeText={onChange}
                    value={value}
                  />
                  {errors.address && (
                    <Text style={styles.errorText}>
                      {errors.address.message}
                    </Text>
                  )}
                </View>
              )}
            />
            <Controller
              control={control}
              name="landlordName"
              render={({ field: { onChange, value } }) => (
                <View style={styles.inputContainer}>
                  <TextInput
                    style={styles.input}
                    placeholder="Landlord Name"
                    onChangeText={onChange}
                    value={value}
                  />
                  {errors.landlordName && (
                    <Text style={styles.errorText}>
                      {errors.landlordName.message}
                    </Text>
                  )}
                </View>
              )}
            />
            <Controller
              control={control}
              name="landlordPhone"
              render={({ field: { onChange, value } }) => (
                <View style={styles.inputContainer}>
                  <TextInput
                    style={styles.input}
                    placeholder="Landlord Phone"
                    onChangeText={onChange}
                    value={value}
                    keyboardType="phone-pad"
                  />
                  {errors.landlordPhone && (
                    <Text style={styles.errorText}>
                      {errors.landlordPhone.message}
                    </Text>
                  )}
                </View>
              )}
            />
            <Controller
              control={control}
              name="moveInDate"
              render={({ field: { onChange, value } }) => (
                <View style={styles.inputContainer}>
                  <TextInput
                    style={styles.input}
                    placeholder="Move-in Date (YYYY-MM-DD)"
                    onChangeText={onChange}
                    value={value}
                  />
                  {errors.moveInDate && (
                    <Text style={styles.errorText}>
                      {errors.moveInDate.message}
                    </Text>
                  )}
                </View>
              )}
            />
            <Controller
              control={control}
              name="moveOutDate"
              render={({ field: { onChange, value } }) => (
                <View style={styles.inputContainer}>
                  <TextInput
                    style={styles.input}
                    placeholder="Move-out Date (YYYY-MM-DD)"
                    onChangeText={onChange}
                    value={value}
                  />
                  {errors.moveOutDate && (
                    <Text style={styles.errorText}>
                      {errors.moveOutDate.message}
                    </Text>
                  )}
                </View>
              )}
            />
          </ScrollView>

          <View style={styles.modalButtons}>
            <TouchableOpacity
              style={[styles.btn, styles.cancelBtn]}
              onPress={onClose}
            >
              <Text style={styles.btnText}>Cancel</Text>
            </TouchableOpacity>
            <TouchableOpacity
              style={[styles.btn, styles.saveBtn]}
              onPress={handleSubmit(handleFormSubmit)}
            >
              <Text style={styles.btnText}>Save</Text>
            </TouchableOpacity>
          </View>
        </View>
      </View>
    </Modal>
  );
};

const styles = StyleSheet.create({
  modalOverlay: {
    flex: 1,
    backgroundColor: "rgba(0,0,0,0.5)",
    justifyContent: "center",
    padding: 20,
  },
  modalContent: {
    backgroundColor: "#fff",
    borderRadius: 12,
    padding: 20,
    maxHeight: "80%",
  },
  modalTitle: {
    fontSize: 18,
    fontWeight: "bold",
    marginBottom: 15,
    textAlign: "center",
  },
  inputContainer: { marginBottom: 12 },
  input: {
    borderWidth: 1,
    borderColor: "#d1d5db",
    padding: 10,
    borderRadius: 8,
    backgroundColor: "#f9fafb",
  },
  errorText: { color: "red", fontSize: 12, marginTop: 4 },
  modalButtons: {
    flexDirection: "row",
    justifyContent: "space-between",
    marginTop: 20,
    gap: 10,
  },
  btn: { flex: 1, padding: 12, borderRadius: 8, alignItems: "center" },
  cancelBtn: { backgroundColor: "#6b7280" },
  saveBtn: { backgroundColor: "#2563eb" },
  btnText: { color: "#fff", fontWeight: "bold" },
});
