import { Address } from '../../model/address';

export function formatAddressString(address: Address): string {
  const parts: string[] = [];

  if (address.strasse) parts.push(address.strasse);
  if (address.plz || address.ort) {
    const location = [address.plz, address.ort].filter(Boolean).join(' ');
    if (location) parts.push(location);
  }
  if (address.land) parts.push(address.land);

  return parts.join(', ');
}

export function hasValidCoordinates(address: Address): boolean {
  return (
    address.lat !== undefined &&
    address.long !== undefined &&
    !isNaN(address.lat) &&
    !isNaN(address.long) &&
    address.lat >= -90 &&
    address.lat <= 90 &&
    address.long >= -180 &&
    address.long <= 180
  );
}
