import { applicationApi } from "@/entities/application/api/applicationApi";
import { useAuthStore } from "@/entities/user/model/store";
import { useQuery } from "@tanstack/react-query";

export function useApplications(options?: { enabled?: boolean }) {
  const user = useAuthStore((state) => state.user);
  const isManager = user?.roles?.includes("Property Manager") || user?.roles?.includes("PropertyManager");

  return useQuery({
    queryKey: ["applications", isManager ? "manager" : "applicant"],
    queryFn: isManager
      ? applicationApi.getManagerApplications
      : applicationApi.getApplicantApplications,
    enabled: options?.enabled ?? true,
  });
}
