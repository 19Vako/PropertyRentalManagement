import { useUnitTypes } from "@/entities/unit/model/useUnits";
import { Unit } from "@/entities/unit/types";
import { zodResolver } from "@hookform/resolvers/zod";
import React, { useEffect } from "react";
import { Controller, useForm } from "react-hook-form";
import {
  ActivityIndicator,
  Modal,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from "react-native";
import { UnitFormValues, unitSchema } from "../model/schema";
import { useManageUnit } from "../model/useManageUnit";

interface UnitModalProps {
  visible: boolean;
  onClose: () => void;
  propertyId: number;
  unit?: Unit | null;
}

const emptyValues: UnitFormValues = {
  unitNumber: "",
  bedrooms: "",
  monthlyRent: "",
  unitTypeId: "",
};

export const UnitModal: React.FC<UnitModalProps> = ({
  visible,
  onClose,
  propertyId,
  unit,
}) => {
  const { createUnit, updateUnit, getSelectableUnitTypes } = useManageUnit();
  const { data: unitTypes, isLoading: loadingTypes } = useUnitTypes();
  const isEdit = Boolean(unit);

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<UnitFormValues>({
    resolver: zodResolver(unitSchema),
    defaultValues: emptyValues,
  });

  useEffect(() => {
    if (!visible) return;
    if (unit) {
      reset({
        unitNumber: unit.unitNumber,
        bedrooms: String(unit.bedrooms),
        monthlyRent: String(unit.monthlyRent),
        unitTypeId: unit.unitTypeId ?? (unit as any).status?.toString() ?? "",
      });
    } else {
      reset(emptyValues);
    }
  }, [visible, unit, reset]);

  const selectableTypes = getSelectableUnitTypes(
    unitTypes ?? [],
    unit?.unitTypeId,
  );

  const onSubmit = (data: UnitFormValues) => {
    if (!propertyId || propertyId <= 0) {
      console.error("Error: propertyId is missing or invalid:", propertyId);
      return;
    }

    const payload = {
      unitNumber: data.unitNumber,
      bedrooms: Number(data.bedrooms),
      monthlyRent: Number(data.monthlyRent),
      unitTypeId: data.unitTypeId,
    };
    const options = { onSuccess: onClose };

    if (unit) {
      updateUnit(
        {
          id: unit.id,
          propertyId: unit.propertyId,
          ...payload,
        },
        options,
      );
    } else {
      createUnit({ propertyId, ...payload }, options);
    }
  };

  return (
    <Modal visible={visible} animationType="slide" transparent>
      <View style={styles.modalOverlay}>
        <View style={styles.modalContent}>
          <Text style={styles.modalTitle}>
            {isEdit ? "Edit Unit" : "Add Unit"}
          </Text>

          <ScrollView>
            <Controller
              control={control}
              name="unitNumber"
              render={({ field: { onChange, onBlur, value } }) => (
                <View style={styles.inputContainer}>
                  <TextInput
                    style={styles.input}
                    placeholder="Unit number"
                    onBlur={onBlur}
                    onChangeText={onChange}
                    value={value}
                  />
                  {errors.unitNumber && (
                    <Text style={styles.fieldError}>
                      {errors.unitNumber.message}
                    </Text>
                  )}
                </View>
              )}
            />

            <View style={styles.row}>
              <View style={[styles.inputContainer, styles.flex1]}>
                <Controller
                  control={control}
                  name="bedrooms"
                  render={({ field: { onChange, onBlur, value } }) => (
                    <View>
                      <TextInput
                        style={styles.input}
                        placeholder="Bedrooms"
                        keyboardType="number-pad"
                        onBlur={onBlur}
                        onChangeText={onChange}
                        value={value}
                      />
                      {errors.bedrooms && (
                        <Text style={styles.fieldError}>
                          {errors.bedrooms.message}
                        </Text>
                      )}
                    </View>
                  )}
                />
              </View>
              <View style={[styles.inputContainer, styles.flex1]}>
                <Controller
                  control={control}
                  name="monthlyRent"
                  render={({ field: { onChange, onBlur, value } }) => (
                    <View>
                      <TextInput
                        style={styles.input}
                        placeholder="Monthly rent"
                        keyboardType="number-pad"
                        onBlur={onBlur}
                        onChangeText={onChange}
                        value={value}
                      />
                      {errors.monthlyRent && (
                        <Text style={styles.fieldError}>
                          {errors.monthlyRent.message}
                        </Text>
                      )}
                    </View>
                  )}
                />
              </View>
            </View>

            <Controller
              control={control}
              name="unitTypeId"
              render={({ field: { value, onChange } }) => (
                <View style={styles.inputContainer}>
                  <Text style={styles.pickerLabel}>Unit type</Text>
                  {loadingTypes ? (
                    <ActivityIndicator color="#2563eb" />
                  ) : (
                    <ScrollView
                      horizontal
                      showsHorizontalScrollIndicator={false}
                      contentContainerStyle={styles.typeRow}
                    >
                      {selectableTypes.map((type) => {
                        const active = value === type.id;
                        return (
                          <TouchableOpacity
                            key={type.id}
                            style={[
                              styles.typeBtn,
                              active && styles.typeBtnActive,
                            ]}
                            onPress={() => onChange(type.id)}
                          >
                            <Text
                              style={[
                                styles.typeText,
                                active && styles.typeTextActive,
                              ]}
                            >
                              {type.name}
                            </Text>
                          </TouchableOpacity>
                        );
                      })}
                    </ScrollView>
                  )}
                  {errors.unitTypeId && (
                    <Text style={styles.fieldError}>
                      {errors.unitTypeId.message}
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
  pickerLabel: { marginBottom: 8, fontWeight: "600" },
  typeRow: { flexDirection: "row", gap: 10, paddingVertical: 2 },
  typeBtn: {
    paddingHorizontal: 14,
    paddingVertical: 10,
    borderWidth: 1,
    borderColor: "#ccc",
    borderRadius: 8,
    alignItems: "center",
  },
  typeBtnActive: { backgroundColor: "#2563eb", borderColor: "#2563eb" },
  typeText: { color: "#000" },
  typeTextActive: { color: "#fff", fontWeight: "bold" },
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
