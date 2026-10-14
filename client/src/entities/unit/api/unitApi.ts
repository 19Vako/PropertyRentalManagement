import { apiClient } from "@/shared/api/client";
import { Unit, UnitType } from "../types";

/** Payload for creating a unit. Tied to `Unit` so it stays in sync. */
export type CreateUnitPayload = Pick<
  Unit,
  "propertyId" | "unitNumber" | "bedrooms" | "monthlyRent" | "unitTypeId"
>;

/** Payload for updating a unit (including propertyId for backend UnitFormViewModel validation). */
export type UpdateUnitPayload = Pick<
  Unit,
  "propertyId" | "unitNumber" | "bedrooms" | "monthlyRent" | "unitTypeId"
>;

export const unitApi = {
  getAll: async (): Promise<Unit[]> => {
    const response = await apiClient.get<Unit[]>("/units");
    return response.data;
  },
  getAvailable: async (): Promise<Unit[]> => {
    const response = await apiClient.get<Unit[]>("/units/available");
    return response.data;
  },
  getById: async (id: string): Promise<Unit> => {
    const response = await apiClient.get<Unit>(`/units/${id}`);
    return response.data;
  },
  getUnitTypes: async (): Promise<UnitType[]> => {
    const response = await apiClient.get<UnitType[]>("/lookups/unit-types");
    return response.data;
  },
  create: async (data: CreateUnitPayload): Promise<Unit> => {
    const response = await apiClient.post<Unit>("/units", data);
    return response.data;
  },
  update: async (
    payload: { id: number } & UpdateUnitPayload,
  ): Promise<Unit> => {
    const response = await apiClient.put<Unit>(`/units/${payload.id}`, payload);
    return response.data;
  },
  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/units/${id}`);
  },
};
