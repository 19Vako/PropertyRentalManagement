import { useAuthStore } from "@/entities/user/model/store";
import { Redirect } from "expo-router";

export default function IndexScreen() {
  const token = useAuthStore((state) => state.token);
  return <Redirect href={token ? "/(tabs)/properties" : "/(auth)/login"} />;
}
