import { zodResolver } from "@hookform/resolvers/zod";
import { Property } from "@/entities/property/types";
import React, { useEffect } from "react";
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
import { useManageProperty } from "../model/useManageProperty";
import { PropertyFormValues, propertySchema } from "../model/schema";

interface PropertyModalProps {
  visible: boolean;
  onClose: () => void;
  /** When provided the modal runs in edit mode, otherwise add mode. */
  property?: Property | null;
}

const emptyValues: PropertyFormValues = {
  name: "",
  streetAddress: "",
  city: "",
  region: "",
  postalCode: "",
  countryCode: "",
  description: "",
};

export const PropertyModal: React.FC<PropertyModalProps> = ({
  visible,
  onClose,
  property,
}) => {
  const { createProperty, updateProperty } = useManageProperty();
  const isEdit = Boolean(property);

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<PropertyFormValues>({
    resolver: zodResolver(propertySchema),
    defaultValues: emptyValues,
  });

  // Re-seed the form whenever the modal opens or the target property changes.
  useEffect(() => {
    if (!visible) return;
    if (property) {
      reset({
        name: property.name,
        streetAddress: property.streetAddress,
        city: property.city,
        region: property.region,
        postalCode: property.postalCode,
        countryCode: property.countryCode,
        description: property.description ?? "",
      });
    } else {
      reset(emptyValues);
    }
  }, [visible, property, reset]);

  const onSubmit = (data: PropertyFormValues) => {
    const options = { onSuccess: onClose };
    if (property) {
      updateProperty({ id: property.id, ...data }, options);
    } else {
      createProperty(data, options);
    }
  };

  return (
    <Modal visible={visible} animationType="slide" transparent>
      <View style={styles.modalOverlay}>
        <View style={styles.modalContent}>
          <Text style={styles.modalTitle}>
            {isEdit ? "Edit Property" : "Add Property"}
          </Text>

          <ScrollView>
            <Controller
              control={control}
              name="name"
              render={({ field: { onChange, onBlur, value } }) => (
                <View style={styles.inputContainer}>
                  <TextInput
                    style={styles.input}
                    placeholder="Property name"
                    onBlur={onBlur}
                    onChangeText={onChange}
                    value={value}
                  />
                  {errors.name && (
                    <Text style={styles.fieldError}>{errors.name.message}</Text>
                  )}
                </View>
              )}
            />

            <Controller
              control={control}
              name="streetAddress"
              render={({ field: { onChange, onBlur, value } }) => (
                <View style={styles.inputContainer}>
                  <TextInput
                    style={styles.input}
                    placeholder="Street address"
                    onBlur={onBlur}
                    onChangeText={onChange}
                    value={value}
                  />
                  {errors.streetAddress && (
                    <Text style={styles.fieldError}>
                      {errors.streetAddress.message}
                    </Text>
                  )}
                </View>
              )}
            />

            <View style={styles.row}>
              <View style={[styles.inputContainer, styles.flex1]}>
                <Controller
                  control={control}
                  name="city"
                  render={({ field: { onChange, onBlur, value } }) => (
                    <View>
                      <TextInput
                        style={styles.input}
                        placeholder="City"
                        onBlur={onBlur}
                        onChangeText={onChange}
                        value={value}
                      />
                      {errors.city && (
                        <Text style={styles.fieldError}>
                          {errors.city.message}
                        </Text>
                      )}
                    </View>
                  )}
                />
              </View>
              <View style={[styles.inputContainer, styles.flex1]}>
                <Controller
                  control={control}
                  name="region"
                  render={({ field: { onChange, onBlur, value } }) => (
                    <View>
                      <TextInput
                        style={styles.input}
                        placeholder="Region / State"
                        onBlur={onBlur}
                        onChangeText={onChange}
                        value={value}
                      />
                      {errors.region && (
                        <Text style={styles.fieldError}>
                          {errors.region.message}
                        </Text>
                      )}
                    </View>
                  )}
                />
              </View>
            </View>
            <View style={styles.row}>
              <View style={[styles.inputContainer, styles.flex1]}>
                <Controller
                  control={control}
                  name="postalCode"
                  render={({ field: { onChange, onBlur, value } }) => (
                    <View>
                      <TextInput
                        style={styles.input}
                        placeholder="Postal code"
                        onBlur={onBlur}
                        onChangeText={onChange}
                        value={value}
                      />
                      {errors.postalCode && (
                        <Text style={styles.fieldError}>
                          {errors.postalCode.message}
                        </Text>
                      )}
                    </View>
                  )}
                />
              </View>
              <View style={[styles.inputContainer, styles.flex1]}>
                <Controller
                  control={control}
                  name="countryCode"
                  render={({ field: { onChange, onBlur, value } }) => (
                    <View>
                      <TextInput
                        style={styles.input}
                        placeholder="Country (US)"
                        autoCapitalize="characters"
                        maxLength={2}
                        onBlur={onBlur}
                        onChangeText={onChange}
                        value={value}
                      />
                      {errors.countryCode && (
                        <Text style={styles.fieldError}>
                          {errors.countryCode.message}
                        </Text>
                      )}
                    </View>
                  )}
                />
              </View>
            </View>

            <Controller
              control={control}
              name="description"
              render={({ field: { onChange, onBlur, value } }) => (
                <View style={styles.inputContainer}>
                  <TextInput
                    style={[styles.input, styles.textarea]}
                    placeholder="Description (optional)"
                    multiline
                    numberOfLines={3}
                    onBlur={onBlur}
                    onChangeText={onChange}
                    value={value}
                  />
                  {errors.description && (
                    <Text style={styles.fieldError}>
                      {errors.description.message}
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
              onPress={handleSubmit(onSubmit)}
            >
              <Text style={styles.btnText}>{isEdit ? "Save" : "Create"}</Text>
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
    maxHeight: "90%",
  },
  modalTitle: {
    fontSize: 18,
    fontWeight: "bold",
    marginBottom: 15,
    textAlign: "center",
  },
  inputContainer: { marginBottom: 12 },
  flex1: { flex: 1 },
  row: { flexDirection: "row", gap: 10 },
  input: {
    borderWidth: 1,
    borderColor: "#d1d5db",
    padding: 10,
    borderRadius: 8,
    backgroundColor: "#f9fafb",
  },
  textarea: { minHeight: 70, textAlignVertical: "top" },
  fieldError: { color: "red", fontSize: 12, marginTop: 4 },
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


