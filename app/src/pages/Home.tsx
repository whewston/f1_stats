import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api, type NextRace } from '../api'

export default function Home() {
    const [next, setNext] = useState<NextRace | null>(null)
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState<string | null>(null)

    useEffect(() => {
        api.nextRace().then(setNext).catch(e => setError(String(e))).finally(() => setLoading(false))
    }, [])

    if (loading) return <p>Loading…</p>
    if (error) return <p>Couldn’t load the next race: {error}</p>
    if (!next) return <p>No upcoming race scheduled. (Have you ingested the current season?)</p>

    return (
        <section>
            <h2>Next race</h2>
            <Link to={`/seasons/${next.season}/races/${next.round}`}
                  style={{ display: 'block', border: '1px solid #ddd', borderRadius: 12, padding: '1.25rem', textDecoration: 'none', color: 'inherit' }}>
                <div style={{ fontSize: '1.4rem', fontWeight: 700 }}>{next.raceName}</div>
                <div style={{ color: '#555' }}>{next.circuitName}{next.country ? `, ${next.country}` : ''}</div>
                <div style={{ marginTop: '.5rem' }}>Round {next.round} · {next.date}{next.time ? ` · ${next.time.slice(0, 5)} UTC` : ''}</div>
            </Link>
        </section>
    )
}