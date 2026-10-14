import { ApplicationStatus } from "@/entities/application/types";

export interface StatusHistoryItem {
  id: string;
  status: ApplicationStatus;
  outcome?: string;
  comment?: string;
  changedBy: string;
  changedAt: string;
}
