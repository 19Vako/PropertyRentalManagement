import { z } from "zod";

export const reviewSchema = z
  .object({
    action: z.enum(["approve", "reject", "return", "startreview"]),
    notes: z.string().optional(),
  })
  .refine(
    (data) => {
      if (
        (data.action === "return" || data.action === "reject") &&
        (!data.notes || data.notes.trim() === "")
      ) {
        return false;
      }
      return true;
    },
    {
      message: "Notes are mandatory for return and reject actions",
      path: ["notes"],
    },
  );

export type ReviewFormValues = z.infer<typeof reviewSchema>;