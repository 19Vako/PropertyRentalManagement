import { RentalApplicationWizard } from "@/features/rental-application-wizard/ui/RentalApplicationWizard";
import { useLocalSearchParams, useRouter } from "expo-router";
import React from "react";

export default function NewApplicationScreen() {
  const { unitId } = useLocalSearchParams<{ unitId: string }>();
  const router = useRouter();

  return (
    <RentalApplicationWizard
      unitId={unitId}
      onSuccess={() => router.replace("/(tabs)/applications")}
    />
  );
}
