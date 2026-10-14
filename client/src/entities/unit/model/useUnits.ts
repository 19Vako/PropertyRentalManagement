import { useQuery } from "@tanstack/react-query";
import { unitApi } from "../api/unitApi";

export function useUnits(options?: { enabled?: boolean }) {
  return useQuery({
    queryKey: ["units"],
    queryFn: unitApi.getAll,
    enabled: options?.enabled ?? true,
  });
}

export function useAvailableUnits(options?: { enabled?: boolean }) {
  return useQuery({
    queryKey: ["units", "available"],
    queryFn: unitApi.getAvailable,
    enabled: options?.enabled ?? true,
  });
}

export function useUnitTypes() {
  return useQuery({
    queryKey: ["unitTypes"],
    queryFn: unitApi.getUnitTypes,
  });
}
