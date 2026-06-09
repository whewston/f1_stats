import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { api, type DriverProfile } from '../api'
import { Stat } from '../components/Stat'
import { Flag } from "../components/Flag"

export default function DriverPage() {
    const { driverId } = useParams()
    const [driver, setDriver] = useState<DriverProfile | null>(null)
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        setLoading(true)
        api.driver(driverId!).then(setDriver).finally(() => setLoading(false))
    }, [driverId])

    if (loading) return <p className="empty">Loading…</p>
    if (!driver) return <p className="empty">Driver not found.</p>

    const t = driver.allTime
    return (
        <section>
            <div className="crumb">Driver</div>
            <h1 className="page-title"><Flag nationality={driver.nationality} />{driver.name}{driver.code && <span style={{ color: 'var(--accent)', fontSize: '1.3rem' }}>{driver.code}</span>}</h1>            <p className="muted">{driver.nationality}</p>

            <div className="stat-grid" style={{ margin: '1.25rem 0 1.75rem' }}>
                <Stat label="Titles" value={t.championships} />
                <Stat label="Wins" value={t.wins} />
                <Stat label="Podiums" value={t.podiums} />
                <Stat label="Poles" value={t.poles} />
                <Stat label="Points" value={t.points} />
                <Stat label="Races" value={t.races} />
                <Stat label="Best" value={t.bestFinish ?? '—'} />
            </div>

            <div className="card">
                <div className="card__head"><span className="card__title">By season</span></div>
                <div className="card__body" style={{ padding: '.3rem .6rem' }}>
                    <table className="table">
                        <thead><tr><th>Year</th><th>Team</th><th className="num">Pos</th><th className="num">Pts</th><th className="num">Wins</th><th className="num">Pod</th><th className="num">Races</th><th className="num">Best</th></tr></thead>
                        <tbody>
                        {driver.seasons.map(s => (
                            <tr key={s.year}>
                                <td><Link to={`/seasons/${s.year}`}>{s.year}</Link></td>
                                <td>{s.team ?? '—'}</td>
                                <td className="num">{s.standingPosition}</td><td className="num">{s.points}</td>
                                <td className="num">{s.wins}</td><td className="num">{s.podiums}</td>
                                <td className="num">{s.races}</td><td className="num">{s.bestFinish ?? '—'}</td>
                            </tr>
                        ))}
                        </tbody>
                    </table>
                </div>
            </div>
        </section>
    )
}