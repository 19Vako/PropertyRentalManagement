export interface Property {
  id: number;
  managerUserId: string;
  name: string;
  streetAddress: string;
  city: string;
  region: string;
  postalCode: string;
  countryCode: string;
  description?: string;
  createdUtc: string;
  isActive: boolean;
}
