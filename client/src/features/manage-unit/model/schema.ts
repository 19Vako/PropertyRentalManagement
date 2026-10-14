import { z } from "zod";

export const unitSchema = z.object({
  unitNumber: z.string().min(1, "Unit number is required"),
  bedrooms: z
    .string()
    .min(1, "Bedrooms is required")
    .refine((v) => Number.isInteger(Number(v)) && Number(v) >= 0, {
      message: "Enter a whole number",
    }),
  monthlyRent: z
    .string()
    .min(1, "Monthly rent is required")
    .refine((v) => !Number.isNaN(Number(v)) && Number(v) > 0, {
      message: "Enter a valid amount",
    }),
  unitTypeId: z.string(),
});

export type UnitFormValues = z.infer<typeof unitSchema>;
