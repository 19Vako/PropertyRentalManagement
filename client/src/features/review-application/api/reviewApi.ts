import { apiClient } from "@/shared/api/client";

export interface ReviewPayload {
  action: "approve" | "reject" | "return" | "startreview";
  notes?: string;
}

export const reviewApi = {
  submitReview: async (applicationId: number, payload: ReviewPayload) => {
    const response = await apiClient.patch(
      `/applications/${applicationId}/status`,
      payload,
    );
    return response.data;
  },
};