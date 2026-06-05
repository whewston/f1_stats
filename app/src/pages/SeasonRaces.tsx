import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { api, type RaceSummary } from '../api'

export default function SeasonRaces() {
    const { year } = useParams()
    const [races, setRaces] = useState<RaceSummary[] | null>(null)
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        setLoading(true)
        api.races(Number(year)).then(setRaces).finally(() => setLoading(false))
    }, [year])

    if (loading) return <p>Loading…</p>
    if (!races) return <p>No data for {year}. (Ingest it via the admin endpoint.)</p>

    return (
        <section>
            <h2>{year} races</h2>
            <ul style={{ listStyle: 'none', padding: 0 }}>
                {races.map(r => (
                    <li key={r.round} style={{ padding: '.5rem 0', borderBottom: '1px solid #eee' }}>
                        <Link to={`/seasons/${year}/races/${r.round}`}>{r.round}. {r.raceName}</Link>{' '}
                        <span style={{ color: '#777' }}>— {r.circuitName}, {r.date}</span>
                    </li>
                ))}
            </ul>
        </section>
    )
}