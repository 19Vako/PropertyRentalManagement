export type ApplicationStatusName =
  | "Draft"
  | "Submitted"
  | "UnderReview"
  | "ReturnedForCorrection"
  | "Approved"
  | "Rejected"
  | "Withdrawn";

export interface ApplicationStatusObject {
  id?: string | number;
  name: ApplicationStatusName;
  rentalApplications?: unknown[];
}

export type ApplicationStatus = ApplicationStatusName | ApplicationStatusObject;

export const APPLICATION_STATUSES: ApplicationStatusName[] = [
  "Draft",
  "Submitted",
  "UnderReview",
  "ReturnedForCorrection",
  "Approved",
  "Rejected",
  "Withdrawn",
];

export const TERMINAL_STATUSES: ApplicationStatusName[] = [
  "Approved",
  "Rejected",
  "Withdrawn",
];

export const EDITABLE_STATUSES: ApplicationStatusName[] = [
  "Draft",
  "ReturnedForCorrection",
];

export const getStatusName = (
  status: ApplicationStatus,
): ApplicationStatusName => {
  if (typeof status === "object" && status !== null && "name" in status) {
    return (status as ApplicationStatusObject).name;
  }
  return status as ApplicationStatusName;
};

export const isEditableStatus = (status: ApplicationStatus): boolean => {
  const name = getStatusName(status);
  return EDITABLE_STATUSES.includes(name);
};

export interface ApplicationReview {
  id: number;
  applicationId: number;
  reviewerUserId: string;
  decision: string;
  notes: string;
  createdUtc: string;
}

export interface RentalApplicationActivity {
  id: number;
  applicationId: number;
  actorUserId: string;
  type: string;
  notes?: string;
  createdUtc: string;
}

export interface Application {
  id: number;
  unitId: number;
  applicantUserId?: string;
  statusId?: number;
  status: ApplicationStatus;
  desiredMoveInDate?: string;
  householdSize?: number;
  monthlyIncome?: number;
  applicantMessage?: string;
  submittedUtc?: string;
  createdUtc?: string;
  updatedUtc?: string;

  // Связанные данные с бэкенда (Include)
  unit?: {
    id: number;
    unitNumber: string;
    propertyId: number;
    property?: {
      id: number;
      name: string;
      city: string;
    };
  };
  applicationReviews?: ApplicationReview[];
  activities?: RentalApplicationActivity[];
}

export interface ApplicationFilterParams {
  status?: ApplicationStatus;
  propertyId?: number | null;
  unitId?: number | null;
  search?: string;
  moveInFrom?: string;
  moveInTo?: string;
}