import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { api } from './api'

interface YearCtx { year: number; setYear: (y: number) => void; seasons: number[] }
const Ctx = createContext<YearCtx | null>(null)

export function YearProvider({ children }: { children: ReactNode }) {
    const [seasons, setSeasons] = useState<number[]>([])
    const [year, setYear] = useState(new Date().getFullYear())

    useEffect(() => {
        api.seasons().then(s => { if (s?.length) { setSeasons(s); setYear(Math.max(...s)) } })
    }, [])

    return <Ctx.Provider value={{ year, setYear, seasons }}>{children}</Ctx.Provider>
}

export function useYear() {
    const c = useContext(Ctx)
    if (!c) throw new Error('useYear must be inside YearProvider')
    return c
}