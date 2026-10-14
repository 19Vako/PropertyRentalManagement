import { applicationApi } from "@/entities/application/api/applicationApi";
import { ApplicationFilterParams } from "@/entities/application/types";
import { useQuery } from "@tanstack/react-query";

export function useFilteredApplications(params: ApplicationFilterParams) {
  return useQuery({
    queryKey: ["applications", params],
    queryFn: () => applicationApi.getAll(params),
  });
}
