export type ListingFilters = {
    keywords: string
    minPrice: string
    maxPrice: string
    location: string
}

export const emptyFilters: ListingFilters = {
    keywords: '',
    minPrice: '',
    maxPrice: '',
    location: '',
}

type Filterable = {
    title: string
    category?: string
    minimumPrice: number
    maximumPrice: number
    location?: string
}

export function hasActiveFilters(f: ListingFilters) {
    return Object.values(f).some((v) => v.trim() !== '')
}

export function filterListings<T extends Filterable>(items: T[], f: ListingFilters): T[] {
    const tokens = f.keywords.toLowerCase().split(/\s+/).filter(Boolean)
    const min = f.minPrice === '' ? null : Number(f.minPrice)
    const max = f.maxPrice === '' ? null : Number(f.maxPrice)
    const loc = f.location.trim().toLowerCase()

    return items.filter((item) => {
    const words = `${item.title} ${item.category ?? ''}`
        .toLowerCase()
        .split(/\W+/)
        .filter(Boolean)
    const keywordOk = tokens.every((t) => words.some((w) => w.startsWith(t)))
    const priceOk =
        (min === null || Number(item.maximumPrice) >= min) &&
        (max === null || Number(item.minimumPrice) <= max)
    const locationOk = !loc || (item.location ?? '').toLowerCase().includes(loc)
    return keywordOk && priceOk && locationOk
    })
}