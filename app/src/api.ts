export interface RaceSummary {
    round: number; raceName: string; date: string; time: string | null
    circuitName: string; country: string | null; locality: string | null
}
export interface ResultRow {
    position: number | null; positionText: string
    driverId: string; driver: string; code: string | null
    constructorId: string; constructor: string
    grid: number | null; points: number; laps: number
    status: string; time: string | null; fastestLap: string | null;
    nationality: string | null

}
export interface RaceResults {
    season: number; round: number; raceName: string; date: string
    circuitName: string; results: ResultRow[]; country: string | null
}
export interface NextRace {
    season: number; round: number; raceName: string; date: string; time: string | null
    circuitName: string; country: string | null; locality: string | null
}
export interface DriverStanding {
    position: number; driverId: string; driver: string; code: string | null
    constructorId: string | null; constructor: string | null; points: number; wins: number; nationality: string | null

}

export interface StatTotals {
    races: number; wins: number; podiums: number; poles: number
    points: number; championships: number; bestFinish: number | null
}
export interface DriverSeasonStat {
    year: number; team: string | null; standingPosition: number; points: number
    wins: number; podiums: number; races: number; bestFinish: number | null
}
export interface DriverProfile {
    driverId: string; name: string; code: string | null; nationality: string | null
    allTime: StatTotals; seasons: DriverSeasonStat[]
}
export interface ConstructorSeasonStat {
    year: number; standingPosition: number; points: number; wins: number
    podiums: number; races: number; bestFinish: number | null
}
export interface ConstructorProfile {
    constructorId: string; name: string; nationality: string | null
    allTime: StatTotals; seasons: ConstructorSeasonStat[]
}
export interface ConstructorStanding {
    position: number; constructorId: string; constructor: string
    nationality: string | null; points: number; wins: number
}

async function getJson<T>(url: string): Promise<T | null> {
    const res = await fetch(url)
    if (res.status === 404) return null
    if (!res.ok) throw new Error(`${res.status} ${res.statusText}`)
    return res.json() as Promise<T>
}

export interface CircuitWin { driverId: string; driver: string; wins: number }
export interface PastEdition {
    year: number; round: number
    winnerDriverId: string | null; winner: string | null; winnerConstructor: string | null
}
export interface RacePreview {
    year: number; round: number; raceName: string; date: string; time: string | null
    circuitId: string; circuitName: string; country: string | null; locality: string | null
    topWinners: CircuitWin[]; pastEditions: PastEdition[]
    lastEditionYear: number | null; lastEditionRound: number | null; latitude: number | null; longitude: number | null
}

export interface PredictionRow {
    predictedPosition: number; driverId: string; driver: string; code: string | null
    nationality: string | null; constructorId: string | null; constructor: string | null
    winProbability: number | null
}
export interface RacePrediction {
    year: number; round: number; modelVersion: string; generatedAt: string
    rows: PredictionRow[]
}


export const api = {
    seasons: () => getJson<number[]>('/api/seasons'),
    races: (year: number) => getJson<RaceSummary[]>(`/api/seasons/${year}/races`),
    results: (year: number, round: number) => getJson<RaceResults>(`/api/seasons/${year}/races/${round}/results`),
    nextRace: () => getJson<NextRace>('/api/races/next'),
    driverStandings: (year: number) => getJson<DriverStanding[]>(`/api/seasons/${year}/standings/drivers`),
    driver: (id: string) => getJson<DriverProfile>(`/api/drivers/${id}`),
    constructor: (id: string) => getJson<ConstructorProfile>(`/api/constructors/${id}`),
    constructorStandings: (year: number) => getJson<ConstructorStanding[]>(`/api/seasons/${year}/standings/constructors`),
    preview: (year: number, round: number) => getJson<RacePreview>(`/api/seasons/${year}/races/${round}/preview`),
    prediction: (year: number, round: number) => getJson<RacePrediction>(`/api/seasons/${year}/races/${round}/prediction`),
}