import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { api, type RaceSummary } from '../api'
import { Flag } from "../components/Flag"

export default function SeasonRaces() {
    const { year } = useParams()
    const [races, setRaces] = useState<RaceSummary[] | null>(null)
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        setLoading(true)
        api.races(Number(year)).then(setRaces).finally(() => setLoading(false))
    }, [year])

    if (loading) return <p className="empty">Loading…</p>
    if (!races) return <p className="empty">No data for {year}. (Ingest it via the admin endpoint.)</p>

    return (
        <section>
            <div className="crumb">Season</div>
            <h1 className="page-title">{year} Races</h1>
            <div className="card" style={{ marginTop: '1.1rem' }}>
                <ul className="race-list">
                    {races.map(r => (
                        <li key={r.round}>
                            <span className="rnd">{r.round}</span>
                            <Link to={`/seasons/${year}/races/${r.round}`} style={{ fontWeight: 600 }}><Flag country={r.country} />{r.raceName}</Link>
                            <span className="muted" style={{ fontSize: '.85rem' }}>{r.circuitName}, {r.date}</span>
                        </li>
                    ))}
                </ul>
            </div>
        </section>
    )
}