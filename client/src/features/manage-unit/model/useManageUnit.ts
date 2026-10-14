import { unitApi } from "@/entities/unit/api/unitApi";
import { useMutation, useQueryClient } from "@tanstack/react-query";

export function useManageUnit() {
  const queryClient = useQueryClient();

  const createMutation = useMutation({
    mutationFn: unitApi.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["units"] });
      if (__DEV__) console.log("Unit created successfully");
    },
  });

  const updateMutation = useMutation({
    mutationFn: unitApi.update,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["units"] });
      if (__DEV__) console.log("Unit updated successfully");
    },
  });

  const deleteMutation = useMutation({
    mutationFn: unitApi.delete,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["units"] });
      if (__DEV__) console.log("Unit deleted successfully");
    },
  });

  const getSelectableUnitTypes = (
    allTypes: { id: string; isActive: boolean; name: string }[],
    currentUnitTypeId?: string,
  ) => {
    return allTypes.filter(
      (type) => type.isActive || type.id === currentUnitTypeId,
    );
  };

  return {
    createUnit: createMutation.mutate,
    updateUnit: updateMutation.mutate,
    deleteUnit: deleteMutation.mutate,
    getSelectableUnitTypes,
  };
}
