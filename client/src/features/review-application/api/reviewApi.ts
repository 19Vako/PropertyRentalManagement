import { apiClient } from "@/shared/api/client";

export interface ReviewPayload {
  outcome: "Approve" | "Return" | "Deny";
  comment?: string;
}

export const reviewApi = {
  submitReview: async (applicationId: string, payload: ReviewPayload) => {
    const response = await apiClient.post(
      `/applications/${applicationId}/review`,
      payload,
    );
    return response.data;
  },
};
