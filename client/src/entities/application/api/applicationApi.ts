import { apiClient } from "@/shared/api/client";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Application, ApplicationFilterParams } from "../types";

export const applicationApi = {
  getAll: async (params?: ApplicationFilterParams) => {
    const response = await apiClient.get("/applications", { params });
    return response.data;
  },
  getById: async (id: string): Promise<Application> => {
    const response = await apiClient.get<Application>(`/applications/${id}`);
    return response.data;
  },

  withdraw: async (id: string): Promise<void> => {
    await apiClient.post(`/applications/${id}/withdraw`);
  },

  getApplicantApplications: async () => {
    const response = await apiClient.get("/dashboard/applicant");
    return response.data;
  },
  getManagerApplications: async () => {
    const response = await apiClient.get("/dashboard/manager");
    return response.data;
  },
};

export function useApplication(id: string) {
  return useQuery({
    queryKey: ["applications", id],
    queryFn: () => applicationApi.getById(id),
    enabled: !!id,
  });
}

export function useWithdrawApplication() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => applicationApi.withdraw(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["applications"] });
    },
  });
}
