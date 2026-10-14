import { useQuery } from "@tanstack/react-query";
import { propertyApi } from "../api/propertyApi";

export function useProperties() {
  return useQuery({
    queryKey: ["properties"],
    queryFn: propertyApi.getAll,
  });
}

export function useProperty(id: string) {
  return useQuery({
    queryKey: ["properties", id],
    queryFn: () => propertyApi.getById(id),
    enabled: !!id,
  });
}
