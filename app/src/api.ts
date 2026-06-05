export interface RaceSummary {
    round: number; raceName: string; date: string; time: string | null
    circuitName: string; country: string | null; locality: string | null
}
export interface ResultRow {
    position: number | null; positionText: string; driver: string; code: string | null
    constructor: string; grid: number | null; points: number; laps: number
    status: string; time: string | null; fastestLap: string | null
}
export interface RaceResults {
    season: number; round: number; raceName: string; date: string
    circuitName: string; results: ResultRow[]
}
export interface NextRace {
    season: number; round: number; raceName: string; date: string; time: string | null
    circuitName: string; country: string | null; locality: string | null
}

async function getJson<T>(url: string): Promise<T | null> {
    const res = await fetch(url)
    if (res.status === 404) return null
    if (!res.ok) throw new Error(`${res.status} ${res.statusText}`)
    return res.json() as Promise<T>
}

export const api = {
    seasons: () => getJson<number[]>('/api/seasons'),
    races: (year: number) => getJson<RaceSummary[]>(`/api/seasons/${year}/races`),
    results: (year: number, round: number) => getJson<RaceResults>(`/api/seasons/${year}/races/${round}/results`),
    nextRace: () => getJson<NextRace>('/api/races/next'),
}