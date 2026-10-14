export type ApplicationStatusName =
  | "Draft"
  | "Submitted"
  | "Returned"
  | "Approved"
  | "Denied"
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
  "Returned",
  "Approved",
  "Denied",
  "Withdrawn",
];

export const TERMINAL_STATUSES: ApplicationStatusName[] = [
  "Approved",
  "Denied",
  "Withdrawn",
];

export const EDITABLE_STATUSES: ApplicationStatusName[] = ["Draft", "Returned"];

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

export interface Application {
  id: string;
  unitId: string;
  status: ApplicationStatus;
  applicantName?: string;
  unitNumber?: string;
  propertyId?: number;
  applicantInfo?: {
    firstName?: string;
    lastName?: string;
    phone?: string;
    email?: string;
    currentAddress?: string;
  };
  createdUtc?: string;
  updatedUtc?: string;
  residences?: ApplicationResidence[];

  history?: ApplicationHistoryItem[];
  statusHistory?: ApplicationHistoryItem[];
}

export interface ApplicationResidence {
  id?: string;
  address: string;
  landlordName: string;
  landlordPhone: string;
  moveInDate: string;
  moveOutDate: string;
}

export interface ApplicationHistoryItem {
  id: string;
  status: ApplicationStatus;
  outcome?: string;
  comment?: string;
  changedBy: string;
  changedAt: string;
}

export interface ApplicationFilterParams {
  status?: ApplicationStatus;
  propertyId?: number | null;
}
