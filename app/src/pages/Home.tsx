import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api, type NextRace, type DriverStanding } from '../api'

const CURRENT_SEASON = new Date().getFullYear()

export default function Home() {
    const [next, setNext] = useState<NextRace | null>(null)
    const [standings, setStandings] = useState<DriverStanding[] | null>(null)
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        Promise.all([api.nextRace(), api.driverStandings(CURRENT_SEASON)])
            .then(([n, s]) => { setNext(n); setStandings(s) })
            .finally(() => setLoading(false))
    }, [])

    if (loading) return <p>Loading…</p>

    return (
        <section>
            <h2>Next race</h2>
            {next ? (
                <Link to={`/seasons/${next.season}/races/${next.round}`}
                      style={{ display: 'block', border: '1px solid #ddd', borderRadius: 12, padding: '1.25rem', textDecoration: 'none', color: 'inherit' }}>
                    <div style={{ fontSize: '1.4rem', fontWeight: 700 }}>{next.raceName}</div>
                    <div style={{ color: '#555' }}>{next.circuitName}{next.country ? `, ${next.country}` : ''}</div>
                    <div style={{ marginTop: '.5rem' }}>Round {next.round} · {next.date}{next.time ? ` · ${next.time.slice(0,5)} UTC` : ''}</div>
                </Link>
            ) : <p>No upcoming race scheduled.</p>}

            <h2 style={{ marginTop: '2rem' }}>{CURRENT_SEASON} drivers’ standings</h2>
            {standings && standings.length > 0 ? (
                <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                    <thead><tr style={{ textAlign: 'left', borderBottom: '2px solid #333' }}>
                        <th>Pos</th><th>Driver</th><th>Team</th><th>Wins</th><th>Pts</th>
                    </tr></thead>
                    <tbody>
                    {standings.map(s => (
                        <tr key={s.driver} style={{ borderBottom: '1px solid #eee' }}>
                            <td>{s.position}</td><td>{s.driver}</td><td>{s.constructor ?? '—'}</td>
                            <td>{s.wins}</td><td>{s.points}</td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            ) : <p>No standings for {CURRENT_SEASON} yet — ingest the season.</p>}
        </section>
    )
}