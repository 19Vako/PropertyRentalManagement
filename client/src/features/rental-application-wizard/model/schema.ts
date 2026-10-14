import { z } from "zod";

export const applicantInfoSchema = z.object({
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
  phone: z.string().min(10, "Valid phone number is required"),
  email: z.string().refine((val) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(val), {
    message: "Invalid email address",
  }),
  currentAddress: z.string().min(5, "Current address is required"),
});

export const residenceSchema = z.object({
  id: z.string().optional(),
  address: z.string().min(5, "Address is required"),
  landlordName: z.string().min(1, "Landlord name is required"),
  landlordPhone: z.string().min(10, "Landlord phone is required"),
  moveInDate: z.string().min(1, "Move-in date is required"),
  moveOutDate: z.string().min(1, "Move-out date is required"),
});

export const applicationWizardSchema = z.object({
  desiredMoveInDate: z.string().min(1, "Move-in date is required"),
  householdSize: z.number().min(1, "Household size must be at least 1"),
  monthlyIncome: z.number().min(0, "Monthly income is required"),
  applicantMessage: z.string().optional(),
});

export type ApplicationWizardValues = z.infer<typeof applicationWizardSchema>;
export type ResidenceValues = z.infer<typeof residenceSchema>;
