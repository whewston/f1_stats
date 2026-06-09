import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { api, type RaceResults as RaceResultsType } from '../api'
import { teamColor } from '../teamColors'
import { Pos } from '../components/Pos'
import { Flag } from "../components/Flag"

export default function RaceResults() {
    const { year, round } = useParams()
    const [data, setData] = useState<RaceResultsType | null>(null)
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        setLoading(true)
        api.results(Number(year), Number(round)).then(setData).finally(() => setLoading(false))
    }, [year, round])

    if (loading) return <p className="empty">Loading…</p>
    if (!data) return <p className="empty">No results found.</p>

    return (
        <section>
            <div className="crumb"><Link to={`/seasons/${year}`}>{year}</Link> · Round {data.round}</div>
            <h1 className="page-title"><Flag country={data.country} />{data.raceName}</h1>
            <p className="muted">{data.circuitName} · {data.date}</p>
            <div className="card" style={{ marginTop: '1.1rem' }}>
                <div className="card__body" style={{ padding: '.3rem .6rem' }}>
                    {data.results.length === 0 ? <div className="empty">No results yet — this race hasn’t run.</div> : (
                        <table className="table">
                            <thead><tr><th>Pos</th><th>Driver</th><th>Team</th><th className="num">Grid</th><th className="num">Pts</th><th>Status</th></tr></thead>
                            <tbody>
                            {data.results.map(r => (
                                <tr key={r.driverId}>
                                    <td><Pos n={r.position} text={r.positionText} /></td>
                                    <td><Flag nationality={r.nationality} /><Link to={`/drivers/${r.driverId}`}>{r.driver}</Link><span className="code">{r.code}</span></td>                                    <td><span className="dot" style={{ background: teamColor(r.constructorId) }} /><Link to={`/constructors/${r.constructorId}`}>{r.constructor}</Link></td>
                                    <td className="num">{r.grid ?? '—'}</td><td className="num">{r.points}</td>
                                    <td className="muted">{r.status}</td>
                                </tr>
                            ))}
                            </tbody>
                        </table>
                    )}
                </div>
            </div>
        </section>
    )
}