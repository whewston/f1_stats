import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { api, type RacePreview, type RaceResults } from '../api'
import { teamColor } from '../teamColors'
import { Pos } from '../components/Pos'
import {Flag} from "../components/Flag.tsx";

export default function RacePreview() {
    const { year, round } = useParams()
    const [preview, setPreview] = useState<RacePreview | null>(null)
    const [last, setLast] = useState<RaceResults | null>(null)
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        let active = true
        setLoading(true)
        ;(async () => {
            const p = await api.preview(Number(year), Number(round))
            let lastRes: RaceResults | null = null
            if (p?.lastEditionYear && p.lastEditionRound)
                lastRes = await api.results(p.lastEditionYear, p.lastEditionRound)
            if (!active) return
            setPreview(p); setLast(lastRes); setLoading(false)
        })()
        return () => { active = false }
    }, [year, round])

    if (loading) return <p className="empty">Loading…</p>
    if (!preview) return <p className="empty">Race not found.</p>

    const podium = last?.results.slice(0, 3) ?? []
    const lead = preview.topWinners[0]
    const since = preview.pastEditions[preview.pastEditions.length - 1]?.year

    return (
        <section>
            <div className="crumb">Upcoming · Round {preview.round}</div>
            <h1 className="page-title"><Flag country={preview.country} />{preview.raceName}</h1>
            <p className="muted">
                {preview.circuitName}{preview.country ? `, ${preview.country}` : ''} · {preview.date}
                {preview.time ? ` · ${preview.time.slice(0, 5)} UTC` : ''}
            </p>

            <div className="dashboard" style={{ marginTop: '1.25rem' }}>
                <div className="dashboard__main">
                    {last && podium.length > 0 ? (
                        <div className="card rise">
                            <div className="card__head">
                                <span className="card__title">Last time out · {last.season}</span>
                                <Link to={`/seasons/${last.season}/races/${last.round}`} className="muted" style={{ fontFamily: 'var(--mono)', fontSize: '.78rem' }}>full result →</Link>
                            </div>
                            <div className="card__body">
                                <div className="podium">
                                    {podium.map(p => (
                                        <div key={p.driverId} className={`podium__step podium__step--${p.position}`}>
                                            <Pos n={p.position} text={String(p.position)} />
                                            <Link to={`/drivers/${p.driverId}`} style={{ fontWeight: 700 }}><Flag nationality={p.nationality} />{p.driver}</Link>                                            <div className="muted" style={{ fontSize: '.8rem' }}>
                                                <span className="dot" style={{ background: teamColor(p.constructorId) }} />{p.constructor}
                                            </div>
                                        </div>
                                    ))}
                                </div>

                                <div style={{ fontFamily: 'var(--display)', textTransform: 'uppercase', letterSpacing: '.06em', color: 'var(--text-dim)', fontSize: '.78rem', margin: '1.25rem 0 .4rem' }}>Grid → Finish</div>
                                <table className="table">
                                    <thead><tr><th>Fin</th><th>Driver</th><th>Team</th><th className="num">Grid</th><th className="num">Pts</th></tr></thead>
                                    <tbody>
                                    {last.results.map(r => (
                                        <tr key={r.driverId}>
                                            <td><Pos n={r.position} text={r.positionText} /></td>
                                            <td><Flag nationality={r.nationality} /><Link to={`/drivers/${r.driverId}`}>{r.driver}</Link></td>                                            <td><span className="dot" style={{ background: teamColor(r.constructorId) }} /><Link to={`/constructors/${r.constructorId}`}>{r.constructor}</Link></td>
                                            <td className="num">{r.grid ?? '—'}</td><td className="num">{r.points}</td>
                                        </tr>
                                    ))}
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    ) : (
                        <div className="card"><div className="card__body"><div className="empty">No prior edition of this race in the data yet.</div></div></div>
                    )}
                </div>

                <div className="dashboard__side">
                    <div className="card rise" style={{ animationDelay: '.06s' }}>
                        <div className="card__head"><span className="card__title">At {preview.circuitName}</span></div>
                        <div className="card__body">
                            {preview.topWinners.length === 0 ? <div className="empty">No past winners in the data.</div> : (
                                <>
                                    {lead && (
                                        <p style={{ marginBottom: '.85rem' }}>
                                            <Link to={`/drivers/${lead.driverId}`} style={{ fontWeight: 700 }}>{lead.driver}</Link> has won here <b>{lead.wins}</b> time{lead.wins > 1 ? 's' : ''}
                                            {since && <span className="muted"> (since {since}).</span>}
                                        </p>
                                    )}
                                    <table className="table">
                                        <thead><tr><th>Most wins here</th><th className="num">Wins</th></tr></thead>
                                        <tbody>
                                        {preview.topWinners.map(w => (
                                            <tr key={w.driverId}>
                                                <td><Link to={`/drivers/${w.driverId}`}>{w.driver}</Link></td>
                                                <td className="num">{w.wins}</td>
                                            </tr>
                                        ))}
                                        </tbody>
                                    </table>
                                </>
                            )}
                        </div>
                    </div>

                    <div className="card rise" style={{ animationDelay: '.1s' }}>
                        <div className="card__head"><span className="card__title">Past winners</span></div>
                        <div className="card__body" style={{ padding: '.3rem .6rem' }}>
                            {preview.pastEditions.length === 0 ? <div className="empty">—</div> : (
                                <table className="table">
                                    <tbody>
                                    {preview.pastEditions.map(e => (
                                        <tr key={e.year}>
                                            <td style={{ color: 'var(--text-faint)', fontFamily: 'var(--mono)' }}>{e.year}</td>
                                            <td>{e.winnerDriverId ? <Link to={`/drivers/${e.winnerDriverId}`}>{e.winner}</Link> : '—'}</td>
                                            <td className="muted">{e.winnerConstructor}</td>
                                        </tr>
                                    ))}
                                    </tbody>
                                </table>
                            )}
                        </div>
                    </div>
                </div>
            </div>
        </section>
    )
}