export function formatPriceRange(minimum: number, maximum: number) {
  return minimum === maximum ? `${minimum}` : `${minimum}–${maximum}`
}
