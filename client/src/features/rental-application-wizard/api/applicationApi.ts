import { apiClient } from "@/shared/api/client";
import { ApplicationPayload } from "../types";

export const applicationApi = {
  create: async (data: ApplicationPayload, idempotencyKey: string) => {
    const response = await apiClient.post("/applications", data, {
      headers: { "Idempotency-Key": idempotencyKey },
    });
    return response.data;
  },

  update: async (
    id: string,
    data: ApplicationPayload,
    idempotencyKey: string,
  ) => {
    const response = await apiClient.put(`/applications/${id}`, data, {
      headers: { "Idempotency-Key": idempotencyKey },
    });
    return response.data;
  },
};
