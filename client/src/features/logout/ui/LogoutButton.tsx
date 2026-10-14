import { useAuthStore } from "@/entities/user/model/store";
import { useRouter } from "expo-router";
import React from "react";
import { Pressable, StyleSheet, Text } from "react-native";

export const LogoutButton: React.FC = () => {
  const clearSession = useAuthStore((s) => s.clearSession);
  const router = useRouter();

  const handleLogout = async () => {
    await clearSession();
    router.replace("/(auth)/login");
  };

  return (
    <Pressable
      onPress={handleLogout}
      hitSlop={8}
      style={({ pressed }) => [styles.button, pressed && styles.pressed]}
    >
      <Text style={styles.text}>Log out</Text>
    </Pressable>
  );
};

const styles = StyleSheet.create({
  button: {
    paddingHorizontal: 8,
    paddingVertical: 4,
    marginRight: 4,
  },
  pressed: {
    opacity: 0.6,
  },
  text: {
    color: "#2563eb",
    fontWeight: "600",
    fontSize: 15,
  },
});
