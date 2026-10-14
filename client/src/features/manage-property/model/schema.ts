import { z } from "zod";

export const propertySchema = z.object({
  name: z.string().min(1, "Name is required"),
  streetAddress: z.string().min(1, "Street address is required"),
  city: z.string().min(1, "City is required"),
  region: z.string().min(1, "Region is required"),
  postalCode: z.string().min(1, "Postal code is required"),
  countryCode: z.string().length(2, "Use a 2-letter code (e.g. US)"),
  description: z.string().optional(),
});

export type PropertyFormValues = z.infer<typeof propertySchema>;
