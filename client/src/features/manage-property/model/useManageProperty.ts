import { propertyApi } from "@/entities/property/api/propertyApi";
import { useMutation, useQueryClient } from "@tanstack/react-query";

export function useManageProperty() {
  const queryClient = useQueryClient();

  const createMutation = useMutation({
    mutationFn: propertyApi.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["properties"] });
      if (__DEV__) console.log("Property created successfully");
    },
    onError: (error) => {
      if (__DEV__) console.error("Failed to create property:", error);
    },
  });

  const updateMutation = useMutation({
    mutationFn: propertyApi.update,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["properties"] });
      if (__DEV__) console.log("Property updated successfully");
    },
  });

  const deleteMutation = useMutation({
    mutationFn: propertyApi.delete,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["properties"] });
      if (__DEV__) console.log("Property deleted successfully");
    },
  });

  return {
    createProperty: createMutation.mutate,
    updateProperty: updateMutation.mutate,
    deleteProperty: deleteMutation.mutate,
  };
}
