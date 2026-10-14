import { z } from "zod";

export const reviewSchema = z
  .object({
    outcome: z.enum(["Approve", "Return", "Deny"]),
    comment: z.string().optional(),
  })
  .refine(
    (data) => {
      if (
        (data.outcome === "Return" || data.outcome === "Deny") &&
        (!data.comment || data.comment.trim() === "")
      ) {
        return false;
      }
      return true;
    },
    {
      message: "Comment is mandatory for Return and Deny outcomes",
      path: ["comment"],
    },
  );

export type ReviewFormValues = z.infer<typeof reviewSchema>;
