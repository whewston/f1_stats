import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { api, type RacePreview, type RaceResults, type RacePrediction, type RaceQualifying } from '../api'
import { teamColor } from '../teamColors'
import { Pos } from '../components/Pos'
import { Flag } from '../components/Flag'
import { TrackMap } from '../components/TrackMap'


export default function RacePage() {
    const { year, round } = useParams()
    const [preview, setPreview] = useState<RacePreview | null>(null)
    const [own, setOwn] = useState<RaceResults | null>(null)        // this race's results
    const [lastEd, setLastEd] = useState<RaceResults | null>(null)  // previous edition (future mode)
    const [loading, setLoading] = useState(true)
    const [prediction, setPrediction] = useState<RacePrediction | null>(null)
    const [quali, setQuali] = useState<RaceQualifying | null>(null)

    useEffect(() => {
        let active = true
        setLoading(true)
        ;(async () => {
            const [p, o, pred, q] = await Promise.all([
                api.preview(Number(year), Number(round)),
                api.results(Number(year), Number(round)),
                api.prediction(Number(year), Number(round)),
                api.qualifying(Number(year), Number(round)),
            ])
            let last: RaceResults | null = null
            const ran = !!o && o.results.length > 0
            if (!ran && p?.lastEditionYear && p.lastEditionRound)
                last = await api.results(p.lastEditionYear, p.lastEditionRound)
            if (!active) return
            setPreview(p); setOwn(o); setLastEd(last); setPrediction(pred); setQuali(q); setLoading(false)
        })()
        return () => { active = false }
    }, [year, round])

    if (loading) return <p className="empty">Loading…</p>
    if (!preview && !own) return <p className="empty">Race not found.</p>

    const hasRun = !!own && own.results.length > 0
    const name = preview?.raceName ?? own?.raceName ?? 'Race'
    const country = preview?.country ?? own?.country ?? null
    const circuit = preview?.circuitName ?? own?.circuitName ?? ''
    const date = preview?.date ?? own?.date ?? ''
    const lead = preview?.topWinners?.[0]
    const since = preview?.pastEditions?.[preview.pastEditions.length - 1]?.year

    return (
        <section>
            <div className="crumb"><Link to={`/seasons/${year}`}>{year}</Link> · Round {round}{!hasRun && ' · Upcoming'}</div>
            <h1 className="page-title"><Flag country={country} />{name}</h1>
            <p className="muted">{circuit}{country ? `, ${country}` : ''} · {date}</p>

            <div className="dashboard" style={{ marginTop: '1.25rem' }}>
                <div className="dashboard__main">

                    {!hasRun && (
                        <div className="card rise">
                            <div className="card__head">
                                <span className="card__title">Predicted result</span>
                                {prediction && <span className="muted" style={{ fontFamily: 'var(--mono)', fontSize: '.72rem' }}>{prediction.modelVersion}</span>}
                            </div>
                            <div className="card__body" style={{ padding: prediction ? '.3rem .6rem' : '1.1rem' }}>
                                {!prediction ? (
                                    <div className="empty">No prediction published yet.</div>
                                ) : (
                                    <table className="table">
                                        <thead><tr><th>Pos</th><th>Driver</th><th>Team</th><th className="num">Win %</th></tr></thead>
                                        <tbody>
                                        {prediction.rows.map(r => (
                                            <tr key={r.driverId}>
                                                <td><Pos n={r.predictedPosition} text={String(r.predictedPosition)} /></td>
                                                <td>
                                                    <Flag nationality={r.nationality} /><Link to={`/drivers/${r.driverId}`}>{r.driver}</Link><span className="code">{r.code}</span>
                                                    {r.reasons.length > 0 && (
                                                        <div className="muted" style={{ fontSize: '.72rem', marginTop: '.15rem', lineHeight: 1.35 }}>
                                                            {r.reasons.join(' · ')}
                                                        </div>
                                                    )}
                                                </td>                                                <td>{r.constructorId
                                                    ? <><span className="dot" style={{ background: teamColor(r.constructorId) }} /><Link to={`/constructors/${r.constructorId}`}>{r.constructor}</Link></>
                                                    : <span className="muted">—</span>}</td>
                                                <td className="num">{r.winProbability != null ? `${(r.winProbability * 100).toFixed(0)}%` : '—'}</td>
                                            </tr>
                                        ))}
                                        </tbody>
                                    </table>
                                )}
                            </div>
                        </div>
                    )}
                    
                    {hasRun ? (
                        <div className="card rise">
                            <div className="card__head"><span className="card__title">Result</span></div>
                            <div className="card__body" style={{ padding: '.3rem .6rem' }}>
                                <table className="table">
                                    <thead><tr><th>Pos</th><th>Driver</th><th>Team</th><th className="num">Grid</th><th className="num">Pts</th><th>Status</th></tr></thead>
                                    <tbody>
                                    {own!.results.map(r => (
                                        <tr key={r.driverId}>
                                            <td><Pos n={r.position} text={r.positionText} /></td>
                                            <td><Flag nationality={r.nationality} /><Link to={`/drivers/${r.driverId}`}>{r.driver}</Link><span className="code">{r.code}</span></td>
                                            <td><span className="dot" style={{ background: teamColor(r.constructorId) }} /><Link to={`/constructors/${r.constructorId}`}>{r.constructor}</Link></td>
                                            <td className="num">{r.grid ?? '—'}</td><td className="num">{r.points}</td>
                                            <td className="muted">{r.status}</td>
                                        </tr>
                                    ))}
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    ) : lastEd && lastEd.results.length > 0 ? (
                        <div className="card rise">
                            <div className="card__head"><span className="card__title">Last time out · {lastEd.season}</span>
                                <Link to={`/seasons/${lastEd.season}/races/${lastEd.round}`} className="muted" style={{ fontFamily: 'var(--mono)', fontSize: '.78rem' }}>full result →</Link>
                            </div>
                            <div className="card__body">
                                <div className="podium">
                                    {lastEd.results.slice(0, 3).map(p => (
                                        <div key={p.driverId} className={`podium__step podium__step--${p.position}`}>
                                            <Pos n={p.position} text={String(p.position)} />
                                            <Link to={`/drivers/${p.driverId}`} style={{ fontWeight: 700 }}><Flag nationality={p.nationality} />{p.driver}</Link>
                                            <div className="muted" style={{ fontSize: '.8rem' }}><span className="dot" style={{ background: teamColor(p.constructorId) }} />{p.constructor}</div>
                                        </div>
                                    ))}
                                </div>
                                <div style={{ fontFamily: 'var(--display)', textTransform: 'uppercase', letterSpacing: '.06em', color: 'var(--text-dim)', fontSize: '.78rem', margin: '1.25rem 0 .4rem' }}>Grid → Finish</div>
                                <table className="table">
                                    <thead><tr><th>Fin</th><th>Driver</th><th>Team</th><th className="num">Grid</th><th className="num">Pts</th></tr></thead>
                                    <tbody>
                                    {lastEd.results.map(r => (
                                        <tr key={r.driverId}>
                                            <td><Pos n={r.position} text={r.positionText} /></td>
                                            <td><Flag nationality={r.nationality} /><Link to={`/drivers/${r.driverId}`}>{r.driver}</Link></td>
                                            <td><span className="dot" style={{ background: teamColor(r.constructorId) }} /><Link to={`/constructors/${r.constructorId}`}>{r.constructor}</Link></td>
                                            <td className="num">{r.grid ?? '—'}</td><td className="num">{r.points}</td>
                                        </tr>
                                    ))}
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    ) : (
                        <div className="card"><div className="card__body"><div className="empty">No results yet, and no prior edition of this race in the data.</div></div></div>
                    )}

                    {quali && quali.rows.length > 0 && (
                        <div className="card rise">
                            <div className="card__head"><span className="card__title">Qualifying</span></div>
                            <div className="card__body" style={{ padding: '.3rem .6rem' }}>
                                <table className="table">
                                    <thead><tr><th>Pos</th><th>Driver</th><th>Team</th><th className="num">Q1</th><th className="num">Q2</th><th className="num">Q3</th></tr></thead>
                                    <tbody>
                                    {quali.rows.map(r => (
                                        <tr key={r.driverId}>
                                            <td><Pos n={r.position} text={String(r.position)} /></td>
                                            <td><Flag nationality={r.nationality} /><Link to={`/drivers/${r.driverId}`}>{r.driver}</Link><span className="code">{r.code}</span></td>
                                            <td>{r.constructorId
                                                ? <><span className="dot" style={{ background: teamColor(r.constructorId) }} /><Link to={`/constructors/${r.constructorId}`}>{r.constructor}</Link></>
                                                : <span className="muted">—</span>}</td>
                                            <td className="num">{r.q1 ?? '—'}</td>
                                            <td className="num">{r.q2 ?? '—'}</td>
                                            <td className="num">{r.q3 ?? '—'}</td>
                                        </tr>
                                    ))}
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    )}
                </div>

                <div className="dashboard__side">
                    <div className="card rise">
                        <div className="card__head"><span className="card__title">Circuit</span>
                            {preview?.locality && <span className="muted" style={{ fontFamily: 'var(--mono)', fontSize: '.78rem' }}>{preview.locality}</span>}
                        </div>
                        <div className="card__body"><TrackMap lat={preview?.latitude} lng={preview?.longitude} /></div>
                    </div>

                    <div className="card rise" style={{ animationDelay: '.06s' }}>
                        <div className="card__head"><span className="card__title">{hasRun ? 'Winners here' : `At ${circuit}`}</span></div>
                        <div className="card__body">
                            {!preview?.topWinners?.length ? <div className="empty">No past winners in the data.</div> : (
                                <>
                                    {lead && !hasRun && (
                                        <p style={{ marginBottom: '.85rem' }}>
                                            <Link to={`/drivers/${lead.driverId}`} style={{ fontWeight: 700 }}>{lead.driver}</Link> has won here <b>{lead.wins}</b> time{lead.wins > 1 ? 's' : ''}
                                            {since && <span className="muted"> (since {since}).</span>}
                                        </p>
                                    )}
                                    <table className="table">
                                        <thead><tr><th>Most wins here</th><th className="num">Wins</th></tr></thead>
                                        <tbody>
                                        {preview.topWinners.map(w => (
                                            <tr key={w.driverId}><td><Link to={`/drivers/${w.driverId}`}>{w.driver}</Link></td><td className="num">{w.wins}</td></tr>
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
                            {!preview?.pastEditions?.length ? <div className="empty">—</div> : (
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