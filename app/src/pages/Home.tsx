import { useEffect, useState, type ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { useYear } from '../YearContext'
import { api, type NextRace, type DriverStanding, type ConstructorStanding, type RaceResults } from '../api'
import { teamColor } from '../teamColors'
import { Pos } from '../components/Pos'
import { Flag } from '../components/Flag'

export default function Home() {
    const { year } = useYear()
    const [next, setNext] = useState<NextRace | null>(null)
    const [drivers, setDrivers] = useState<DriverStanding[] | null>(null)
    const [teams, setTeams] = useState<ConstructorStanding[] | null>(null)
    const [last, setLast] = useState<RaceResults | null>(null)
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        let active = true
        setLoading(true)
        ;(async () => {
            const [n, d, c, races] = await Promise.all([
                api.nextRace(), api.driverStandings(year), api.constructorStandings(year), api.races(year),
            ])
            let lastRace: RaceResults | null = null
            if (races?.length) {
                const today = new Date().toISOString().slice(0, 10)
                const past = races.filter(r => r.date <= today)
                const target = past.length ? past[past.length - 1] : null
                if (target) lastRace = await api.results(year, target.round)
            }
            if (!active) return
            setNext(n); setDrivers(d); setTeams(c); setLast(lastRace); setLoading(false)
        })()
        return () => { active = false }
    }, [year])

    if (loading) return <p className="empty">Loading…</p>

    const showNext = next && next.season === year
    const champ = drivers?.[0]

    return (
        <div className="dashboard">
            <div className="dashboard__main">
                {showNext ? (
                    <Link to={`/seasons/${next!.season}/races/${next!.round}/preview`} className="next rise">
                        <div className="next__label">Next race · Round {next!.round}</div>
                        <div className="next__name">{next!.raceName}</div>
                        <div className="next__sub"><Flag country={next!.country} />{next!.circuitName}{next!.country ? `, ${next!.country}` : ''}</div>                        <div className="next__meta">
                            <span>{next!.date}</span>
                            {next!.time && <span><b>{next!.time.slice(0, 5)}</b> UTC</span>}
                        </div>
                    </Link>
                ) : champ ? (
                    <div className="next rise">
                        <div className="next__label">{year} · Season complete</div>
                        <div className="next__name">{champ.driver}</div>
                        <div className="next__sub">World champion — {champ.points} pts · {champ.wins} wins</div>
                    </div>
                ) : null}

                <div className="card rise" style={{ animationDelay: '.06s' }}>
                    <div className="card__head">
                        <span className="card__title">Latest result</span>
                        {last && <Link to={`/seasons/${year}/races/${last.round}`} className="muted" style={{ fontFamily: 'var(--mono)', fontSize: '.78rem' }}>full →</Link>}
                    </div>
                    <div className="card__body">
                        {!last ? <div className="empty">No completed races yet for {year}.</div> : (
                            <>
                                <div style={{ fontFamily: 'var(--display)', fontWeight: 700, fontSize: '1.3rem', textTransform: 'uppercase' }}>{last.raceName}</div>
                                <div className="muted" style={{ fontSize: '.85rem', marginBottom: '.6rem' }}>{last.circuitName} · {last.date}</div>
                                <table className="table">
                                    <thead><tr><th>Pos</th><th>Driver</th><th>Team</th><th className="num">Pts</th></tr></thead>
                                    <tbody>
                                    {last.results.slice(0, 10).map(r => (
                                        <tr key={r.driverId}>
                                            <td><Pos n={r.position} text={r.positionText} /></td>
                                            <td><Flag nationality={r.nationality} /><Link to={`/drivers/${r.driverId}`}>{r.driver}</Link><span className="code">{r.code}</span></td>                                            
                                            <td><span className="dot" style={{ background: teamColor(r.constructorId) }} /><Link to={`/constructors/${r.constructorId}`}>{r.constructor}</Link></td>
                                            <td className="num">{r.points}</td>
                                        </tr>
                                    ))}
                                    </tbody>
                                </table>
                            </>
                        )}
                    </div>
                </div>
            </div>

            <div className="dashboard__side">
                <StandingsCard title="Drivers' standings" delay=".1s">
                    {!drivers?.length ? <div className="empty">No standings for {year}.</div> : (
                        <table className="table">
                            <tbody>
                            {drivers.slice(0, 10).map(s => (
                                <tr key={s.driverId}>
                                    <td><Pos n={s.position} text={String(s.position)} /></td>
                                    <td><Flag nationality={s.nationality} /><span className="dot" style={{ background: teamColor(s.constructorId) }} /><Link to={`/drivers/${s.driverId}`}>{s.driver}</Link></td>                                    <td className="num">{s.points}</td>
                                </tr>
                            ))}
                            </tbody>
                        </table>
                    )}
                </StandingsCard>

                <StandingsCard title="Constructors' standings" delay=".14s">
                    {!teams?.length ? <div className="empty">No standings for {year}.</div> : (
                        <table className="table">
                            <tbody>
                            {teams.slice(0, 10).map(s => (
                                <tr key={s.constructorId}>
                                    <td><Pos n={s.position} text={String(s.position)} /></td>
                                    <td><Flag nationality={s.nationality} /><span className="dot" style={{ background: teamColor(s.constructorId) }} /><Link to={`/constructors/${s.constructorId}`}>{s.constructor}</Link></td>                                    <td className="num">{s.points}</td>
                                </tr>
                            ))}
                            </tbody>
                        </table>
                    )}
                </StandingsCard>
            </div>
        </div>
    )
}

function StandingsCard({ title, delay, children }: { title: string; delay: string; children: ReactNode }) {
    return (
        <div className="card rise" style={{ animationDelay: delay }}>
            <div className="card__head"><span className="card__title">{title}</span></div>
            <div className="card__body" style={{ padding: '.3rem .6rem' }}>{children}</div>
        </div>
    )
}