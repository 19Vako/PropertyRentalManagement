import { apiClient } from "@/shared/api/client";
import { Property } from "../types";

/** Editable fields a manager provides when creating a property. */
export type CreatePropertyPayload = Pick<
  Property,
  | "name"
  | "streetAddress"
  | "city"
  | "region"
  | "postalCode"
  | "countryCode"
  | "description"
>;

/** Editable fields when updating a property. */
export type UpdatePropertyPayload = CreatePropertyPayload;

export const propertyApi = {
  getAll: async (): Promise<Property[]> => {
    const response = await apiClient.get<Property[]>("/properties");
    return response.data;
  },
  getById: async (id: string): Promise<Property> => {
    const response = await apiClient.get<Property>(`/properties/${id}`);
    return response.data;
  },
  create: async (data: CreatePropertyPayload): Promise<Property> => {
    const response = await apiClient.post<Property>("/properties", data);
    return response.data;
  },
  update: async ({
    id,
    ...data
  }: { id: number } & UpdatePropertyPayload): Promise<Property> => {
    const response = await apiClient.put<Property>(`/properties/${id}`, data);
    return response.data;
  },
  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/properties/${id}`);
  },
};
