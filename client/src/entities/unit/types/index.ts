export interface UnitType {
  id: string;
  name: string;
  isActive: boolean;
}
export interface Unit {
  id: number;
  propertyId: number;
  unitNumber: string;
  bedrooms: number;
  monthlyRent: number;
  unitTypeId: string;
  unitTypeName?: string;
  createdUtc: string;
  rowVersion: string;
}
