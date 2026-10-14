export interface Residence {
  id?: string;
  address: string;
  landlordName: string;
  landlordPhone: string;
  moveInDate: string;
  moveOutDate: string;
}

export interface ApplicationPayload {
  unitId: number;
  desiredMoveInDate?: string;
  householdSize: number;
  monthlyIncome: number;
  applicantMessage?: string;
}
